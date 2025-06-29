using Microsoft.Extensions.Logging;

using Modeller.Mcp.Shared.CodeGeneration.Prompts;
using Modeller.Mcp.Shared.CodeGeneration.Prompts.VSA;
using Modeller.Mcp.Shared.CodeGeneration.Security;
using Modeller.Mcp.Shared.Services;

namespace Modeller.Mcp.Shared.CodeGeneration;

/// <summary>
/// Service for generating Minimal API projects from SDK and domain models
/// </summary>
public interface IApiGenerationService
{
    /// <summary>
    /// Generate a complete Minimal API project from SDK and domain models
    /// </summary>
    Task<ApiGenerationResult> GenerateAPIAsync(ApiGenerationRequest request);
    
    /// <summary>
    /// Generate the prompt that would be used for API creation
    /// </summary>
    Task<string> GeneratePromptAsync(ApiGenerationRequest request);
}

/// <summary>
/// Request for API generation
/// </summary>
public record ApiGenerationRequest
{
    /// <summary>
    /// Path to the generated SDK (e.g., playschool/generated-sdk)
    /// </summary>
    public required string SdkPath { get; init; }
    
    /// <summary>
    /// Path to the domain models (e.g., playschool/models/JJs/PotentialSales)
    /// </summary>
    public required string DomainPath { get; init; }
    
    /// <summary>
    /// Name of the API project (e.g., "JJs.PotentialSales.Api")
    /// </summary>
    public required string ProjectName { get; init; }
    
    /// <summary>
    /// Target namespace for the API (e.g., "JJs.PotentialSales.Api")
    /// </summary>
    public required string Namespace { get; init; }
    
    /// <summary>
    /// Output directory for the API project
    /// </summary>
    public required string OutputPath { get; init; }
}

/// <summary>
/// Result of API generation
/// </summary>
public record ApiGenerationResult
{
    /// <summary>
    /// Whether the generation was successful
    /// </summary>
    public bool IsSuccess { get; init; }
    
    /// <summary>
    /// Generated prompt used for API creation
    /// </summary>
    public string? GeneratedPrompt { get; init; }
    
    /// <summary>
    /// Path where files were generated
    /// </summary>
    public string? OutputPath { get; init; }
    
    /// <summary>
    /// List of generated files
    /// </summary>
    public List<string> GeneratedFiles { get; init; } = new();
    
    /// <summary>
    /// Error message if generation failed
    /// </summary>
    public string? ErrorMessage { get; init; }
    
    public static ApiGenerationResult Success(string prompt, string outputPath, List<string> files) => new()
    {
        IsSuccess = true,
        GeneratedPrompt = prompt,
        OutputPath = outputPath,
        GeneratedFiles = files
    };
    
    public static ApiGenerationResult Failure(string errorMessage) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage
    };
}

