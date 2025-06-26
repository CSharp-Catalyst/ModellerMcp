using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Modeller.McpServer.CodeGeneration.Prompts;
using Modeller.McpServer.CodeGeneration.Security;

namespace Modeller.McpServer.Tests.Unit;

public class ProspectSdkGenerationTests
{
    private readonly IServiceProvider _serviceProvider;

    public ProspectSdkGenerationTests()
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
        var combinedYaml = $"{domainYaml}\n\n---\n\n{behaviourYaml}";
        var prompt = await vsaPromptService.GenerateSDKFromDomainModelAsync(
            combinedYaml,
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
        Console.WriteLine("=== GENERATED PROMPT ===");
        Console.WriteLine(prompt);
        Console.WriteLine("=== END PROMPT ===");
    }
}
