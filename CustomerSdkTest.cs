using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Modeller.Mcp.Shared.CodeGeneration.Security;
using Modeller.Mcp.Shared.CodeGeneration.Prompts;

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
    model: Customer
    summary: A business customer entity
    attributeUsages:
      - name: customerId
        type: primaryKey
        required: true
        summary: The unique identifier for the customer
      - name: customerNumber
        type: customerNumber
        required: true
        summary: A unique business identifier assigned to each customer
      - name: companyName
        type: baseString
        required: true
        summary: The official company name of the customer
      - name: customerStatus
        type: CustomerStatus
        required: true
        summary: The current status of the customer relationship
      - name: customerType
        type: baseString
        required: true
        summary: The classification of the customer
      - name: contactEmail
        type: emailAddress
        required: false
        summary: The primary email address for customer communication
    """;

var behaviourYaml = """
    model: Customer
    behaviours:
      - name: getCustomerByNumber
        summary: Retrieve a customer by their unique customer number
        entities:
          - Customer
        preconditions:
          - Customer.customerNumber is provided
        effects:
          - return Customer data
      - name: createCustomer
        summary: Create a new customer record
        entities:
          - Customer
        preconditions:
          - required fields are provided
          - customerNumber is unique
        effects:
          - new Customer is created
          - customerStatus is set to Active
    """;

try
{
    // Generate the prompt
    var prompt = await vsaPromptService.GenerateSDKFromDomainModelAsync(@"c:\jjs\set\dev\ModellerMcp\models\Business\CustomerManagement");

    Console.WriteLine("=== GENERATED CUSTOMER SDK PROMPT ===");
    Console.WriteLine(prompt);
    Console.WriteLine("=== END PROMPT ===");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
}

serviceProvider.Dispose();
