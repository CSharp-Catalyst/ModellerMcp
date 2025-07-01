using Modeller.Mcp.Shared.Models;
using Modeller.Mcp.Shared.Resources;
using Modeller.Mcp.Shared.Services;

namespace Modeller.McpServer.Tests.Unit;

public class ValidationToResourceIntegrationTests
{
    [Fact]
    public async Task ValidateModel_RegistersValidatedModelAsResource()
    {
        // Arrange
        var service = new ModelDiscoveryService();
        var structureValidator = new ModelStructureValidator();
        var modelResources = new ModelDefinitionResources(service);
        var validator = new YamlSchemaValidator(structureValidator, modelResources);

        // Create a simple test YAML model file in memory
        var tempPath = Path.GetTempPath();
        var testDir = Path.Combine(tempPath, "TestModels");
        Directory.CreateDirectory(testDir);

        var modelFilePath = Path.Combine(testDir, "TestModel.Type.yaml");
        var yamlContent = @"
model: TestModel
summary: A test model for validation
attributeUsages:
  - name: Name
    type: string
    required: true
    summary: The name attribute
";
        await File.WriteAllTextAsync(modelFilePath, yamlContent, TestContext.Current.CancellationToken);

        try
        {
            // Act - Validate the model
            var validationResult = await validator.ValidateAsync(modelFilePath, TestContext.Current.CancellationToken);

            // Assert - Check that validation succeeded
            Assert.NotNull(validationResult);
            Assert.Contains(validationResult.Results, r => r.Severity == ValidationSeverity.Info && r.Message.Contains("registered as MCP resource"));

            // Assert - Check that the model is now available as a resource
            // The domain path should be extracted from the temp directory path
            var resourceResult = modelResources.GetAllValidatedModels();
            Assert.Contains("TestModel", resourceResult);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(testDir))
            {
                Directory.Delete(testDir, true);
            }
        }
    }
}
