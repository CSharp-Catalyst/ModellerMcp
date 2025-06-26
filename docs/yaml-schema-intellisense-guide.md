# YAML Schema IntelliSense Setup Guide

This guide explains how to set up IntelliSense support for your Modeller YAML files using JSON Schema validation in VS Code and other editors.

## 📋 Overview

The Modeller project includes JSON schemas for all YAML file types, providing:

- ✅ **Auto-completion** for property names and values
- ✅ **Real-time validation** with error highlighting  
- ✅ **Documentation hints** on hover
- ✅ **Enum suggestions** for attribute types and other constrained values
- ✅ **Type checking** for required fields and data types

## 🎯 Available Schemas

| Schema File | Applies To | Description |
|-------------|------------|-------------|
| `bdd-model-schema.json` | `*.Type.yaml`, `*.Behaviour.yaml` | BDD models with attributes, behaviors, and scenarios |
| `enum-schema.json` | `Shared/Enums/*.yaml` | Enum definitions with items and values |
| `attribute-types-schema.json` | `Shared/AttributeTypes/*.yaml` | Shared attribute type definitions |
| `metadata-schema.json` | `_meta.yaml` | Folder/domain metadata files |
| `validation-profiles-schema.json` | `*validation-profiles*.yaml` | Security and validation profile definitions |

## ⚙️ VS Code Setup

### Method 1: Workspace Settings (Recommended)

1. **Install the YAML Extension**:
   - Install the **YAML** extension by Red Hat from the VS Code marketplace

2. **Configure Schema Associations**:
   - Open your workspace settings (`.vscode/settings.json`)
   - Add the following configuration:

``` json
{
  "yaml.schemas": {
    "./schemas/bdd-model-schema.json": [
      "models/**/*.Type.yaml",
      "models/**/*.Behaviour.yaml"
    ],
    "./schemas/enum-schema.json": [
      "models/**/Enums/*.yaml"
    ],
    "./schemas/attribute-types-schema.json": [
      "models/**/AttributeTypes/*.yaml"
    ],
    "./schemas/metadata-schema.json": [
      "models/**/_meta.yaml"
    ],
    "./schemas/validation-profiles-schema.json": [
      "models/**/*validation-profiles*.yaml"
    ]
  },
  "yaml.validate": true,
  "yaml.hover": true,
  "yaml.completion": true
}
```

### Method 2: In-File Schema Declaration

Add schema references directly to your YAML files:

``` yaml
# yaml-language-server: $schema=../../../schemas/bdd-model-schema.json

model: MyModel
summary: This model will have full IntelliSense support
attributeUsages:
  - name: # <-- Auto-complete will suggest 'name', 'type', 'summary', etc.
```

## 📁 File-Specific Guidelines

### BDD Models (`*.Type.yaml`, `*.Behaviour.yaml`)

**IntelliSense Features:**

- Auto-complete for `model`, `summary`, `remarks`, `attributeUsages`, `behaviours`, `scenarios`
- Attribute type suggestions from your shared definitions
- camelCase validation for attribute names
- Required field validation

**Example with IntelliSense:**

``` yaml
model: Customer
summary: Represents a customer in the system
remarks: Extends the base entity with customer-specific attributes

attributeUsages:
  - name: customerId  # Auto-complete will suggest camelCase naming
    type: primaryKey   # Dropdown with available attribute types
    required: true     # Boolean auto-complete
    summary: Unique identifier for the customer
    
  - name: email
    type: emailAddress # Will suggest from shared attribute types
    required: true
    unique: true       # New unique field support
    summary: Customer's email address
```

### Enums (`Shared/Enums/*.yaml`)

**IntelliSense Features:**

- PascalCase validation for enum and item names
- Required fields validation
- Unique value enforcement

**Example:**

``` yaml
enum: CustomerStatus
summary: Represents the current status of a customer
items:
  - name: Active      # PascalCase validation
    display: Active   # Human-readable text
    value: 1          # Integer validation
  - name: Inactive
    display: Inactive
    value: 2
```

### Attribute Types (`Shared/AttributeTypes/*.yaml`)

**IntelliSense Features:**

- Base type suggestions (`string`, `integer`, `boolean`, etc.)
- Format validation (`date`, `email`, `uri`, etc.)
- Constraint property auto-complete

**Example:**

``` yaml
attributeTypes:
  - name: customerEmail  # camelCase validation
    type: string         # Dropdown: string, integer, boolean, etc.
    extends: baseString  # References to other attribute types
    format: email        # Dropdown: email, date, uri, etc.
    summary: Customer email address format
    constraints:
      pattern: ^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$  # Regex validation
      maxLength: 255
```

### Metadata (`_meta.yaml`)

**IntelliSense Features:**

- Status dropdown with predefined values
- Version pattern validation (semantic versioning)
- Date format validation

**Example:**

``` yaml
name: PotentialSales
summary: Domain for managing potential sales and prospects
owners:
  - SalesTeam
  - CRM-Team
status: approved        # Dropdown: draft, review, approved, etc.
version: 1.2.0         # Semantic version validation
lastReviewed: 2025-06-25  # Date format validation
```

## 🚀 Benefits in Action

### Real-Time Validation

- **Red squiggles** under invalid values
- **Missing required fields** highlighted immediately
- **Type mismatches** caught before saving

### Smart Auto-Completion

- **Ctrl+Space** triggers suggestions
- **Attribute types** pulled from your shared definitions
- **Enum values** suggested based on context

### Documentation on Hover

- **Hover over properties** to see descriptions
- **Examples** and constraints shown in tooltips
- **Type information** displayed inline

## 🔧 Other Editor Support

### JetBrains IDEs (IntelliJ, WebStorm, etc.)

1. Install the **YAML** plugin
2. Configure schemas in **Settings > Languages & Frameworks > Schemas and DTDs > JSON Schema Mappings**
3. Add mappings for each schema file

### Vim/Neovim

1. Use **coc-yaml** or **vim-lsp** with yaml-language-server
2. Configure schema associations in your LSP settings

### Emacs

1. Use **lsp-mode** with yaml-language-server
2. Configure schema mappings in your LSP configuration

## 🎯 Best Practices

### 1. **Keep Schemas Updated**

- Update enum lists in `bdd-model-schema.json` when adding new shared types
- Regenerate schemas if your model structure changes significantly

### 2. **Use Consistent Naming**

- Follow camelCase for attribute names and variable names
- Use PascalCase for model names and enum names
- Stick to kebab-case for tags and identifiers

### 3. **Validate Before Committing**

- Use the IntelliSense warnings to catch issues early
- Run your MCP validator as a final check
- Consider adding schema validation to your CI/CD pipeline

### 4. **Documentation First**

- Always provide meaningful `summary` fields
- Use `remarks` for additional context when needed
- Keep examples up to date in attribute type definitions

## 🔍 Troubleshooting

### Schema Not Loading

1. Check file paths in `settings.json` are correct
2. Ensure YAML extension is installed and enabled
3. Reload VS Code window after changing settings

### Auto-Complete Not Working

1. Verify schema is associated with your file pattern
2. Check for YAML syntax errors that might break parsing
3. Use `Ctrl+Space` to manually trigger completion

### Validation Errors

1. Check that all required fields are present
2. Verify data types match schema expectations
3. Ensure enum values match exactly (case-sensitive)

## 📝 Schema Maintenance

The schemas are located in the `/schemas` directory and should be updated when:

- New shared attribute types are added
- New enum values are introduced
- Model structure changes significantly
- New validation rules are required

For questions or issues with schema configuration, contact the Modeller team or refer to the MCP validator documentation.
