using System.ComponentModel;
using Modeller.McpServer.CodeGeneration;
using ModelContextProtocol.Server;

namespace Modeller.McpServer.McpValidatorServer;

/// <summary>
/// MCP tools for SDK generation using VSA patterns
/// </summary>
[McpServerToolType]
public class SdkGenerationTool
{
    private readonly ISdkGenerationService _sdkGenerationService;
    private readonly ILogger<SdkGenerationTool> _logger;

    public SdkGenerationTool(
        ISdkGenerationService sdkGenerationService,
        ILogger<SdkGenerationTool> logger)
    {
        _sdkGenerationService = sdkGenerationService;
        _logger = logger;
    }

    [McpServerTool(Title = "Generate SDK from Domain Model")]
    [Description("Generate a complete SDK using VSA patterns from domain models for a specific feature.")]
    public async Task<string> GenerateSDK(
        [Description("Path to the domain models (e.g., models/JJs/PotentialSales)")]
        string domainPath,
        [Description("Name of the feature (e.g., 'Prospects')")]
        string featureName,
        [Description("Target namespace for the SDK (e.g., 'JJs.PotentialSales.Sdk')")]
        string namespaceName,
        [Description("Output directory for generated files")]
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating SDK for feature '{FeatureName}' from '{DomainPath}'", featureName, domainPath);
            
            var request = new SdkGenerationRequest
            {
                DomainPath = domainPath,
                FeatureName = featureName,
                Namespace = namespaceName,
                OutputPath = outputPath
            };

            var result = await _sdkGenerationService.GenerateSDKAsync(request);
            
            if (result.IsSuccess)
            {
                var response = $"""
                ✅ **SDK Generation Successful**
                
                **Feature:** {featureName}
                **Namespace:** {namespaceName}
                **Output Path:** {result.OutputPath}
                **Files Generated:** {result.GeneratedFiles.Count}
                
                **Generated Files:**
                {string.Join("\n", result.GeneratedFiles.Select(f => $"- {Path.GetFileName(f)}"))}
                
                **Architecture:** Vertical Slice Architecture (VSA)
                **Patterns Used:**
                - ✅ Feature folders ({featureName}/)
                - ✅ C# Records for immutable models
                - ✅ FluentValidation for validation
                - ✅ Extension methods for mapping
                - ✅ Result pattern for error handling
                
                The generated SDK follows modern C# best practices and is ready for production use.
                """;
                
                return response;
            }
            else
            {
                return $"""
                ❌ **SDK Generation Failed**
                
                **Error:** {result.ErrorMessage}
                **Feature:** {featureName}
                **Domain Path:** {domainPath}
                
                Please check the domain path and ensure the required model files exist.
                """;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating SDK for feature '{FeatureName}'", featureName);
            return $"""
            ❌ **SDK Generation Error**
            
            **Exception:** {ex.Message}
            **Feature:** {featureName}
            
            Please check the logs for more details.
            """;
        }
    }

    [McpServerTool(Title = "Generate SDK Prompt")]
    [Description("Generate a VSA prompt for SDK generation from domain models (without actually generating the SDK).")]
    public async Task<string> GenerateSDKPrompt(
        [Description("Path to the domain models (e.g., models/JJs/PotentialSales)")]
        string domainPath,
        [Description("Name of the feature (e.g., 'Prospects')")]
        string featureName,
        [Description("Target namespace for the SDK (e.g., 'JJs.PotentialSales.Sdk')")]
        string namespaceName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating SDK prompt for feature '{FeatureName}' from '{DomainPath}'", featureName, domainPath);
            
            var request = new SdkGenerationRequest
            {
                DomainPath = domainPath,
                FeatureName = featureName,
                Namespace = namespaceName,
                OutputPath = "" // Not needed for prompt generation
            };

            var prompt = await _sdkGenerationService.GeneratePromptAsync(request);
            
            return $"""
            ✅ **VSA SDK Prompt Generated Successfully**
            
            **Feature:** {featureName}
            **Target Namespace:** {namespaceName}
            **Prompt Length:** {prompt.Length:N0} characters
            
            ---
            
            {prompt}
            """;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating SDK prompt for feature '{FeatureName}'", featureName);
            return $"""
            ❌ **Prompt Generation Error**
            
            **Exception:** {ex.Message}
            **Feature:** {featureName}
            **Domain Path:** {domainPath}
            
            Please check that the domain path exists and contains the required model files.
            """;
        }
    }
}