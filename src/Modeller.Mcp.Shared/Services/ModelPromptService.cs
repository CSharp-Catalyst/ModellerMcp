using Microsoft.Extensions.Logging;

namespace Modeller.Mcp.Shared.Services;

public class ModelPromptService(ILogger<ModelPromptService> logger)
{
    public List<PromptDefinition> GetAvailablePrompts() => [
            new PromptDefinition
            {
                Name = "analyze_model",
                Title = "Analyze Model Definition",
                Description = "Analyze a specific Modeller model definition for potential issues and best practices",
                Arguments =
                [
                    new PromptArgument
                    {
                        Name = "modelPath",
                        Description = "Path to the model file to analyze",
                        Required = true
                    },
                    new PromptArgument
                    {
                        Name = "analysisType",
                        Description = "Type of analysis: 'structure', 'validation', 'best-practices', or 'all'",
                        Required = false
                    }
                ]
            },
            new PromptDefinition
            {
                Name = "review_domain",
                Title = "Review Domain Models",
                Description = "Review all models within a specific domain for consistency and compliance",
                Arguments =
                [
                    new PromptArgument
                    {
                        Name = "domainPath",
                        Description = "Path to the domain folder (e.g., models/ProjectName/Organisation)",
                        Required = true
                    },
                    new PromptArgument
                    {
                        Name = "includeShared",
                        Description = "Whether to include shared attribute types and enums in the review",
                        Required = false
                    }
                ]
            },
            new PromptDefinition
            {
                Name = "create_model_template",
                Title = "Create Model Template",
                Description = "Generate a template for a new Modeller model based on requirements",
                Arguments =
                [
                    new PromptArgument
                    {
                        Name = "modelType",
                        Description = "Type of model: 'Type', 'Behaviour', 'AttributeType', or 'Enum'",
                        Required = true
                    },
                    new PromptArgument
                    {
                        Name = "domain",
                        Description = "Domain/area this model belongs to",
                        Required = true
                    },
                    new PromptArgument
                    {
                        Name = "description",
                        Description = "Brief description of what this model represents",
                        Required = false
                    }
                ]
            },
            new PromptDefinition
            {
                Name = "validate_structure",
                Title = "Validate Project Structure",
                Description = "Validate the folder structure and naming conventions for a Modeller project",
                Arguments =
                [
                    new PromptArgument
                    {
                        Name = "projectPath",
                        Description = "Path to the project root directory",
                        Required = true
                    },
                    new PromptArgument
                    {
                        Name = "strictMode",
                        Description = "Whether to apply strict validation rules",
                        Required = false
                    }
                ]
            },
            new PromptDefinition
            {
                Name = "migration_guide",
                Title = "Generate Migration Guide",
                Description = "Generate guidance for migrating models between versions or updating structure",
                Arguments =
                [
                    new PromptArgument
                    {
                        Name = "fromVersion",
                        Description = "Current version or structure to migrate from",
                        Required = true
                    },
                    new PromptArgument
                    {
                        Name = "toVersion",
                        Description = "Target version or structure to migrate to",
                        Required = true
                    },
                    new PromptArgument
                    {
                        Name = "modelPath",
                        Description = "Specific model or domain path to focus on",
                        Required = false
                    }
                ]
            }
        ];

    public async Task<PromptResponse> GetPromptContent(string promptName, Dictionary<string, object> arguments)
    {
        var messages = new List<PromptMessage>();

        return promptName switch
        {
            "analyze_model" => await GenerateAnalyzeModelPrompt(arguments),
            "review_domain" => await GenerateReviewDomainPrompt(arguments),
            "create_model_template" => await GenerateCreateModelTemplatePrompt(arguments),
            "validate_structure" => await GenerateValidateStructurePrompt(arguments),
            "migration_guide" => await GenerateMigrationGuidePrompt(arguments),
            _ => throw new ArgumentException($"Unknown prompt: {promptName}"),
        };
    }

