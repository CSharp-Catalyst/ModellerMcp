using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modeller.Mcp.Shared.CodeGeneration.Prompts.VSA;
using Modeller.Mcp.Shared.CodeGeneration.Security;

namespace SdkGenerator;

class Program
{
    static async Task Main(string[] args)
    {
        var services = new ServiceCollection();
        
        // Create test configuration
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Security:AuditLogPath"] = "c:\\temp\\audit"
            })
            .Build();
            
        services.AddLogging(builder => builder.AddConsole()); // Add logging services
        services.AddSecurityServices(configuration);
        services.AddTransient<IVsaPromptService, VsaPromptService>();
        var serviceProvider = services.BuildServiceProvider();

        var vsaPromptService = serviceProvider.GetRequiredService<IVsaPromptService>();

        Console.WriteLine("Generating SDK from PotentialSales domain...");
        
        try
        {
            var prompt = await vsaPromptService.GenerateSDKFromDomainModelAsync(@"c:\jjs\set\dev\playschool\models\JJs\PotentialSales");
            
            // Save the prompt to a file
            var outputPath = @"c:\jjs\set\dev\ModellerMcp\generated\PotentialSalesSDK\SDK_Generation_Prompt.md";
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            await File.WriteAllTextAsync(outputPath, prompt);
            
            Console.WriteLine($"SDK generation prompt saved to: {outputPath}");
            Console.WriteLine();
            Console.WriteLine("=== GENERATED PROMPT ===");
            Console.WriteLine(prompt);
            Console.WriteLine("=== END PROMPT ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
