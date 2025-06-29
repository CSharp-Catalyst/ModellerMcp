# SDK & API Generation Guide

## Overview

Modeller MCP provides comprehensive code generation capabilities that transform domain models into production-ready .NET SDKs and Minimal API projects. This guide covers the complete workflow from domain modeling to running applications.

## Generation Workflow

### 1. Domain Modeling
Start by defining your domain models using YAML with proper BDD structure:

```yaml
# models/Business/CustomerManagement/Customer.Type.yaml
domain: CustomerManagement
boundedContext: Business
summary: "Customer entity representing a business or individual customer"
category: Type

Customer:
  id:
    type: string
    description: "Unique identifier for the customer"
    constraints:
      - required: true
      - maxLength: 50
  
  name:
    type: string
    description: "Customer display name"
    constraints:
      - required: true
      - maxLength: 200
  
  email:
    type: string
    description: "Primary email contact"
    constraints:
      - required: true
      - email: true
      - maxLength: 255
```

### 2. SDK Generation

Generate a complete .NET SDK from your domain models:

#### Using MCP Tools (Recommended)
```text
@Modeller GenerateSDK 
  --domainPath "models/Business/CustomerManagement" 
  --featureName "CustomerManagement" 
  --namespaceName "Business.CustomerManagement.Sdk" 
  --outputPath "./generated-sdk"
```

#### What Gets Generated

The SDK generation creates a complete .NET library with:

- **Models**: DTOs and entity classes from Type definitions
- **Validators**: FluentValidation validators for all models
- **Enums**: Strongly-typed enumerations from enum definitions
- **Result Patterns**: Standardized API result types
- **Extensions**: Utility methods and validation extensions
- **Project File**: Properly configured .csproj with dependencies

#### Generated Structure
```
generated-sdk/
├── CustomerManagement.Sdk.csproj
├── GlobalUsings.cs
├── Common/
│   ├── ApiResult.cs
│   └── ValidationExtensions.cs
├── Customer/
│   ├── CustomerModels.cs
│   ├── CustomerValidators.cs
│   └── CustomerExtensions.cs
├── Enums/
│   └── CustomerEnums.cs
└── README.md
```

### 3. API Generation

Generate a Minimal API project that integrates with your SDK:

#### Using MCP Tools
```text
@Modeller GenerateMinimalAPI 
  --sdkPath "./generated-sdk" 
  --domainPath "models/Business/CustomerManagement" 
  --projectName "CustomerManagement.Api" 
  --namespaceName "CustomerManagement.Api" 
  --outputPath "./generated-api"
```

#### What Gets Generated

The API generation creates a complete .NET Minimal API project with:

- **Project Configuration**: Properly configured .csproj with SDK reference
- **Entity Framework**: DbContext with in-memory database for development
- **Service Layer**: Business services implementing domain behaviors
- **API Endpoints**: RESTful endpoints for all domain entities
- **Validation**: Integration with SDK validators
- **Error Handling**: Comprehensive error responses
- **Swagger/OpenAPI**: Automatic API documentation
- **Dependency Injection**: Proper service registration

#### Generated Structure
```
generated-api/
├── CustomerManagement.Api.csproj
├── Program.cs
├── GlobalUsings.cs
├── appsettings.json
├── appsettings.Development.json
├── Data/
│   ├── CustomerManagementDbContext.cs
│   └── SeedData.cs
├── Extensions/
│   └── ServiceCollectionExtensions.cs
├── Services/
│   ├── CustomerService.cs
│   └── BusinessServices.cs
├── Endpoints/
│   └── CustomerEndpoints.cs
└── Middleware/
    ├── ErrorHandlingMiddleware.cs
    └── ValidationMiddleware.cs
```

## Features & Capabilities

### SDK Features

#### Models
- **Type Safety**: Strongly-typed C# classes from YAML definitions
- **Validation**: Built-in FluentValidation rules from constraints
- **Documentation**: XML documentation from YAML descriptions
- **Nullability**: Proper nullable reference type annotations

