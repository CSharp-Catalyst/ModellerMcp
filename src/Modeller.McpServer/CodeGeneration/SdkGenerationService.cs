using Modeller.Mcp.Shared.Services;
using Modeller.McpServer.CodeGeneration.Prompts;
using Modeller.McpServer.CodeGeneration.Security;

namespace Modeller.McpServer.CodeGeneration;

/// <summary>
/// Service for generating SDK code from domain models
/// </summary>
public interface ISdkGenerationService
{
    /// <summary>
    /// Generate a complete SDK from domain models in a specified path
    /// </summary>
    Task<SdkGenerationResult> GenerateSDKAsync(SdkGenerationRequest request);
    
    /// <summary>
    /// Generate VSA prompt for SDK generation
    /// </summary>
    Task<string> GeneratePromptAsync(SdkGenerationRequest request);
}

/// <summary>
/// Request for SDK generation
/// </summary>
public record SdkGenerationRequest
{
    /// <summary>
    /// Path to the domain models (e.g., models/JJs/PotentialSales)
    /// </summary>
    public required string DomainPath { get; init; }
    
    /// <summary>
    /// Name of the feature (e.g., "Prospects")
    /// </summary>
    public required string FeatureName { get; init; }
    
    /// <summary>
    /// Target namespace for the SDK (e.g., "JJs.PotentialSales.Sdk")
    /// </summary>
    public required string Namespace { get; init; }
    
    /// <summary>
    /// Output directory for generated files
    /// </summary>
    public required string OutputPath { get; init; }
}

/// <summary>
/// Result of SDK generation
/// </summary>
public record SdkGenerationResult
{
    /// <summary>
    /// Whether the generation was successful
    /// </summary>
    public bool IsSuccess { get; init; }
    
    /// <summary>
    /// Generated prompt used for SDK creation
    /// </summary>
    public string? GeneratedPrompt { get; init; }
    
    /// <summary>
    /// Path where files were generated
    /// </summary>
    public string? OutputPath { get; init; }
    
    /// <summary>
    /// List of generated files
    /// </summary>
    public List<string> GeneratedFiles { get; init; } = new();
    
    /// <summary>
    /// Error message if generation failed
    /// </summary>
    public string? ErrorMessage { get; init; }
    
    /// <summary>
    /// Validation results from the domain models
    /// </summary>
    public string? ValidationResults { get; init; }
    
    public static SdkGenerationResult Success(string prompt, string outputPath, List<string> files) => new()
    {
        IsSuccess = true,
        GeneratedPrompt = prompt,
        OutputPath = outputPath,
        GeneratedFiles = files
    };
    
    public static SdkGenerationResult Failure(string errorMessage) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage
    };
}

/// <summary>
/// Implementation of SDK generation service
/// </summary>
public class SdkGenerationService : ISdkGenerationService
{
    private readonly IVsaPromptService _vsaPromptService;
    private readonly ModelDiscoveryService _modelDiscoveryService;
    private readonly ISecureLlmService _secureLlmService;
    private readonly ILogger<SdkGenerationService> _logger;

    public SdkGenerationService(
        IVsaPromptService vsaPromptService,
        ModelDiscoveryService modelDiscoveryService,
        ISecureLlmService secureLlmService,
        ILogger<SdkGenerationService> logger)
    {
        _vsaPromptService = vsaPromptService;
        _modelDiscoveryService = modelDiscoveryService;
        _secureLlmService = secureLlmService;
        _logger = logger;
    }

