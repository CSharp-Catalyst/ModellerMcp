# Modeller MCP - Intelligent Domain Modeling & Code Generation

AI-Powered Domain Modeling with LLM-Driven SDK & API Generation

---

## Overview

Modeller MCP is an advanced domain modeling and code generation platform
that combines Behaviour-Driven Development (BDD) principles with Large Language
Model (LLM) intelligence. Transform your domain models into production-ready
modern .NET SDKs and Minimal APIs using Vertical Slice Architecture (VSA) patterns.

### Key Capabilities

🤖 **LLM-Driven SDK Generation**: Generate complete .NET SDKs from domain models using AI  
� **Minimal API Generation**: Create full API projects that integrate with generated SDKs  
�📋 **Domain Model Validation**: Comprehensive YAML schema validation and business rule checking  
🔄 **Intelligent Code Modification**: Update existing code while preserving custom developer changes  
🎯 **Vertical Slice Architecture**: Generate complete feature slices with models, validators, and services  
📊 **Model Context Protocol**: Integrate with VS Code, GitHub Copilot, and other AI development tools  
🛡️ **Safety & Quality**: Built-in security guards, validation pipelines, and rollback capabilities

---

## Architecture

### Current State (Production-Ready)

- ✅ **YAML Schema Validation**: Comprehensive validation of domain models
- ✅ **Business Rule Checking**: Validation of model constraints and relationships  
- ✅ **MCP Integration**: Model Context Protocol server for AI tool integration
- ✅ **Prompt-Based Analysis**: AI-powered model analysis and recommendations
- ✅ **VS Code Integration**: IntelliSense and schema support for model authoring
- ✅ **Security Framework**: Enterprise-grade security for LLM interactions
- ✅ **Audit Logging**: Comprehensive audit trails for compliance and monitoring
- ✅ **Secure Code Generation**: LLM-driven code generation with security controls
- ✅ **SDK Generation**: Complete .NET SDK generation from domain models
- ✅ **Minimal API Generation**: Full API projects with SDK integration

### Advanced Features (Ready for Integration)

- ✅ **Intelligent Code Generation**: Transform models into production-ready
  .NET code
- ✅ **Security Context Validation**: Multi-level security validation and
  sanitization
- ✅ **Prompt Injection Prevention**: Advanced protection against malicious
  prompts
- ✅ **Immutable Response Tracking**: Tamper-proof recording of all LLM
  interactions
- ✅ **Post-Generation Validation**: Automated validation of generated code
  quality
- ✅ **Enterprise Audit Support**: Full audit trails for regulatory compliance

---

## Quick Start

### Prerequisites

- .NET SDK (latest stable LTS version) or later
- Visual Studio Code (recommended)
- YAML extension for VS Code
- Git for version control

### Installation

```bash
# Clone the repository
git clone <repository-url>
cd ModellerMcp

# Build the MCP server
dotnet build src/Modeller.McpServer

# Run the MCP server
dotnet run --project src/Modeller.McpServer
```

### Basic Usage

#### 1. Model Validation

```bash
# Discover all models in your project
dotnet run --project src/Modeller.McpServer -- DiscoverModels --solutionPath "."

# Validate a specific domain
dotnet run --project src/Modeller.McpServer -- ValidateDomain --domainPath "models/Business/CustomerManagement"

# Validate project structure
dotnet run --project src/Modeller.McpServer -- ValidateStructure --modelsPath "models"
```

#### 2. AI-Powered Analysis (via MCP)

Use with VS Code and GitHub Copilot:

```text
@Modeller analyze_model --modelPath "models/Business/CustomerManagement/Customer.Type.yaml"
@Modeller review_domain --domainPath "models/Business/CustomerManagement"
@Modeller create_template --modelType "Type" --domain "Sales"
```

#### 3. SDK Generation

```bash
# Generate complete .NET SDK from domain models
@Modeller GenerateSDK --domainPath "models/Business/CustomerManagement" --featureName "CustomerManagement" --namespaceName "Business.CustomerManagement.Sdk" --outputPath "./generated-sdk"
```

#### 4. Minimal API Generation

```bash
# Generate complete Minimal API project that uses the generated SDK
@Modeller GenerateMinimalAPI --sdkPath "./generated-sdk" --domainPath "models/Business/CustomerManagement" --projectName "CustomerManagement.Api" --namespaceName "CustomerManagement.Api" --outputPath "./generated-api"
```

---

## Project Structure

