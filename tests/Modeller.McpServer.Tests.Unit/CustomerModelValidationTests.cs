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
        // Assert
        Assert.True(File.Exists(@"c:\jjs\set\dev\ModellerMcp\models\Business\CustomerManagement\Customer.Type.yaml"));
        Assert.True(File.Exists(@"c:\jjs\set\dev\ModellerMcp\models\Business\CustomerManagement\Order.Type.yaml"));
        Assert.True(File.Exists(@"c:\jjs\set\dev\ModellerMcp\models\Business\CustomerManagement\CustomerContact.Type.yaml"));
    }

    [Fact]
    public async Task CustomerModels_ShouldContainValidContent()
    {
        // Arrange
        var customerContent = await File.ReadAllTextAsync(@"c:\jjs\set\dev\ModellerMcp\models\Business\CustomerManagement\Customer.Type.yaml");
        var orderContent = await File.ReadAllTextAsync(@"c:\jjs\set\dev\ModellerMcp\models\Business\CustomerManagement\Order.Type.yaml");
        var contactContent = await File.ReadAllTextAsync(@"c:\jjs\set\dev\ModellerMcp\models\Business\CustomerManagement\CustomerContact.Type.yaml");

        // Assert
        Assert.Contains("model: Customer", customerContent);
        Assert.Contains("customerId", customerContent);
        Assert.Contains("customerNumber", customerContent);
        Assert.Contains("registerNewCustomer", customerContent);

        Assert.Contains("model: Order", orderContent);
        Assert.Contains("orderId", orderContent);
        Assert.Contains("orderNumber", orderContent);
        Assert.Contains("createOrder", orderContent);

        Assert.Contains("model: CustomerContact", contactContent);
        Assert.Contains("contactId", contactContent);
        Assert.Contains("recordCustomerContact", contactContent);
    }

    [Fact]
    public async Task VsaPromptService_ShouldGenerateValidPromptForCustomerModels()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Security:AuditLogPath"] = "c:\\temp\\audit"
            })
            .Build();
            
        services.AddLogging();
        services.AddSecurityServices(configuration);
        services.AddTransient<IVsaPromptService, VsaPromptService>();
        var serviceProvider = services.BuildServiceProvider();
        
        var vsaPromptService = serviceProvider.GetRequiredService<IVsaPromptService>();

        // Act
        var prompt = await vsaPromptService.GenerateSDKFromDomainModelAsync(@"c:\jjs\set\dev\ModellerMcp\models\Business\CustomerManagement");

        // Assert
        Assert.NotNull(prompt);
        Assert.NotEmpty(prompt);
        Assert.Contains("Business.CustomerManagement.Sdk", prompt);
        Assert.Contains("Customer", prompt);
        Assert.Contains("Order", prompt);
        Assert.Contains("CustomerContact", prompt);
    }
}