    private async Task<PromptResponse> GenerateAnalyzeModelPrompt(Dictionary<string, object> arguments)
    {
        var modelPath = arguments["modelPath"].ToString();
        var analysisType = arguments.GetValueOrDefault("analysisType", "all").ToString();

        var messages = new List<PromptMessage>
        {
            new PromptMessage
            {
                Role = "user",
                Content = new TextContent
                {
                    Type = "text",
                    Text = $@"Please analyze the Modeller model definition at path: {modelPath}

Analysis type requested: {analysisType}

Please examine the model for:
1. YAML structure and syntax correctness
2. Compliance with Modeller naming conventions
3. Proper use of attribute types and relationships
4. Missing required fields or metadata
5. Best practice recommendations

Provide a comprehensive analysis with specific recommendations for improvement."
                }
            }
        };

        // Try to include the actual model content if available
        try
        {
            if (File.Exists(modelPath))
            {
                var modelContent = await File.ReadAllTextAsync(modelPath);
                messages.Add(new PromptMessage
                {
                    Role = "user",
                    Content = new TextContent
                    {
                        Type = "text",
                        Text = $@"Here is the model content to analyze:

```yaml
{modelContent}
```"
                    }
                });
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not read model file for prompt: {ModelPath}", modelPath);
        }

        return new PromptResponse { Messages = messages };
    }

    private Task<PromptResponse> GenerateReviewDomainPrompt(Dictionary<string, object> arguments)
    {
        var domainPath = arguments["domainPath"].ToString();
        var includeShared = bool.Parse(arguments.GetValueOrDefault("includeShared", "false").ToString() ?? "false");

        var messages = new List<PromptMessage>
        {
            new PromptMessage
            {
                Role = "user",
                Content = new TextContent
                {
                    Type = "text",
                    Text = $@"Please review all Modeller models in the domain: {domainPath}

Include shared components: {includeShared}

For this domain review, please:
1. Check consistency across all models in the domain
2. Verify proper use of shared attribute types and enums
3. Identify missing or inconsistent relationships
4. Review naming conventions and organization
5. Suggest improvements for domain coherence
6. Check for duplicate or redundant definitions

Provide a domain-level analysis with recommendations for improving the overall model architecture."
                }
            }
        };

        return Task.FromResult(new PromptResponse { Messages = messages });
    }

    private Task<PromptResponse> GenerateCreateModelTemplatePrompt(Dictionary<string, object> arguments)
    {
        var modelType = arguments["modelType"].ToString();
        var domain = arguments["domain"].ToString();
        var description = arguments.GetValueOrDefault("description", "").ToString();

        var templateGuidance = modelType switch
        {
            "Type" => @"
Create a Type model with:
- Proper metadata including name, title, description
- Attribute definitions with appropriate types
- Inheritance relationships if applicable
- Validation rules and constraints",

            "Behaviour" => @"
Create a Behaviour model with:
- State definitions and transitions
- Event handling specifications
- Business rule implementations
- Integration points with other models",

            "AttributeType" => @"
Create an AttributeType definition with:
- Base type specification
- Validation rules and constraints
- Format requirements
- Usage examples",

            "Enum" => @"
Create an Enum definition with:
- Value specifications
- Display names and descriptions
- Ordering and categorization
- Usage context",

            _ => "Create a generic model template"
        };

        var messages = new List<PromptMessage>
        {
            new PromptMessage
            {
                Role = "user",
                Content = new TextContent
                {
                    Type = "text",
                    Text = $@"Please create a Modeller {modelType} template for the domain: {domain}

Description: {description}

{templateGuidance}

Please provide:
1. A complete YAML template following Modeller conventions
2. Comments explaining each section
3. Best practice recommendations
4. Example values where appropriate
5. Integration guidance with other domain models

Ensure the template follows the proper folder structure for domain: {domain}"
                }
            }
        };

        return Task.FromResult(new PromptResponse { Messages = messages });
    }

    private Task<PromptResponse> GenerateValidateStructurePrompt(Dictionary<string, object> arguments)
    {
        var projectPath = arguments["projectPath"].ToString();
        var strictMode = bool.Parse(arguments.GetValueOrDefault("strictMode", "false").ToString() ?? "false");

        var messages = new List<PromptMessage>
        {
            new PromptMessage
            {
                Role = "user",
                Content = new TextContent
                {
                    Type = "text",
                    Text = $@"Please validate the Modeller project structure at: {projectPath}

Strict mode: {strictMode}

Please check:
1. Folder hierarchy compliance (ProjectName/Organisation/Sites/ structure)
2. File naming conventions
3. Required metadata files (_meta.yaml)
4. Shared components organization (AttributeTypes, Enums)
5. Domain separation and organization
6. File extension consistency (.yaml)

{(strictMode ? "Apply strict validation rules and flag any deviations." : "Apply standard validation with flexibility for project variations.")}

Provide a detailed report on structure compliance and recommendations for improvement."
                }
            }
        };

        return Task.FromResult(new PromptResponse { Messages = messages });
    }

    private Task<PromptResponse> GenerateMigrationGuidePrompt(Dictionary<string, object> arguments)
    {
        var fromVersion = arguments["fromVersion"].ToString();
        var toVersion = arguments["toVersion"].ToString();
        var modelPath = arguments.GetValueOrDefault("modelPath", "").ToString();

        var messages = new List<PromptMessage>
        {
            new PromptMessage
            {
                Role = "user",
                Content = new TextContent
                {
                    Type = "text",
                    Text = $@"Please generate a migration guide for Modeller models:

From version/structure: {fromVersion}
To version/structure: {toVersion}
{(string.IsNullOrEmpty(modelPath) ? "" : $"Focus area: {modelPath}")}

Please provide:
1. Overview of changes between versions
2. Step-by-step migration instructions
3. Automated migration script recommendations
4. Breaking changes and impact analysis
5. Validation steps to ensure successful migration
6. Rollback procedures if needed
7. Best practices for future migrations

Include specific examples for common migration scenarios."
                }
            }
        };

        return Task.FromResult(new PromptResponse { Messages = messages });
    }
}

// Supporting classes for prompts
public class PromptDefinition
{
    public required string Name { get; set; }
    public required string Title { get; set; }
    public string Description { get; set; } = string.Empty; // Optional description can default to empty
    public List<PromptArgument> Arguments { get; set; } = [];
}

public class PromptArgument
{
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty; // Optional description can default to empty
    public bool Required { get; set; }
}

public class PromptResponse
{
    public List<PromptMessage> Messages { get; set; } = [];
}

public class PromptMessage
{
    public string Role { get; set; } = string.Empty;
    public MessageContent Content { get; set; } = new TextContent();
}

public abstract class MessageContent
{
    public string Type { get; set; } = string.Empty;
}

public class TextContent : MessageContent
{
    public string Text { get; set; } = string.Empty;

    public TextContent() => Type = "text";

    public TextContent(string text) : this() => Text = text;
}