```text
ModellerMcp/
├── models/                          # Domain model definitions
│   └── Business/                   # Organization namespace
│       ├── CustomerManagement/    # Example domain
│       │   ├── _meta.yaml
│       │   ├── Customer.Type.yaml
│       │   ├── Activity.Type.yaml
│       │   └── ...
│       └── Shared/                 # Shared types and enums
│           ├── AttributeTypes/
│           └── Enums/
├── src/
│   ├── Modeller.McpServer/         # MCP server implementation
│   └── Modeller.Mcp.Shared/       # Shared libraries and services
│       ├── CodeGeneration/         # SDK and API generation services
│       ├── Tools/                  # MCP tools for validation and generation
│       └── Services/               # Core business services
├── generated-sdk/                  # Generated SDK output (example)
│   ├── CustomerManagement.Sdk.csproj
│   ├── Models/                     # Generated entity models
│   ├── Validators/                 # FluentValidation validators
│   ├── Enums/                      # Strongly-typed enumerations
│   └── Common/                     # Result patterns and utilities
├── generated-api/                  # Generated API output (example)
│   ├── CustomerManagement.Api.csproj
│   ├── Program.cs                  # Minimal API setup
│   ├── Data/                       # Entity Framework context
│   ├── Services/                   # Business services
│   └── Endpoints/                  # REST API endpoints
├── tests/                          # Unit and integration tests
├── docs/                           # Documentation
│   ├── code-generation-design.md   # LLM-driven code generation design
│   ├── sdk-api-generation-guide.md # Complete generation workflow guide
│   ├── yaml-schema-intellisense-guide.md
│   └── modeller-mcp-prompts-guide.md
└── schemas/                        # JSON schemas for validation
```

---

## Development Workflow

### 1. Model Definition

- Define domain models using YAML with IntelliSense support
- Validate models using comprehensive schema validation
- Use BDD scenarios for business rule specification

### 2. AI-Powered Analysis

- Leverage MCP integration with VS Code and GitHub Copilot
- Get intelligent suggestions for model improvements
- Analyze cross-model consistency and best practices

### 3. SDK & API Generation

- Generate complete .NET SDKs from domain models with security controls
- Create Minimal API projects that integrate with generated SDKs
- LLM-driven code generation with comprehensive security framework
- Enterprise-grade audit logging and compliance tracking
- Multi-level security validation and prompt injection prevention
- Use Vertical Slice Architecture patterns
- Maintain developer customizations during updates

---

## Model Definition Best Practices

- **Naming**: Use PascalCase for types, camelCase for attributes
- **Structure**: Separate entity types (`.Type.yaml`) and behaviours
  (`.Behaviour.yaml`)
- **Reusability**: Leverage shared attribute types and enums
- **Documentation**: Provide clear summaries and descriptions
- **Validation**: Use BDD scenarios for business rules
- **Consistency**: Follow established domain patterns

---

## Documentation

### Core Guides

- [SDK & API Generation Guide](docs/sdk-api-generation-guide.md) - Complete
  workflow from models to running applications
- [Code Generation Design](docs/code-generation-design.md) - LLM-driven code
  generation architecture
- [BDD Model User Guide](docs/BDD_Model_User_Guide.md) - Comprehensive
  modeling guide
- [MCP Prompts Guide](docs/modeller-mcp-prompts-guide.md) - AI integration
  usage
- [YAML Schema Guide](docs/yaml-schema-intellisense-guide.md) - Schema and
  IntelliSense setup

### Technical References

- [Model Definition Specification](docs/bdd_model_definition.md) - YAML format
  reference
- [Security Implementation Status](docs/security-implementation-status.md) -
  Enterprise security features

---

## Contributing

This project uses advanced AI-driven development practices. When contributing:

1. **Model Changes**: Validate all models before committing
2. **Code Generation**: Test generated code thoroughly
3. **Documentation**: Update guides to reflect changes
4. **AI Integration**: Ensure MCP tools work correctly

---

## Roadmap

### Current Release - COMPLETED

- ✅ YAML schema validation and IntelliSense
- ✅ MCP server integration with AI tools
- ✅ Comprehensive model analysis prompts
- ✅ Enterprise-grade security framework implementation
- ✅ LLM-driven secure code generation with audit logging
- ✅ Prompt injection prevention and security validation
- ✅ Immutable response tracking and compliance support
- ✅ Complete .NET SDK generation from domain models
- ✅ Minimal API generation with SDK integration
- ✅ VSA pattern implementation for generated code

### Next Release

- 🎯 Production deployment and integration testing
- 🎯 Enhanced code modification with safety checks
- 🎯 Developer workflow and tooling integration
- 🎯 Advanced template marketplace and community contributions

### Future Releases

- 🔮 Multi-language code generation support
- 🔮 Advanced AI code optimization and learning
- 🔮 Community template marketplace
- 🔮 Advanced enterprise governance and policy management

---

*For questions or support, please raise an issue in the repository or contact
the development team.*
