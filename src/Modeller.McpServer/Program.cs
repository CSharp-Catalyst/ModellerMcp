using ModelContextProtocol.Server;

using Modeller.McpServer.McpValidatorServer;
using Modeller.McpServer.McpValidatorServer.Models;
using Modeller.McpServer.McpValidatorServer.Services;

using System.ComponentModel;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(consoleLogOptions =>
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace);

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();
builder.Services.AddTransient<ValidationTool>();
builder.Services.AddTransient<IMcpModelValidator, McpYamlSchemaValidator>();
builder.Services.AddTransient<ModelDiscoveryService>();
builder.Services.AddTransient<ModelStructureValidator>();

await builder.Build().RunAsync();

[McpServerToolType]
public class ValidationTool(ModelDiscoveryService discoveryService, IMcpModelValidator validator)
{
    [McpServerTool(Title = "Discover models"), Description("Discovers Modeller definitions in a solution or project directory. Searches for YAML files and analyzes the project structure.")]
    public Task<string> DiscoverModels([Description("Path to the solution or project root directory")] string solutionPath, CancellationToken cancellationToken = default)
    {
        try
        {
            var discoveryResult = discoveryService.DiscoverModels(solutionPath);

            if (discoveryResult.Errors.Any())
            {
                var errorSummary = "❌ **Discovery Errors:**\n";
                foreach (var error in discoveryResult.Errors)
                {
                    errorSummary += $"  • {error}\n";
                }
                errorSummary += "\n";
            }

            if (!discoveryResult.HasModels)
            {
                return Task.FromResult($"🔍 No Modeller model definitions found in: {solutionPath}\n\n" +
                       "**Searched for:**\n" +
                       "• /models/ directory\n" +
                       "• /Models/ directory\n" +
                       "• /Definitions/ directory\n" +
                       "• /src/Models/ directory\n" +
                       "• *.yaml and *.yml files throughout the solution\n\n" +
                       "**To get started:**\n" +
                       "1. Create a /models/ directory in your solution root\n" +
                       "2. Follow the recommended folder structure from the BDD documentation\n" +
                       "3. Use the validation tools to ensure your models are correct");
            }

            var summary = $"🔍 **Model Discovery Results for:** {Path.GetFileName(solutionPath)}\n\n";
            summary += $"**Total Files Found:** {discoveryResult.TotalFileCount}\n\n";

            if (discoveryResult.ModelDirectories.Any())
            {
                summary += "📁 **Structured Model Directories:**\n";
                foreach (var modelDir in discoveryResult.ModelDirectories)
                {
                    summary += $"\n🏗️ **{Path.GetFileName(modelDir.Path)}**\n";
                    summary += $"   📍 {modelDir.Path}\n";

                    foreach (var group in modelDir.ModelGroups)
                    {
                        summary += $"\n   📦 **{group.Name}**\n";
                        summary += $"      📍 {group.Directory}\n";

                        if (group.HasMetadata)
                        {
                            summary += "      ✅ Has metadata (_meta.yaml)\n";
                        }

                        if (group.HasTypeFile)
                        {
                            summary += "      ✅ Has type definition\n";
                        }

                        if (group.HasBehaviourFile)
                        {
                            summary += "      ✅ Has behaviour definition\n";
                        }

                        foreach (var file in group.Files)
                        {
                            var typeIcon = file.Type switch
                            {
                                ModelFileType.BddModel => "🎯",
                                ModelFileType.AttributeTypes => "🔤",
                                ModelFileType.Enum => "📋",
                                ModelFileType.ValidationProfiles => "✅",
                                ModelFileType.Metadata => "📝",
                                _ => "📄"
                            };

                            summary += $"      {typeIcon} {file.Name} ({file.Type})\n";
                        }
                    }
                }
            }

            if (discoveryResult.LooseFiles.Any())
            {
                summary += "\n📄 **Individual Model Files:**\n";
                var groupedLooseFiles = discoveryResult.LooseFiles.GroupBy(f => Path.GetDirectoryName(f.Path));

                foreach (var group in groupedLooseFiles)
                {
                    summary += $"\n   📍 {group.Key}\n";
                    foreach (var file in group)
                    {
                        var typeIcon = file.Type switch
                        {
                            ModelFileType.BddModel => "🎯",
                            _ => "📄"
                        };
                        summary += $"      {typeIcon} {file.Name} ({file.Type})\n";
                    }
                }
            }

            summary += "\n💡 **Next Steps:**\n";
            summary += "• Use `ValidateModel` to check your model definitions\n";
            summary += "• Use `ValidateStructure` to verify folder organization\n";
            summary += "• Use `ValidateDomain` to check specific bounded contexts\n";

            return Task.FromResult(summary);
        }
        catch (Exception ex)
        {
            return Task.FromResult($"❌ Model discovery failed: {ex.Message}");
        }
    }

