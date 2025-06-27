using ModelContextProtocol.Server;

using Modeller.Mcp.Shared.CodeGeneration;
using Modeller.Mcp.Shared.CodeGeneration.Prompts;
using Modeller.Mcp.Shared.CodeGeneration.Prompts.VSA;

using System.ComponentModel;

namespace Modeller.Mcp.Shared;

/// <summary>
/// Provides prompts for discovering and validating model definitions within a specified folder.
/// </summary>
/// <remarks>This class contains static methods that generate descriptive prompts for operations related to model
/// definitions, such as discovering and validating models in a given folder. These prompts can be used in server-side
/// operations or user-facing messages.</remarks>
[McpServerPromptType]
public class ModellerPrompts(IVsaPromptService vsaPromptService)
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
    /// </summary>
    /// <param name="folder">The absolute path to the parent folder containing the models to validate. This parameter cannot be null or
    /// empty.</param>
    /// <returns>A string containing the validation prompt for the specified folder.</returns>
    [McpServerPrompt, Description("Validate model definitions.")]
    public static string ValidateModelsPrompt([Description("Absolute path to the parent folder")] string folder) => $"Validate the definition structure and all the models within '{folder}'";

    /// <summary>
    /// Generates a prompt for creating an SDK from model definitions in a specified folder.
    /// This method is designed to be used in a server-side context where the folder path is provided
    /// </summary>
    /// <param name="folder"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    [McpServerPrompt, Description("Generate SDK project from model definitions")]
    public async Task<string> GenerateSDKPrompt([Description("Absolute path to the parent folder")] string folder, CancellationToken cancellationToken = default)
    {
        return await vsaPromptService.GenerateSDKFromDomainModelAsync(folder);
    }
}
