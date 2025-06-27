using System.ComponentModel;

using Microsoft.Extensions.Logging;

using ModelContextProtocol.Server;

using Modeller.Mcp.Shared.CodeGeneration;

namespace Modeller.Mcp.Shared.Tools;

/// <summary>
/// MCP tools for SDK generation using VSA patterns
/// </summary>
[McpServerToolType]
public class SdkGenerationTools(
    ISdkGenerationService sdkGenerationService,
    ILogger<SdkGenerationTools> logger)
{
    [McpServerTool(Title = "Generate SDK from Domain Model")]
    [Description("Generate a complete SDK using VSA patterns from domain models for a specific feature.")]
    public async Task<string> GenerateSDK(
        [Description("Path to the domain models (e.g., models/Business/CustomerManagement)")]
        string domainPath,
        [Description("Name of the feature (e.g., 'Customers')")]
        string featureName,
        [Description("Target namespace for the SDK (e.g., 'Business.CustomerManagement.Sdk')")]
        string namespaceName,
        [Description("Output directory for generated files")]
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Generating SDK for feature '{FeatureName}' from '{DomainPath}'", featureName, domainPath);

            var request = new SdkGenerationRequest
            {
                DomainPath = domainPath,
                FeatureName = featureName,
                Namespace = namespaceName,
                OutputPath = outputPath
            };

            var result = await sdkGenerationService.GenerateSDKAsync(request);

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
            logger.LogError(ex, "Error generating SDK for feature '{FeatureName}'", featureName);
            return $"""
            ❌ **SDK Generation Error**
            
            **Exception:** {ex.Message}
            **Feature:** {featureName}
            
            Please check the logs for more details.
            """;
        }
    }
}