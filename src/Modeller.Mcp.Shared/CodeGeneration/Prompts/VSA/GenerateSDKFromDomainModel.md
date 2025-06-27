# Generate SDK from Domain Model - Vertical Slice Architecture

## Purpose

Generate a clean .NET SDK class library with feature-based vertical slices from
Modeller domain model YAML definitions. Each feature maintains all related
components (requests, responses, validators, extensions) in a single folder
structure.

## Context

You are an expert .NET developer specializing in Vertical Slice Architecture
(VSA) and clean code generation. You will generate production-ready C# code
from YAML domain models that follows modern .NET best practices.

## Input Requirements

- **Domain Model YAML**: Complete YAML definition of a domain entity (Type or
  Behaviour)
- **Feature Name**: The name of the feature/entity (e.g., "Customers",
  "Orders")
- **Namespace**: Target namespace for the SDK (e.g., "Business.CustomerManagement.Sdk")

## Architecture Principles

- **Vertical Slice Architecture**: Each feature contains all related components
- **Record Types**: Use C# records for immutable request/response models
- **FluentValidation**: Single validation approach for consistency
- **Extension Methods**: Clean mapping without AutoMapper complexity
- **Result Pattern**: Success/failure return types
- **Feature Folders**: Group by business capability, not technical layer

## Generated Structure

```text
{Namespace}/
├── {FeatureName}/
│   ├── Create{EntityName}Request.cs
│   ├── Create{EntityName}Response.cs
│   ├── Update{EntityName}Request.cs
│   ├── Update{EntityName}Response.cs
│   ├── Get{EntityName}Request.cs
│   ├── {EntityName}Response.cs
│   ├── {EntityName}ListResponse.cs
│   ├── {EntityName}Extensions.cs
│   ├── Create{EntityName}Validator.cs
│   ├── Update{EntityName}Validator.cs
│   └── {EntityName}Result.cs
└── Common/
    ├── ApiResult.cs
    └── ValidationExtensions.cs
```

**Note**: All related components for a feature are organized within the feature
folder (e.g., `Customers/`) to maintain vertical slice architecture. This keeps
related request/response models, validators, and extensions together rather than
separating them into technical layers.

## Code Generation Guidelines

### 1. Request Records

```csharp
public record Create{EntityName}Request
{
    // Properties from YAML attributes
    // Use appropriate C# types (string, int, DateTime, etc.)
    // Include XML documentation from YAML descriptions
}
```

### 2. Response Records

```csharp
public record {EntityName}Response
{
    // Include Id and all entity properties
    // Add audit fields (CreatedAt, UpdatedAt, etc.)
    // Use nullable types where appropriate
}
```

### 3. FluentValidation Validators

```csharp
public class Create{EntityName}Validator : AbstractValidator<Create{EntityName}Request>
{
    public Create{EntityName}Validator()
    {
        // Rules based on YAML constraints
        // Business rules from YAML descriptions
        // Required field validation
        // Length and format validation
    }
}
```

### 4. Extension Methods

```csharp
public static class {EntityName}Extensions
{
    public static {EntityName}Response ToResponse(this {EntityName} entity) => new()
    {
        // Map properties
    };

    public static {EntityName} ToEntity(this Create{EntityName}Request request) 
        => new()
    {
        // Map properties
    };
}
```

### 5. Result Pattern

```csharp
public record {EntityName}Result<T> : ApiResult<T>
{
    // Specific result type for this entity
    // Include validation errors
    // Success/failure states
}
```

## YAML Mapping Rules

### Attribute Type Mapping

- `string` → `string`
- `integer` → `int`
- `decimal` → `decimal`
- `boolean` → `bool`
- `date` → `DateTime`
- `email` → `string` (with email validation)
- `url` → `string` (with URL validation)

### Constraint Mapping

- `required: true` → FluentValidation `.NotEmpty()`
- `maxLength: X` → FluentValidation `.MaximumLength(X)`
- `minLength: X` → FluentValidation `.MinimumLength(X)`
- `pattern: "regex"` → FluentValidation `.Matches("regex")`

### Enum Handling

- Generate C# enums from YAML enum definitions
- Use enum validation in FluentValidation
- Include XML documentation for enum values

## Security Considerations

- Validate all inputs using FluentValidation
- Use record types for immutability
- Include XML documentation for API consumers
- Follow principle of least privilege in property exposure

## Example Output

Given a `Customer.Type.yaml` with:

```yaml
name: Customer
summary: Represents a business customer entity
attributes:
  name:
    type: string
    required: true
    maxLength: 100
  email:
    type: email
    required: true
  phone:
    type: string
    maxLength: 20
  status:
    type: CustomerStatus
    required: true
```

Generate:

1. **CreateCustomerRequest.cs** - Input model for creating customers
2. **CustomerResponse.cs** - Output model for customer data
3. **CreateCustomerValidator.cs** - FluentValidation rules
4. **CustomerExtensions.cs** - Mapping methods
5. **Common/ApiResult.cs** - Base result pattern

## Output Format

Provide complete, compilable C# files with:

- Proper namespaces
- XML documentation
- FluentValidation rules
- Extension methods
- Modern C# features (records, nullable reference types)
- Clean, readable code following .NET conventions

## Usage

This prompt template will be used with the Modeller MCP secure prompt
building system to generate production-ready SDK code from domain models.
