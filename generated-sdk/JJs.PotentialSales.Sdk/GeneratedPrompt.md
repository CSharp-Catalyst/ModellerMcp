# System Context
# Generate SDK from Domain Model - Vertical Slice Architecture

## Purpose
Generate a clean .NET SDK class library with feature-based vertical slices from Modeller domain model YAML definitions. Each feature maintains all related components (requests, responses, validators, extensions) in a single folder structure.

## Context
You are an expert .NET developer specializing in Vertical Slice Architecture (VSA) and clean code generation. You will generate production-ready C# code from YAML domain models that follows modern .NET best practices.

## Input Requirements
- **Domain Model YAML**: Complete YAML definition of a domain entity (Type or Behaviour)
- **Feature Name**: The name of the feature/entity (e.g., "Prospects", "Activities")
- **Namespace**: Target namespace for the SDK (e.g., "JJs.PotentialSales.Sdk")

## Architecture Principles
- **Vertical Slice Architecture**: Each feature contains all related components
- **Record Types**: Use C# records for immutable request/response models
- **FluentValidation**: Single validation approach for consistency
- **Extension Methods**: Clean mapping without AutoMapper complexity
- **Result Pattern**: Success/failure return types
- **Feature Folders**: Group by business capability, not technical layer

## Generated Structure
```
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

**Note**: All related components for a feature are organized within the feature folder (e.g., `Prospects/`) to maintain vertical slice architecture. This keeps related request/response models, validators, and extensions together rather than separating them into technical layers.

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

    public static {EntityName} ToEntity(this Create{EntityName}Request request) => new()
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

Given a `Prospect.Type.yaml` with:
```yaml
name: Prospect
summary: Represents a potential customer in the sales pipeline
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
    type: ProspectStatus
    required: true
```

Generate:
1. **CreateProspectRequest.cs** - Input model for creating prospects
2. **ProspectResponse.cs** - Output model for prospect data
3. **CreateProspectValidator.cs** - FluentValidation rules
4. **ProspectExtensions.cs** - Mapping methods
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
This prompt template will be used with the Modeller MCP secure prompt building system to generate production-ready SDK code from domain models.


# Generation Request
**Target Namespace**: JJs.PotentialSales.Sdk
**Feature Name**: Prospects

# Domain Model YAML
```yaml
model: Prospect
summary: A potential sale opportunity
remarks: >
  A potential sale refers to a prospective transaction or business opportunity where there is an 
  expressed interest or likelihood that a product or service may be purchased by a customer or client. 
  This typically involves leads or inquiries that have shown some level of engagement, but the sale 
  has not yet been finalized or converted. Potential sales are key indicators in the sales pipeline, 
  representing opportunities that require further nurturing, follow-up, and negotiation to convert into actual sales.

attributeUsages:
  - name: prospectId
    type: primaryKey
    required: true
    summary: The unique identifier for the prospect

  - name: potentialSaleNumber
    type: prospectNumber
    required: true
    summary: A unique identifier assigned to each potential sale, ensuring traceability and differentiation within the system

  - name: siteNumber
    type: siteNumber
    required: true
    summary: The site associated with the potential sale

  - name: assignee
    type: mediumString
    required: true
    summary: The individual responsible for managing the potential sale, ensuring accountability and clear ownership within the sales team

  - name: prospectTypeId
    type: primaryKey
    required: true
    summary: The classification of the potential sale, which could include categories like 'New Business', 'Renewal', or 'Upsell'

  - name: sourceId
    type: primaryKey
    required: true
    summary: The origin or channel through which the potential sale was initiated, such as 'Referral', 'Website', or 'Campaign'

  - name: customerStatus
    type: CustomerStatus
    required: true
    summary: The status of the customer

  - name: customerNumber
    type: customerNumber
    required: false
    summary: A unique identifier assigned to each customer, used to accurately reference and track customer-related information

  - name: tradingName
    type: baseString
    required: true
    summary: The trading name of the customer

  - name: prospectStatus
    type: ProspectStatus
    required: true
    summary: The current state of the potential sale, indicating progress or actions needed

  - name: interest
    type: Interest
    required: true
    summary: The interest of the customer in the potential sale, indicating the likelihood of conversion

  - name: salesFollowUpDate
    type: dateField
    required: false
    summary: The scheduled date for the sales team to follow up on the potential sale

  - name: salesFollowUpDescription
    type: longString
    required: false
    summary: A detailed note or plan regarding the follow-up actions to be taken on the specified date

  - name: quoteProvided
    type: boolean
    required: false
    summary: A flag indicating whether a quote has been provided to the customer

  - name: quotedProvidedDate
    type: dateField
    required: false
    summary: The specific date when a quote was delivered to the customer

  - name: quoteProvidedDescription
    type: longString
    required: false
    summary: A summary or explanation of the quote provided, including key details or terms

  - name: addressLine
    type: addressString
    required: true
    summary: The address associated with the customer, this maybe a new address or an existing customer address

  - name: contactFirstName
    type: mediumString
    required: false
    summary: The first name of the contact person managing the potential sale

  - name: contactLastName
    type: mediumString
    required: false
    summary: The last name of the contact person managing the potential sale

  - name: contactPhone
    type: phoneNumber
    required: false
    summary: The phone number of the contact person managing the potential sale

  - name: contactEmail
    type: emailAddress
    required: false
    summary: The email address of the contact person managing the potential sale

  - name: description
    type: veryLongString
    required: false
    summary: A detailed description of the potential sale, including key information, requirements, and objectives


