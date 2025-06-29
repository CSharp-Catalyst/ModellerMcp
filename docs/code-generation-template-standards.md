# Code Generation Template Standards

## Overview

This document establishes the standard approach for creating code generation services in the Modeller MCP system. All future code generators should follow these patterns for consistency, maintainability, and extensibility.

## Architecture Pattern

### 1. Structured Prompt Templates

All code generation should use structured prompt templates stored as Markdown files, following the pattern established by:

- **SDK Generation**: `GenerateSDKFromDomainModel.md`
- **API Generation**: `GenerateMinimalAPIFromSDK.md`

### 2. Template Location

Prompt templates should be stored in:
```
src/Modeller.Mcp.Shared/CodeGeneration/Prompts/VSA/
```

### 3. Template Structure

Each prompt template should follow this structure:

```markdown
# Generate [Output Type] from [Input Type] - Vertical Slice Architecture

## Purpose
Clear description of what the generator creates

## Context
Expert role definition and background

## Input Requirements
- Required inputs with examples
- Optional parameters with defaults

## Architecture Principles
- Core architectural patterns to follow
- Design principles and constraints

## Framework Requirements
- Target framework versions
- Language versions
- Key feature requirements

## Project Configuration
Complete .csproj template with necessary packages

## GlobalUsings Configuration
Standard using statements for the generated code

## Generated Structure
Directory tree showing expected output structure

## Code Generation Guidelines
Detailed sections for each type of file to generate:
### 1. [File Type 1]
### 2. [File Type 2]
### 3. [File Type 3]
etc.

## [Input] Mapping Rules
How to transform input definitions to output code

## Security Considerations
Security requirements and best practices

## Example Output
Concrete example showing input and expected output

## Output Format
Summary of deliverables and file requirements

## Usage
How the template integrates with the MCP system
```

## Service Implementation Pattern

### 1. Service Interface

```csharp
public interface I[Type]GenerationService
{
    /// <summary>
    /// Generate complete [output type] from [input type]
    /// </summary>
    Task<[Type]GenerationResult> Generate[Type]Async([Type]GenerationRequest request);

    /// <summary>
    /// Generate the prompt that would be used for [type] creation
    /// </summary>
    Task<string> GeneratePromptAsync([Type]GenerationRequest request);
}
```

### 2. Request Model

```csharp
public record [Type]GenerationRequest
{
    /// <summary>
    /// Primary input path (required)
    /// </summary>
    public required string InputPath { get; init; }

    /// <summary>
    /// Output path for generated files (required)
    /// </summary>
    public required string OutputPath { get; init; }

    /// <summary>
    /// Project name (required)
    /// </summary>
    public required string ProjectName { get; init; }

    /// <summary>
    /// Target namespace (required)
    /// </summary>
    public required string Namespace { get; init; }

    // Additional optional parameters as needed
}
```

### 3. Result Model

```csharp
public record [Type]GenerationResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public string? GeneratedPrompt { get; init; }
    public string? OutputPath { get; init; }
    public List<string> GeneratedFiles { get; init; } = [];

    public static [Type]GenerationResult Success(string prompt, string outputPath, List<string> files) =>
        new() { IsSuccess = true, GeneratedPrompt = prompt, OutputPath = outputPath, GeneratedFiles = files };

    public static [Type]GenerationResult Failure(string error) =>
        new() { IsSuccess = false, ErrorMessage = error };
}
```

### 4. Service Implementation

```csharp
public class [Type]GenerationService(
    ModelDiscoveryService modelDiscoveryService, 
    ISecureLlmService secureLlmService, 
    ILogger<[Type]GenerationService> logger) : I[Type]GenerationService
{
    public async Task<[Type]GenerationResult> Generate[Type]Async([Type]GenerationRequest request)
    {
        try
        {
            // 1. Validate inputs
            // 2. Generate prompt
            // 3. Use secure LLM service
            // 4. Save generated files
            // 5. Return success result
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during [type] generation");
            return [Type]GenerationResult.Failure($"Generation failed: {ex.Message}");
        }
    }

    public async Task<string> GeneratePromptAsync([Type]GenerationRequest request)
    {
        try
        {
            // 1. Load structured prompt template
            var templatePath = Path.Combine("CodeGeneration", "Prompts", "VSA", "Generate[Type]From[Input].md");
            var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, templatePath);
            
            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Template not found: {fullPath}");
                
            var template = await File.ReadAllTextAsync(fullPath);

            // 2. Discover and process inputs
            // 3. Extract context information
            // 4. Replace template placeholders
            // 5. Return final prompt
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating prompt");
            throw;
        }
    }
}
```

