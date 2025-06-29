using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Modeller.Mcp.Shared.CodeGeneration.Security;
using Modeller.Mcp.Shared.CodeGeneration.Prompts;
using Modeller.Mcp.Shared.CodeGeneration.Prompts.VSA;

namespace Modeller.McpServer.Tests.Unit;

/// <summary>
/// Tests for VSA prompt generation functionality
/// </summary>
public class VsaPromptServiceTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    public VsaPromptServiceTests()
    {
        var services = new ServiceCollection();
        
        // Setup configuration
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Security:EnableFileAuditLogging"] = "true",
                ["Security:AuditLogPath"] = "test_audit_logs",
                ["Security:EnableStructuredLogging"] = "true",
                ["Security:MinimumLogLevel"] = "Low"
            })
            .Build();

        // Add logging
        services.AddLogging(builder => builder.AddConsole());
        
        // Add configuration
        services.AddSingleton<IConfiguration>(configuration);
        
        // Add security services (which includes VSA prompt services)
        services.AddSecurityServices(configuration);
        
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void VsaPromptService_Should_BeRegistered_InDependencyInjection()
    {
        // Act
        var vsaPromptService = _serviceProvider.GetService<IVsaPromptService>();
        
        // Assert
        Assert.NotNull(vsaPromptService);
        Assert.IsType<VsaPromptService>(vsaPromptService);
    }

    [Fact]
    public async Task GetAvailableTemplatesAsync_Should_Return_GenerateSDKFromDomainModel()
    {
        // Arrange
        var vsaPromptService = _serviceProvider.GetRequiredService<IVsaPromptService>();
        
        // Act
        var templates = await vsaPromptService.GetAvailableTemplatesAsync();
        
        // Assert
        Assert.NotNull(templates);
        Assert.Contains("GenerateSDKFromDomainModel", templates);
    }

    [Fact]
    public async Task GenerateSDKFromDomainModelAsync_Should_Create_Valid_Prompt()
    {
        // Arrange
        var vsaPromptService = _serviceProvider.GetRequiredService<IVsaPromptService>();
        var solutionPath = Helper.GetSolutionFolder();
        var domainPath = Path.Combine(solutionPath!, "models", "Business", "CustomerManagement");

        // Act
        var prompt = await vsaPromptService.GenerateSDKFromDomainModelAsync(domainPath);

        // Assert
        Assert.NotNull(prompt);
        Assert.NotEmpty(prompt);
        Assert.Contains("Business.CustomerManagement.Sdk", prompt);
        Assert.Contains("Customer", prompt);
        Assert.Contains("Task Overview", prompt);
        Assert.Contains("Project Configuration", prompt);
        Assert.Contains("Domain Model Definitions", prompt);
        Assert.Contains("Vertical Slice Architecture", prompt);
    }

    [Fact]
    public async Task GenerateSDKFromDomainModelAsync_Should_Include_All_Required_Sections()
    {
        // Arrange
        var vsaPromptService = _serviceProvider.GetRequiredService<IVsaPromptService>();
        var solutionPath = Helper.GetSolutionFolder();
        var domainPath = Path.Combine(solutionPath!, "models", "Business", "CustomerManagement");

        // Act
        var prompt = await vsaPromptService.GenerateSDKFromDomainModelAsync(domainPath);

        // Assert
        Assert.Contains("## Task Overview", prompt);
        Assert.Contains("## Project Configuration", prompt);
        Assert.Contains("Business.CustomerManagement.Sdk", prompt);
        Assert.Contains("CustomerManagement", prompt);
        Assert.Contains("## Domain Model Definitions", prompt);
        Assert.Contains("## VSA Template Instructions", prompt);
        Assert.Contains("feature-based vertical slices", prompt);
    }

    [Fact]
    public async Task GenerateCustomerSdkCode_ShouldReturnValidPrompt()
    {
        // Arrange
        var vsaPromptService = _serviceProvider.GetRequiredService<IVsaPromptService>();
        var solutionPath = Helper.GetSolutionFolder();
        var domainPath = Path.Combine(solutionPath!, "models", "Business", "CustomerManagement");

        // Act
        var prompt = await vsaPromptService.GenerateSDKFromDomainModelAsync(domainPath);

        // Assert
        Assert.NotNull(prompt);
        Assert.Contains("Customer", prompt);
        Assert.Contains("FluentValidation", prompt);
        Assert.Contains("Business.CustomerManagement.Sdk", prompt);
        Assert.Contains("customerId", prompt);
        Assert.Contains("getCustomerByNumber", prompt);
        Assert.Contains("createCustomer", prompt);
        
        // Output the generated prompt for manual review
        Console.WriteLine("=== GENERATED CUSTOMER SDK PROMPT ===");
        Console.WriteLine(prompt);
        Console.WriteLine("=== END PROMPT ===");
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}
