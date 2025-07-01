using Microsoft.Extensions.Logging;

using System.Text;

namespace Modeller.Mcp.Shared.CodeGeneration.Prompts.VSA;

/// <summary>
/// Service for loading and processing VSA prompt templates
/// </summary>
public interface IVsaPromptService
{
    /// <summary>
    /// Generate SDK prompt from domain folder path - discovers models and extracts features/namespace
    /// </summary>
    /// <param name="domainFolderPath">Absolute path to the domain folder containing model definitions</param>
    /// <returns>Generated SDK prompt as a string</returns>
    Task<string> GenerateSDKFromDomainModelAsync(string domainFolderPath);

    /// <summary>
    /// Get available VSA prompt templates
    /// </summary>
    Task<IEnumerable<string>> GetAvailableTemplatesAsync();
}

/// <summary>
/// Implementation of VSA prompt service
/// </summary>
public class VsaPromptService(ILogger<VsaPromptService> logger) : IVsaPromptService
{
    private readonly string _promptsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CodeGeneration", "Prompts", "VSA");

    public async Task<string> GenerateSDKFromDomainModelAsync(string domainFolderPath)
    {
        try
        {
            logger.LogInformation("Generating SDK prompt from domain folder: {DomainPath}", domainFolderPath);

            if (!Directory.Exists(domainFolderPath))
                throw new DirectoryNotFoundException($"Domain folder not found: {domainFolderPath}");

            // Extract namespace from directory structure
            // e.g., "C:\path\models\Business\CustomerManagement" -> "Business.CustomerManagement.Sdk"
            var pathParts = domainFolderPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
            var modelsIndex = Array.FindLastIndex(pathParts, p => p.Equals("models", StringComparison.OrdinalIgnoreCase));

            if (modelsIndex == -1 || modelsIndex >= pathParts.Length - 1)
                throw new ArgumentException($"Invalid domain path structure. Expected path containing 'models' folder: {domainFolderPath}");

            var namespaceParts = pathParts.Skip(modelsIndex + 1).ToList();
            var namespaceName = string.Join(".", namespaceParts) + ".Sdk";

            // Discover all YAML files in the domain folder
            var yamlFiles = Directory.GetFiles(domainFolderPath, "*.yaml", SearchOption.TopDirectoryOnly)
                .Concat(Directory.GetFiles(domainFolderPath, "*.yml", SearchOption.TopDirectoryOnly))
                .ToList();

            if (yamlFiles.Count == 0)
                throw new InvalidOperationException($"No YAML model files found in domain folder: {domainFolderPath}");

            // Read all model files and combine them
            var allModelContent = new StringBuilder();
            var featureNames = new HashSet<string>();

            foreach (var yamlFile in yamlFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(yamlFile);

                // Extract feature name from .Type.yaml files
                if (fileName.EndsWith(".Type", StringComparison.OrdinalIgnoreCase))
                {
                    var featureName = fileName[..^5]; // Remove ".Type"
                    featureNames.Add(featureName);
                }

                var content = await File.ReadAllTextAsync(yamlFile);
                allModelContent.AppendLine($"# {Path.GetFileName(yamlFile)}");
                allModelContent.AppendLine("```yaml");
                allModelContent.AppendLine(content);
                allModelContent.AppendLine("```");
                allModelContent.AppendLine();
            }

            // Determine primary feature name (use the domain folder name if multiple features)
            var primaryFeatureName = featureNames.Count == 1 ? featureNames.First() : pathParts.Last();

            // Build the comprehensive SDK generation prompt
            var templatePath = Path.Combine(_promptsPath, "GenerateSDKFromDomainModel.md");

            if (!File.Exists(templatePath))
                throw new FileNotFoundException($"VSA template not found: {templatePath}");

            var templateContent = await File.ReadAllTextAsync(templatePath);

            var prompt = new StringBuilder(templateContent);
            prompt.AppendLine();
            prompt.AppendLine("---");
            prompt.AppendLine();
            prompt.AppendLine("# Project Configuration");
            prompt.AppendLine($"**Target Namespace**: {namespaceName}");
            prompt.AppendLine($"**Primary Feature**: {primaryFeatureName}");
            prompt.AppendLine($"**All Features**: {string.Join(", ", featureNames)}");
            prompt.AppendLine($"**Domain Path**: {domainFolderPath}");
            prompt.AppendLine();

            prompt.AppendLine("## Domain Model Definitions");
            prompt.AppendLine(allModelContent.ToString());

            return prompt.ToString();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate SDK prompt from domain folder: {DomainPath}", domainFolderPath);
            throw;
        }
    }

    public Task<IEnumerable<string>> GetAvailableTemplatesAsync()
    {
        try
        {
            if (!Directory.Exists(_promptsPath))
            {
                logger.LogWarning("VSA prompts directory not found: {Path}", _promptsPath);
                return Task.FromResult<IEnumerable<string>>([]);
            }

            var templateFiles = Directory.GetFiles(_promptsPath, "*.md");
            var templates = new List<string>();

            foreach (var templateFile in templateFiles)
            {
                var templateName = Path.GetFileNameWithoutExtension(templateFile);
                templates.Add(templateName);
            }

            return Task.FromResult<IEnumerable<string>>(templates);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get available VSA templates");
            return Task.FromResult<IEnumerable<string>>([]);
        }
    }
}