## Template Placeholder Standards

### Required Placeholders

All templates should support these standard placeholders:

- `{ProjectName}` - The target project name
- `{Namespace}` - The target namespace
- `{OutputPath}` - Where files will be generated
- `{InputPath}` - Source input path

### Entity-Based Placeholders

For entity-based generation:

- `{Entity}` - Singular entity name (e.g., "Customer")
- `{Entities}` - Plural entity name (e.g., "Customers")
- `{entities}` - Lowercase plural (e.g., "customers")
- `{FeatureName}` - Feature/module name

### Context Placeholders

- `{ModelDefinitions}` - YAML model content
- `{InputStructure}` - Structure of input files
- `{Dependencies}` - Required dependencies

## Security Integration

### 1. Secure LLM Service Usage

```csharp
var secureRequest = new SecureLlmRequest
{
    RawPrompt = prompt,
    ModelId = "gpt-4",
    PromptType = "[type]_generation",
    SecurityContext = new SecurityContext
    {
        UserId = "system",
        SessionId = Guid.CreateVersion7().ToString(),
        IPAddress = "127.0.0.1",
        UserAgent = "ModellerMcp/1.0"
    },
    PromptInputs = new Dictionary<string, string>
    {
        ["project_name"] = request.ProjectName,
        ["namespace"] = request.Namespace,
        // Add all relevant inputs for audit trail
    }
};
```

### 2. Audit Requirements

- All generation requests must be logged
- Prompt content must be audited
- Security validation must be performed
- Input sanitization must be applied

## Quality Standards

### 1. Modern C# Requirements

- Target .NET 8.0 LTS minimum
- Use C# 12 features (required keyword, pattern matching, etc.)
- Enable nullable reference types
- Include comprehensive XML documentation
- Follow VSA principles

### 2. Code Quality

- Include proper error handling
- Use dependency injection
- Follow SOLID principles
- Include comprehensive validation
- Generate production-ready code

### 3. Testing Requirements

- Include unit tests for the service
- Test template loading and processing
- Test error scenarios
- Validate generated code compiles

## Future Generator Examples

### Potential Generators to Implement

1. **Blazor UI Generator**: `GenerateBlazorUIFromSDK.md`
   - Input: SDK project + UI specifications
   - Output: Complete Blazor Server/WASM project

2. **Azure Function Generator**: `GenerateAzureFunctionsFromSDK.md`
   - Input: SDK project + function specifications
   - Output: Azure Functions project with HTTP triggers

3. **gRPC Service Generator**: `GenerateGrpcServiceFromSDK.md`
   - Input: SDK project + service specifications
   - Output: gRPC service implementation

4. **Docker Configuration Generator**: `GenerateDockerFromAPI.md`
   - Input: API project
   - Output: Dockerfile, docker-compose, k8s manifests

5. **Test Project Generator**: `GenerateTestsFromSDK.md`
   - Input: SDK project
   - Output: Comprehensive test suite (unit, integration, performance)

## Migration Guidelines

### Updating Existing Generators

When updating existing generators that don't follow this pattern:

1. Create the structured prompt template file
2. Update the service to load the template
3. Replace inline prompts with template processing
4. Add proper placeholder replacement
5. Update tests to validate template loading
6. Document any template-specific requirements

### Backward Compatibility

- Maintain existing public interfaces
- Support existing request/response models
- Ensure audit logs continue to work
- Preserve security validation

## Benefits of This Approach

### 1. Maintainability
- Prompts are version-controlled and reviewable
- Changes to prompts don't require code recompilation
- Easy to test prompt changes independently

### 2. Consistency
- All generators follow the same pattern
- Standardized security integration
- Consistent audit logging

### 3. Extensibility
- Easy to add new generators
- Template inheritance possible
- Shared utilities and patterns

### 4. Quality
- Comprehensive templates ensure complete output
- Security requirements built-in
- Modern C# patterns enforced

## Implementation Checklist

When creating a new generator:

- [ ] Create structured prompt template in VSA folder
- [ ] Implement service interface following the pattern
- [ ] Add comprehensive request/response models
- [ ] Integrate with secure LLM service
- [ ] Include proper error handling and logging
- [ ] Add template placeholder replacement
- [ ] Write unit tests for the service
- [ ] Document any generator-specific requirements
- [ ] Test with real inputs to validate output quality
- [ ] Add to dependency injection configuration

This standardized approach ensures that all code generators in the Modeller MCP system are consistent, maintainable, and follow modern development best practices.
