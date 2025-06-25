# Modeller Code Generation - Technical Design Document

## Overview

This document outlines the technical design for implementing Vertical Slice Architecture (VSA) code generation within the Modeller MCP server. The goal is to transform validated Modeller domain models into production-ready .NET 9 applications following modern development practices.

## Architecture Principles

### Vertical Slice Architecture (VSA)
- **Feature-Centric**: Each domain model becomes a complete feature slice
- **Self-Contained**: Features include all layers (API, business logic, data access)
- **Minimal Dependencies**: Reduce cross-feature coupling
- **Clear Boundaries**: Well-defined feature interfaces

### Technology Stack
- **.NET 9**: Latest framework with native capabilities
- **No MediatR**: Leverage .NET 9's native DI and request/response patterns
- **xUnit v3**: Modern testing framework
- **OpenTelemetry (OTEL)**: Comprehensive observability
- **.NET Aspire**: Enhanced local development experience
- **EF Core**: Data access with minimal configuration
- **Minimal APIs**: Lightweight API endpoints

## LLM-Assisted Code Generation Strategy

### Template-as-Prompt Approach
Instead of static text replacement templates, we use **structured prompts** that guide LLMs to generate and modify code intelligently. This provides:

- **Intelligent Generation**: LLM understands context and intent
- **Adaptive Modifications**: Can update existing code without breaking structure
- **Developer Control**: Final code structure remains in developer's hands
- **Contextual Awareness**: LLM considers existing codebase patterns

### Prompt Template Distribution Model
**NuGet Package Approach:**
- `Modeller.Prompts.WebAPI` - Web API generation prompts
- `Modeller.Prompts.Console` - Console application prompts  
- `Modeller.Prompts.Library` - Class library prompts
- `Modeller.Prompts.Blazor` - Blazor web app prompts

**Benefits:**
- Versioned prompt distribution
- LLM-guided intelligent generation
- Adaptive to existing code
- Community contribution of prompts
- Context-aware modifications

### Prompt Template Versioning & Local Copy
```
/project-root/
  .modeller/
    prompts/
      version.json              # Prompt template version tracking
      webapi-vsa/              # Local prompt templates
        v1.0.0/
          generation-prompts/   # Initial code generation
          modification-prompts/ # Code modification prompts
          context-prompts/      # Context understanding prompts
      console-app/
        v1.0.0/
          generation-prompts/
```

**Version Management:**
- Prompt templates copied locally on first use
- Version compatibility checking
- Optional prompt template updates
- Fallback to embedded defaults

## Project Structure

### Generated Solution Layout
```
/solution-root/
  models/                      # Modeller definitions (existing)
  docs/                       # Documentation (existing)
  src/                        # Generated application code
    Company.Domain.sln
    Company.Domain.Api/         # Web API project
      Features/                 # VSA feature slices
        Prospects/
          ProspectEndpoints.cs  # Minimal API endpoints
          ProspectService.cs    # Business logic
          ProspectModels.cs     # DTOs and entities
        Activities/
          ActivityEndpoints.cs
          ActivityService.cs
          ActivityModels.cs
      Shared/                   # Cross-cutting concerns
        Database/
          ApplicationDbContext.cs
        Extensions/
          ServiceCollectionExtensions.cs
        Middleware/
          ExceptionHandlingMiddleware.cs
      Program.cs                # Application entry point
      appsettings.json
      Company.Domain.Api.csproj
    Company.Domain.Database/    # EF Core migrations project
      Migrations/
      Company.Domain.Database.csproj
  tests/                       # Generated test projects
    Company.Domain.Api.Tests/
      Features/
        Prospects/
          ProspectEndpointsTests.cs
        Activities/
          ActivityEndpointsTests.cs
      Integration/
        ApiIntegrationTests.cs
      Company.Domain.Api.Tests.csproj
  aspire/                      # .NET Aspire orchestration
    Company.Domain.AppHost/
      Program.cs
      Company.Domain.AppHost.csproj
    Company.Domain.ServiceDefaults/
      Extensions.cs
      Company.Domain.ServiceDefaults.csproj
```

## Feature Slice Implementation

