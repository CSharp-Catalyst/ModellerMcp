using Microsoft.Extensions.Logging;
using System.Text;

namespace Modeller.McpServer.CodeGeneration.Prompts;

/// <summary>
/// Service for loading and processing VSA prompt templates
/// </summary>
public interface IVsaPromptService
{
    /// <summary>
    /// Generate SDK code from domain model using VSA patterns
    /// </summary>
    Task<string> GenerateSDKFromDomainModelAsync(string domainModelYaml, string featureName, string namespaceName);
    
    /// <summary>
    /// Get available VSA prompt templates
    /// </summary>
    Task<IEnumerable<string>> GetAvailableTemplatesAsync();
}

/// <summary>
/// Implementation of VSA prompt service
/// </summary>
public class VsaPromptService : IVsaPromptService
{
    private readonly ILogger<VsaPromptService> _logger;
    private readonly string _promptsPath;

    public VsaPromptService(ILogger<VsaPromptService> logger)
    {
        _logger = logger;
        _promptsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CodeGeneration", "Prompts", "VSA");
    }

    public async Task<string> GenerateSDKFromDomainModelAsync(string domainModelYaml, string featureName, string namespaceName)
    {
        try
        {
            var templatePath = Path.Combine(_promptsPath, "GenerateSDKFromDomainModel.md");
            
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"VSA template not found: {templatePath}");
            }

            var templateContent = await File.ReadAllTextAsync(templatePath);
            
            // Build the complete prompt with domain model context
            var prompt = new StringBuilder();
            
            // Add the template as the system context
            prompt.AppendLine("# System Context");
            prompt.AppendLine(templateContent);
            prompt.AppendLine();
            
            // Add the specific request
            prompt.AppendLine("# Generation Request");
            prompt.AppendLine($"**Target Namespace**: {namespaceName}");
            prompt.AppendLine($"**Feature Name**: {featureName}");
            prompt.AppendLine();
            
            prompt.AppendLine("# Domain Model YAML");
            prompt.AppendLine("```yaml");
            prompt.AppendLine(domainModelYaml);
            prompt.AppendLine("```");
            prompt.AppendLine();
            
            prompt.AppendLine("# Instructions");
            prompt.AppendLine($"Generate a complete SDK vertical slice for the {featureName} feature using the provided domain model.");
            prompt.AppendLine("Follow the VSA patterns and guidelines specified above.");
            prompt.AppendLine("Provide complete, compilable C# files with proper namespaces and documentation.");
            
            return prompt.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate SDK prompt for feature {FeatureName}", featureName);
            throw;
        }
    }

    public Task<IEnumerable<string>> GetAvailableTemplatesAsync()
    {
        try
        {
            if (!Directory.Exists(_promptsPath))
            {
                _logger.LogWarning("VSA prompts directory not found: {Path}", _promptsPath);
                return Task.FromResult<IEnumerable<string>>(Array.Empty<string>());
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
            _logger.LogError(ex, "Failed to get available VSA templates");
            return Task.FromResult<IEnumerable<string>>(Array.Empty<string>());
        }
    }
}
