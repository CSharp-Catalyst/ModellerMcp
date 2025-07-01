using Microsoft.Extensions.Logging;

using Modeller.Mcp.Shared.CodeGeneration.Prompts.VSA;
using Modeller.Mcp.Shared.CodeGeneration.Security;
using Modeller.Mcp.Shared.Services;

namespace Modeller.Mcp.Shared.CodeGeneration;

/// <summary>
/// Service for generating Minimal API projects from SDK and domain models
/// </summary>
public interface IApiGenerationService
{
    /// <summary>
    /// Generate a complete Minimal API project from SDK and domain models
    /// </summary>
    Task<ApiGenerationResult> GenerateAPIAsync(ApiGenerationRequest request);

    /// <summary>
    /// Generate the prompt that would be used for API creation
    /// </summary>
    Task<string> GeneratePromptAsync(ApiGenerationRequest request);
}

/// <summary>
/// Request for API generation
/// </summary>
public record ApiGenerationRequest
{
    /// <summary>
    /// Path to the generated SDK (e.g., playschool/generated-sdk)
    /// </summary>
    public required string SdkPath { get; init; }

    /// <summary>
    /// Path to the domain models (e.g., playschool/models/JJs/PotentialSales)
    /// </summary>
    public required string DomainPath { get; init; }

    /// <summary>
    /// Name of the API project (e.g., "JJs.PotentialSales.Api")
    /// </summary>
    public required string ProjectName { get; init; }

    /// <summary>
    /// Target namespace for the API (e.g., "JJs.PotentialSales.Api")
    /// </summary>
    public required string Namespace { get; init; }

    /// <summary>
    /// Output directory for the API project
    /// </summary>
    public required string OutputPath { get; init; }
}

/// <summary>
/// Result of API generation
/// </summary>
public record ApiGenerationResult
{
    /// <summary>
    /// Whether the generation was successful
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Generated prompt used for API creation
    /// </summary>
    public string? GeneratedPrompt { get; init; }

    /// <summary>
    /// Path where files were generated
    /// </summary>
    public string? OutputPath { get; init; }

    /// <summary>
    /// List of generated files
    /// </summary>
    public List<string> GeneratedFiles { get; init; } = [];

    /// <summary>
    /// Error message if generation failed
    /// </summary>
    public string? ErrorMessage { get; init; }

    public static ApiGenerationResult Success(string prompt, string outputPath, List<string> files) => new()
    {
        IsSuccess = true,
        GeneratedPrompt = prompt,
        OutputPath = outputPath,
        GeneratedFiles = files
    };

    public static ApiGenerationResult Failure(string errorMessage) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage
    };
}

/// <summary>
/// Implementation of API generation service
/// </summary>
public class ApiGenerationService(ModelDiscoveryService modelDiscoveryService, ISecureLlmService secureLlmService, ILogger<ApiGenerationService> logger) : IApiGenerationService
{
    public async Task<ApiGenerationResult> GenerateAPIAsync(ApiGenerationRequest request)
    {
        try
        {
            logger.LogInformation("Starting Minimal API generation for project '{ProjectName}' from SDK '{SdkPath}'",
                request.ProjectName, request.SdkPath);

            // 1. Validate SDK path exists
            if (!Directory.Exists(request.SdkPath))
                return ApiGenerationResult.Failure($"SDK path does not exist: {request.SdkPath}");

            // 2. Validate and discover domain models
            var discoveryResult = modelDiscoveryService.DiscoverModels(request.DomainPath);
            if (discoveryResult.Errors.Count > 0)
                return ApiGenerationResult.Failure($"Failed to discover models: {string.Join("; ", discoveryResult.Errors)}");

            // 3. Generate the API prompt
            var prompt = await GeneratePromptAsync(request);

            // 4. Use secure LLM service to generate the actual code
            var secureRequest = new SecureLlmRequest
            {
                RawPrompt = prompt,
                ModelId = "gpt-4",
                PromptType = "api_generation",
                SecurityContext = new SecurityContext
                {
                    UserId = "system",
                    SessionId = Guid.CreateVersion7().ToString(),
                    IPAddress = "127.0.0.1",
                    UserAgent = "ModellerMcp/1.0"
                },
                PromptInputs = new Dictionary<string, string>
                {
                    ["project_name"] = request.ProjectName,
                    ["namespace"] = request.Namespace,
                    ["sdk_path"] = request.SdkPath,
                    ["domain_path"] = request.DomainPath
                }
            };

            var codeGenerationResult = await secureLlmService.GenerateSecureCodeAsync(secureRequest);

            if (!codeGenerationResult.IsSuccess)
                return ApiGenerationResult.Failure($"Code generation failed: {codeGenerationResult.ErrorMessage}");

            // 5. Save generated files to output directory
            Directory.CreateDirectory(request.OutputPath);
            var generatedFiles = new List<string>();

            // Save the prompt and generated code for reference
            var promptPath = Path.Combine(request.OutputPath, "GeneratedPrompt.md");
            await File.WriteAllTextAsync(promptPath, prompt);
            generatedFiles.Add(promptPath);

            var codePath = Path.Combine(request.OutputPath, "GeneratedCode.md");
            await File.WriteAllTextAsync(codePath, codeGenerationResult.Content ?? "");
            generatedFiles.Add(codePath);

            logger.LogInformation("Minimal API generation completed successfully. Generated {Count} files", generatedFiles.Count);

            return ApiGenerationResult.Success(prompt, request.OutputPath, generatedFiles);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during Minimal API generation for project '{ProjectName}'", request.ProjectName);
            return ApiGenerationResult.Failure($"API generation failed: {ex.Message}");
        }
    }