### No MediatR Approach
Instead of MediatR commands/queries, use direct service injection:

```csharp
// ProspectService.cs
public class ProspectService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProspectService> _logger;
    
    public ProspectService(ApplicationDbContext context, ILogger<ProspectService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<ProspectDto> CreateAsync(CreateProspectRequest request, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("Prospect.Create");
        
        var prospect = new Prospect
        {
            // Map from request
        };
        
        _context.Prospects.Add(prospect);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Created prospect {ProspectId}", prospect.Id);
        
        return prospect.ToDto();
    }
}

// ProspectEndpoints.cs
public static class ProspectEndpoints
{
    public static void MapProspectEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/prospects")
            .WithTags("Prospects")
            .WithOpenApi();

        group.MapPost("/", CreateProspect);
        group.MapGet("/{id}", GetProspect);
        group.MapGet("/", ListProspects);
        group.MapPut("/{id}", UpdateProspect);
        group.MapDelete("/{id}", DeleteProspect);
    }

    private static async Task<IResult> CreateProspect(
        CreateProspectRequest request,
        ProspectService service,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);
        return Results.Created($"/api/prospects/{result.Id}", result);
    }
}
```

### OpenTelemetry Integration
```csharp
// Program.cs
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddConsoleExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddConsoleExporter());
```

### .NET Aspire Integration
```csharp
// AppHost/Program.cs
var builder = DistributedApplication.CreateBuilder(args);

var database = builder.AddPostgres("postgres")
    .WithDataVolume()
    .AddDatabase("companydb");

var api = builder.AddProject<Projects.Company_Domain_Api>("api")
    .WithReference(database);

builder.Build().Run();
```

## LLM Prompt Examples

### Entity Generation Prompt
```yaml
# entity-generation-prompt.yaml
name: "VSA Entity Generation"
description: "Generate C# entity classes following VSA patterns"
version: "1.0.0"

system_prompt: |
  You are an expert C# developer specializing in Vertical Slice Architecture (VSA).
  Generate clean, modern C# code following these principles:
  
  - Use .NET 9 features and conventions
  - Follow VSA patterns with self-contained features
  - Include proper validation attributes
  - Generate DTOs, request/response models
  - Use modern C# syntax (records, primary constructors, etc.)
  - Include XML documentation
  - Follow established naming conventions

user_prompt_template: |
  Generate a complete C# entity class and related models for the following Modeller definition:
  
  **Model Name:** {{ModelName}}
  **Summary:** {{ModelSummary}}
  **Remarks:** {{ModelRemarks}}
  
  **Attributes:**
  {{#each Attributes}}
  - {{Name}} ({{Type}}): {{Summary}}
    - Required: {{Required}}
    - Constraints: {{Constraints}}
  {{/each}}
  
  **Shared Types Available:**
  {{#each SharedTypes}}
  - {{Name}}: {{Description}}
  {{/each}}
  
  **Enums Available:**
  {{#each Enums}}
  - {{Name}}: {{Values}}
  {{/each}}
  
  Please generate:
  1. Entity class with proper EF Core configuration
  2. DTO class for API responses
  3. Create request class for POST operations
  4. Update request class for PUT operations
  5. Extension methods for mapping between types
  
  Ensure the code follows VSA principles and .NET 9 best practices.

output_constraints:
  - language: "csharp"
  - namespace_pattern: "{{ProjectName}}.Api.Features.{{FeatureName}}"
  - file_pattern: "{{FeatureName}}Models.cs"
  - include_usings: true
  - include_documentation: true
```