    [McpServerTool(Title = "Validate a specific model"), Description("Validates Modeller definitions. Can validate a single file or entire directory structure.")]
    public async Task<string> ValidateModel([Description("Path to the model file or directory to validate")] string solutionPath, CancellationToken cancellationToken = default)
    {
        try
        {
            var results = await validator.ValidateAsync(solutionPath, cancellationToken);

            if (!results.Any())
            {
                return "✅ Validation completed successfully - no issues found.";
            }

            var errorCount = results.Count(r => r.Severity == ValidationSeverity.Error);
            var warningCount = results.Count(r => r.Severity == ValidationSeverity.Warning);
            var infoCount = results.Count(r => r.Severity == ValidationSeverity.Info);

            var summary = $"Validation completed with {errorCount} errors, {warningCount} warnings, and {infoCount} info messages.\n\n";

            var groupedResults = results.GroupBy(r => r.File);

            foreach (var fileGroup in groupedResults)
            {
                summary += $"📄 **{Path.GetFileName(fileGroup.Key)}**\n";

                foreach (var result in fileGroup.OrderBy(r => r.Severity))
                {
                    var icon = result.Severity switch
                    {
                        ValidationSeverity.Error => "❌",
                        ValidationSeverity.Warning => "⚠️",
                        ValidationSeverity.Info => "ℹ️",
                        _ => "•"
                    };

                    summary += $"  {icon} {result.Message}\n";
                }

                summary += "\n";
            }

            return summary;
        }
        catch (Exception ex)
        {
            return $"❌ Validation failed: {ex.Message}";
        }
    }

    [McpServerTool(Title = "Validate the definition structure"), Description("Validates the folder structure and naming conventions for Modeller definitions based on the recommended project layout.")]
    public async Task<string> ValidateStructure([Description("Path to the root models directory to validate structure")] string modelsPath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Directory.Exists(modelsPath))
            {
                return $"❌ Directory not found: {modelsPath}";
            }

            var results = await validator.ValidateAsync(modelsPath, cancellationToken);

            var structureResults = results.Where(r =>
                r.Message.Contains("structure") ||
                r.Message.Contains("naming") ||
                r.Message.Contains("PascalCase") ||
                r.Message.Contains("metadata"));

            if (!structureResults.Any())
            {
                return "✅ Project structure validation completed successfully - no issues found.";
            }

            var summary = "📁 **Project Structure Validation Results**\n\n";

            foreach (var result in structureResults.OrderBy(r => r.Severity))
            {
                var icon = result.Severity switch
                {
                    ValidationSeverity.Error => "❌",
                    ValidationSeverity.Warning => "⚠️",
                    ValidationSeverity.Info => "ℹ️",
                    _ => "•"
                };

                summary += $"{icon} {result.Message}\n";
                summary += $"   📍 {result.File}\n\n";
            }

