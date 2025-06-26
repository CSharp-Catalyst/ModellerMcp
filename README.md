# Modeller MCP - Intelligent Domain Modeling & Code Generation

AI-Powered Domain Modeling with LLM-Driven Code Generation

---

## Overview

Modeller MCP is an advanced domain modeling and code generation platform
that combines Behaviour-Driven Development (BDD) principles with Large Language
Model (LLM) intelligence. Transform your domain models into production-ready
.NET 9 applications using Vertical Slice Architecture (VSA) patterns.

### Key Capabilities

🤖 **LLM-Driven Code Generation**: Generate production-ready C# code from
domain models using AI  
📋 **Domain Model Validation**: Comprehensive YAML schema validation and
business rule checking  
🔄 **Intelligent Code Modification**: Update existing code while preserving
custom developer changes  
🎯 **Vertical Slice Architecture**: Generate complete feature slices with API,
business logic, and data access  
📊 **Model Context Protocol**: Integrate with VS Code, GitHub Copilot, and
other AI development tools  
🛡️ **Safety & Quality**: Built-in security guards, validation pipelines, and
rollback capabilities

---

## Architecture

### Current State (Production-Ready)

- ✅ **YAML Schema Validation**: Comprehensive validation of domain models
- ✅ **Business Rule Checking**: Validation of model constraints and
  relationships  
- ✅ **MCP Integration**: Model Context Protocol server for AI tool integration
- ✅ **Prompt-Based Analysis**: AI-powered model analysis and recommendations
- ✅ **VS Code Integration**: IntelliSense and schema support for model
  authoring
- ✅ **Security Framework**: Enterprise-grade security for LLM interactions
- ✅ **Audit Logging**: Comprehensive audit trails for compliance and
  monitoring
- ✅ **Secure Code Generation**: LLM-driven code generation with security
  controls

### Advanced Features (Ready for Integration)

- ✅ **Intelligent Code Generation**: Transform models into production-ready
  .NET 9 code
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

- .NET 9.0 SDK or later
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
dotnet run --project src/Modeller.McpServer -- ValidateDomain --domainPath "models/JJs/PotentialSales"

# Validate project structure
dotnet run --project src/Modeller.McpServer -- ValidateStructure --modelsPath "models"
```

#### 2. AI-Powered Analysis (via MCP)

Use with VS Code and GitHub Copilot:

```text
@Modeller analyze_model --modelPath "models/JJs/PotentialSales/Prospect.Type.yaml"
@Modeller review_domain --domainPath "models/JJs/PotentialSales"
@Modeller create_template --modelType "Type" --domain "Sales"
```

#### 3. Code Generation (Coming Soon)

```bash
# Generate complete VSA project from domain models
modeller generate --project WebAPI --output ./src --models ./models/JJs

# Update existing code based on model changes  
modeller update --project ./src --models ./JJs --diff
```

---

## Project Structure

```text
ModellerMcp/
├── models/                          # Domain model definitions
│   └── JJs/
│       ├── PotentialSales/         # Example domain
│       │   ├── _meta.yaml
│       │   ├── Prospect.Type.yaml
│       │   ├── Activity.Type.yaml
│       │   └── ...
│       └── Shared/                 # Shared types and enums
│           ├── AttributeTypes/
│           └── Enums/
├── src/
│   └── Modeller.McpServer/         # MCP server implementation
├── tests/                          # Unit and integration tests
├── docs/                           # Documentation
│   ├── code-generation-design.md   # LLM-driven code generation design
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

### 3. Secure Code Generation (Production Ready)

- Generate production-ready .NET 9 applications with security controls
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
- [Validation Framework](docs/) - Schema validation and business rules

---

## Contributing

This project uses advanced AI-driven development practices. When contributing:

1. **Model Changes**: Validate all models before committing
2. **Code Generation**: Test generated code thoroughly
3. **Documentation**: Update guides to reflect changes
4. **AI Integration**: Ensure MCP tools work correctly

---

## Roadmap

### Current (Q2 2025) - COMPLETED

- ✅ YAML schema validation and IntelliSense
- ✅ MCP server integration with AI tools
- ✅ Comprehensive model analysis prompts
- ✅ Enterprise-grade security framework implementation
- ✅ LLM-driven secure code generation with audit logging
- ✅ Prompt injection prevention and security validation
- ✅ Immutable response tracking and compliance support

### Next (Q3 2025)

- 🎯 Production deployment and integration testing
- 🎯 Vertical Slice Architecture template library
- 🎯 Advanced code modification with safety checks
- 🎯 Developer workflow and tooling integration

### Future (Q4 2025+)

- 🔮 Multi-language code generation support
- 🔮 Advanced AI code optimization and learning
- 🔮 Community template marketplace
- 🔮 Advanced enterprise governance and policy management

---

*For questions or support, please raise an issue in the repository or contact
the development team.*