### Service Generation Prompt
```yaml
# service-generation-prompt.yaml
name: "VSA Service Generation"
description: "Generate C# service classes with CRUD operations and business logic"
version: "1.0.0"

system_prompt: |
  You are an expert C# developer creating service classes for Vertical Slice Architecture.
  Generate a service class that:
  
  - Uses Entity Framework Core for data access with async/await patterns
  - Implements proper error handling and validation
  - Includes OpenTelemetry tracing and structured logging
  - Follows .NET 9 conventions and performance best practices
  - Uses dependency injection correctly
  - Implements business rules from Modeller definitions
  - Provides comprehensive API operations (CRUD + business operations)
  - Uses FluentValidation for complex validation scenarios
  - Implements proper transaction handling

user_prompt_template: |
  Generate a comprehensive service class for the {{ModelName}} entity with the following operations:
  
  **Entity Information:**
  - Primary Key: {{PrimaryKeyName}} ({{PrimaryKeyType}})
  - Entity Name: {{ModelName}}
  - Table Name: {{TableName}}
  - Domain Context: {{DomainContext}}
  
  **Required Operations:**
  {{#each Operations}}
  - {{Name}}: {{Description}}
    {{#if BusinessRules}}Business Rules: {{BusinessRules}}{{/if}}
    {{#if ValidationRules}}Validation: {{ValidationRules}}{{/if}}
  {{/each}}
  
  **Business Rules:**
  {{#each BusinessRules}}
  - {{Rule}}
    {{#if Validation}}Validation: {{Validation}}{{/if}}
    {{#if ErrorMessage}}Error Message: {{ErrorMessage}}{{/if}}
  {{/each}}
  
  **Relationships:**
  {{#each Relationships}}
  - {{Name}}: {{Type}} ({{Cardinality}})
    {{#if CascadeRules}}Cascade: {{CascadeRules}}{{/if}}
  {{/each}}
  
  **Performance Considerations:**
  {{#each PerformanceHints}}
  - {{Hint}}
  {{/each}}
  
  Generate:
  1. Service interface with all operations
  2. Service implementation with:
     - Full CRUD operations
     - Business logic operations
     - Proper error handling
     - Activity tracing
     - Structured logging
     - Input validation
     - Transaction management
  3. Custom exceptions for domain-specific errors
  4. Result/Response patterns for operation outcomes
  
  Follow VSA principles, .NET 9 best practices, and comprehensive error handling.

output_constraints:
  - language: "csharp"
  - namespace_pattern: "{{ProjectName}}.Api.Features.{{FeatureName}}"
  - file_pattern: "{{FeatureName}}Service.cs"
  - include_interface: true
  - include_error_handling: true
  - include_logging: true
  - include_tracing: true
  - include_validation: true
```

system_prompt: |
  You are an expert C# developer creating service classes for Vertical Slice Architecture.
  Generate a service class that:
  
  - Uses Entity Framework Core for data access
  - Implements proper async/await patterns
  - Includes OpenTelemetry tracing
  - Uses structured logging
  - Follows .NET 9 conventions
  - Handles common exceptions gracefully
  - Uses dependency injection properly

