using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Modeller.Mcp.Shared.CodeGeneration.Prompts.VSA;
using Modeller.Mcp.Shared.CodeGeneration.Security;

namespace Modeller.McpServer.Tests.Unit;

public class CustomerSdkGenerationTests
{
    private readonly IServiceProvider _serviceProvider;

    public CustomerSdkGenerationTests()
    {
        var services = new ServiceCollection();
        
        // Create test configuration
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Security:AuditLogPath"] = "c:\\temp\\audit"
            })
            .Build();
            
        services.AddLogging(); // Add logging services
        services.AddSecurityServices(configuration);
        services.AddTransient<IVsaPromptService, VsaPromptService>();
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task GenerateCustomerSdkCode_ShouldReturnValidPrompt()
    {
        // Arrange
        var vsaPromptService = _serviceProvider.GetRequiredService<IVsaPromptService>();

        // Act
        var prompt = await vsaPromptService.GenerateSDKFromDomainModelAsync(@"c:\jjs\set\dev\ModellerMcp\models\Business\CustomerManagement");

        // Assert
        Assert.NotNull(prompt);
        Assert.Contains("Customer", prompt);
        Assert.Contains("FluentValidation", prompt);
        Assert.Contains("Business.CustomerManagement.Sdk", prompt);
        Assert.Contains("customerId", prompt);
        Assert.Contains("getCustomerByNumber", prompt);
        Assert.Contains("createCustomer", prompt);
        
        // Output the generated prompt for manual review
        Console.WriteLine("=== GENERATED PROMPT ===");
        Console.WriteLine(prompt);
        Console.WriteLine("=== END PROMPT ===");
    }
}
