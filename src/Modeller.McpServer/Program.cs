using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Modeller.Mcp.Shared;
using Modeller.Mcp.Shared.CodeGeneration;
using Modeller.Mcp.Shared.CodeGeneration.Prompts.VSA;
using Modeller.Mcp.Shared.CodeGeneration.Security;
using Modeller.Mcp.Shared.Resources;
using Modeller.Mcp.Shared.Services;

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
    .AddSingleton<ModelDefinitionResources>()
    .AddTransient<IMcpModelValidator, YamlSchemaValidator>()
    .AddTransient<ModelDiscoveryService>()
    .AddTransient<ModelStructureValidator>()
    .AddTransient<IVsaPromptService, VsaPromptService>()
    .AddTransient<ISdkGenerationService, SdkGenerationService>()
    .AddTransient<IApiGenerationService, ApiGenerationService>()
    .AddTransient<ModelPromptService>();

await builder.Build().RunAsync();