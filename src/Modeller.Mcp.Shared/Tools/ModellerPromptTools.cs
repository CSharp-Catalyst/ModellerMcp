using ModelContextProtocol.Server;

using Modeller.Mcp.Shared.Services;

using System.ComponentModel;

namespace Modeller.Mcp.Shared;

[McpServerToolType]
public class ModellerPromptTools(ModelPromptService promptService)
{
    [McpServerTool(Title = "Analyze Model Definition"), Description("Generate a prompt for analyzing a specific Modeller model definition for potential issues and best practices")]
    public async Task<string> AnalyzeModelPrompt(
        [Description("Path to the model file to analyze")] string modelPath,
        [Description("Type of analysis: 'structure', 'validation', 'best-practices', or 'all'")] string analysisType = "all")
    {
        var arguments = new Dictionary<string, object>
        {
            ["modelPath"] = modelPath,
            ["analysisType"] = analysisType
        };

        var response = await promptService.GetPromptContent("analyze_model", arguments);
        var promptText = string.Join("\n\n", response.Messages.Select(m => ((TextContent)m.Content).Text));

        return $"📝 **Model Analysis Prompt Generated**\n\n" +
               $"**Model Path:** {modelPath}\n" +
               $"**Analysis Type:** {analysisType}\n\n" +
               $"**Prompt Content:**\n\n{promptText}\n\n" +
               $"💡 **Usage:** Copy this prompt and use it with your preferred LLM to get detailed analysis of your model definition.";
    }

    [McpServerTool(Title = "Review Domain Models"), Description("Generate a prompt for reviewing all models within a specific domain for consistency and compliance")]
    public async Task<string> ReviewDomainPrompt(
        [Description("Path to the domain folder (e.g., models/ProjectName/Organisation)")] string domainPath,
        [Description("Whether to include shared attribute types and enums in the review")] bool includeShared = false)
    {
        var arguments = new Dictionary<string, object>
        {
            ["domainPath"] = domainPath,
            ["includeShared"] = includeShared.ToString()
        };

        var response = await promptService.GetPromptContent("review_domain", arguments);
        var promptText = string.Join("\n\n", response.Messages.Select(m => ((TextContent)m.Content).Text));

        return $"📝 **Domain Review Prompt Generated**\n\n" +
               $"**Domain Path:** {domainPath}\n" +
               $"**Include Shared:** {includeShared}\n\n" +
               $"**Prompt Content:**\n\n{promptText}\n\n" +
               $"💡 **Usage:** Use this prompt to get a comprehensive review of all models in your domain.";
    }

    [McpServerTool(Title = "Create Model Template"), Description("Generate a prompt for creating a template for a new Modeller model based on requirements")]
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

        var response = await promptService.GetPromptContent("create_model_template", arguments);
        var promptText = string.Join("\n\n", response.Messages.Select(m => ((TextContent)m.Content).Text));

        return $"📝 **Model Template Creation Prompt Generated**\n\n" +
               $"**Model Type:** {modelType}\n" +
               $"**Domain:** {domain}\n" +
               $"**Description:** {description}\n\n" +
               $"**Prompt Content:**\n\n{promptText}\n\n" +
               $"💡 **Usage:** Use this prompt to generate a properly structured {modelType} model template.";
    }

    [McpServerTool(Title = "Validate Project Structure"), Description("Generate a prompt for validating the folder structure and naming conventions for a Modeller project")]
    public async Task<string> ValidateStructurePrompt(
        [Description("Path to the project root directory")] string projectPath,
        [Description("Whether to apply strict validation rules")] bool strictMode = false)
    {
        var arguments = new Dictionary<string, object>
        {
            ["projectPath"] = projectPath,
            ["strictMode"] = strictMode.ToString()
        };

        var response = await promptService.GetPromptContent("validate_structure", arguments);
        var promptText = string.Join("\n\n", response.Messages.Select(m => ((TextContent)m.Content).Text));

        return $"📝 **Structure Validation Prompt Generated**\n\n" +
               $"**Project Path:** {projectPath}\n" +
               $"**Strict Mode:** {strictMode}\n\n" +
               $"**Prompt Content:**\n\n{promptText}\n\n" +
               $"💡 **Usage:** Use this prompt to validate your project's folder structure and naming conventions.";
    }

    [McpServerTool(Title = "Generate Migration Guide"), Description("Generate a prompt for creating guidance on migrating models between versions or updating structure")]
    public async Task<string> MigrationGuidePrompt(
        [Description("Current version or structure to migrate from")] string fromVersion,
        [Description("Target version or structure to migrate to")] string toVersion,
        [Description("Specific model or domain path to focus on")] string modelPath = "")
    {
        var arguments = new Dictionary<string, object>
        {
            ["fromVersion"] = fromVersion,
            ["toVersion"] = toVersion,
            ["modelPath"] = modelPath
        };

        var response = await promptService.GetPromptContent("migration_guide", arguments);
        var promptText = string.Join("\n\n", response.Messages.Select(m => ((TextContent)m.Content).Text));

        return $"📝 **Migration Guide Prompt Generated**\n\n" +
               $"**From Version:** {fromVersion}\n" +
               $"**To Version:** {toVersion}\n" +
               $"**Focus Path:** {(string.IsNullOrEmpty(modelPath) ? "All models" : modelPath)}\n\n" +
               $"**Prompt Content:**\n\n{promptText}\n\n" +
               $"💡 **Usage:** Use this prompt to generate detailed migration guidance for your models.";
    }

    [McpServerTool(Title = "List Available Prompts"), Description("List all available prompt templates with their descriptions and required parameters")]
    public string ListAvailablePrompts()
    {
        var prompts = promptService.GetAvailablePrompts();

        var result = "📋 **Available Modeller Prompt Templates**\n\n";

        foreach (var prompt in prompts)
        {
            result += $"### 🎯 {prompt.Title}\n";
            result += $"**Name:** `{prompt.Name}`\n";
            result += $"**Description:** {prompt.Description}\n\n";

            if (prompt.Arguments.Any())
            {
                result += "**Parameters:**\n";
                foreach (var arg in prompt.Arguments)
                {
                    var required = arg.Required ? " *(required)*" : " *(optional)*";
                    result += $"- **{arg.Name}**{required}: {arg.Description}\n";
                }
            }

            result += "\n---\n\n";
        }

        result += "💡 **How to use:** Call the specific prompt tool (e.g., `AnalyzeModelPrompt`) with the required parameters to generate the prompt content.\n\n";
        result += "🚀 **Workflow:** Generate prompt → Copy content → Use with your preferred LLM → Get expert guidance!";

        return result;
    }
}