/// <summary>
/// Implementation of API generation service
/// </summary>
public class ApiGenerationService(
    IVsaPromptService vsaPromptService,
    ModelDiscoveryService modelDiscoveryService,
    ISecureLlmService secureLlmService,
    ILogger<ApiGenerationService> logger) : IApiGenerationService
{
    public async Task<ApiGenerationResult> GenerateAPIAsync(ApiGenerationRequest request)
    {
        try
        {
            logger.LogInformation("Starting Minimal API generation for project '{ProjectName}' from SDK '{SdkPath}'", 
                request.ProjectName, request.SdkPath);

            // 1. Validate SDK path exists
            if (!Directory.Exists(request.SdkPath))
                return ApiGenerationResult.Failure($"SDK path does not exist: {request.SdkPath}");

            // 2. Validate and discover domain models
            var discoveryResult = modelDiscoveryService.DiscoverModels(request.DomainPath);
            if (discoveryResult.Errors.Any())
                return ApiGenerationResult.Failure($"Failed to discover models: {string.Join("; ", discoveryResult.Errors)}");

            // 3. Generate the API prompt
            var prompt = await GeneratePromptAsync(request);

            // 4. Use secure LLM service to generate the actual code
            var secureRequest = new SecureLlmRequest
            {
                RawPrompt = prompt,
                ModelId = "gpt-4",
                PromptType = "api_generation",
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
                    ["sdk_path"] = request.SdkPath,
                    ["domain_path"] = request.DomainPath
                }
            };

            var codeGenerationResult = await secureLlmService.GenerateSecureCodeAsync(secureRequest);

            if (!codeGenerationResult.IsSuccess)
                return ApiGenerationResult.Failure($"Code generation failed: {codeGenerationResult.ErrorMessage}");

            // 5. Save generated files to output directory
            Directory.CreateDirectory(request.OutputPath);
            var generatedFiles = new List<string>();
            
            // Save the prompt and generated code for reference
            var promptPath = Path.Combine(request.OutputPath, "GeneratedPrompt.md");
            await File.WriteAllTextAsync(promptPath, prompt);
            generatedFiles.Add(promptPath);
            
            var codePath = Path.Combine(request.OutputPath, "GeneratedCode.md");
            await File.WriteAllTextAsync(codePath, codeGenerationResult.Content ?? "");
            generatedFiles.Add(codePath);

            logger.LogInformation("Minimal API generation completed successfully. Generated {Count} files", generatedFiles.Count);

            return ApiGenerationResult.Success(prompt, request.OutputPath, generatedFiles);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during Minimal API generation for project '{ProjectName}'", request.ProjectName);
            return ApiGenerationResult.Failure($"API generation failed: {ex.Message}");
        }
    }

    public async Task<string> GeneratePromptAsync(ApiGenerationRequest request)
    {
        try
        {
            // 1. Discover domain models
            var discoveryResult = modelDiscoveryService.DiscoverModels(request.DomainPath);
            if (discoveryResult.Errors.Any())
                throw new InvalidOperationException($"Failed to discover models: {string.Join(", ", discoveryResult.Errors)}");

            // 2. Read SDK structure to understand available components
            var sdkFiles = Directory.GetFiles(request.SdkPath, "*.cs", SearchOption.AllDirectories);
            var sdkStructure = string.Join("\n", sdkFiles.Select(f => $"- {Path.GetRelativePath(request.SdkPath, f)}"));

            // 3. Read model definitions
            var modelFiles = Directory.GetFiles(request.DomainPath, "*.yaml", SearchOption.AllDirectories);
            var modelDefinitions = new List<string>();
            
            foreach (var file in modelFiles)
            {
                var content = await File.ReadAllTextAsync(file);
                var fileName = Path.GetFileName(file);
                modelDefinitions.Add($"## {fileName}\n```yaml\n{content}\n```");
            }

            // 4. Generate comprehensive API prompt
            var prompt = @"# Generate Minimal API Project from SDK

## Project Requirements
- **Project Name:** " + request.ProjectName + @"
- **Namespace:** " + request.Namespace + @"
- **SDK Reference:** " + request.SdkPath + @"
- **Output Path:** " + request.OutputPath + @"

## SDK Structure Available
The following SDK components are available for use:
```
" + sdkStructure + @"
```

## Domain Model Definitions
" + string.Join("\n\n", modelDefinitions) + @"

## Requirements

### 1. Project Structure
Create a complete .NET Minimal API project with the following structure:
```
" + request.ProjectName + @"/
├── " + request.ProjectName + @".csproj
├── Program.cs
├── GlobalUsings.cs
├── appsettings.json
├── appsettings.Development.json
├── Data/
│   ├── " + request.ProjectName.Split('.').Last() + @"DbContext.cs
│   └── SeedData.cs
├── Extensions/
│   └── ServiceCollectionExtensions.cs
├── Services/
│   ├── [Entity]Service.cs (for each domain entity)
│   └── BusinessServices.cs
├── Endpoints/
│   └── [Entity]Endpoints.cs (for each domain entity)
└── Middleware/
    ├── ErrorHandlingMiddleware.cs
    └── ValidationMiddleware.cs
```

### 2. Project File Requirements
- Target .NET 9.0
- Reference the generated SDK project
- Include Entity Framework Core In-Memory provider
- Include Swagger/OpenAPI
- Include health checks
- Include logging

### 3. Entity Framework Setup
- Create DbContext with DbSets for all domain entities
- Use in-memory database for development
- Include proper configuration for all entities
- Create seed data with realistic test data

### 4. Service Layer
- Create service classes for each domain entity
- Implement business logic methods from the behavior models
- Use the SDK validators and result patterns
- Handle validation and error scenarios

### 5. Minimal API Endpoints
For each domain entity, create endpoints for:
- GET /[entities] - List all with filtering
- GET /[entities]/{id} - Get by ID
- POST /[entities] - Create new
- PUT /[entities]/{id} - Update existing
- DELETE /[entities]/{id} - Delete
- Additional business action endpoints based on behavior models

### 6. Validation and Error Handling
- Use SDK validators for input validation
- Implement proper error responses
- Use the SDK result patterns consistently
- Include proper HTTP status codes

### 7. Configuration
- Configure services properly
- Set up dependency injection
- Configure Entity Framework
- Configure Swagger/OpenAPI
- Configure logging

### 8. Business Logic Integration
Follow the behavior models defined in the YAML files to implement:
- Entity creation and updates
- Business validation rules
- State transitions
- Relationship management

## Implementation Guidelines

### Use VSA Patterns
- Follow Vertical Slice Architecture principles
- Organize by feature, not by technical layer
- Keep related code together
- Use the SDK models and validators

### Follow Domain Models
- Implement all entities defined in the YAML files
- Respect the relationships and constraints
- Use the business behaviors as API operations
- Maintain data integrity

### Modern API Practices
- Use minimal APIs instead of controllers
- Implement proper HTTP status codes
- Use async/await throughout
- Include comprehensive error handling
- Add input validation
- Include API documentation

### Code Quality
- Use proper naming conventions
- Include XML documentation
- Handle exceptions gracefully
- Use dependency injection
- Follow SOLID principles

## Expected Output
Generate all the necessary files for a complete, production-ready Minimal API project that:
1. References and uses the generated SDK
2. Implements all domain entities and behaviors
3. Provides a complete REST API
4. Includes proper validation and error handling
5. Can be built and run immediately
6. Includes comprehensive API documentation

The generated project should be a perfect integration showcase for the SDK, demonstrating how to build APIs using the domain models and VSA patterns.";

            return prompt;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating API prompt for project '{ProjectName}'", request.ProjectName);
            throw;
        }
    }
}
