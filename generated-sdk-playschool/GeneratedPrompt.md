# Generate Modern .NET SDK Project from Domain Models

## Task Overview
Create a complete, production-ready .NET SDK project using Vertical Slice Architecture (VSA) patterns from the provided domain model definitions.
Use the **latest stable .NET LTS version** with modern C# language features and current best practices.

## Project Configuration
**Target Namespace**: JJs.PotentialSales.Sdk
**Primary Feature**: PotentialSales
**All Features**: Activity, IdentifiedCompetitor, IdentifiedCompetitorWasteProduct, Prospect, ProspectType, ProspectWasteProduct, Source, WasteProduct
**Domain Path**: C:\jjs\set\dev\playschool\models\JJs\PotentialSales

## Domain Model Definitions
# Activity.Behaviour.yaml
```yaml
model: Activity

behaviours:
  - name: createActivity
    summary: Create a new activity for a prospect
    entities:
      - Activity
      - Prospect
    preconditions:
      - Prospect exists
      - activity details are provided
    effects:
      - new Activity is created
      - activity is linked to prospect

  - name: updateActivityDescription
    summary: Update the description of an existing activity
    entities:
      - Activity
    preconditions:
      - Activity exists
      - new description is provided
    effects:
      - Activity.description is updated

  - name: getActivitiesForProspect
    summary: Retrieve all activities for a specific prospect
    entities:
      - Activity
      - Prospect
    preconditions:
      - Prospect exists
    effects:
      - return list of Activities for the prospect

scenarios:
  - name: log follow-up call activity
    given:
      - Prospect exists
      - sales person makes follow-up call
    when:
      - createActivity is called with PhoneCall method
    then:
      - new Activity is created
      - type is set to SalesFollowUp
      - method is set to PhoneCall
      - date is set to today

  - name: log quote provision activity
    given:
      - Prospect exists
      - quote is provided to customer
    when:
      - createActivity is called with Quote type
    then:
      - new Activity is created
      - type is set to Quote
      - description contains quote details

  - name: update activity with additional details
    given:
      - Activity exists
      - additional information becomes available
    when:
      - updateActivityDescription is called
    then:
      - Activity.description is updated with new information

```

# Activity.Type.yaml
```yaml
model: Activity
summary: Defines the activities for a potential sale
remarks: Activities and interactions related to managing prospects through the sales pipeline
ownedBy: Prospect

attributeUsages:
  - name: activityId
    type: primaryKey
    required: true
    summary: The unique identifier for the activity

  - name: prospectId
    type: primaryKey
    required: true
    summary: The prospect this activity is associated with

  - name: type
    type: ActivityType
    required: true
    summary: The current activity type set on this potential sale

  - name: method
    type: ActivityMethod
    required: true
    summary: The current activity method set on this potential sale

  - name: contactName
    type: mediumString
    required: true
    summary: The display value of the contact name managing the potential

  - name: date
    type: dateField
    required: true
    summary: The date and time the record was created

  - name: description
    type: veryLongString
    required: false
    summary: A detailed activity description of the case

```

# IdentifiedCompetitor.Type.yaml
```yaml
model: IdentifiedCompetitor
summary: Defines all potential sales competitors within the system
remarks: Competitors identified for specific prospects
ownedBy: Prospect

attributeUsages:
  - name: identifiedCompetitorId
    type: primaryKey
    required: true
    summary: The unique identifier for the identified competitor

  - name: prospectId
    type: primaryKey
    required: true
    summary: The prospect this competitor is associated with

  - name: competitorName
    type: baseString
    required: true
    summary: The name of the competitor

  - name: agreementEndDate
    type: dateField
    required: true
    summary: The date the agreement with the competitor ends

```

# IdentifiedCompetitorWasteProduct.Type.yaml
```yaml
model: IdentifiedCompetitorWasteProduct
summary: Defines all waste products associated with a competitor
remarks: Junction table linking competitors to their waste products
ownedBy: IdentifiedCompetitor

attributeUsages:
  - name: identifiedCompetitorWasteProductId
    type: primaryKey
    required: true
    summary: The unique identifier for the competitor waste product relationship

  - name: identifiedCompetitorId
    type: primaryKey
    required: true
    summary: The competitor this waste product is associated with

  - name: wasteProductId
    type: primaryKey
    required: true
    summary: The waste product associated with the competitor

```

# Prospect.Behaviour.yaml
```yaml
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

# Prospect.Type.yaml
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

```

# ProspectType.Type.yaml
```yaml
model: ProspectType
summary: Defines all types of prospects within the system
remarks: Classification categories for potential sales such as 'New Business', 'Renewal', or 'Upsell'

attributeUsages:
  - name: prospectTypeId
    type: primaryKey
    required: true
    summary: The unique identifier for the prospect type

  - name: name
    type: shortString
    required: true
    summary: The display value of the prospect type

  - name: active
    type: isActive
    required: true
    default: true
    summary: Determines if the record is active or not

```

# ProspectWasteProduct.Type.yaml
```yaml
model: ProspectWasteProduct
summary: Defines all waste products associated with a prospect
remarks: Junction table linking prospects to their associated waste products
ownedBy: Prospect

attributeUsages:
  - name: prospectWasteProductId
    type: primaryKey
    required: true
    summary: The unique identifier for the prospect waste product relationship

  - name: prospectId
    type: primaryKey
    required: true
    summary: The prospect this waste product is associated with

  - name: wasteProductId
    type: primaryKey
    required: true
    summary: The waste product associated with the prospect

```