    public async Task<SdkGenerationResult> GenerateSDKAsync(SdkGenerationRequest request)
    {
        try
        {
            _logger.LogInformation("Starting SDK generation for feature '{FeatureName}' from '{DomainPath}'", 
                request.FeatureName, request.DomainPath);

            // 1. Validate and discover domain models
            var discoveryResult = _modelDiscoveryService.DiscoverModels(request.DomainPath);
            if (discoveryResult.Errors.Any())
            {
                return SdkGenerationResult.Failure($"Failed to discover models: {string.Join("; ", discoveryResult.Errors)}");
            }

            // 2. Find the specific model files for the feature
            var typeFile = discoveryResult.ModelDirectories
                .SelectMany(d => d.ModelGroups)
                .SelectMany(g => g.Files)
                .FirstOrDefault(f => f.Name.Contains($"{request.FeatureName.TrimEnd('s')}.Type.yaml", StringComparison.OrdinalIgnoreCase));
            var behaviourFile = discoveryResult.ModelDirectories
                .SelectMany(d => d.ModelGroups)
                .SelectMany(g => g.Files)
                .FirstOrDefault(f => f.Name.Contains($"{request.FeatureName.TrimEnd('s')}.Behaviour.yaml", StringComparison.OrdinalIgnoreCase));

            if (typeFile == null)
            {
                return SdkGenerationResult.Failure($"Could not find Type definition for feature '{request.FeatureName}'");
            }

            // 3. Load the domain model content
            var typeContent = await File.ReadAllTextAsync(typeFile.Path);
            var behaviourContent = behaviourFile != null ? await File.ReadAllTextAsync(behaviourFile.Path) : "";
            
            var combinedYaml = string.IsNullOrEmpty(behaviourContent) 
                ? typeContent 
                : $"{typeContent}\n\n---\n\n{behaviourContent}";

            // 4. Generate the VSA prompt
            var prompt = await _vsaPromptService.GenerateSDKFromDomainModelAsync(
                combinedYaml, 
                request.FeatureName, 
                request.Namespace);

            // 5. Use secure LLM service to generate the actual code
            var secureRequest = new SecureLlmRequest
            {
                RawPrompt = prompt,
                ModelId = "gpt-4",
                PromptType = "sdk_generation",
                SecurityContext = new SecurityContext
                {
                    UserId = "system",
                    SessionId = Guid.CreateVersion7().ToString(),
                    IPAddress = "127.0.0.1",
                    UserAgent = "ModellerMcp/1.0"
                },
                PromptInputs = new Dictionary<string, string>
                {
                    ["feature"] = request.FeatureName,
                    ["namespace"] = request.Namespace,
                    ["domain_path"] = request.DomainPath
                }
            };

            var codeGenerationResult = await _secureLlmService.GenerateSecureCodeAsync(secureRequest);

            if (!codeGenerationResult.IsSuccess)
            {
                return SdkGenerationResult.Failure($"Code generation failed: {codeGenerationResult.ErrorMessage}");
            }

            // 6. Save generated files to output directory
            Directory.CreateDirectory(request.OutputPath);
            var generatedFiles = new List<string>();
            
            // For now, save the prompt and generated code
            var promptPath = Path.Combine(request.OutputPath, "GeneratedPrompt.md");
            await File.WriteAllTextAsync(promptPath, prompt);
            generatedFiles.Add(promptPath);
            
            var codePath = Path.Combine(request.OutputPath, "GeneratedCode.md");
            await File.WriteAllTextAsync(codePath, codeGenerationResult.Content ?? "");
            generatedFiles.Add(codePath);

            _logger.LogInformation("SDK generation completed successfully. Generated {Count} files", generatedFiles.Count);

            return SdkGenerationResult.Success(prompt, request.OutputPath, generatedFiles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during SDK generation for feature '{FeatureName}'", request.FeatureName);
            return SdkGenerationResult.Failure($"SDK generation failed: {ex.Message}");
        }
    }

    public async Task<string> GeneratePromptAsync(SdkGenerationRequest request)
    {
        try
        {
            var discoveryResult = _modelDiscoveryService.DiscoverModels(request.DomainPath);
            
            if (discoveryResult.Errors.Any())
            {
                throw new InvalidOperationException($"Failed to discover models: {string.Join(", ", discoveryResult.Errors)}");
            }

            // Get all YAML files from the discovery result
            var allFiles = new List<ModelFileInfo>();
            allFiles.AddRange(discoveryResult.LooseFiles);
            allFiles.AddRange(discoveryResult.ModelDirectories
                .SelectMany(d => d.ModelGroups.SelectMany(g => g.Files)));

            var typeFile = allFiles.FirstOrDefault(f => 
                f.Path.Contains($"{request.FeatureName.TrimEnd('s')}.Type.yaml", StringComparison.OrdinalIgnoreCase));
            var behaviourFile = allFiles.FirstOrDefault(f => 
                f.Path.Contains($"{request.FeatureName.TrimEnd('s')}.Behaviour.yaml", StringComparison.OrdinalIgnoreCase));

            if (typeFile == null)
            {
                throw new InvalidOperationException($"Could not find Type definition for feature '{request.FeatureName}'");
            }

            var typeContent = await File.ReadAllTextAsync(typeFile.Path);
            var behaviourContent = behaviourFile != null ? await File.ReadAllTextAsync(behaviourFile.Path) : "";
            
            var combinedYaml = string.IsNullOrEmpty(behaviourContent) 
                ? typeContent 
                : $"{typeContent}\n\n---\n\n{behaviourContent}";

            return await _vsaPromptService.GenerateSDKFromDomainModelAsync(
                combinedYaml, 
                request.FeatureName, 
                request.Namespace);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating prompt for feature '{FeatureName}'", request.FeatureName);
            throw;
        }
    }
}
