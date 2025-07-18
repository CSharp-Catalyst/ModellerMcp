---
# STOP: READ THIS FIRST
Before writing ANY code, you MUST confirm understanding of:
1. Property declaration rules (required keyword)
2. Project structure (feature folders only)
3. Validation requirements (Version 7 UUID)
4. Extension method requirements (ToResponse/ToEntity)
---

# CRITICAL: STRICT IMPLEMENTATION REQUIREMENTS

You are an expert .NET developer specializing in Vertical Slice Architecture (VSA) and clean code generation. You will generate production-ready C# code from YAML domain models that follows modern .NET best practices.  Use the checklist here to confirm success.

## MANDATORY COMPLIANCE CHECKLIST

**YOU MUST FOLLOW EVERY ITEM BELOW. NO EXCEPTIONS.**

### ✅ BEFORE YOU START - VERIFY UNDERSTANDING

1. Read ALL guidelines TWICE before writing any code
2. Confirm you understand the property declaration rules
3. Confirm you understand the project structure requirements
4. Confirm you understand the validation requirements

### ✅ PROJECT STRUCTURE - EXACT REQUIREMENTS

```text
{Namespace}/
├── GlobalUsings.cs                             # MANDATORY - Create this first
├── {FeatureName}/                              # Feature folder (e.g., Cases/)
│   ├── {CRUD}{EntityName}Request.cs            # If applicable, add Create, Read, Update and Delete requests
│   ├── {CRUD}{EntityName}Response.cs           # MANDATORY - If request was added, add correspondong response
│   ├── {CRUD}{EntityName}Validator.cs          # MANDATORY - If request was added, add correspondong validator
│   ├── {BehaviourName}{EntityName}Request.cs   # MANDATORY
│   ├── {BehaviourName}{EntityName}Response.cs  # MANDATORY
│   ├── {BehaviourName}{EntityName}Validator.cs # MANDATORY - If request was added, add correspondong validator
│   ├── {EntityName}Extensions.cs               # MANDATORY - Extension methods
└── Common/
    ├── ApiResult.cs                            # MANDATORY - Result pattern
    └── ValidationExtensions.cs                 # MANDATORY
```

**Note**: All related components for a feature are organized within the feature folder (e.g., `Customers/`) to maintain vertical slice architecture.

#### ✅ Special rules for CRUD READ: MUST have
│   ├── Read{EntityName}Request.cs
│   ├── Read{EntityName}Response.cs
│   ├── ReadAll{EntityName}Request.cs           # MANDATORY - Must include Page, Size, Filter and Order properties
│   ├── Read{EntityName}ListResponse.cs         # MANDATORY - A List response should only contain a subset of important required fields

**❌ DO NOT CREATE:**
- Models/ folder
- Validators/ folder  
- Services/ folder
- Any technical layer folders

### ✅ PROPERTY DECLARATION - MANDATORY RULES

**NEVER use `= string.Empty` or default assignments for required properties**

```csharp
// ✅ CORRECT - Required non-nullable
public required string Name { get; init; }
public required Guid Id { get; init; }

// ✅ CORRECT - Optional nullable
public string? Description { get; init; }
public int? OptionalCount { get; init; }

// ❌ WRONG - Never do this for required fields
public string Name { get; init; } = string.Empty;
public Guid Id { get; init; } = Guid.NewGuid();
```

### ✅ GLOBALUSINGS.CS - MANDATORY FIRST FILE

**Create this EXACT file first:**
```csharp
// GlobalUsings.cs
global using System;
global using System.Collections.Generic;
global using System.ComponentModel.DataAnnotations;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;
```

### ✅ VALIDATION RULES - MANDATORY GUID VALIDATION

