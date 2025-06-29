using System.ComponentModel;

using Microsoft.Extensions.Logging;

using ModelContextProtocol.Server;

using Modeller.Mcp.Shared.CodeGeneration;

namespace Modeller.Mcp.Shared.Tools;

/// <summary>
/// MCP tools for Minimal API generation using VSA patterns and SDK components
/// </summary>
[McpServerToolType]
public class ApiGenerationTools(
    IApiGenerationService apiGenerationService,
    ILogger<ApiGenerationTools> logger)
{
    [McpServerTool(Title = "Generate Minimal API from SDK")]
    [Description("Generate a complete Minimal API project that uses the generated SDK, following VSA patterns and domain models.")]
    public async Task<string> GenerateMinimalAPI(
        [Description("Path to the generated SDK (e.g., playschool/generated-sdk)")]
        string sdkPath,
        [Description("Path to the domain models (e.g., playschool/models/JJs/PotentialSales)")]
        string domainPath,
        [Description("Name of the API project (e.g., 'JJs.PotentialSales.Api')")]
        string projectName,
        [Description("Target namespace for the API (e.g., 'JJs.PotentialSales.Api')")]
        string namespaceName,
        [Description("Output directory for the API project")]
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Generating Minimal API project '{ProjectName}' from SDK '{SdkPath}'", projectName, sdkPath);

            var request = new ApiGenerationRequest
            {
                SdkPath = sdkPath,
                DomainPath = domainPath,
                ProjectName = projectName,
                Namespace = namespaceName,
                OutputPath = outputPath
            };

            var result = await apiGenerationService.GenerateAPIAsync(request);

            if (result.IsSuccess)
            {
                var response = $"""
                ✅ **Minimal API Generation Successful**
                
                **Project:** {projectName}
                **Output Path:** {result.OutputPath}
                **Generated Files:** {result.GeneratedFiles.Count}
                
                ### Generated Components:
                {string.Join("\n", result.GeneratedFiles.Select(f => $"- {Path.GetFileName(f)}"))}
                
                ### Next Steps:
                1. Navigate to the API project directory
                2. Run `dotnet restore` to install dependencies
                3. Run `dotnet build` to verify compilation
                4. Run `dotnet run` to start the API server
                5. Test endpoints using Swagger UI or HTTP client
                
                ### Features Included:
                - Entity Framework Core with In-Memory database
                - Dependency injection setup
                - Business service layer
                - Validation and error handling
                - Minimal API endpoints for all domain entities
                - Swagger/OpenAPI documentation
                - Health checks
                - Logging configuration
                
                **SDK Reference:** The API project references the generated SDK for models, validators, and business logic.
                """;

                return response;
            }
            else
            {
                var errorResponse = $"""
                ❌ **Minimal API Generation Failed**
                
                **Error:** {result.ErrorMessage}
                
                Please check the following:
                1. SDK path exists and contains valid SDK files
                2. Domain path contains valid model definitions
                3. Output directory is writable
                4. Project name is valid for .NET projects
                """;

                logger.LogError("Minimal API generation failed: {Error}", result.ErrorMessage);
                return errorResponse;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during Minimal API generation for project '{ProjectName}'", projectName);
            return $"❌ **Unexpected Error:** {ex.Message}";
        }
    }

    [McpServerTool(Title = "Generate API Prompt")]
    [Description("Generate the prompt that would be used to create a Minimal API project from the SDK and domain models.")]
    public async Task<string> GenerateAPIPrompt(
        [Description("Path to the generated SDK")]
        string sdkPath,
        [Description("Path to the domain models")]
        string domainPath,
        [Description("Name of the API project")]
        string projectName,
        [Description("Target namespace for the API")]
        string namespaceName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new ApiGenerationRequest
            {
                SdkPath = sdkPath,
                DomainPath = domainPath,
                ProjectName = projectName,
                Namespace = namespaceName,
                OutputPath = "temp" // Not used for prompt generation
            };

            var prompt = await apiGenerationService.GeneratePromptAsync(request);
            
            return $"""
            ## Generated Minimal API Creation Prompt
            
            ```
            {prompt}
            ```
            
            This prompt can be used with any LLM to generate a complete Minimal API project that integrates with the generated SDK.
            """;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating API prompt for project '{ProjectName}'", projectName);
            return $"❌ **Error generating prompt:** {ex.Message}";
        }
    }
}
