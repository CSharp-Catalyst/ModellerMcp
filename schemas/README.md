# Modeller YAML Schemas

This directory contains JSON Schema definitions for all Modeller YAML file types, providing IntelliSense support in VS Code and other editors.

## Schema Files

- **`bdd-model-schema.json`** - BDD models (*.Type.yaml, *.Behaviour.yaml)
- **`enum-schema.json`** - Enum definitions (Shared/Enums/*.yaml)  
- **`attribute-types-schema.json`** - Attribute types (Shared/AttributeTypes/*.yaml)
- **`metadata-schema.json`** - Metadata files (_meta.yaml)
- **`validation-profiles-schema.json`** - Validation profiles

## Setup

See [../docs/yaml-schema-intellisense-guide.md](../docs/yaml-schema-intellisense-guide.md) for complete setup instructions.

## Quick Start (VS Code)

1. Install the **YAML** extension by Red Hat
2. The workspace settings in `.vscode/settings.json` are already configured
3. Open any YAML file and enjoy IntelliSense!

## Benefits

- ✅ Auto-completion for property names and values
- ✅ Real-time validation with error highlighting  
- ✅ Documentation hints on hover
- ✅ Type checking for required fields
- ✅ Enum suggestions for attribute types
