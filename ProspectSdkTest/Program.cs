using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Modeller.McpServer.CodeGeneration.Security;
using Modeller.McpServer.CodeGeneration.Prompts;
using System.Text;

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

// Read the actual domain model files
var modelsPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "models", "JJs", "PotentialSales");
var prospectTypePath = Path.Combine(modelsPath, "Prospect.Type.yaml");
var prospectBehaviourPath = Path.Combine(modelsPath, "Prospect.Behaviour.yaml");

Console.WriteLine($"Reading domain models from: {modelsPath}");
Console.WriteLine($"Prospect Type: {prospectTypePath}");
Console.WriteLine($"Prospect Behaviour: {prospectBehaviourPath}");

if (!File.Exists(prospectTypePath) || !File.Exists(prospectBehaviourPath))
{
    Console.WriteLine("ERROR: Could not find Prospect domain model files");
    return;
}

var prospectTypeYaml = await File.ReadAllTextAsync(prospectTypePath);
var prospectBehaviourYaml = await File.ReadAllTextAsync(prospectBehaviourPath);

var combinedYaml = $"{prospectTypeYaml}\n\n---\n\n{prospectBehaviourYaml}";

Console.WriteLine("✓ Loaded domain model files");
Console.WriteLine("\n=== GENERATING SDK PROMPT ===");

try
{
    // Generate the prompt
    var prompt = await vsaPromptService.GenerateSDKFromDomainModelAsync(
        combinedYaml,
        "Prospects",
        "JJs.PotentialSales.Sdk");

    // Create output directory for the generated SDK
    var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "..", "generated-sdk", "JJs.PotentialSales.Sdk");
    Directory.CreateDirectory(outputDir);
    
    // Save the prompt to a file
    var promptPath = Path.Combine(outputDir, "GeneratedPrompt.md");
    await File.WriteAllTextAsync(promptPath, prompt);
    
    Console.WriteLine("\n=== PROMPT GENERATED SUCCESSFULLY ===");
    Console.WriteLine($"Prompt saved to: {promptPath}");
    Console.WriteLine($"Output directory created: {outputDir}");
    
    Console.WriteLine("\n=== NEXT STEPS ===");
    Console.WriteLine("1. Review the generated prompt in the output file");
    Console.WriteLine("2. Use this prompt with an LLM to generate the actual C# SDK code");
    Console.WriteLine("3. The generated code should follow VSA patterns with Prospects/ feature folder");
    Console.WriteLine("4. All files will use FluentValidation, records, and extension methods");
    
    Console.WriteLine("\n=== PREVIEW OF GENERATED PROMPT ===");
    Console.WriteLine(prompt.Substring(0, Math.Min(1000, prompt.Length)));
    if (prompt.Length > 1000)
    {
        Console.WriteLine("\n... (truncated - see full prompt in output file)");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"ERROR: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
}

serviceProvider.Dispose();

Console.WriteLine("\n=== GENERATION COMPLETE ===");
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
