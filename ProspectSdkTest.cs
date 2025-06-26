using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Modeller.McpServer.CodeGeneration.Security;
using Modeller.McpServer.CodeGeneration.Prompts;

// Setup DI container
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

// Add services
services.AddLogging(builder => builder.AddConsole());
services.AddSingleton<IConfiguration>(configuration);
services.AddSecurityServices(configuration);

var serviceProvider = services.BuildServiceProvider();

// Get the VSA prompt service
var vsaPromptService = serviceProvider.GetRequiredService<IVsaPromptService>();

// Domain model YAML
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

try
{
    // Generate the prompt
    var prompt = await vsaPromptService.GenerateSDKFromDomainModelAsync(
        $"{domainYaml}\n\n---\n\n{behaviourYaml}",
        "Prospects",
        "JJs.PotentialSales.Sdk");

    Console.WriteLine("=== GENERATED PROSPECT SDK PROMPT ===");
    Console.WriteLine(prompt);
    Console.WriteLine("=== END PROMPT ===");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
}

serviceProvider.Dispose();
