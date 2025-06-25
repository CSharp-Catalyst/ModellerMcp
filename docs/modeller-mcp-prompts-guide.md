# Modeller MCP Server - Prompts Guide

This document describes the prompts available in the Modeller MCP (Model Context Protocol) server and how to use them with VS Code GitHub Copilot and other MCP clients.

## Overview

The Modeller MCP server provides 5 specialized prompts designed to help with Modeller domain model development, validation, and maintenance:

1. **Analyze Model Definition** - Deep analysis of individual model files
2. **Review Domain Models** - Comprehensive review of all models within a domain
3. **Create Model Template** - Generate templates for new models
4. **Validate Project Structure** - Check folder organization and naming conventions
5. **Generate Migration Guide** - Create migration guidance between versions

## Available Prompts

### 1. Analyze Model Definition (`analyze_model`)

**Purpose**: Analyze a specific Modeller model definition for potential issues and best practices.

**Parameters**:
- `modelPath` (required): Path to the model file to analyze
- `analysisType` (optional): Type of analysis - 'structure', 'validation', 'best-practices', or 'all' (default: 'all')

**Example Usage**:
```
@Modeller analyze_model
- modelPath: "c:/path/to/models/JJs/PotentialSales/Prospect.Type.yaml"
- analysisType: "best-practices"
```

**What it does**: Generates a comprehensive prompt for analyzing YAML structure, naming conventions, attribute usage, relationships, and best practice compliance.

### 2. Review Domain Models (`review_domain`)

**Purpose**: Review all models within a specific domain for consistency and compliance.

**Parameters**:
- `domainPath` (required): Path to the domain folder (e.g., models/ProjectName/Organisation)
- `includeShared` (optional): Whether to include shared attribute types and enums (default: true)

**Example Usage**:
```
@Modeller review_domain
- domainPath: "c:/path/to/models/JJs/PotentialSales"
- includeShared: true
```

**What it does**: Creates a prompt for comprehensive domain review, checking consistency across all models, shared type usage, and domain-specific patterns.

### 3. Create Model Template (`create_model_template`)

**Purpose**: Generate a template for a new Modeller model based on requirements.

**Parameters**:
- `modelType` (required): Type of model - 'Type', 'Behaviour', 'AttributeType', or 'Enum'
- `domain` (required): Domain/area this model belongs to
- `description` (optional): Brief description of what this model represents

**Example Usage**:
```
@Modeller create_model_template
- modelType: "Type"
- domain: "PotentialSales"
- description: "A customer inquiry about waste collection services"
```

**What it does**: Generates a prompt that will create a proper YAML template following Modeller conventions, including appropriate metadata, structure, and best practices.

### 4. Validate Project Structure (`validate_structure`)

**Purpose**: Validate the folder structure and naming conventions for a Modeller project.

**Parameters**:
- `projectPath` (required): Path to the project root directory
- `strictMode` (optional): Whether to apply strict validation rules (default: false)

**Example Usage**:
```
@Modeller validate_structure
- projectPath: "c:/path/to/ModellerProject"
- strictMode: true
```

**What it does**: Creates a prompt for validating folder organization, file naming, metadata presence, and adherence to recommended project structure.

### 5. Generate Migration Guide (`migration_guide`)

**Purpose**: Generate guidance for migrating models between versions or updating structure.

**Parameters**:
- `fromVersion` (required): Current version or structure to migrate from
- `toVersion` (required): Target version or structure to migrate to
- `modelPath` (optional): Specific model or domain path to focus on

**Example Usage**:
```
@Modeller migration_guide
- fromVersion: "v1.0"
- toVersion: "v2.0"
- modelPath: "models/JJs/PotentialSales"
```

**What it does**: Generates a prompt for creating step-by-step migration instructions, including breaking changes, new features, and recommended approaches.

## Using with VS Code GitHub Copilot

### Setup

1. Ensure your `.vscode/mcp.json` file includes the Modeller server configuration
2. Open VS Code with GitHub Copilot enabled
3. Use the prompts in Copilot Chat with the `@Modeller` prefix

### Workflow Example

1. **Discovery**: Start with the `DiscoverModels` tool to understand your project structure
2. **Analysis**: Use `analyze_model` prompt to get detailed analysis guidance
3. **Review**: Use the generated prompt with GitHub Copilot to get AI-powered insights
4. **Action**: Apply the recommendations and validate with the MCP tools

### Sample Conversation Flow

```
# Step 1: Discover models
@Modeller DiscoverModels solutionPath="C:/path/to/your/project"

# Step 2: Generate analysis prompt
@Modeller analyze_model modelPath="C:/path/to/specific/model.yaml" analysisType="all"

# Step 3: Use the generated prompt in a new chat
[Copy the generated prompt content and use it in GitHub Copilot]

# Step 4: Validate improvements
@Modeller ValidateModel solutionPath="C:/path/to/updated/model.yaml"
```

## Configuration in mcp.json

Your `.vscode/mcp.json` should include all available prompts in the `inputs` section:

```json
{
  "inputs": [
    {"type": "promptString", "id": "discover_models", "description": "Discover the models within the solution"},
    {"type": "promptString", "id": "analyze_model", "description": "Analyze a specific Modeller model definition for potential issues and best practices"},
    {"type": "promptString", "id": "review_domain", "description": "Review all models within a specific domain for consistency and compliance"},
    {"type": "promptString", "id": "create_model_template", "description": "Generate a template for a new Modeller model based on requirements"},
    {"type": "promptString", "id": "validate_structure", "description": "Validate the folder structure and naming conventions for a Modeller project"},
    {"type": "promptString", "id": "migration_guide", "description": "Generate guidance for migrating models between versions or updating structure"}
  ],
  "servers": {
    "Modeller": {
      "type": "stdio",
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "C:\\path\\to\\your\\Modeller.McpServer.csproj"
      ]
    }
  }
}
```

## Best Practices

1. **Start with Discovery**: Always use `DiscoverModels` first to understand your project structure
2. **Use Appropriate Analysis**: Choose the right `analysisType` for your needs
3. **Domain-Level Reviews**: Use `review_domain` for comprehensive domain analysis
4. **Template Generation**: Use `create_model_template` for consistent new model creation
5. **Structure Validation**: Regularly use `validate_structure` to maintain project organization
6. **Migration Planning**: Use `migration_guide` for version upgrades

## Integration with Other Tools

The Modeller MCP server works seamlessly with:
- VS Code GitHub Copilot (primary integration)
- Claude Desktop
- Continue extension
- Any MCP-compatible client

## Troubleshooting

If prompts aren't working:
1. Verify the MCP server is running (`dotnet run --project path/to/Modeller.McpServer.csproj`)
2. Check the `mcp.json` configuration
3. Ensure GitHub Copilot has access to the MCP server
4. Test with basic tools first (`DiscoverModels`, `ValidateModel`)

## Future Enhancements

Potential future prompt additions:
- **Performance Analysis**: Analyze model performance implications
- **Documentation Generation**: Generate comprehensive model documentation
- **Test Generation**: Create test scenarios for model validation
- **Integration Mapping**: Map models to external system integrations
