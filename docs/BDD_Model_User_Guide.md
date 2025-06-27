# BDD Model Definition User Guide for Modern Development

## Table of Contents

1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Getting Started](#getting-started)
4. [AI-Powered Development Workflow](#ai-powered-development-workflow)
5. [Model Definition Best Practices](#model-definition-best-practices)
6. [Validation and Quality Assurance](#validation-and-quality-assurance)
7. [Code Generation (Available)](#3-code-generation-available)
8. [Common Patterns and Examples](#common-patterns-and-examples)
9. [Troubleshooting](#troubleshooting)
10. [Advanced Scenarios](#advanced-scenarios)

---

## Overview

This guide provides comprehensive instructions for **Domain Owners**, **Solution Architects**, and **Developers** on creating, validating, and generating code from BDD (Behaviour-Driven Development) model definitions using the Modeller framework with AI assistance.

### Modern Development Approach

The Modeller framework embraces AI-powered development through:

- **LLM-Driven Code Generation**: Transform domain models into production-ready code
- **Intelligent Analysis**: AI-powered model review and optimization suggestions
- **Context-Aware Development**: Code generation that understands your existing patterns
- **Continuous Learning**: System improves based on developer feedback

### Target Audience

- **Domain Owners**: Business stakeholders defining domain models with AI assistance
- **Solution Architects**: Technical leaders designing systems with intelligent code generation
- **Developers**: Engineers implementing and maintaining AI-generated code from models

### Benefits

- **Accelerated Development**: Generate complete applications from domain models
- **Consistent Quality**: AI ensures adherence to patterns and best practices
- **Living Documentation**: Self-documenting models with generated code
- **Intelligent Evolution**: AI-assisted model and code maintenance
- **Developer Control**: AI assists while developers maintain architectural decisions

---

## Prerequisites

### Required Tools

- **.NET SDK** (latest stable LTS version) or later
- **Visual Studio Code** (recommended) or Visual Studio
- **YAML extension** for your IDE
- **Git** for version control

### Required Knowledge

- Basic understanding of domain modeling
- Familiarity with YAML syntax
- Understanding of REST APIs and database concepts
- Basic knowledge of BDD principles

### Environment Setup

```bash
# Clone the Modeller repository
git clone <repository-url>
cd Modeller

# Build the validation tools
dotnet build src/Modeller.Domain

# Verify tools are working
dotnet run --project src/Modeller.Domain
```

---

## Getting Started

### Step 1: Create Your Project Structure

Start by creating the recommended folder hierarchy:

``` text
models/
└── {CompanyName}/
    ├── {BoundedContext}/
    │   ├── _meta.yaml                    # Domain metadata
    │   ├── {Entity}.Type.yaml            # Entity definitions
    │   ├── {Entity}.Behaviour.yaml       # Entity behaviors
    │   └── ...
    └── Shared/
        ├── AttributeTypes/
        │   └── CommonAttributes.yaml     # Reusable attribute types
        ├── Enums/
        │   └── {EnumName}.yaml           # Shared enumerations
        └── ValueTypes/
            └── {ValueType}.yaml          # Value objects
```

### Step 2: Initialize Your First Domain

Create your domain metadata file:

```yaml
# models/YourCompany/YourDomain/_meta.yaml
name: YourDomain
summary: Brief description of your domain
description: >
  Detailed description of what this domain encompasses,
  its responsibilities, and how it fits into the larger system.
owners:
  - domain.owner@yourcompany.com
  - architect@yourcompany.com
tags:
  - domain-tag
  - business-area
dependencies:
  - ../SharedDomain/Entity.Type.yaml
version: 1.0.0
status: active
lastReviewed: 2025-06-24
```

### Step 3: Create Your First Entity

Define your entity structure:

```yaml
# models/YourCompany/YourDomain/Customer.Type.yaml
model: Customer
summary: Represents a customer in the system
description: >
  A customer entity that contains all information about
  individuals or organizations that purchase our services.

attributeUsages:
  - name: customerId
    type: primaryKey
    required: true
    summary: Unique identifier for the customer

  - name: customerNumber
    type: customerNumber
    required: true
    summary: Business-friendly customer identifier

  - name: companyName
    type: companyName
    required: true
    summary: Legal name of the customer organization

  - name: contactEmail
    type: emailAddress
    required: true
    summary: Primary email contact for the customer

  - name: isActive
    type: isActive
    required: true
    default: true
    summary: Indicates if the customer account is active
```

---

## Folder Structure Guide

### Recommended Hierarchy

#### Level 1: Company/Organization

```text
models/YourCompany/
```

- Use your organization's name
- PascalCase naming convention
- Single root per organization

#### Level 2: Bounded Context

```text
models/YourCompany/Sales/
models/YourCompany/Inventory/
models/YourCompany/CustomerService/
```

- Represents business domains or bounded contexts
- PascalCase naming convention
- Should align with team/department boundaries

#### Level 3: Entity Groupings

```text
models/YourCompany/Sales/Customers/
models/YourCompany/Sales/Orders/
models/YourCompany/Sales/Products/
```

- Groups related entities
- PascalCase naming convention
- Keep groupings focused and cohesive

#### Level 4: Entity Files

```text
models/YourCompany/Sales/Customers/Customer.Type.yaml
models/YourCompany/Sales/Customers/Customer.Behaviour.yaml
models/YourCompany/Sales/Orders/Order.Type.yaml
models/YourCompany/Sales/Orders/Order.Behaviour.yaml
```

### Shared Components Structure

```text
models/YourCompany/Shared/
├── AttributeTypes/
│   ├── CommonAttributes.yaml       # Basic types (string, int, etc.)
│   ├── BusinessAttributes.yaml     # Business-specific types
│   └── ValidationAttributes.yaml   # Validation-heavy types
├── Enums/
│   ├── Status.yaml                 # Generic status enum
│   ├── Priority.yaml               # Priority levels
│   └── Department.yaml             # Organization structure
└── ValueTypes/
    ├── Address.yaml                # Address value object
    ├── Money.yaml                  # Currency and amount
    └── DateRange.yaml              # Date/time ranges
```

### File Naming Conventions

| File Type | Pattern | Example |
|-----------|---------|---------|
| Entity Type | `{EntityName}.Type.yaml` | `Customer.Type.yaml` |
| Entity Behaviour | `{EntityName}.Behaviour.yaml` | `Customer.Behaviour.yaml` |
| Enumeration | `{EnumName}.yaml` | `CustomerStatus.yaml` |
| Value Type | `{ValueTypeName}.yaml` | `Address.yaml` |
| Attribute Types | `{CategoryName}Attributes.yaml` | `CommonAttributes.yaml` |
| Metadata | `_meta.yaml` | `_meta.yaml` |

---

## Model Definition Best Practices

### Entity Definition Guidelines

#### 1. Use Descriptive Names

```yaml
# ✅ Good
model: Customer
model: SalesOrder
model: InventoryItem

# ❌ Avoid
model: Cust
model: SO
model: Item
```

#### 2. Provide Clear Documentation

```yaml
model: Customer
summary: Represents a customer in the system
description: >
  A customer entity that contains all information about
  individuals or organizations that purchase our services.
  Includes contact details, billing information, and account status.
```

#### 3. Use Consistent Attribute Naming

```yaml
attributeUsages:
  # ✅ Good - camelCase, descriptive
  - name: customerId
  - name: companyName  
  - name: contactEmail
  - name: isActive

  # ❌ Avoid - inconsistent casing, abbreviations
  - name: CustomerID
  - name: company_name
  - name: email
  - name: active
```

#### 4. Leverage Shared Attribute Types

```yaml
# ✅ Good - uses shared types
attributeUsages:
  - name: customerId
    type: primaryKey
  - name: email
    type: emailAddress
  - name: phoneNumber
    type: phoneNumber

# ❌ Avoid - repeating definitions
attributeUsages:
  - name: customerId
    type: integer
    format: int32
  - name: email
    type: string
    constraints:
      pattern: ^[^@\s]+@[^@\s]+\.[^@\s]+$
```

### Behaviour Definition Guidelines

#### 1. Define Clear Business Operations

```yaml
behaviours:
  - name: createCustomer
    description: Create a new customer account
    entities:
      - Customer
    preconditions:
      - customer email is unique
      - required fields are provided
    effects:
      - new Customer is created
      - welcome email is sent
```

#### 2. Include Meaningful Scenarios

```yaml
scenarios:
  - name: create customer with valid data
    given:
      - valid customer information is provided
      - email address is unique
    when:
      - createCustomer is called
    then:
      - new Customer is created with Active status
      - customerId is generated
      - welcome email is sent to customer

  - name: reject duplicate email
    given:
      - customer information is provided
      - email address already exists in system
    when:
      - createCustomer is called
    then:
      - operation fails with validation error
      - no Customer is created
```

### Enumeration Guidelines

#### 1. Use Descriptive Values

```yaml
# ✅ Good
name: CustomerStatus
items:
  - name: Active
    display: Active
    value: 1
  - name: Suspended
    display: Suspended
    value: 2
  - name: Cancelled
    display: Cancelled
    value: 3

# ❌ Avoid
name: Status
items:
  - name: A
    display: A
    value: 1
  - name: S
    display: S
    value: 2
```

#### 2. Plan for Future Values

```yaml
# ✅ Good - leaves room for expansion
items:
  - name: Active
    value: 10
  - name: Suspended
    value: 20
  - name: Cancelled
    value: 30

# ❌ Avoid - sequential numbering
items:
  - name: Active
    value: 1
  - name: Suspended
    value: 2
  - name: Cancelled
    value: 3
```

---

## Validation and Quality Assurance

### Using the MCP Validation Tools

#### 1. Discover Models

```bash
# Navigate to your solution root
cd /path/to/your/solution

# Discover all models
dotnet run --project src/Modeller.Domain -- DiscoverModels --solutionPath "."
```

#### 2. Validate Domain Structure

```bash
# Validate specific domain
dotnet run --project src/Modeller.Domain -- ValidateDomain --domainPath "models/YourCompany/YourDomain"

# Validate overall structure
dotnet run --project src/Modeller.Domain -- ValidateStructure --modelsPath "models"
```

#### 3. Validate Individual Files

```bash
# Validate single model file
dotnet run --project src/Modeller.Domain -- ValidateModel --path "models/YourCompany/YourDomain/Customer.Type.yaml"
```

### Common Validation Rules

#### File Structure Validation

- ✅ Files use PascalCase naming
- ✅ Model names match file names
- ✅ Proper file extensions (.yaml)
- ✅ Required metadata files present

#### Content Validation

- ✅ Required fields present (model name, attributeUsages)
- ✅ Attribute names use camelCase
- ✅ Behaviour names use camelCase
- ✅ Valid YAML syntax
- ✅ No duplicate enum values

#### Business Rule Validation

- ✅ BDD scenarios follow Given-When-Then pattern
- ✅ Behaviours have entities, preconditions, and effects
- ✅ Metadata reviewed within 90 days
- ✅ Dependencies properly referenced

### Quality Checklist

Before committing your models, ensure:

- [ ] All files pass validation without errors
- [ ] Naming conventions are followed consistently
- [ ] Documentation is complete and accurate
- [ ] BDD scenarios cover key business rules
- [ ] Shared components are properly utilized
- [ ] Dependencies are correctly specified
- [ ] Metadata is up to date

---

## AI-Powered Development Workflow

### Model-First Development with AI Assistance

The Modeller framework supports a modern, AI-enhanced development approach:

#### 1. Domain Model Definition

- Define your domain models in YAML with IntelliSense support
- Use AI-powered analysis to validate and improve model definitions
- Leverage BDD scenarios for business rule specification

#### 2. AI-Powered Model Analysis

Use the MCP integration with GitHub Copilot for intelligent model review:

```text
@Modeller analyze_model --modelPath "models/JJs/PotentialSales/Prospect.Type.yaml"
@Modeller review_domain --domainPath "models/JJs/PotentialSales"
```

The AI will provide insights on:

- Model structure and naming conventions
- Relationship consistency across the domain
- Best practice recommendations
- Potential performance considerations

#### 3. Code Generation (Available)

Transform validated models into production-ready code:

```text
@Modeller generate_vsa_webapi 
  --domainPath "models/JJs/PotentialSales"
  --projectName "JJs.PotentialSales"
  --outputPath "./src"
```

Generated output includes:

- Complete .NET Web API with Vertical Slice Architecture
- Entity classes with EF Core configuration
- Service implementations with business logic
- Minimal API endpoints with proper routing
- Comprehensive unit and integration tests
- .NET Aspire orchestration setup

#### 4. Intelligent Code Evolution

When models change, AI assists with code updates:

```text
@Modeller modify_feature_code
  --modelPath "models/JJs/PotentialSales/Prospect.Type.yaml"
  --codePath "src/JJs.Api/Features/Prospects"
  --changeDescription "Added priority field and validation rules"
```

The AI ensures:

- Preservation of custom developer code
- Maintenance of established patterns
- Backward compatibility where possible
- Proper migration guidance for breaking changes

### Development Best Practices with AI

#### Iterative Refinement

1. **Start Simple**: Begin with basic model definitions
2. **AI Review**: Use AI analysis to identify improvements
3. **Incremental Enhancement**: Add complexity gradually
4. **Continuous Validation**: Validate changes with AI assistance

#### Quality Assurance

- **Multi-Stage Validation**: Schema → Business Rules → AI Analysis
- **Peer Review**: Combine AI insights with human expertise
- **Testing Strategy**: Leverage AI-generated tests as a foundation
- **Documentation**: Maintain living documentation with AI assistance

#### Team Collaboration

- **Domain Experts**: Focus on business rules and scenarios
- **Architects**: Leverage AI for pattern consistency
- **Developers**: Use AI for implementation guidance
- **QA**: Validate generated code meets requirements

---

## Common Patterns and Examples

### 1. Aggregate Root Pattern

```yaml
# Order.Type.yaml - Aggregate root
model: Order
summary: Customer order aggregate root

attributeUsages:
  - name: orderId
    type: primaryKey
    required: true

# OrderLineItem.Type.yaml - Owned entity
model: OrderLineItem
summary: Individual item within an order
ownedBy: Order

attributeUsages:
  - name: orderLineItemId
    type: primaryKey
    required: true
  - name: orderId
    type: primaryKey
    required: true
```

### 2. Value Object Pattern

```yaml
# Address.Type.yaml - Value object
model: Address
summary: Represents a physical address

attributeUsages:
  - name: street
    type: streetAddress
    required: true
  - name: city
    type: cityName
    required: true
  - name: postalCode
    type: postalCode
    required: true
  - name: country
    type: countryCode
    required: true

# Customer.Type.yaml - Using value object
model: Customer
attributeUsages:
  - name: customerId
    type: primaryKey
    required: true
  - name: billingAddress
    type: Address
    required: true
  - name: shippingAddress
    type: Address
    required: false
```

### 3. State Machine Pattern

```yaml
# Order.Behaviour.yaml
behaviours:
  - name: submitOrder
    description: Submit order for processing
    entities:
      - Order
    preconditions:
      - Order.status is Draft
      - Order has at least one line item
    effects:
      - Order.status is set to Submitted
      - Order.submittedDate is set

scenarios:
  - name: submit valid draft order
    given:
      - Order exists with Draft status
      - Order contains line items
    when:
      - submitOrder is called
    then:
      - Order.status becomes Submitted
      - Order.submittedDate is set to current timestamp
      - confirmation email is sent
```

### 4. Lookup/Reference Data Pattern

```yaml
# ProductCategory.Type.yaml - Reference data
model: ProductCategory
summary: Product categorization lookup

attributeUsages:
  - name: categoryId
    type: primaryKey
    required: true
  - name: categoryName
    type: shortString
    required: true
  - name: isActive
    type: isActive
    required: true
    default: true

# Product.Type.yaml - Using reference
model: Product
attributeUsages:
  - name: productId
    type: primaryKey
    required: true
  - name: categoryId
    type: primaryKey
    required: true
    summary: Reference to ProductCategory
```

---

## Troubleshooting

### Common Issues and Solutions

#### Issue: "Model name should match file name"

```yaml
# ❌ Problem: File named Customer.Type.yaml
model: Cust

# ✅ Solution: Match the model name to file name
model: Customer
```

#### Issue: "Attribute name should be camelCase"

```yaml
# ❌ Problem
attributeUsages:
  - name: CustomerID
  - name: customer_name

# ✅ Solution
attributeUsages:
  - name: customerId
  - name: customerName
```

#### Issue: "Empty YAML document"

```yaml
# ❌ Problem: Empty or malformed file

# ✅ Solution: Ensure minimum required structure
model: EntityName
attributeUsages: []
```

#### Issue: "Metadata has not been reviewed"

```yaml
# ❌ Problem: lastReviewed is too old or missing
lastReviewed: 2024-01-01

# ✅ Solution: Update review date
lastReviewed: 2025-06-24
```

#### Issue: "Behaviour should specify entities"

```yaml
# ❌ Problem
behaviours:
  - name: createCustomer
    description: Create customer
    entities: []

# ✅ Solution
behaviours:
  - name: createCustomer
    description: Create customer
    entities:
      - Customer
```

### Debugging Validation Errors

1. **Run validation with verbose output**:

   ```bash
   dotnet run --project src/Modeller.Domain -- ValidateModel --path "your-file.yaml" --verbose
   ```

2. **Check YAML syntax** using online validators or IDE extensions

3. **Verify file encoding** is UTF-8

4. **Ensure consistent line endings** (use .gitattributes)

---

## Integration with Build Pipelines

### CI/CD Pipeline Integration

#### Azure DevOps Example

```yaml
# azure-pipelines.yml
steps:
- task: DotNetCoreCLI@2
  displayName: 'Validate Models'
  inputs:
    command: 'run'
    projects: 'src/Modeller.Domain'
    arguments: '-- ValidateStructure --modelsPath "models"'

- task: PublishTestResults@2
  condition: always()
  inputs:
    testResultsFormat: 'JUnit'
    testResultsFiles: '**/validation-results.xml'
    searchFolder: '$(Agent.TempDirectory)'
```

#### GitHub Actions Example

```yaml
# .github/workflows/model-validation.yml
name: Model Validation

on:
  pull_request:
    paths:
      - 'models/**'

jobs:
  validate:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '9.0.x'
    
    - name: Validate Models
      run: |
        dotnet run --project src/Modeller.Domain -- ValidateStructure --modelsPath "models"
```

### Pre-commit Hooks

```bash
#!/bin/sh
# .git/hooks/pre-commit

# Validate models before commit
dotnet run --project src/Modeller.Domain -- ValidateStructure --modelsPath "models"

if [ $? -ne 0 ]; then
    echo "Model validation failed. Please fix errors before committing."
    exit 1
fi
```

### Code Generation Integration

```yaml
# Build script example
steps:
  - name: Validate Models
    run: dotnet run --project src/Modeller.Domain -- ValidateStructure --modelsPath "models"
    
  - name: Generate Code
    run: dotnet run --project src/Modeller.Generator -- --input "models" --output "src/Generated"
    
  - name: Build Generated Code
    run: dotnet build src/Generated
```

---

## Advanced Scenarios

### Multi-Team Collaboration

#### Team Boundaries

```text
models/
├── SharedKernel/           # Shared across all teams
│   └── Shared/
├── Sales/                  # Sales team domain
│   ├── Customers/
│   └── Orders/
├── Inventory/              # Inventory team domain
│   ├── Products/
│   └── Warehouses/
└── Billing/                # Billing team domain
    ├── Invoices/
    └── Payments/
```

#### Cross-Domain Dependencies

```yaml
# models/Sales/Orders/_meta.yaml
dependencies:
  - ../../Inventory/Products/Product.Type.yaml
  - ../../SharedKernel/Shared/ValueTypes/Money.yaml
```

### Versioning Strategy

#### Semantic Versioning for Domains

```yaml
# _meta.yaml
version: 2.1.0  # Major.Minor.Patch

# Breaking changes increment major
# New features increment minor  
# Bug fixes increment patch
```

#### Backward Compatibility

```yaml
# Old version support
attributeUsages:
  - name: customerId
    type: primaryKey
    required: true
  
  # Deprecated field - mark for removal
  - name: legacyCustomerCode
    type: string
    required: false
    deprecated: true
    replacedBy: customerId
```

### Performance Considerations

#### Large Model Repositories

- Use git LFS for large schema files
- Implement incremental validation
- Cache validation results
- Parallel validation for independent domains

#### Build Optimization

```yaml
# Only validate changed files
- name: Get Changed Files
  run: |
    CHANGED_FILES=$(git diff --name-only HEAD~1 HEAD -- models/)
    echo "::set-output name=files::$CHANGED_FILES"

- name: Validate Changed Models
  if: steps.changes.outputs.files != ''
  run: |
    for file in ${{ steps.changes.outputs.files }}; do
      dotnet run --project src/Modeller.Domain -- ValidateModel --path "$file"
    done
```

---

## Conclusion

This guide provides a comprehensive foundation for creating maintainable, validated BDD model definitions. Key takeaways:

1. **Follow the established folder structure** for consistency
2. **Use the validation tools** early and often
3. **Leverage shared components** to reduce duplication
4. **Write meaningful BDD scenarios** for living documentation
5. **Integrate validation into your build pipeline** for quality assurance

### Next Steps

1. **Start Small**: Begin with one domain and a few entities
2. **Iterate**: Refine your approach based on team feedback
3. **Scale Gradually**: Add more domains as confidence grows
4. **Automate**: Integrate validation and generation into CI/CD
5. **Train Teams**: Ensure all stakeholders understand the approach

### Resources

- [BDD Model Definition Reference](bdd_model_definition.md)
- [MCP Validation Tools Documentation](url-endpoints.md)
- [Code Generation Guide](advice.md)

---

*For questions or support, contact the Platform Architecture team or raise an issue in the Modeller repository.*