    public async Task<string> GeneratePromptAsync(ApiGenerationRequest request)
    {
        try
        {
            // 1. Load the structured prompt template
            var promptTemplatePath = Path.Combine("CodeGeneration", "Prompts", "VSA", "GenerateMinimalAPIFromSDK.md");
            var promptTemplateFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, promptTemplatePath);
            
            if (!File.Exists(promptTemplateFullPath))
                throw new FileNotFoundException($"Prompt template not found: {promptTemplateFullPath}");
                
            var promptTemplate = await File.ReadAllTextAsync(promptTemplateFullPath);

            // 2. Discover domain models
            var discoveryResult = modelDiscoveryService.DiscoverModels(request.DomainPath);
            if (discoveryResult.Errors.Count > 0)
                throw new InvalidOperationException($"Failed to discover models: {string.Join(", ", discoveryResult.Errors)}");

            // 3. Read SDK structure to understand available components
            var sdkFiles = Directory.GetFiles(request.SdkPath, "*.cs", SearchOption.AllDirectories);
            var sdkStructure = string.Join("\n", sdkFiles.Select(f => $"- {Path.GetRelativePath(request.SdkPath, f)}"));

            // 4. Read model definitions
            var modelFiles = Directory.GetFiles(request.DomainPath, "*.yaml", SearchOption.AllDirectories);
            var modelDefinitions = new List<string>();

            foreach (var file in modelFiles)
            {
                var content = await File.ReadAllTextAsync(file);
                var fileName = Path.GetFileName(file);
                modelDefinitions.Add($"## {fileName}\n```yaml\n{content}\n```");
            }

            // 5. Determine SDK project name and namespace from SDK structure
            var sdkProjectFile = Directory.GetFiles(request.SdkPath, "*.csproj", SearchOption.TopDirectoryOnly).FirstOrDefault();
            var sdkProjectName = sdkProjectFile != null ? Path.GetFileNameWithoutExtension(sdkProjectFile) : "Unknown.Sdk";
            var sdkNamespace = sdkProjectName; // Assume namespace matches project name

            // 6. Extract entity information from discovered models
            var entityName = ExtractEntityName(discoveryResult);
            var entityNamePlural = ExtractEntityNamePlural(entityName);
            var featureName = ExtractFeatureName(request.ProjectName);

            // 7. Replace placeholders in the prompt template
            var finalPrompt = promptTemplate
                .Replace("{ProjectName}", request.ProjectName)
                .Replace("{Namespace}", request.Namespace)
                .Replace("{SdkPath}", request.SdkPath)
                .Replace("{OutputPath}", request.OutputPath)
                .Replace("{SdkProjectName}", sdkProjectName)
                .Replace("{SdkNamespace}", sdkNamespace)
                .Replace("{SdkStructure}", sdkStructure)
                .Replace("{ModelDefinitions}", string.Join("\n\n", modelDefinitions))
                .Replace("{FeatureName}", featureName)
                .Replace("{Entity}", entityName)
                .Replace("{Entities}", entityNamePlural)
                .Replace("{entities}", entityNamePlural.ToLowerInvariant());

            return finalPrompt;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating API prompt for project '{ProjectName}'", request.ProjectName);
            throw;
        }
    }

    /// <summary>
    /// Extract feature name from project name (e.g., "Business.CustomerManagement.Api" -> "CustomerManagement")
    /// </summary>
    private static string ExtractFeatureName(string projectName)
    {
        var parts = projectName.Split('.');
        return parts.Length >= 2 ? parts[^2] : parts[0]; // Take second-to-last part, or first if only one part
    }

    /// <summary>
    /// Extract primary entity name from discovered models
    /// </summary>
    private static string ExtractEntityName(ModelDiscoveryResult discoveryResult)
    {
        // Look for BDD model files that might represent entity types
        var bddFiles = discoveryResult.ModelDirectories
            .SelectMany(d => d.ModelGroups)
            .SelectMany(g => g.Files)
            .Where(f => f.Type == ModelFileType.BddModel && f.Name.Contains(".Type."))
            .ToList();

        if (bddFiles.Count != 0)
        {
            // Extract entity name from filename like "Customer.Type.yaml"
            var firstBddFile = bddFiles.First();
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(firstBddFile.Name);
            var entityName = nameWithoutExtension.Split('.').FirstOrDefault();
            return entityName ?? "Entity";
        }

        // Fallback: look for any BDD model file
        var anyBddFile = discoveryResult.ModelDirectories
            .SelectMany(d => d.ModelGroups)
            .SelectMany(g => g.Files)
            .FirstOrDefault(f => f.Type == ModelFileType.BddModel);

        if (anyBddFile != null)
        {
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(anyBddFile.Name);
            var entityName = nameWithoutExtension.Split('.').FirstOrDefault();
            return entityName ?? "Entity";
        }

        return "Entity"; // Ultimate fallback
    }

    /// <summary>
    /// Extract plural entity name from discovered models
    /// </summary>
    private static string ExtractEntityNamePlural(string entityName)
    {
        // Simple pluralization - add 's' or 'ies' for words ending in 'y'
        return entityName.EndsWith("y") 
            ? entityName[..^1] + "ies" 
            : entityName + "s";
    }
}