---

model: Prospect

behaviours:
  - name: getProspectByNumber
    summary: Retrieve a prospect by its unique number
    entities:
      - Prospect
    preconditions:
      - Prospect.potentialSaleNumber is provided
    effects:
      - return Prospect data

  - name: getProspectsByAssignee
    summary: Retrieve all prospects assigned to a specific person
    entities:
      - Prospect
    preconditions:
      - Prospect.assignee is provided
    effects:
      - return list of Prospects

  - name: getProspectsBySites
    summary: Retrieve all prospects for specific sites
    entities:
      - Prospect
    preconditions:
      - Prospect.siteNumber is provided
    effects:
      - return list of Prospects

  - name: createProspect
    summary: Create a new potential sale opportunity
    entities:
      - Prospect
    preconditions:
      - required fields are provided
      - potentialSaleNumber is unique
    effects:
      - new Prospect is created
      - prospectStatus is set to Open

  - name: updateProspectStatus
    summary: Update the status of a prospect
    entities:
      - Prospect
    preconditions:
      - Prospect exists
      - new status is valid
    effects:
      - prospectStatus is updated

  - name: scheduleFollowUp
    summary: Schedule a follow-up for a prospect
    entities:
      - Prospect
    preconditions:
      - Prospect exists
      - follow-up date is in the future
    effects:
      - salesFollowUpDate is set
      - salesFollowUpDescription is updated

  - name: provideQuote
    summary: Mark that a quote has been provided to the prospect
    entities:
      - Prospect
    preconditions:
      - Prospect exists
      - quote information is provided
    effects:
      - quoteProvided is set to true
      - quotedProvidedDate is set
      - quoteProvidedDescription is updated

scenarios:
  - name: create new prospect from lead
    given:
      - a new sales lead is identified
      - required prospect information is available
    when:
      - createProspect is called
    then:
      - new Prospect is created with Open status
      - potentialSaleNumber is generated
      - assignee is set

  - name: follow up on prospect
    given:
      - Prospect exists with Open status
      - follow-up date has arrived
    when:
      - scheduleFollowUp is called
    then:
      - salesFollowUpDate is updated
      - salesFollowUpDescription contains next actions

  - name: provide quote to interested prospect
    given:
      - Prospect exists
      - interest is Yes or Maybe
    when:
      - provideQuote is called
    then:
      - quoteProvided is true
      - quotedProvidedDate is set to today
      - quoteProvidedDescription contains quote details

  - name: close prospect as won
    given:
      - Prospect exists
      - customer has agreed to contract
    when:
      - updateProspectStatus is called with Won
    then:
      - prospectStatus is Won
      - prospect is no longer in active pipeline

  - name: close prospect as lost
    given:
      - Prospect exists
      - customer has declined or gone with competitor
    when:
      - updateProspectStatus is called with Lost
    then:
      - prospectStatus is Lost
      - prospect is no longer in active pipeline

```

# Instructions
Generate a complete SDK vertical slice for the Prospects feature using the provided domain model.
Follow the VSA patterns and guidelines specified above.
Provide complete, compilable C# files with proper namespaces and documentation.
