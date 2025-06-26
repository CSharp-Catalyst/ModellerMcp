using ModelContextProtocol.Server;

using Modeller.McpServer.McpValidatorServer;
using Modeller.McpServer.McpValidatorServer.Models;
using Modeller.McpServer.McpValidatorServer.Services;
using Modeller.McpServer.CodeGeneration.Security;
using Modeller.McpServer.CodeGeneration.Prompts;
using Modeller.McpServer.CodeGeneration;
using Microsoft.Extensions.Configuration;

using System.ComponentModel;

var builder = Host.CreateApplicationBuilder(args);

// Add security configuration (optional, for enhanced security features)
builder.Configuration.AddJsonFile("appsettings.Security.json", optional: true, reloadOnChange: true);

builder.Logging.AddConsole(consoleLogOptions =>
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace);

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly()
    .WithPromptsFromAssembly();

// Add security services for LLM-driven code generation
builder.Services.AddSecurityServices(builder.Configuration);

builder.Services.AddTransient<ValidationTool>();
builder.Services.AddTransient<SdkGenerationTool>();
builder.Services.AddTransient<ModellerPrompts>();
builder.Services.AddTransient<IMcpModelValidator, YamlSchemaValidator>();
builder.Services.AddTransient<ModelDiscoveryService>();
builder.Services.AddTransient<ModelStructureValidator>();
builder.Services.AddTransient<ModelPromptService>();
builder.Services.AddTransient<ISdkGenerationService, SdkGenerationService>();

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
            var response = await validator.ValidateAsync(solutionPath, cancellationToken);

            if (!response.Results.Any())
            {
                return "✅ Validation completed successfully - no issues found.";
            }

            var errorCount = response.Results.Count(r => r.Severity == ValidationSeverity.Error);
            var warningCount = response.Results.Count(r => r.Severity == ValidationSeverity.Warning);
            var infoCount = response.Results.Count(r => r.Severity == ValidationSeverity.Info);

            var summary = $"Validation completed with {errorCount} errors, {warningCount} warnings, and {infoCount} info messages.\n\n";

            var groupedResults = response.Results.GroupBy(r => r.File);

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

            var response = await validator.ValidateAsync(modelsPath, cancellationToken);

            var structureResults = response.Results.Where(r =>
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

            var response = await validator.ValidateAsync(domainPath, cancellationToken);

            if (!response.Results.Any())
            {
                return $"✅ Domain validation completed successfully for '{Path.GetFileName(domainPath)}' - no issues found.";
            }

            var errorCount = response.Results.Count(r => r.Severity == ValidationSeverity.Error);
            var warningCount = response.Results.Count(r => r.Severity == ValidationSeverity.Warning);

            var summary = $"🏗️ **Domain '{Path.GetFileName(domainPath)}' Validation Results**\n";
            summary += $"Found {errorCount} errors and {warningCount} warnings.\n\n";

            // Group by model/entity
            var entityResults = response.Results.GroupBy(r => Path.GetDirectoryName(r.File))
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

[McpServerToolType]
public class GenerationTool
{
    [McpServerTool(Title = "Generate models"), Description("Using the passed model definitions, this will generate the necessary code files.")]
    public void GenerateModels()
    {
        
    }
}

[McpServerToolType]
public class ModellerPromptTools(ModelPromptService promptService)
{
    [McpServerTool(Title = "Analyze Model Definition"), Description("Generate a prompt for analyzing a specific Modeller model definition for potential issues and best practices")]
    public async Task<string> AnalyzeModelPrompt(
        [Description("Path to the model file to analyze")] string modelPath,
        [Description("Type of analysis: 'structure', 'validation', 'best-practices', or 'all'")] string analysisType = "all")
    {
        var arguments = new Dictionary<string, object>
        {
            ["modelPath"] = modelPath,
            ["analysisType"] = analysisType
        };

        var response = await promptService.GetPromptContent("analyze_model", arguments);
        var promptText = string.Join("\n\n", response.Messages.Select(m => ((TextContent)m.Content).Text));
        
        return $"📝 **Model Analysis Prompt Generated**\n\n" +
               $"**Model Path:** {modelPath}\n" +
               $"**Analysis Type:** {analysisType}\n\n" +
               $"**Prompt Content:**\n\n{promptText}\n\n" +
               $"💡 **Usage:** Copy this prompt and use it with your preferred LLM to get detailed analysis of your model definition.";
    }

    [McpServerTool(Title = "Review Domain Models"), Description("Generate a prompt for reviewing all models within a specific domain for consistency and compliance")]
    public async Task<string> ReviewDomainPrompt(
        [Description("Path to the domain folder (e.g., models/ProjectName/Organisation)")] string domainPath,
        [Description("Whether to include shared attribute types and enums in the review")] bool includeShared = false)
    {
        var arguments = new Dictionary<string, object>
        {
            ["domainPath"] = domainPath,
            ["includeShared"] = includeShared.ToString()
        };

        var response = await promptService.GetPromptContent("review_domain", arguments);
        var promptText = string.Join("\n\n", response.Messages.Select(m => ((TextContent)m.Content).Text));
        
        return $"📝 **Domain Review Prompt Generated**\n\n" +
               $"**Domain Path:** {domainPath}\n" +
               $"**Include Shared:** {includeShared}\n\n" +
               $"**Prompt Content:**\n\n{promptText}\n\n" +
               $"💡 **Usage:** Use this prompt to get a comprehensive review of all models in your domain.";
    }

    [McpServerTool(Title = "Create Model Template"), Description("Generate a prompt for creating a template for a new Modeller model based on requirements")]
    public async Task<string> CreateModelTemplatePrompt(
        [Description("Type of model: 'Type', 'Behaviour', 'AttributeType', or 'Enum'")] string modelType,
        [Description("Domain/area this model belongs to")] string domain,
        [Description("Brief description of what this model represents")] string description = "")
    {
        var arguments = new Dictionary<string, object>
        {
            ["modelType"] = modelType,
            ["domain"] = domain,
            ["description"] = description
        };

        var response = await promptService.GetPromptContent("create_model_template", arguments);
        var promptText = string.Join("\n\n", response.Messages.Select(m => ((TextContent)m.Content).Text));
        
        return $"📝 **Model Template Creation Prompt Generated**\n\n" +
               $"**Model Type:** {modelType}\n" +
               $"**Domain:** {domain}\n" +
               $"**Description:** {description}\n\n" +
               $"**Prompt Content:**\n\n{promptText}\n\n" +
               $"💡 **Usage:** Use this prompt to generate a properly structured {modelType} model template.";
    }

    [McpServerTool(Title = "Validate Project Structure"), Description("Generate a prompt for validating the folder structure and naming conventions for a Modeller project")]
    public async Task<string> ValidateStructurePrompt(
        [Description("Path to the project root directory")] string projectPath,
        [Description("Whether to apply strict validation rules")] bool strictMode = false)
    {
        var arguments = new Dictionary<string, object>
        {
            ["projectPath"] = projectPath,
            ["strictMode"] = strictMode.ToString()
        };

        var response = await promptService.GetPromptContent("validate_structure", arguments);
        var promptText = string.Join("\n\n", response.Messages.Select(m => ((TextContent)m.Content).Text));
        
        return $"📝 **Structure Validation Prompt Generated**\n\n" +
               $"**Project Path:** {projectPath}\n" +
               $"**Strict Mode:** {strictMode}\n\n" +
               $"**Prompt Content:**\n\n{promptText}\n\n" +
               $"💡 **Usage:** Use this prompt to validate your project's folder structure and naming conventions.";
    }

    [McpServerTool(Title = "Generate Migration Guide"), Description("Generate a prompt for creating guidance on migrating models between versions or updating structure")]
    public async Task<string> MigrationGuidePrompt(
        [Description("Current version or structure to migrate from")] string fromVersion,
        [Description("Target version or structure to migrate to")] string toVersion,
        [Description("Specific model or domain path to focus on")] string modelPath = "")
    {
        var arguments = new Dictionary<string, object>
        {
            ["fromVersion"] = fromVersion,
            ["toVersion"] = toVersion,
            ["modelPath"] = modelPath
        };

        var response = await promptService.GetPromptContent("migration_guide", arguments);
        var promptText = string.Join("\n\n", response.Messages.Select(m => ((TextContent)m.Content).Text));
        
        return $"📝 **Migration Guide Prompt Generated**\n\n" +
               $"**From Version:** {fromVersion}\n" +
               $"**To Version:** {toVersion}\n" +
               $"**Focus Path:** {(string.IsNullOrEmpty(modelPath) ? "All models" : modelPath)}\n\n" +
               $"**Prompt Content:**\n\n{promptText}\n\n" +
               $"💡 **Usage:** Use this prompt to generate detailed migration guidance for your models.";
    }

    [McpServerTool(Title = "List Available Prompts"), Description("List all available prompt templates with their descriptions and required parameters")]
    public string ListAvailablePrompts()
    {
        var prompts = promptService.GetAvailablePrompts();
        
        var result = "📋 **Available Modeller Prompt Templates**\n\n";
        
        foreach (var prompt in prompts)
        {
            result += $"### 🎯 {prompt.Title}\n";
            result += $"**Name:** `{prompt.Name}`\n";
            result += $"**Description:** {prompt.Description}\n\n";
            
            if (prompt.Arguments.Any())
            {
                result += "**Parameters:**\n";
                foreach (var arg in prompt.Arguments)
                {
                    var required = arg.Required ? " *(required)*" : " *(optional)*";
                    result += $"- **{arg.Name}**{required}: {arg.Description}\n";
                }
            }
            
            result += "\n---\n\n";
        }
        
        result += "💡 **How to use:** Call the specific prompt tool (e.g., `AnalyzeModelPrompt`) with the required parameters to generate the prompt content.\n\n";
        result += "🚀 **Workflow:** Generate prompt → Copy content → Use with your preferred LLM → Get expert guidance!";
        
        return result;
    }
}

// Remove the separate PromptMessage class as it's not needed with this approach