#### Validators
```csharp
public class CustomerValidator : AbstractValidator<Customer>
{
    public CustomerValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Customer ID is required")
            .MaximumLength(50).WithMessage("Customer ID cannot exceed 50 characters");
            
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters");
    }
}
```

#### Result Patterns
```csharp
public class ApiResult<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public string? ErrorMessage { get; init; }
    public List<ValidationError> ValidationErrors { get; init; } = new();
    
    public static ApiResult<T> Success(T data) => new() { IsSuccess = true, Data = data };
    public static ApiResult<T> Failure(string error) => new() { IsSuccess = false, ErrorMessage = error };
}
```

### API Features

#### Endpoints
Generated APIs include standard REST endpoints for each entity:

- `GET /customers` - List all customers with filtering
- `GET /customers/{id}` - Get customer by ID
- `POST /customers` - Create new customer
- `PUT /customers/{id}` - Update customer
- `DELETE /customers/{id}` - Delete customer

#### Business Logic Integration
```csharp
app.MapPost("/customers/{id}/activate", async (
    string id, 
    CustomerService customerService) =>
{
    var result = await customerService.ActivateCustomerAsync(id);
    return result.IsSuccess 
        ? Results.Ok(result.Data) 
        : Results.BadRequest(result.ErrorMessage);
});
```

#### Entity Framework Integration
```csharp
public class CustomerManagementDbContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
        });
    }
}
```

## Best Practices

### Domain Modeling
- Use clear, descriptive names for entities and attributes
- Include comprehensive validation constraints
- Document business rules in behavior YAML files
- Group related entities in logical domains

### SDK Usage
- Reference the generated SDK from your API project
- Use the provided validators in your business logic
- Leverage result patterns for consistent error handling
- Extend generated classes when needed (use partial classes)

### API Development
- Use the generated service layer for business logic
- Implement additional business operations as needed
- Configure Entity Framework for your target database
- Add authentication and authorization as required

## Integration Examples

### Using Generated SDK in Custom Code
```csharp
// In your custom business service
public class CustomerBusinessService
{
    private readonly CustomerValidator _validator;
    
    public async Task<ApiResult<Customer>> CreateCustomerAsync(Customer customer)
    {
        var validationResult = await _validator.ValidateAsync(customer);
        if (!validationResult.IsValid)
        {
            return ApiResult<Customer>.ValidationFailure(validationResult.Errors);
        }
        
        // Business logic here
        return ApiResult<Customer>.Success(customer);
    }
}
```

### Extending Generated APIs
```csharp
// Add custom endpoints to generated APIs
app.MapPost("/customers/import", async (
    IFormFile file,
    CustomerService customerService) =>
{
    // Custom import logic
    var result = await customerService.ImportCustomersAsync(file);
    return Results.Ok(result);
});
```

## Development Workflow

1. **Model Definition**: Create or update YAML domain models
2. **Validation**: Validate models using MCP tools
3. **SDK Generation**: Generate or regenerate SDK
4. **API Generation**: Generate or update API project
5. **Customization**: Add custom business logic and endpoints
6. **Testing**: Build and test the complete solution
7. **Deployment**: Deploy SDK as NuGet package and API as service

## Troubleshooting

### Common Issues

#### Build Errors
- Ensure .NET SDK version compatibility
- Check that all required NuGet packages are restored
- Verify that SDK project builds before generating API

#### Validation Errors
- Validate YAML models before generation
- Check that all required fields are properly defined
- Ensure enum values are correctly specified

#### Runtime Issues
- Configure Entity Framework connection strings
- Ensure all dependencies are registered in DI container
- Check that API endpoints are properly mapped

### Support
For issues or questions:
1. Validate your models first using `@Modeller ValidateModel`
2. Check the generated code for compilation errors
3. Review the audit logs for generation details
4. Consult the documentation for specific patterns

---

*This guide covers the core SDK and API generation features. For advanced scenarios and customization options, refer to the full documentation.*