**For ALL Guid primary keys, use this EXACT code:**
```csharp
RuleFor(x => x.Id)
    .NotEmpty()
    .Must(BeVersion7Uuid)
    .WithMessage("Primary key must be a Version 7 UUID for optimal database performance");

private static bool BeVersion7Uuid(Guid guid)
{
    if (guid == Guid.Empty) return false;
    var bytes = guid.ToByteArray();
    var versionByte = bytes[7];
    var version = (versionByte & 0xF0) >> 4;
    return version == 7;
}
```

### ✅ EXTENSION METHODS - MANDATORY IMPLEMENTATIONS

**MUST create ToResponse and ToEntity methods:**
```csharp
public static class {EntityName}Extensions
{
    public static {EntityName}Response ToResponse(this {EntityName} entity) => new()
    {
        // Map ALL properties from entity to response
    };

    public static {EntityName} ToEntity(this Create{EntityName}Request request) => new()
    {
        // Map ALL properties from request to entity
    };
}
```

### ✅ USING STATEMENTS - MANDATORY RULES

**After creating GlobalUsings.cs, files should ONLY include:**
- Project-specific namespaces
- FluentValidation (only in validator files)
- No System.* imports (handled by GlobalUsings)

### ✅ VERIFICATION CHECKLIST

Before submitting your implementation, verify:
- [ ] GlobalUsings.cs exists and is correct
- [ ] No `= string.Empty` in any required properties
- [ ] All Guid primary keys have Version 7 validation
- [ ] Extension methods exist with ToResponse/ToEntity
- [ ] Feature folder structure (not technical folders)
- [ ] All using statements follow rules
- [ ] Followed the **Generate Modern .NET SDK Project using Vertical Slice patterns from Domain Models** section recommendations where possible

## IMPLEMENTATION FAILURE EXAMPLES

### ❌ FAILURE: Technical Folder Structure
```
Models/
Validators/
Services/
```

### ❌ FAILURE: Wrong Property Declarations
```csharp
public string Name { get; init; } = string.Empty;
public Guid Id { get; init; } = Guid.NewGuid();
```

### ❌ FAILURE: Basic Guid Validation
```csharp
RuleFor(x => x.Id).NotEmpty(); // Missing Version 7 validation
```

### ❌ FAILURE: Missing Extension Methods
No ToResponse/ToEntity methods created

### ❌ FAILURE: Missing GlobalUsings
Individual using statements in every file

### ❌ FAILURE: Deviated from guidelines below
Halucinated and didn't follow the guidelines in **Generate Modern .NET SDK Project** section

---

## Generate Modern .NET SDK Project using Vertical Slice patterns from Domain Models

### Task Overview

Create a complete, production-ready .NET SDK project using Vertical Slice Architecture (VSA) patterns from the provided domain model definitions.
Use the **latest stable .NET version** with modern C# language features and current best practices.

**CRITICAL:** Follow the MANDATORY COMPLIANCE CHECKLIST above. Failure to follow these rules means the implementation is incorrect and must be redone.

### Project Configuration

**Target Namespace**: Branch.Cases.Sdk
**Primary Feature**: Cases
**All Features**: Case, CaseActivity, CaseSource, CaseStatus, CaseStatusReason, CaseType, CaseTypeComplaint, Customer, Group
**Domain Path**: c:\jjs\set\dev\mcpdemo\models\Branch\Cases

### Input Requirements

- **Domain Model YAML**: Complete YAML definition of a domain entity (Type or  Behaviour)
- **Feature Name**: The name of the feature/entity (e.g., "Customers",  "Orders")
- **Namespace**: Target namespace for the SDK (e.g., "Business.CustomerManagement.Sdk")

### Architecture Principles

- **Vertical Slice Architecture**: Each feature contains all related components
- **Record Types**: Use C# records for immutable request/response models
- **FluentValidation**: Single validation approach for consistency
- **Extension Methods**: Clean mapping without AutoMapper complexity
- **Result Pattern**: Success/failure return types
- **Feature Folders**: Group by business capability, not technical layer
- **GlobalUsings**: Centralize common using statements to reduce boilerplate

