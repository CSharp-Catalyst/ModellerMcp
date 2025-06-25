using ModelContextProtocol.Server;
using Modeller.McpServer.McpValidatorServer.Services;
using System.ComponentModel;
using static Modeller.McpServer.McpValidatorServer.Services.ModelPromptService;

namespace Modeller.McpServer.McpValidatorServer;

/// <summary>
/// Provides Modeller-specific prompts for the MCP server.
/// These prompts help users generate appropriate prompts for analyzing, reviewing, and working with Modeller definitions.
/// </summary>
public class ModellerPrompts(ModelPromptService promptService)
{
    private readonly ModelPromptService _promptService = promptService;

    /// <summary>
    /// Gets all available prompts for the Modeller MCP server
    /// </summary>
    public List<PromptDefinition> GetAvailablePrompts()
    {
        return _promptService.GetAvailablePrompts();
    }

    /// <summary>
    /// Gets the content for a specific prompt with the provided arguments
    /// </summary>
    public async Task<PromptResponse> GetPromptContent(string promptName, Dictionary<string, object> arguments)
    {
        return await _promptService.GetPromptContent(promptName, arguments);
    }
}
