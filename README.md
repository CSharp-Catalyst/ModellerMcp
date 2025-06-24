# ModellerMcp

Behaviour-Driven Development (BDD) Model Definition & Validation Framework

---

## Overview

ModellerMcp is a framework for defining, validating, and managing domain models using Behaviour-Driven Development (BDD) principles. It enables teams to create structured, self-documenting models in YAML, supporting code generation, validation, and collaboration between technical and business stakeholders.

---

## Key Features

- **Standardized Model Definitions**: Consistent YAML-based model files for entities, value types, enums, and behaviours
- **BDD Integration**: Living documentation with Given-When-Then scenarios
- **Validation Tools**: Schema and business rule validation for all models
- **Code Generation Ready**: Models act as the source of truth for API, database, and documentation generation
- **CI/CD Integration**: Easily validate models in build pipelines and pre-commit hooks
- **Collaboration**: Accessible format for both developers and domain experts

---

## Folder Structure Example
```
models/
└── YourCompany/
    ├── Sales/
    │   ├── _meta.yaml
    │   ├── Customer.Type.yaml
    │   ├── Customer.Behaviour.yaml
    │   └── ...
    └── Shared/
        ├── AttributeTypes/
        │   └── CommonAttributes.yaml
        ├── Enums/
        │   └── Status.yaml
        └── ValueTypes/
            └── Address.yaml
```
---

## Getting Started

### Prerequisites
- .NET 9.0 SDK or later
- Visual Studio Code (recommended) or Visual Studio
- YAML extension for your IDE
- Git for version control

### Setup# Clone the repository
git clone <repository-url>
cd ModellerMcp

# Build the validation tools
dotnet build src/Modeller.Domain

# Verify tools are working
dotnet run --project src/Modeller.Domain
### Model Validation# Discover all models
dotnet run --project src/Modeller.Domain -- DiscoverModels --solutionPath "."

# Validate a domain
dotnet run --project src/Modeller.Domain -- ValidateDomain --domainPath "models/YourCompany/YourDomain"

# Validate overall structure
dotnet run --project src/Modeller.Domain -- ValidateStructure --modelsPath "models"

# Validate a single model file
dotnet run --project src/Modeller.Domain -- ValidateModel --path "models/YourCompany/YourDomain/Customer.Type.yaml"
---

## Model Definition Best Practices
- Use PascalCase for file names and model names
- Use camelCase for attribute and behaviour names
- Separate entity types (`.Type.yaml`) and behaviours (`.Behaviour.yaml`)
- Leverage shared attribute types and enums
- Provide clear summaries and documentation in YAML
- Use BDD scenarios for business rules
- Keep metadata up to date in `_meta.yaml`

---

## CI/CD Integration Example

GitHub Actions:

``` yaml
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
---

## References & Further Reading
- [BDD Model Definition User Guide](docs/BDD_Model_User_Guide.md)
- [BDD Model Definition Reference](docs/bdd_model_definition.md)
- [MCP Validation Tools Documentation](docs/url-endpoints.md)
- [Code Generation Guide](docs/advice.md)

---

*For questions or support, contact the Platform Architecture team or raise an issue in the repository.*