            return summary;
        }
        catch (Exception ex)
        {
            return $"❌ Structure validation failed: {ex.Message}";
        }
    }

    [McpServerTool(Title = "Validate the domain"), Description("Validates a specific domain's models within the recommended folder structure (e.g., ProjectName/Organisation/Sites/).")]
    public async Task<string> ValidateDomain([Description("Path to the domain folder (e.g., models/ProjectName/Organisation)")] string domainPath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Directory.Exists(domainPath))
            {
                return $"❌ Domain directory not found: {domainPath}";
            }

            var results = await validator.ValidateAsync(domainPath, cancellationToken);

            if (!results.Any())
            {
                return $"✅ Domain validation completed successfully for '{Path.GetFileName(domainPath)}' - no issues found.";
            }

            var errorCount = results.Count(r => r.Severity == ValidationSeverity.Error);
            var warningCount = results.Count(r => r.Severity == ValidationSeverity.Warning);

            var summary = $"🏗️ **Domain '{Path.GetFileName(domainPath)}' Validation Results**\n";
            summary += $"Found {errorCount} errors and {warningCount} warnings.\n\n";

            // Group by model/entity
            var entityResults = results.GroupBy(r => Path.GetDirectoryName(r.File))
                .Where(g => g.Key != null);

            foreach (var entityGroup in entityResults)
            {
                var entityName = Path.GetFileName(entityGroup.Key);
                summary += $"📦 **{entityName}**\n";

                foreach (var result in entityGroup.OrderBy(r => r.Severity))
                {
                    var icon = result.Severity switch
                    {
                        ValidationSeverity.Error => "❌",
                        ValidationSeverity.Warning => "⚠️",
                        ValidationSeverity.Info => "ℹ️",
                        _ => "•"
                    };

                    summary += $"  {icon} {result.Message}\n";
                    summary += $"     📄 {Path.GetFileName(result.File)}\n";
                }

                summary += "\n";
            }

            return summary;
        }
        catch (Exception ex)
        {
            return $"❌ Domain validation failed: {ex.Message}";
        }
    }

    [McpServerTool(Title = "Get Validation Guidance"), Description("Get validation recommendations and best practices for Modeller definitions.")]
    public string GetValidationGuidance() => """
        📚 **Modeller Validation Guide**
        
        ## File Structure Best Practices
        
        ### Recommended Directory Layout:
        ```
        /models/
          ProjectName/                  # Top-level project
            Organisation/              # Bounded context
              Sites/                   # Entity grouping
                Site.Type.yaml         # Entity definition
                Site.Behaviour.yaml    # Behaviors
            Shared/
              ValueTypes/
                Address.yaml
              Enums/
                Priority.yaml
              Attributes.yaml
        ```
        
        ### Naming Conventions:
        - **Files**: Use PascalCase (e.g., `UserAccount.Type.yaml`)
        - **Models**: PascalCase (e.g., `model: UserAccount`)
        - **Attributes**: camelCase (e.g., `type: emailAddress`)
        - **Behaviours**: camelCase (e.g., `name: activateUserAccount`)
        
        ## Model Definition Format
        
        ### Model Structure:
        ```yaml
        model: UserAccount
        attributeUsages:
          - type: email
            required: true
            summary: Email address used for login
        behaviours:
          - name: activateUserAccount
            description: Activate a user account
            entities:
              - UserAccount
        scenarios:
          - name: activate inactive user
            given:
              - UserAccount.isActive is false
            when:
              - activateUserAccount is called
            then:
              - UserAccount.isActive is true
        ```
        
        ### Key Validation Rules:
        1. **Required Fields**: Model name, attribute types, behavior names
        2. **File Matching**: Model name should match filename
        3. **Naming**: Follow camelCase/PascalCase conventions
        4. **BDD Scenarios**: Include Given-When-Then structure
        5. **Metadata**: Include _meta.yaml for bounded contexts
        6. **Review Dates**: Metadata should be reviewed within 90 days
        
        ## Common Issues:
        - Missing required fields (model name, attribute types)
        - Incorrect naming conventions
        - Empty or malformed YAML
        - Missing BDD scenario components
        - Outdated metadata files
        
        Use the validation tools to check your models before committing!
        """;
}