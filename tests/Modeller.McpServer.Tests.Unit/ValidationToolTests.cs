using Modeller.Mcp.Shared;
using Modeller.Mcp.Shared.Services;
using Modeller.McpServer.McpValidatorServer;
using Modeller.McpServer.McpValidatorServer.Services;

namespace Modeller.McpServer.Tests.Unit;

public class ValidationToolTests
{
    [Fact]
    public async Task DiscoverModels_ReturnsSuccess()
    {
        // Arrange
        var service = new ModelDiscoveryService();
        var structureValidator = new ModelStructureValidator();
        var validator = new YamlSchemaValidator(structureValidator);
        var tool = new ValidationTool(service, validator);
        var path = Helper.GetSolutionFolder();

        // Act
        var result = await tool.DiscoverModels(Helper.GetSolutionFolder()!, CancellationToken.None);

        // Assert
        await Verify(result);
    }

    [Fact]
    public async Task ValidateModels_ReturnsSuccess()
    {
        // Arrange
        var service = new ModelDiscoveryService();
        var structureValidator = new ModelStructureValidator();
        var validator = new YamlSchemaValidator(structureValidator);
        var tool = new ValidationTool(service, validator);
        var path = Helper.GetSolutionFolder();

        // Act
        var result = await tool.ValidateModel(Helper.GetSolutionFolder()!, CancellationToken.None);

        // Assert
        await Verify(result);
    }
}