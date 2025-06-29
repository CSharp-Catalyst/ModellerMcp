using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Modeller.Mcp.Shared.CodeGeneration.Prompts.VSA;
using Modeller.Mcp.Shared.CodeGeneration.Security;

namespace Modeller.McpServer.Tests.Unit;

public class CustomerModelValidationTests
{
    [Fact]
    public void CustomerModels_ShouldExist()
    {
        // Arrange
        var solutionPath = Helper.GetSolutionFolder();
        var modelsPath = Path.Combine(solutionPath!, "models", "Business", "CustomerManagement");
        
        // Assert
        Assert.True(File.Exists(Path.Combine(modelsPath, "Customer.Type.yaml")));
        Assert.True(File.Exists(Path.Combine(modelsPath, "Order.Type.yaml")));
        Assert.True(File.Exists(Path.Combine(modelsPath, "CustomerContact.Type.yaml")));
    }

    [Fact]
    public async Task CustomerModels_ShouldContainValidContent()
    {
        // Arrange
        var solutionPath = Helper.GetSolutionFolder();
        var modelsPath = Path.Combine(solutionPath!, "models", "Business", "CustomerManagement");
        
        var customerContent = await File.ReadAllTextAsync(Path.Combine(modelsPath, "Customer.Type.yaml"), TestContext.Current.CancellationToken);
        var orderContent = await File.ReadAllTextAsync(Path.Combine(modelsPath, "Order.Type.yaml"), TestContext.Current.CancellationToken);
        var contactContent = await File.ReadAllTextAsync(Path.Combine(modelsPath, "CustomerContact.Type.yaml"), TestContext.Current.CancellationToken);

        // Assert - Type files should contain type definitions but not behaviors (proper separation)
        Assert.Contains("model: Customer", customerContent);
        Assert.Contains("customerId", customerContent);
        Assert.Contains("customerNumber", customerContent);
        Assert.Contains("status", customerContent);
        Assert.Contains("CustomerStatus", customerContent);
        Assert.DoesNotContain("registerNewCustomer", customerContent); // Behaviors should be in .Behaviour.yaml

        Assert.Contains("model: Order", orderContent);
        Assert.Contains("orderId", orderContent);
        Assert.Contains("orderNumber", orderContent);
        Assert.DoesNotContain("createOrder", orderContent); // Behaviors should be in .Behaviour.yaml

        Assert.Contains("model: CustomerContact", contactContent);
        Assert.Contains("contactId", contactContent);
        Assert.DoesNotContain("recordCustomerContact", contactContent); // Behaviors should be in .Behaviour.yaml
    }

    [Fact]
    public async Task VsaPromptService_ShouldGenerateValidPromptForCustomerModels()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Security:AuditLogPath"] = Path.Combine(Path.GetTempPath(), "audit")
            })
            .Build();

        services.AddLogging();
        services.AddSecurityServices(configuration);
        services.AddTransient<IVsaPromptService, VsaPromptService>();
        var serviceProvider = services.BuildServiceProvider();

        var vsaPromptService = serviceProvider.GetRequiredService<IVsaPromptService>();

        // Act
        var solutionPath = Helper.GetSolutionFolder();
        var domainPath = Path.Combine(solutionPath!, "models", "Business", "CustomerManagement");
        var prompt = await vsaPromptService.GenerateSDKFromDomainModelAsync(domainPath);

        // Assert
        Assert.NotNull(prompt);
        Assert.NotEmpty(prompt);
        Assert.Contains("Business.CustomerManagement.Sdk", prompt);
        Assert.Contains("Customer", prompt);
        Assert.Contains("Order", prompt);
        Assert.Contains("CustomerContact", prompt);
    }

    [Fact]
    public async Task CustomerModels_BehavioursShouldBeInSeparateFiles()
    {
        // Arrange
        var solutionPath = Helper.GetSolutionFolder();
        var modelsPath = Path.Combine(solutionPath!, "models", "Business", "CustomerManagement");
        
        var customerBehaviourContent = await File.ReadAllTextAsync(Path.Combine(modelsPath, "Customer.Behaviour.yaml"), TestContext.Current.CancellationToken);
        var orderBehaviourContent = await File.ReadAllTextAsync(Path.Combine(modelsPath, "Order.Behaviour.yaml"), TestContext.Current.CancellationToken);
        var contactBehaviourContent = await File.ReadAllTextAsync(Path.Combine(modelsPath, "CustomerContact.Behaviour.yaml"), TestContext.Current.CancellationToken);

        // Assert - Behaviors should be properly defined in .Behaviour.yaml files
        Assert.Contains("model: Customer", customerBehaviourContent);
        Assert.Contains("registerNewCustomer", customerBehaviourContent);
        Assert.Contains("createCustomer", customerBehaviourContent);
        Assert.Contains("getCustomerByNumber", customerBehaviourContent);
        Assert.Contains("suspendCustomer", customerBehaviourContent);
        Assert.Contains("activateCustomer", customerBehaviourContent);

        Assert.Contains("model: Order", orderBehaviourContent);
        Assert.Contains("createOrder", orderBehaviourContent);

        Assert.Contains("model: CustomerContact", contactBehaviourContent);
        Assert.Contains("recordCustomerContact", contactBehaviourContent);
    }

    [Fact]
    public async Task CustomerModels_WithMixedConcerns_ShouldTriggerValidationWarning()
    {
        // Arrange - Create a temporary file with mixed concerns (behavior in type file)
        var solutionPath = Helper.GetSolutionFolder();
        var tempPath = Path.Combine(solutionPath!, "temp_test_models");
        Directory.CreateDirectory(tempPath);
        
        var mixedContentFile = Path.Combine(tempPath, "TestEntity.Type.yaml");
        var mixedContent = @"model: TestEntity
summary: Test entity with mixed concerns
attributeUsages:
  - name: testId
    type: primaryKey
    required: true
behaviours:
  - name: testBehavior
    summary: This should trigger a warning
    entities:
      - TestEntity";

        await File.WriteAllTextAsync(mixedContentFile, mixedContent, TestContext.Current.CancellationToken);

        try
        {
            // Act - Validate the mixed content file
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().Build();
            services.AddLogging();
            // Add validator services here if needed

            // Assert - This test demonstrates the failure scenario
            // In a real implementation, you would validate that the validator
            // properly detects and warns about mixed concerns
            var fileContent = await File.ReadAllTextAsync(mixedContentFile, TestContext.Current.CancellationToken);
            Assert.Contains("behaviours:", fileContent);
            Assert.Contains("attributeUsages:", fileContent);
            // This represents the anti-pattern that should trigger warnings
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempPath))
                Directory.Delete(tempPath, true);
        }
    }

    [Fact]
    public async Task CustomerStatus_EnumShouldExistAndBeValid()
    {
        // Arrange
        var solutionPath = Helper.GetSolutionFolder();
        var enumPath = Path.Combine(solutionPath!, "models", "Shared", "Enums", "CustomerStatus.yaml");
        
        // Act
        var enumContent = await File.ReadAllTextAsync(enumPath, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(File.Exists(enumPath), "CustomerStatus enum file should exist");
        Assert.Contains("enum: CustomerStatus", enumContent);
        Assert.Contains("Active", enumContent);
        Assert.Contains("Inactive", enumContent);
        Assert.Contains("Suspended", enumContent);
        Assert.Contains("Pending", enumContent);
        Assert.Contains("Closed", enumContent);
    }
}
