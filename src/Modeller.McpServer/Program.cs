using Microsoft.Extensions.Configuration;

using Modeller.Mcp.Shared;
using Modeller.Mcp.Shared.Services;
using Modeller.McpServer.CodeGeneration;
using Modeller.McpServer.CodeGeneration.Security;
using Modeller.McpServer.McpValidatorServer;
using Modeller.McpServer.McpValidatorServer.Services;

var builder = Host.CreateApplicationBuilder(args);

// Add security configuration (optional, for enhanced security features)
builder.Configuration.AddJsonFile("appsettings.Security.json", optional: true, reloadOnChange: true);

builder.Logging.AddConsole(consoleLogOptions =>
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace);

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly(typeof(ModellerPromptTools).Assembly)
    .WithPromptsFromAssembly(typeof(ModellerPrompts).Assembly);

// Add security services for LLM-driven code generation
builder.Services.AddSecurityServices(builder.Configuration);

builder.Services
    .AddTransient<IMcpModelValidator, YamlSchemaValidator>()
    .AddTransient<ModelDiscoveryService>()
    .AddTransient<ModelStructureValidator>()
    .AddTransient<ISdkGenerationService, SdkGenerationService>();

//builder.Services.AddTransient<ValidationTool>();
//builder.Services.AddTransient<SdkGenerationTool>();
//builder.Services.AddTransient<ModellerPrompts>();
//builder.Services.AddTransient<ModelPromptService>();

await builder.Build().RunAsync();