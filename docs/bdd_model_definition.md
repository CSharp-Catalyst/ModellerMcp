# Modeller MCP

## Behaviour-Driven Development (BDD) and Model Definition for MCP Tools

### Purpose

This document provides an introduction to Behaviour-Driven Development (BDD) and describes how to define a domain model in a structured format that can be consumed by MCP tools for generating software solutions.

---

### 1. What is Behaviour-Driven Development (BDD)?

Behaviour-Driven Development is a software development process that encourages collaboration among developers, QA, and non-technical stakeholders. It uses simple language constructs to describe system behaviour in terms of examples.

**Key Goals of BDD:**

- Improve shared understanding of system requirements
- Encourage ubiquitous language between technical and non-technical roles
- Provide living documentation through executable specifications

**Typical BDD Syntax (Gherkin):**

```gherkin
Feature: User Login
  Scenario: Successful login
    Given a registered user
    When they provide valid credentials
    Then they are redirected to the dashboard
```

This structured format ensures that behaviours are consistently defined and testable.

---

### 2. Model-Driven Approach in MCP Tools

In Modeller MCP, models are defined using structured files (e.g., YAML or JSON) that describe domain concepts, attributes, behaviours, and relationships. These models act as the source of truth for code generation, documentation, and testing.

#### Key Concepts:

- **Entity**: A clearly defined object in the domain model with identity and lifecycle (e.g., User, Order)
- **Attribute Type**: A reusable definition of a data value, including its name, data type, format, and constraints (e.g., string, email, GUID)
- **Attribute Usage**: The contextual application of an attribute type, including validation, required status, default values, and descriptive metadata
- **Behaviour**: A domain-level operation that may utilise one or more entities
- **Scenario**: Optional BDD-style definition of usage or edge cases to describe behaviour in context
- **Model Ownership**: Entities can specify which model they are owned by using an `ownedBy` field. This supports a bottom-up understanding of aggregate structures by describing containment from the perspective of the child.

---

### 3. Example Model Structure

```yaml
model: UserAccount
attributeUsages:
  - type: email
    required: true
    summary: Email address used for login
    remarks: Must be a unique, valid email format
  - type: isActive
    default: false
    summary: Indicates whether the account is active
    remarks: Default is false until user activation occurs

behaviours:
  - name: activateUserAccount
    description: Activate a user account if it is inactive
    entities:
      - UserAccount
    preconditions:
      - UserAccount.isActive is false
    effects:
      - set UserAccount.isActive to true

scenarios:
  - name: activate inactive user
    given:
      - UserAccount.isActive is false
    when:
      - activateUserAccount is called
    then:
      - UserAccount.isActive is true
```

```yaml
attributeTypes:
  - name: baseString
    type: string
    constraints:
      maxLength: 512
  
  - name: email
    extends: baseString
    format: email
    constraints:
      pattern: ^[^@\s]+@[^@\s]+\.[^@\s]+$
      maxLength: 255
      example: user@example.com

  - name: isActive
    type: boolean
```

---

### 4. Writing Effective Models for BDD & MCP

To maximise the benefit of this approach:

- Use meaningful names for models, attributes, and behaviours
- Separate attribute type from usage to promote reuse and clarity
- Define attributes with summaries and remarks for self-documenting models
- Write clear, atomic behaviours that represent user intentions
- Use `ownedBy` to describe containment and support aggregate discovery
- Keep scenarios short and expressive to document edge cases and business rules

---

### 5. Attribute Types and Constraints

Attribute types define the structure and constraints of a value at the data level. These constraints are intrinsic to the type itself and ensure consistent validation across usages.

#### Common Constraint Fields

- `minLength` / `maxLength`: Length bounds for strings
- `minimum` / `maximum`: Value ranges for numbers and dates
- `pattern`: Regex pattern for strings
- `decimalPlaces`: Fixed precision for numeric values
- `enum`: Explicit list of acceptable values
- `format`: Semantic hints such as `email`, `uri`, `date`
- `nullable`: Whether the type can be null
- `unit`: Domain-specific unit of measure (e.g. %, AUD, kg)
- `example`: Representative value used for documentation

#### Constraint Inheritance

Attribute types may extend other types to inherit base constraints. This reduces duplication and promotes consistent modelling.

```yaml
attributeTypes:
  - name: baseString
    type: string
    constraints:
      maxLength: 512

  - name: email
    extends: baseString
    format: email
    constraints:
      pattern: ^[^@\s]+@[^@\s]+\.[^@\s]+$
      maxLength: 255
```

#### Validation Profiles

