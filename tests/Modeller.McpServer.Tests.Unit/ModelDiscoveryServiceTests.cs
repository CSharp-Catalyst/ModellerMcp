using Modeller.Mcp.Shared.Services;
using Modeller.McpServer.McpValidatorServer.Services;

namespace Modeller.McpServer.Tests.Unit;

public class ModelDiscoveryServiceTests
{
    [Fact]
    public void DiscoverModelsAsync_ReturnsModels_WhenModelsExist()
    {
        // Arrange
        var service = new ModelDiscoveryService();
        var path = Helper.GetSolutionFolder();
        Assert.NotNull(path);
        
        // Act
        var result = service.DiscoverModels(path!);

        // Assert
        Verify(result);
    }
}
