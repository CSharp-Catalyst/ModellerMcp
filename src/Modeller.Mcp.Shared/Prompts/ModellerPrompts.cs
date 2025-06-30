using ModelContextProtocol.Server;

using Modeller.Mcp.Shared.CodeGeneration.Prompts.VSA;
using Modeller.Mcp.Shared.Services;
using Modeller.Mcp.Shared.Resources;

using System.ComponentModel;

namespace Modeller.Mcp.Shared;

/// <summary>
/// Provides prompts for discovering and validating model definitions within a specified folder.
/// </summary>
/// <remarks>This class contains static methods that generate descriptive prompts for operations related to model
/// definitions, such as discovering and validating models in a given folder. These prompts can be used in server-side
/// operations or user-facing messages.</remarks>
[McpServerPromptType]
public class ModellerPrompts(IVsaPromptService vsaPromptService, ModelPromptService modelPromptService, ModelDefinitionResources modelResources)
{
    /// <summary>
    /// Generates a prompt to locate model definitions within a specified folder.
    /// </summary>
    /// <param name="folder">The absolute path to the parent folder containing the models.</param>
    /// <returns>A string prompt indicating the folder where models should be located.</returns>
    [McpServerPrompt, Description("Discover model definitions.")]
    public static string DiscoverModelsPrompt([Description("Absolute path to the parent folder")] string folder) => $"Locate the models within the folder '{folder}'";

    /// <summary>
    /// Generates a prompt to validate the definition structure and all models within the specified folder.
    /// Validated models are automatically registered as MCP resources for use in subsequent operations.
    /// </summary>
    /// <param name="folder">The absolute path to the parent folder containing the models to validate. This parameter cannot be null or
    /// empty.</param>
    /// <returns>A string containing the validation prompt for the specified folder. Validated models will be available as resources.</returns>
    [McpServerPrompt, Description("Validate model definitions and register as resources.")]
    public string ValidateModelsPrompt([Description("Absolute path to the parent folder")] string folder)
    {
        // Note: The actual validation and resource registration happens in the validation tools
        // This prompt triggers the validation process which will populate the ModelDefinitionResources
        return $"Validate the definition structure and all the models within '{folder}'. " +
               $"Validated models will be automatically registered as MCP resources at 'modeller://models/domain/{folder.Replace('\\', '/')}' " +
               $"for use in subsequent operations like SDK generation.";
    }

    /// <summary>
    /// Generates a prompt for creating an SDK from validated model definitions in a specified folder.
    /// This method references models that have been previously validated and registered as MCP resources.
    /// </summary>
    /// <param name="folder">The absolute path to the domain folder containing validated models</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>SDK generation prompt that references validated model resources</returns>
    /// <exception cref="ArgumentException">Thrown when folder path is invalid</exception>
    [McpServerPrompt, Description("Generate SDK project from validated model definitions")]
    public async Task<string> GenerateSDKPrompt([Description("Absolute path to the domain folder")] string folder, CancellationToken cancellationToken = default)
    {
        // First, check if we have validated models in resources for this domain
        var domainPath = folder.Replace('\\', '/');
        
        try
        {
            // Get the validated models from resources
            var validatedModelsJson = modelResources.GetDomainModels(domainPath);
            
            // Generate the SDK prompt using validated models
            var sdkPrompt = await vsaPromptService.GenerateSDKFromDomainModelAsync(folder);
            
            return $"🔧 **SDK Generation from Validated Models**\n\n" +
                   $"**Domain:** {domainPath}\n" +
                   $"**Validated Models Resource:** `modeller://models/domain/{domainPath}`\n\n" +
                   $"**Available Models:**\n{validatedModelsJson}\n\n" +
                   $"**SDK Generation Prompt:**\n\n{sdkPrompt}\n\n" +
                   "  **CRITICAL**: read and understand the generated SDK prompt before proceeding.\n\n" +
                   $"💡 **Note:** This SDK generation uses previously validated model definitions available as MCP resources. " +
                   $"If no validated models are found, run the ValidateModelsPrompt first.";
        }
        catch (Exception ex)
        {
            return $"❌ **SDK Generation Error**\n\n" +
                   $"Could not generate SDK for domain '{domainPath}': {ex.Message}\n\n" +
                   $"💡 **Suggestion:** Ensure models in '{folder}' have been validated first using the ValidateModelsPrompt.";
        }
    }

    [McpServerPrompt, Description("Create a template for a new model definition based on requirements")]
    public async Task<string> CreateModelTemplatePrompt(
        [Description("Type of model: 'Type', 'Behaviour', 'AttributeType', or 'Enum'")] string modelType,
        [Description("Domain/area this model belongs to")] string domain,
        [Description("Brief description of what this model represents")] string description = "")
    {
        var arguments = new Dictionary<string, object>
        {
            ["modelType"] = modelType,
            ["domain"] = domain,
            ["description"] = description
        };

        var response = await modelPromptService.GetPromptContent("create_model_template", arguments);
        var promptText = string.Join("\n\n", response.Messages.Select(m => ((TextContent)m.Content).Text));

        return $"📝 **Model Template Creation Prompt Generated**\n\n" +
               $"**Model Type:** {modelType}\n" +
               $"**Domain:** {domain}\n" +
               $"**Description:** {description}\n\n" +
               $"**Prompt Content:**\n\n{promptText}\n\n" +
               $"💡 **Usage:** Use this prompt to generate a properly structured {modelType} model template.";
    }

}