### Framework Requirements

- **Target Framework**: .NET Latest (Latest stable version, don't use preview versions)
- **C# Language Version**: C# latest available for .NET Latest
- **Nullable Reference Types**: Enabled for enhanced type safety
- **ImplicitUsings**: Enabled to work with GlobalUsings pattern
- **Documentation Generation**: Enable XML documentation file generation

### Project File Guide

Generate a `.csproj` file with the following specifications:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <WarningsAsErrors />
    <WarningsNotAsErrors>CS1591</WarningsNotAsErrors>
    <Title>Business Domain SDK</Title>
    <Description>Production-ready SDK generated from domain models</Description>
    <Version>1.0.0</Version>
    <Authors>Generated by Modeller MCP</Authors>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="FluentValidation" Version="11.8.1" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="8.0.0" />
  </ItemGroup>
</Project>
```

### File-Level Using Guidelines

After implementing GlobalUsings, files should only include:

- **Namespace-specific imports**: Only the project's own namespaces (e.g., `using Business.CustomerManagement.Sdk.Common;`)
- **Specialized imports**: Libraries with potential conflicts (e.g., `FluentValidation`, `FluentValidation.Results`)
- **Framework-specific imports**: Specialized System namespaces not commonly used (e.g., `System.Text.Json`)

#### IMPORTAANT Notes

- **FluentValidation Conflict**: Do not include `FluentValidation` in GlobalUsings due to `ValidationResult` type conflicts with `System.ComponentModel.DataAnnotations.ValidationResult`
- **Explicit Types**: Use fully qualified type names when conflicts arise (e.g., `FluentValidation.Results.ValidationResult`)
- **Validator Files**: Include `using FluentValidation;` and `using FluentValidation.Results;` only in validator files
- **ValidationExtensions Files**: Use type alias to resolve ValidationResult ambiguity: `using ValidationResult = FluentValidation.Results.ValidationResult;`

### Security Guidelines - IMPORTANT

- **Input Validation**: Validate all inputs using FluentValidation with comprehensive rules
- **Immutability**: Use record types for immutability and thread safety
- **Data Exposure**: Follow principle of least privilege in property exposure
- **XML Documentation**: Include comprehensive XML documentation for API consumers
- **Sanitization**: Implement input sanitization for string fields to prevent injection attacks
- **Length Limits**: Enforce maximum length constraints on all string properties
- **Type Safety**: Use strongly-typed enums instead of magic strings where possible
- **Nullable References**: Leverage nullable reference types to prevent null reference exceptions
- **UUID Version 7**: Enforce Version 7 UUIDs for primary keys to ensure optimal database performance, natural ordering, and prevent timing attacks through predictable ID generation

### Code Generation Guidelines

#### 1. Request/Response Records

```csharp
using Business.CustomerManagement.Sdk.Common;

namespace Business.CustomerManagement.Sdk.{FeatureName};

public record {CRUD}{EntityName}Request
{
    // Properties from YAML attributes
    // Use appropriate C# types (string, int, DateTime, etc.)
    // Include XML documentation from YAML descriptions
    // DataAnnotation attributes available via GlobalUsings
    
    // REQUIRED PROPERTIES: Use 'required' keyword for non-nullable required fields
    // ✅ Good: public required string FirstName { get; init; }
    // ❌ Bad:  public string FirstName { get; init; } = string.Empty;
    
    // OPTIONAL PROPERTIES: Use nullable types for optional fields
    // ✅ Good: public string? Phone { get; init; }
    // ❌ Bad:  public string Phone { get; init; }
}
```

For YAML fields marked as `required: false` or no requirement specified:
```csharp
// ✅ Correct implementation
public string? OptionalField { get; init; }
```

- **Required Non-Nullable Fields**: Use `public required string PropertyName { get; init; }`
- **Optional Nullable Fields**: Use `public string? PropertyName { get; init; }`
- **Required Value Types**: Use `public required int PropertyName { get; init; }`
- **Optional Value Types**: Use `public int? PropertyName { get; init; }` or provide default values
- **Avoid `= string.Empty`**: Use `required` keyword instead for compile-time safety

#### 2. Response Records

```csharp
using Business.CustomerManagement.Sdk.Common;

namespace Business.CustomerManagement.Sdk.{FeatureName};

public record {EntityName}Response
{
    // Include Id and all entity properties
    // Add audit fields (CreatedAt, UpdatedAt, etc.)
    // Include CreatedBy, UpdatedBy for user tracking
    // Add Version field for optimistic concurrency control
    // Use nullable types where appropriate
    // System types available via GlobalUsings
    
    // RESPONSE PROPERTIES: Use 'required' for guaranteed non-null properties
    // ✅ Good: public required Guid Id { get; init; }
    // ✅ Good: public required string Name { get; init; }
    // ✅ Good: public string? OptionalField { get; init; }
}
```

#### 3. FluentValidation Validators

```csharp
using FluentValidation;
using Business.CustomerManagement.Sdk.Common;

namespace Business.CustomerManagement.Sdk.{FeatureName};

public class Create{EntityName}Validator : AbstractValidator<Create{EntityName}Request>
{
    public Create{EntityName}Validator()
    {
        // Rules based on YAML constraints
        // Business rules from YAML descriptions
        // Required field validation
        // Length and format validation
        
        // IMPORTANT: For Guid primary keys, enforce Version 7 UUID requirement
        // RuleFor(x => x.Id)
        //     .NotEmpty()
        //     .Must(BeVersion7Uuid)
        //     .WithMessage("Primary key must be a Version 7 UUID for optimal database performance");
    }
    
    /// <summary>
    /// Validates that a Guid is a Version 7 UUID for optimal database performance
    /// </summary>
    /// <param name="guid">The Guid to validate</param>
    /// <returns>True if the Guid is a valid Version 7 UUID, false otherwise</returns>
    private static bool BeVersion7Uuid(Guid guid)
    {
        if (guid == Guid.Empty) return false;
        
        // Version 7 UUIDs have version bits set to 0111 (7) in the 13th nibble
        var bytes = guid.ToByteArray();
        var versionByte = bytes[7]; // 8th byte contains version in upper nibble
        var version = (versionByte & 0xF0) >> 4;
        
        return version == 7;
    }
}
```

##### Special Case - Primary Key Validation Rules

When Guid fields are used as primary keys (typically Id fields), they MUST be validated as Version 7 UUIDs:

**Rationale for Version 7 UUIDs:**

- **Database Performance**: Time-ordered for better B-tree index performance
- **Natural Sorting**: Chronological ordering without additional timestamp fields
- **Reduced Fragmentation**: Sequential nature reduces database page fragmentation
- **Modern Standard**: Latest UUID specification (RFC 4122bis) recommended approach

#### 4. Extension Methods

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

#### 5. Result Pattern

```csharp
public record {EntityName}Result<T> : ApiResult<T>
{
    // Specific result type for this entity
    // Include validation errors
    // Success/failure states
}
```

### YAML Mapping Rules

#### Attribute Type Mapping

- `string` → `string`
- `integer` → `int`
- `decimal` → `decimal`
- `boolean` → `bool`
- `date` → `DateTime`
- `email` → `string` (with email validation)
- `url` → `string` (with URL validation)

#### Constraint Mapping

- `required: true` → C# `required` keyword + FluentValidation `.NotEmpty()`
- `required: false` → Nullable type (e.g., `string?`)
- `maxLength: X` → FluentValidation `.MaximumLength(X)`
- `minLength: X` → FluentValidation `.MinimumLength(X)`
- `pattern: "regex"` → FluentValidation `.Matches("regex")`

### Enum Guidelines

- Generate C# enums from YAML enum definitions
- Use enum validation in FluentValidation
- Include XML documentation for enum values