user_prompt_template: |
  Generate a service class for the {{ModelName}} entity with the following operations:
  
  **Entity Information:**
  - Primary Key: {{PrimaryKeyName}} ({{PrimaryKeyType}})
  - Entity Name: {{ModelName}}
  - Table Name: {{TableName}}
  
  **Required Operations:**
  - CreateAsync: Create new {{ModelName}}
  - GetByIdAsync: Retrieve by {{PrimaryKeyName}}
  - ListAsync: Get paginated list with optional filtering
  - UpdateAsync: Update existing {{ModelName}}
  - DeleteAsync: Soft or hard delete
  
  **Business Rules:**
  {{#each BusinessRules}}
  - {{Description}}
  {{/each}}
  
  **Validation Rules:**
  {{#each ValidationRules}}
  - {{Field}}: {{Rule}}
  {{/each}}
  
  Include proper error handling, logging, and OpenTelemetry tracing.
  Use the ApplicationDbContext and follow dependency injection patterns.

output_constraints:
  - include_activity_source: true
  - use_structured_logging: true
  - include_cancellation_tokens: true
  - follow_async_patterns: true
```

### Code Modification Prompt
```yaml
# code-modification-prompt.yaml
name: "Intelligent Code Modification"
description: "Modify existing code while preserving structure and intent"
version: "1.0.0"

system_prompt: |
  You are an expert C# developer tasked with modifying existing code.
  Always preserve:
  - Existing class structure and organization
  - Developer-added custom code and comments
  - Established patterns and conventions
  - Business logic and validation rules
  
  When modifying:
  - Add new functionality without breaking existing code
  - Update method signatures carefully
  - Maintain backward compatibility where possible
  - Follow the existing code style and patterns
  - Preserve any custom modifications made by developers

user_prompt_template: |
  I need to modify the following C# code to accommodate changes in the Modeller definition:
  
  **Current Code:**
  ```csharp
  {{ExistingCode}}
  ```
  
  **Changes Required:**
  {{#each Changes}}
  - {{Type}}: {{Description}}
    {{#if NewAttributes}}
    New Attributes: {{NewAttributes}}
    {{/if}}
    {{#if RemovedAttributes}}
    Removed Attributes: {{RemovedAttributes}}
    {{/if}}
    {{#if ModifiedAttributes}}
    Modified Attributes: {{ModifiedAttributes}}
    {{/if}}
  {{/each}}
  
  **Updated Model Definition:**
  {{UpdatedModelDefinition}}
  
  Please modify the code to reflect these changes while:
  1. Preserving all existing custom code and comments
  2. Maintaining the current class structure
  3. Following the established patterns
  4. Adding proper migration comments for significant changes
  5. Ensuring backward compatibility where possible

output_constraints:
  - preserve_custom_code: true
  - maintain_structure: true
  - add_migration_comments: true
  - include_change_summary: true
```

## MCP Tools Implementation

### LLM-Assisted Generation Tools
```csharp
[McpServerToolType]
public class LLMCodeGenerationTools(ModelDiscoveryService discoveryService, LLMPromptManager promptManager, ILLMService llmService)
{
    [McpServerTool(Title = "Generate VSA Web API Project")]
    public async Task<string> GenerateVSAWebAPI(
        [Description("Path to domain models")] string domainPath,
        [Description("Project name (e.g., 'Company.Domain')")] string projectName,
        [Description("Output directory (relative to models folder)")] string outputPath = "../src",
        [Description("LLM provider to use (openai, anthropic, local)")] string llmProvider = "openai")
    {
        // 1. Discover domain models
        // 2. Load generation prompt templates
        // 3. For each model, generate LLM prompts
        // 4. Call LLM service to generate code
        // 5. Validate generated code compiles
        // 6. Write files to output directory
        // 7. Return generation summary
    }

    [McpServerTool(Title = "Generate Feature Slice")]
    public async Task<string> GenerateFeatureSlice(
        [Description("Path to specific model file")] string modelPath,
        [Description("Existing project path")] string projectPath,
        [Description("LLM provider to use")] string llmProvider = "openai")
    {
        // 1. Parse single model definition
        // 2. Load appropriate prompt templates
        // 3. Generate contextual LLM prompt
        // 4. Call LLM to generate feature code
        // 5. Integrate with existing project structure
        // 6. Return integration summary
    }

    [McpServerTool(Title = "Modify Feature Code")]
    public async Task<string> ModifyFeatureCode(
        [Description("Path to model file with changes")] string modelPath,
        [Description("Path to existing feature code")] string codePath,
        [Description("Description of changes required")] string changeDescription,
        [Description("LLM provider to use")] string llmProvider = "openai")
    {
        // 1. Parse updated model definition
        // 2. Read existing code files
        // 3. Analyze differences and required changes
        // 4. Load modification prompt templates
        // 5. Generate contextual modification prompt
        // 6. Call LLM to modify existing code
        // 7. Preserve developer customizations
        // 8. Return modification summary
    }

    [McpServerTool(Title = "Update Project from Domain")]
    public async Task<string> UpdateProjectFromDomain(
        [Description("Path to domain models")] string domainPath,
        [Description("Path to existing project")] string projectPath,
        [Description("LLM provider to use")] string llmProvider = "openai")
    {
        // 1. Discover all model changes
        // 2. Analyze existing codebase
        // 3. Determine required modifications
        // 4. Generate appropriate prompts for each change
        // 5. Apply LLM-guided modifications
        // 6. Validate all changes compile
        // 7. Return comprehensive update summary
    }

    [McpServerTool(Title = "Validate Generated Code")]
    public async Task<string> ValidateGeneratedCode(
        [Description("Path to generated project")] string projectPath,
        [Description("Run tests after compilation")] bool runTests = true)
    {
        // 1. Compile the project using Roslyn
        // 2. Report compilation errors/warnings
        // 3. Optionally run generated tests
        // 4. Return validation results
    }

    [McpServerTool(Title = "Update Prompt Templates")]
    public async Task<string> UpdatePromptTemplates(
        [Description("Prompt package name")] string packageName,
        [Description("Target version (optional)")] string? version = null)
    {
        // 1. Download prompt template package
        // 2. Validate prompt template format
        // 3. Update local prompt template cache
        // 4. Return update summary
    }
}
```

### LLM Integration Architecture
```csharp
public interface ILLMService
{
    Task<string> GenerateCodeAsync(string prompt, LLMOptions options, CancellationToken cancellationToken = default);
    Task<string> ModifyCodeAsync(string existingCode, string modificationPrompt, LLMOptions options, CancellationToken cancellationToken = default);
}

public class LLMOptions
{
    public string Provider { get; set; } = "openai"; // openai, anthropic, local
    public string Model { get; set; } = "gpt-4";
    public double Temperature { get; set; } = 0.1; // Low for code generation
    public int MaxTokens { get; set; } = 4000;
    public List<string> StopSequences { get; set; } = new();
}

public class LLMPromptManager
{
    public async Task<string> BuildGenerationPromptAsync(string promptTemplateName, object modelData)
    {
        // 1. Load prompt template
        // 2. Substitute model data using template engine
        // 3. Add context about existing codebase
        // 4. Return complete prompt
    }

    public async Task<string> BuildModificationPromptAsync(string existingCode, object changeData)
    {
        // 1. Load modification prompt template
        // 2. Analyze existing code structure
        // 3. Build contextual modification prompt
        // 4. Include preservation instructions
        // 5. Return complete prompt
    }
}
```

### Intelligent Code Modification Flow
```
1. Model Change Detection
   ├── Parse updated Modeller definition
   ├── Compare with previous version
   └── Identify specific changes (added/removed/modified attributes)

2. Existing Code Analysis
   ├── Read current code files
   ├── Parse AST to understand structure
   ├── Identify custom developer modifications
   └── Map code elements to model attributes

3. Change Strategy Planning
   ├── Determine modification approach
   ├── Identify breaking vs non-breaking changes
   ├── Plan preservation of custom code
   └── Select appropriate prompt templates

4. LLM-Guided Modification
   ├── Generate contextual modification prompt
   ├── Include preservation instructions
   ├── Call LLM with existing code + requirements
   └── Receive modified code maintaining structure

5. Code Integration & Validation
   ├── Apply LLM modifications
   ├── Preserve developer customizations
   ├── Validate code compiles
   ├── Run tests to ensure functionality
   └── Report modification results
```

## Developer Experience & LLM Integration

### Developer Control and Customization
The LLM-driven approach maintains developer control through:

**1. Generation Review Process**
```csharp
public class CodeGenerationWorkflow
{
    public async Task<GenerationResult> GenerateWithReviewAsync(
        GenerationRequest request,
        bool requireManualReview = true)
    {
        // Generate initial code
        var generatedCode = await _llmService.GenerateCodeAsync(request.Prompt);
        
        if (requireManualReview)
        {
            // Present code to developer for review
            var reviewResult = await PresentForReviewAsync(generatedCode, request);
            
            if (reviewResult.RequiresChanges)
            {
                // Apply developer feedback to generation
                var refinedPrompt = IncorporateFeedback(request.Prompt, reviewResult.Feedback);
                generatedCode = await _llmService.GenerateCodeAsync(refinedPrompt);
            }
        }
        
        return new GenerationResult(generatedCode, reviewResult);
    }
}
```

**2. Custom Prompt Templates**
Developers can override and customize prompt templates:
```
.modeller/
  custom-prompts/
    my-entity-generation.yaml    # Custom entity generation logic
    my-service-patterns.yaml     # Organization-specific patterns
    my-testing-strategy.yaml     # Custom testing approaches
```

**3. Code Pattern Learning**
The system learns from developer modifications:
```csharp
public class PatternLearningService
{
    public async Task LearnFromModificationAsync(
        string originalCode,
        string modifiedCode,
        string modificationReason)
    {
        var pattern = ExtractPattern(originalCode, modifiedCode);
        
        // Store pattern for future generations
        await _patternStore.SavePatternAsync(pattern, modificationReason);
        
        // Update prompt templates to include learned patterns
        await UpdatePromptTemplatesAsync(pattern);
    }
}
```

### Integration with Development Workflow

**1. IDE Integration**
- VS Code extension for direct model-to-code generation
- IntelliSense support for Modeller definitions
- Real-time validation and preview of generated code
- Integrated diff view for code modifications

**2. CLI Tools**
```powershell
# Generate complete project from models
modeller generate --project WebAPI --output ./src --models ./models/JJs

# Update existing code based on model changes
modeller update --project ./src --models ./models/JJs --diff

# Validate generated code
modeller validate --project ./src --run-tests

# Learn from custom modifications
modeller learn --before ./backup --after ./current --reason "Added custom validation"
```

**3. CI/CD Integration**
```yaml
# .github/workflows/model-sync.yml
name: Model Sync
on:
  push:
    paths: ['models/**/*.yaml']

jobs:
  generate-code:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Generate Code Changes
        run: |
          modeller update --project ./src --models ./models --auto-approve
          
      - name: Validate Changes
        run: |
          dotnet build
          dotnet test
          
      - name: Create Pull Request
        if: changes-detected
        uses: peter-evans/create-pull-request@v5
        with:
          title: "Auto-generated code updates from model changes"
          body: "Generated by Modeller MCP based on model definition changes"
```

### Quality Assurance and Validation

**1. Multi-Stage Validation Pipeline**
```csharp
public class CodeValidationPipeline
{
    public async Task<ValidationResult> ValidateGeneratedCodeAsync(
        GeneratedCode code,
        ValidationContext context)
    {
        var results = new List<ValidationResult>();
        
        // Stage 1: Syntax and compilation validation
        results.Add(await ValidateSyntaxAsync(code));
        
        // Stage 2: Architecture compliance validation
        results.Add(await ValidateArchitectureAsync(code, context.ArchitectureRules));
        
        // Stage 3: Business rule validation
        results.Add(await ValidateBusinessRulesAsync(code, context.BusinessRules));
        
        // Stage 4: Performance validation
        results.Add(await ValidatePerformanceAsync(code, context.PerformanceRules));
        
        // Stage 5: Security validation
        results.Add(await ValidateSecurityAsync(code, context.SecurityRules));
        
        // Stage 6: Test coverage validation
        results.Add(await ValidateTestCoverageAsync(code, context.TestRequirements));
        
        return CombineValidationResults(results);
    }
}
```

**2. Automated Testing of Generated Code**
```csharp
public class GeneratedCodeTestRunner
{
    public async Task<TestResults> RunGeneratedTestsAsync(string projectPath)
    {
        // Compile the generated project
        var compilation = await CompileProjectAsync(projectPath);
        if (!compilation.Success)
            return TestResults.CompilationFailed(compilation.Errors);
        
        // Run generated unit tests
        var unitTestResults = await RunTestsAsync(projectPath, "unit");
        
        // Run generated integration tests
        var integrationTestResults = await RunTestsAsync(projectPath, "integration");
        
        // Run architecture tests
        var architectureTestResults = await RunArchitectureTestsAsync(projectPath);
        
        return new TestResults
        {
            UnitTests = unitTestResults,
            IntegrationTests = integrationTestResults,
            ArchitectureTests = architectureTestResults,
            OverallSuccess = AllTestsPassed(unitTestResults, integrationTestResults, architectureTestResults)
        };
    }
}
```

### Enhanced Prompt Engineering Strategy

### Multi-Stage Prompt Refinement
Instead of single-shot prompts, use a multi-stage approach:

```yaml
# multi-stage-generation-prompt.yaml
stages:
  - stage: "analysis"
    description: "Understand the domain model and requirements"
    prompt_template: |
      Analyze this Modeller definition and extract key architectural decisions:
      {{ModelDefinition}}
      
      Identify:
      1. Primary entity responsibilities
      2. Required relationships and their cardinality
      3. Business rules and constraints
      4. Performance considerations
      5. Security requirements
      
  - stage: "design"
    description: "Create architectural design"
    prompt_template: |
      Based on the analysis: {{AnalysisResult}}
      
      Design the C# implementation architecture:
      1. Class structure and inheritance hierarchy
      2. Interface design for testability
      3. Dependency injection strategy
      4. Error handling approach
      5. Performance optimization points
      
  - stage: "implementation"
    description: "Generate the actual code"
    prompt_template: |
      Implementation design: {{DesignResult}}
      
      Generate production-ready C# code following VSA principles.
      Focus on clean, maintainable, and testable code.
```

### Prompt Validation & Testing
```csharp
public class PromptValidationService
{
    public async Task<PromptQualityResult> ValidatePromptQualityAsync(
        string prompt, 
        ModelDefinition testModel)
    {
        var results = new List<ValidationResult>();
        
        // Test prompt with multiple LLM providers
        var providers = new[] { "gpt-4", "claude-3", "local-llm" };
        
        foreach (var provider in providers)
        {
            var generated = await GenerateWithProviderAsync(prompt, testModel, provider);
            results.Add(await ValidateGeneratedCodeAsync(generated));
        }
        
        return AnalyzeConsistency(results);
    }
}
```

## Advanced Context Management

### Codebase Context Engine
```csharp
public class CodebaseContextEngine
{
    public async Task<CodebaseContext> BuildContextAsync(string projectPath)
    {
        return new CodebaseContext
        {
            // Architectural patterns detected
            Architecture = await AnalyzeArchitecturalPatternsAsync(projectPath),
            
            // Existing code patterns and conventions
            Conventions = await ExtractCodingConventionsAsync(projectPath),
            
            // Dependencies and their usage patterns
            Dependencies = await AnalyzeDependencyUsageAsync(projectPath),
            
            // Performance hotspots and patterns
            Performance = await AnalyzePerformancePatternsAsync(projectPath),
            
            // Testing strategies and patterns
            Testing = await AnalyzeTestingPatternsAsync(projectPath),
            
            // Custom business logic patterns
            BusinessLogic = await ExtractBusinessLogicPatternsAsync(projectPath)
        };
    }
}

public class CodebaseContext
{
    public ArchitecturalPatterns Architecture { get; set; }
    public CodingConventions Conventions { get; set; }
    public DependencyPatterns Dependencies { get; set; }
    public PerformancePatterns Performance { get; set; }
    public TestingPatterns Testing { get; set; }
    public BusinessLogicPatterns BusinessLogic { get; set; }
    
    // Convert to prompt context
    public string ToPromptContext()
    {
        return $"""
        **Existing Codebase Patterns:**
        - Architecture: {Architecture.Summary}
        - Naming Conventions: {Conventions.Summary}
        - Error Handling: {Architecture.ErrorHandlingStrategy}
        - Logging Patterns: {Architecture.LoggingStrategy}
        - Validation Approach: {BusinessLogic.ValidationStrategy}
        - Testing Strategy: {Testing.Strategy}
        
        **Dependencies in Use:**
        {string.Join("\n", Dependencies.ActiveLibraries.Select(d => $"- {d.Name}: {d.UsagePattern}"))}
        
        **Performance Considerations:**
        {string.Join("\n", Performance.OptimizationPatterns.Select(p => $"- {p}"))}
        """;
    }
}
```

### Intelligent Code Analysis
```csharp
public class IntelligentCodeAnalyzer
{
    public async Task<CodeAnalysisResult> AnalyzeForModificationAsync(
        string existingCode, 
        ModelDefinition newModel)
    {
        // Use AST parsing to understand code structure
        var syntaxTree = CSharpSyntaxTree.ParseText(existingCode);
        var root = syntaxTree.GetRoot();
        
        return new CodeAnalysisResult
        {
            // Identify custom developer code vs generated code
            CustomCodeSections = IdentifyCustomCode(root),
            
            // Map code elements to model attributes
            AttributeMappings = MapCodeToModelAttributes(root, newModel),
            
            // Identify breaking vs non-breaking changes
            ChangeImpact = AnalyzeChangeImpact(root, newModel),
            
            // Suggest modification strategy
            ModificationStrategy = DetermineModificationStrategy(root, newModel),
            
            // Identify dependencies that might be affected
            AffectedDependencies = AnalyzeAffectedDependencies(root, newModel)
        };
    }
}
```

## Key Design Improvements Summary

### 1. **Robustness Through Multi-Stage Generation**
- **Analysis → Design → Implementation** pipeline ensures better code quality
- **Prompt validation** across multiple LLM providers for consistency
- **Context-rich prompts** that understand existing codebase patterns

### 2. **Intelligent Context Management**
- **Codebase analysis** extracts architectural patterns and conventions
- **Code understanding** maps existing code to model definitions
- **Impact analysis** determines modification strategies

### 3. **Continuous Improvement Loop**
- **Quality metrics** track generation success and developer satisfaction
- **Adaptive prompt evolution** improves templates based on usage patterns
- **Developer feedback integration** learns from manual modifications

### 4. **Safety & Reliability First**
- **Security safety guards** prevent dangerous code generation
- **Rollback system** provides recovery from failed generations
- **Monitoring & alerting** ensures system health

### 5. **Performance & Scalability**
- **LLM request optimization** with caching and rate limiting
- **Parallel processing** for multiple feature generation
- **Resource management** prevents system overload

## Pre-Implementation Checklist

### Technical Readiness
- [ ] **Prompt Template Design**: Create comprehensive prompt templates with validation
- [ ] **LLM Provider Integration**: Implement provider abstraction with fallback mechanisms
- [ ] **Code Analysis Engine**: Build AST-based code understanding capabilities
- [ ] **Safety Framework**: Implement security and reliability safeguards
- [ ] **Monitoring Infrastructure**: Set up metrics collection and alerting

### Process Readiness
- [ ] **Developer Workflow**: Define clear generation and modification workflows
- [ ] **Quality Gates**: Establish validation criteria and success metrics
- [ ] **Feedback Mechanisms**: Create channels for developer input and learning
- [ ] **Documentation**: Provide comprehensive usage and customization guides
- [ ] **Testing Strategy**: Plan for validating generated code quality

### Business Readiness
- [ ] **Cost Management**: Implement LLM usage monitoring and budgeting
- [ ] **Risk Mitigation**: Establish rollback procedures and safety protocols
- [ ] **Team Training**: Prepare developers for LLM-assisted development
- [ ] **Governance**: Define approval processes for prompt template changes
- [ ] **Compliance**: Ensure generated code meets security and regulatory requirements

## Recommended Implementation Sequence

### Phase 1: Foundation (Weeks 1-2)
1. **Basic LLM Integration**: Simple prompt-to-code generation
2. **Code Analysis**: AST parsing and pattern recognition
3. **Safety Guards**: Basic security and reliability checks
4. **Prompt Management**: Template loading and versioning

### Phase 2: Intelligence (Weeks 3-4)
1. **Context Engine**: Codebase pattern extraction
2. **Multi-Stage Generation**: Analysis → Design → Implementation
3. **Modification Intelligence**: Code understanding and preservation
4. **Quality Tracking**: Metrics collection and basic feedback

### Phase 3: Optimization (Weeks 5-6)
1. **Performance Enhancement**: Caching, parallel processing
2. **Adaptive Learning**: Prompt evolution based on feedback
3. **Advanced Safety**: Comprehensive security scanning
4. **Monitoring & Alerting**: Full observability implementation

### Phase 4: Ecosystem (Weeks 7-8)
1. **Developer Tools**: VS Code extension, CLI tools
2. **CI/CD Integration**: Automated generation workflows
3. **Documentation & Training**: Comprehensive guides
4. **Community Features**: Template sharing and collaboration

This enhanced design provides a robust foundation for LLM-driven code generation that maintains developer control while providing intelligent assistance. The focus on safety, quality, and continuous improvement ensures the system can evolve and improve over time.
