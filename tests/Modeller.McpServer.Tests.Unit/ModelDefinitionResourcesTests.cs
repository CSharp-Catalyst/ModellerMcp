using Modeller.Mcp.Shared;
using Modeller.Mcp.Shared.Models;
using Modeller.Mcp.Shared.Resources;
using Modeller.Mcp.Shared.Services;
using System.Text.Json;

namespace Modeller.McpServer.Tests.Unit;

public class ModelDefinitionResourcesTests
{
    [Fact]
    public void RegisterValidatedModel_StoresModelCorrectly()
    {
        // Arrange
        var service = new ModelDiscoveryService();
        var resources = new ModelDefinitionResources(service);
        var modelDefinition = new ModelDefinition
        {
            Model = "TestModel",
            Summary = "A test model",
            AttributeUsages = new List<AttributeUsage>
            {
                new() { Name = "TestAttribute", Type = "string", Required = true, Summary = "Test attribute" }
            }
        };

        // Act
        resources.RegisterValidatedModel("TestModel", "TestDomain", modelDefinition);

        // Assert
        var result = resources.GetModelDefinition("TestDomain", "TestModel");
        Assert.Contains("TestModel", result);
        Assert.Contains("TestDomain", result);
        Assert.Contains("Validated", result);
    }

    [Fact]
    public void GetDomainModels_ReturnsValidatedModels()
    {
        // Arrange
        var service = new ModelDiscoveryService();
        var resources = new ModelDefinitionResources(service);
        var modelDefinition = new ModelDefinition
        {
            Model = "TestModel",
            Summary = "A test model"
        };

        resources.RegisterValidatedModel("TestModel", "TestDomain", modelDefinition);

        // Act
        var result = resources.GetDomainModels("TestDomain");

        // Assert
        Assert.Contains("TestModel", result);
        Assert.Contains("TestDomain", result);
        Assert.Contains("ModelCount", result);
    }

    [Fact]
    public void GetModelDefinition_ReturnsErrorForNonExistentModel()
    {
        // Arrange
        var service = new ModelDiscoveryService();
        var resources = new ModelDefinitionResources(service);

        // Act
        var result = resources.GetModelDefinition("NonExistentDomain", "NonExistentModel");

        // Assert
        Assert.Contains("Error", result);
        Assert.Contains("not found", result);
    }

    [Fact]
    public void GetAllValidatedModels_ReturnsAllModels()
    {
        // Arrange
        var service = new ModelDiscoveryService();
        var resources = new ModelDefinitionResources(service);
        
        var model1 = new ModelDefinition { Model = "Model1", Summary = "First model" };
        var model2 = new ModelDefinition { Model = "Model2", Summary = "Second model" };

        resources.RegisterValidatedModel("Model1", "Domain1", model1);
        resources.RegisterValidatedModel("Model2", "Domain2", model2);

        // Act
        var result = resources.GetAllValidatedModels();

        // Assert
        Assert.Contains("TotalValidatedModels", result);
        Assert.Contains("Model1", result);
        Assert.Contains("Model2", result);
        Assert.Contains("Domain1", result);
        Assert.Contains("Domain2", result);
    }
}
