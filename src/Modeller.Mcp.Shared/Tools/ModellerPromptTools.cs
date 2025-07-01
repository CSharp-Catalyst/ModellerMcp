using ModelContextProtocol.Server;

using Modeller.Mcp.Shared.Services;

using System.ComponentModel;

namespace Modeller.Mcp.Shared;

[McpServerToolType]
public class ModellerPromptTools(ModelPromptService promptService)
{

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

            if (prompt.Arguments.Count != 0)
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
