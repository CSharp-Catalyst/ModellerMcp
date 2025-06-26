using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Modeller.McpServer.CodeGeneration.Security;
using Modeller.McpServer.CodeGeneration.Prompts;

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
        
        var sampleYaml = @"
name: Prospect
summary: Represents a potential customer in the sales pipeline
attributes:
  name:
    type: string
    required: true
    maxLength: 100
    summary: The prospect's full name
  email:
    type: email
    required: true
    summary: Primary email address
  phone:
    type: string
    maxLength: 20
    summary: Contact phone number
  status:
    type: ProspectStatus
    required: true
    summary: Current status in the sales pipeline";

        // Act
        var prompt = await vsaPromptService.GenerateSDKFromDomainModelAsync(
            sampleYaml, 
            "Prospects", 
            "JJs.PotentialSales.Sdk");

        // Assert
        Assert.NotNull(prompt);
        Assert.NotEmpty(prompt);
        Assert.Contains("JJs.PotentialSales.Sdk", prompt);
        Assert.Contains("Prospects", prompt);
        Assert.Contains("Prospect", prompt);
        Assert.Contains("System Context", prompt);
        Assert.Contains("Generation Request", prompt);
        Assert.Contains("Domain Model YAML", prompt);
        Assert.Contains("Vertical Slice Architecture", prompt);
    }

    [Fact]
    public async Task GenerateSDKFromDomainModelAsync_Should_Include_All_Required_Sections()
    {
        // Arrange
        var vsaPromptService = _serviceProvider.GetRequiredService<IVsaPromptService>();
        
        var simpleYaml = @"
name: TestEntity
summary: A test entity
attributes:
  id:
    type: integer
    required: true";

        // Act
        var prompt = await vsaPromptService.GenerateSDKFromDomainModelAsync(
            simpleYaml, 
            "TestEntities", 
            "Test.Sdk");

        // Assert
        Assert.Contains("# System Context", prompt);
        Assert.Contains("# Generation Request", prompt);
        Assert.Contains("**Target Namespace**: Test.Sdk", prompt);
        Assert.Contains("**Feature Name**: TestEntities", prompt);
        Assert.Contains("# Domain Model YAML", prompt);
        Assert.Contains("# Instructions", prompt);
        Assert.Contains("Generate a complete SDK vertical slice", prompt);
    }

    [Fact]
    public async Task GenerateProspectSdkCode_ShouldReturnValidPrompt()
    {
        // Arrange
        var vsaPromptService = _serviceProvider.GetRequiredService<IVsaPromptService>();
        
        var domainYaml = """
            model: Prospect
            summary: A potential sale opportunity
            attributeUsages:
              - name: prospectId
                type: primaryKey
                required: true
                summary: The unique identifier for the prospect
              - name: potentialSaleNumber
                type: prospectNumber
                required: true
                summary: A unique identifier assigned to each potential sale
              - name: tradingName
                type: baseString
                required: true
                summary: The trading name of the customer
              - name: prospectStatus
                type: ProspectStatus
                required: true
                summary: The current state of the potential sale
              - name: interest
                type: Interest
                required: true
                summary: The interest of the customer in the potential sale
              - name: contactEmail
                type: emailAddress
                required: false
                summary: The email address of the contact person
            """;

        var behaviourYaml = """
            model: Prospect
            behaviours:
              - name: getProspectByNumber
                summary: Retrieve a prospect by its unique number
                entities:
                  - Prospect
                preconditions:
                  - Prospect.potentialSaleNumber is provided
                effects:
                  - return Prospect data
              - name: createProspect
                summary: Create a new potential sale opportunity
                entities:
                  - Prospect
                preconditions:
                  - required fields are provided
                  - potentialSaleNumber is unique
                effects:
                  - new Prospect is created
                  - prospectStatus is set to Open
            """;

        // Act
        var prompt = await vsaPromptService.GenerateSDKFromDomainModelAsync(
            $"{domainYaml}\n\n---\n\n{behaviourYaml}",
            "Prospects",
            "JJs.PotentialSales.Sdk");

        // Assert
        Assert.NotNull(prompt);
        Assert.Contains("Prospects", prompt);
        Assert.Contains("FluentValidation", prompt);
        Assert.Contains("JJs.PotentialSales.Sdk", prompt);
        Assert.Contains("prospectId", prompt);
        Assert.Contains("getProspectByNumber", prompt);
        Assert.Contains("createProspect", prompt);
        
        // Output the generated prompt for manual review
        Console.WriteLine("=== GENERATED PROSPECT SDK PROMPT ===");
        Console.WriteLine(prompt);
        Console.WriteLine("=== END PROMPT ===");
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}