# Source.Type.yaml
```yaml
model: Source
summary: Defines the supported sources where potential sales can originate
remarks: The origin or channel through which the potential sale was initiated

attributeUsages:
  - name: sourceId
    type: primaryKey
    required: true
    summary: The unique identifier for the source

  - name: name
    type: shortString
    required: true
    summary: The display value of the source name

  - name: active
    type: isActive
    required: true
    default: true
    summary: Determines if the record is active or not

```

# WasteProduct.Type.yaml
```yaml
model: WasteProduct
summary: Defines all waste products within the system
remarks: Waste products that can be associated with prospects and competitors

attributeUsages:
  - name: wasteProductId
    type: primaryKey
    required: true
    summary: The unique identifier for the waste product

  - name: name
    type: shortString
    required: true
    summary: The display value of the waste product

  - name: active
    type: isActive
    required: true
    default: true
    summary: Determines if the record is active or not

```

# _meta.yaml
```yaml
name: PotentialSales
summary: The Potential Sales Management System
remarks: >
  A robust platform designed to streamline and optimize the entire sales process. 
  It integrates key services such as managing prospective sales opportunities, 
  retrieving and synchronizing essential data from external sources, and overseeing 
  the organizational structure and operations. Together, these components provide 
  a cohesive and efficient environment for tracking leads, managing sales pipelines, 
  and ensuring data consistency and organizational alignment throughout the sales lifecycle.
owners:
  - potentialsales-team@jjswaste.com.au
tags:
  - sales
  - prospects
  - lead-management
  - sales-pipeline
dependencies: []
version: 1.0.0
status: approved
lastReviewed: 2025-06-24

```


## VSA Template Instructions
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
- **GlobalUsings**: Centralize common using statements to reduce boilerplate

## GlobalUsings Configuration

Create a `GlobalUsings.cs` file at the project root to include commonly used namespaces across all files:

```csharp
// GlobalUsings.cs
global using System;
global using System.Collections.Generic;
global using System.ComponentModel.DataAnnotations;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;
// Note: FluentValidation not included globally due to ValidationResult ambiguity
// Include FluentValidation only in validator files
```

This approach eliminates the need to include these using statements in every file, significantly reducing code duplication and maintenance overhead.

### File-Level Using Guidelines

After implementing GlobalUsings, files should only include:

- **Namespace-specific imports**: Only the project's own namespaces (e.g., `using Business.CustomerManagement.Sdk.Common;`)
- **Specialized imports**: Libraries with potential conflicts (e.g., `FluentValidation`, `FluentValidation.Results`)
- **Framework-specific imports**: Specialized System namespaces not commonly used (e.g., `System.Text.Json`)

### Important Notes

- **FluentValidation Conflict**: Do not include `FluentValidation` in GlobalUsings due to `ValidationResult` type conflicts with `System.ComponentModel.DataAnnotations.ValidationResult`
- **Explicit Types**: Use fully qualified type names when conflicts arise (e.g., `FluentValidation.Results.ValidationResult`)
- **Validator Files**: Include `using FluentValidation;` and `using FluentValidation.Results;` only in validator files

## Generated Structure

```text
{Namespace}/
├── GlobalUsings.cs                      # Common using statements
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
// Only project-specific usings needed (GlobalUsings handles System.*, etc.)
using Business.CustomerManagement.Sdk.Common;

namespace Business.CustomerManagement.Sdk.{FeatureName};

public record Create{EntityName}Request
{
    // Properties from YAML attributes
    // Use appropriate C# types (string, int, DateTime, etc.)
    // Include XML documentation from YAML descriptions
    // DataAnnotation attributes available via GlobalUsings
}
```

### 2. Response Records

```csharp
using Business.CustomerManagement.Sdk.Common;

namespace Business.CustomerManagement.Sdk.{FeatureName};

public record {EntityName}Response
{
    // Include Id and all entity properties
    // Add audit fields (CreatedAt, UpdatedAt, etc.)
    // Use nullable types where appropriate
    // System types available via GlobalUsings
}
```

### 3. FluentValidation Validators

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

1. **GlobalUsings.cs** - Common namespace imports to reduce boilerplate
2. **CreateCustomerRequest.cs** - Input model for creating customers
3. **CustomerResponse.cs** - Output model for customer data
4. **CreateCustomerValidator.cs** - FluentValidation rules
5. **CustomerExtensions.cs** - Mapping methods
6. **Common/ApiResult.cs** - Base result pattern

## Output Format

Provide complete, compilable C# files with:

- **GlobalUsings.cs** with common namespace imports
- Proper namespaces with minimal file-specific using statements
- XML documentation
- FluentValidation rules
- Extension methods
- Modern C# features (records, nullable reference types)
- Clean, readable code following .NET conventions

## Usage

This prompt template will be used with the Modeller MCP secure prompt
building system to generate production-ready SDK code from domain models.


## Generation Requirements
1. **Create complete project structure** with proper VSA organization
2. **Generate all model files** for each feature found in the YAML definitions
3. **Include FluentValidation** rules based on the attribute constraints
4. **Use Result pattern** for error handling (no exceptions for business logic)
5. **Add extension methods** for mapping between DTOs and entities
6. **Include complete .csproj file** with latest stable .NET target framework
7. **Add comprehensive XML documentation** for all public APIs
8. **Follow modern .NET conventions** and latest C# language features

Generate the complete SDK project ready for production use.