Validation profiles allow conditional constraint enforcement. A profile groups a named set of rules that can be applied selectively during validation or at runtime. Instead of relying on static roles, profiles may reference **claims** to express access or permission logic more flexibly.

```yaml
validationProfiles:
  - name: registration
    claims:
      - action: create
        resource: user
    attributeRules:
      email:
        required: true
      isActive:
        default: false

  - name: updateProfile
    claims:
      - action: update
        resource: user
    attributeRules:
      email:
        required: false
    behaviourRules:
      activateUserAccount:
        allowed: true

  - name: readonly
    claims:
      - action: view
        resource: user
    behaviourRules:
      activateUserAccount:
        allowed: false
```

---

### 6. Value Types and Identity-Free Modelling

Not all models represent entities with identities or lifecycle operations. Some exist purely to encapsulate values with consistent structure and rules — for example, `Money`, `Address`, or `DateRange`.

These value types:

- Have no identity
- Are often immutable
- Are defined like models but typically lack `id` or behaviours

They can be used directly inside other models, promoting reuse and a rich, semantically expressive domain.

```yaml
model: Money
attributeUsages:
  - type: amount
    required: true
  - type: currency
    required: true
```

Used in:

```yaml
model: Invoice
attributeUsages:
  - type: invoiceId
    required: true
  - name: total
    type: Money
    required: true
```

---

### 7. Project Layout and Modularity

As domains scale, centralised model files become harder to maintain. To support modularisation, it is recommended to structure models in folders that reflect the project’s architecture and bounded contexts.

#### Suggested Folder Structure

```bash
/models/
  ProjectName/                  # Top-level project or solution name
    Organisation/              # 2nd level: bounded context / domain
      Sites/                   # 3rd level: feature or entity grouping
        Site.Type.yaml         # Entity and attribute usage definition
        Site.Behaviour.yaml    # Behaviour definitions for this model
    Cases/
      Case.Type.yaml
      Case.Behaviour.yaml
    Shared/
      ValueTypes/
        Address.yaml
      Enums/
        Priority.yaml
      Attributes.yaml
```

#### Conventions

- \`\`: Contains the structure and attribute usages
- \`\`: Contains behaviours and optional BDD-style scenarios
- **Value types and enums** should reside in shared folders

#### Naming Conventions

- File names must use **PascalCase** to match model names (e.g. `Case.Type.yaml` not `case.type.yaml`)
- The root key (`model`, `enum`, etc.) must match the file name (excluding extension)
- Attribute and behaviour names must be **camelCase** for consistency
- Avoid abbreviations unless they’re domain-specific and well understood

#### Folder-Level Metadata

Each folder may contain an optional `_meta.yaml` file that describes the purpose and context of that bounded context or grouping. For example:

```yaml
name: Cases
summary: Models and behaviours related to the lifecycle of customer-reported cases
description: >
  This context encapsulates all entities, behaviours, and types necessary for managing
  customer and internal cases. It includes state transitions, origin tracking, and activity logging.
owners:
  - catherine.huang@jjswaste.com.au
  - mcp-model-admin@yourdomain.local
tags:
  - case-management
  - incidents
  - support
dependencies:
  - ../Organisation/Tenant.Type.yaml
  - ../Organisation/Site.Type.yaml
  - ../Shared/Enums/CasePriorityType.yaml
  - ../Shared/ValueTypes/Address.yaml
version: 1.0.0
status: active
lastReviewed: 2025-06-24
```

This metadata format supports documentation tooling and helps teams understand context ownership, relationships, and status.

#### Schema Linting Rules

- Each file must validate against the MCP schema spec
- Required sections: `model` or `enum`, `attributeUsages` (for models), `name` fields
- Avoid duplicate model names across contexts unless explicitly versioned
- Prefer referencing shared types rather than redefining them
- Warn if `lastReviewed` in `_meta.yaml` is older than a set threshold (e.g. 90 days)

---

### 8. Next Steps

Once your model is defined:

1. Validate the syntax with the MCP model validator
2. Run the model through the code generation pipeline
3. Use generated tests as specifications
4. Iterate based on feedback and extend models with new scenarios

---

### References

- Dan North’s original BDD article: [https://dannorth.net/introducing-bdd/](https://dannorth.net/introducing-bdd/)
- Gherkin syntax: [https://cucumber.io/docs/gherkin/](https://cucumber.io/docs/gherkin/)
- YAML 1.2 Specification: [https://yaml.org/spec/1.2/spec.html](https://yaml.org/spec/1.2/spec.html)

---

By aligning BDD scenarios with structured model definitions, the MCP toolchain enables clear communication, maintainable systems, and traceable requirements in software projects.

