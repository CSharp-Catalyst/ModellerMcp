namespace Modeller.Mcp.Templates.VerticalSlice.Rules;

/// <summary>
/// Provides a set of rules for SDK generation using Vertical Slice Architecture, based on the mandatory compliance checklist.
/// </summary>
public static class SdkGenerationRules
{
    public static readonly List<CodeGenerationRule> All =
    [
        new CodeGenerationRule
        {
            Name = "GlobalUsings.cs File",
            Description = "A GlobalUsings.cs file must exist at the root of the SDK project with the System.* and Microsoft.* using statements.",
            Severity = RuleSeverity.Mandatory,
            Scope = RuleScope.Global,
            GoodExample = "global using System;\nglobal using System.Collections.Generic;\n...",
            BadExample = "using System; // in every file"
        },
        new CodeGenerationRule
        {
            Name = "Project Folder Structure",
            Description = """
                          ```text
                          {Namespace}/
                          ├── GlobalUsings.cs                             # MANDATORY - Create this first
                          ├── {FeatureName}/                              # Feature folder (e.g., Cases/)
                          └── Common/
                              ├── ApiResult.cs
                              └── ValidationExtensions.cs
                          ```
                          """,
            Severity = RuleSeverity.Critical,
            Scope = RuleScope.Global,
            BadExample = """                         
                         - Models/ folder
                         - Validators/ folder  
                         - Services/ folder
                         - Any technical layer folders
                         """
        },
        new CodeGenerationRule
        {
            Name = "Feature Folder Structure",
            Description = "All related components for a feature must be organized within a feature folder. Do not create technical layer folders like Models/, Validators/, or Services/.",
            Severity = RuleSeverity.Mandatory,
            Scope = RuleScope.Domain,
            GoodExample = "Customers/\n  CreateCustomerRequest.cs\n  CreateCustomerResponse.cs",
            BadExample = "Models/\nValidators/\nServices/"
        },
        new CodeGenerationRule
        {
            Name = "Feature Folder Files",
            Description = """
                          MUST include all relevent request and response files from the behaviours and the request validators.  
                          **NOTE** If the entity doesn't include behaviours assume Create, Read, Update, Delete is supported.
                          """,
            Format ="""
                    - {CRUD}{EntityName}Request.cs            # If applicable, add Create, Read, Update and Delete requests
                    - {CRUD}{EntityName}Response.cs           # MANDATORY - If request was added, add correspondong response
                    - {CRUD}{EntityName}Validator.cs          # MANDATORY - If request was added, add correspondong validator
                    or
                    - {BehaviourName}{EntityName}Request.cs   # MANDATORY
                    - {BehaviourName}{EntityName}Response.cs  # MANDATORY - If request was added, add correspondong response
                    - {BehaviourName}{EntityName}Validator.cs # MANDATORY - If request was added, add correspondong validator
                    """,
            Severity = RuleSeverity.Mandatory,
            Scope = RuleScope.File,
            GoodExample = "Customers/\n  CreateCustomerRequest.cs\n  CreateCustomerResponse.cs\n  CreateCustomerValidator.cs",
            BadExample = "Models/\nValidators/\nServices/"
        },
        new CodeGenerationRule
        {
            Name = "Property Declaration - Non-nullable String Fields",
            Description = "Required non-nullable string properties must use the 'required' keyword.",
            Severity = RuleSeverity.Mandatory,
            Scope = RuleScope.File,
            GoodExample = "public required string Name { get; init; }",
            BadExample = "public string Name { get; init; } = string.Empty;"
        },
        new CodeGenerationRule
        {
            Name = "Property Declaration - Optional Fields",
            Description = "Optional properties must use nullable types (e.g., string? or int?).",
            Severity = RuleSeverity.Mandatory,
            Scope = RuleScope.File,
            GoodExample = "public string? Description { get; init; }",
            BadExample = "public string Description { get; init; }"
        },
        new CodeGenerationRule
        {
            Name = "Guid Primary Key Validation",
            Description = "All Guid primary keys must be validated as Version 7 UUIDs using the provided BeVersion7Uuid method in FluentValidation rules.",
            Severity = RuleSeverity.Critical,
            Scope = RuleScope.File,
            GoodExample = "RuleFor(x => x.Id).NotEmpty().Must(BeVersion7Uuid)",
            BadExample = "RuleFor(x => x.Id).NotEmpty(); // Missing Version 7 validation"
        },
        new CodeGenerationRule
        {
            Name = "Extension Methods - ToResponse/ToEntity",
            Description = "Each entity must have extension methods for ToResponse and ToEntity mapping.",
            Severity = RuleSeverity.Mandatory,
            Scope = RuleScope.File,
            GoodExample = "public static CustomerResponse ToResponse(this Customer entity) => new() { ... };",
            BadExample = "// No extension methods implemented"
        },
        new CodeGenerationRule
        {
            Name = "Validator File Usings",
            Description = "Validator files must include 'using FluentValidation;' and 'using FluentValidation.Results;' only in validator files, not in GlobalUsings.",
            Severity = RuleSeverity.Guideline,
            Scope = RuleScope.File,
            GoodExample = "using FluentValidation;\nusing FluentValidation.Results;",
            BadExample = "global using FluentValidation; // in GlobalUsings.cs"
        },
        new CodeGenerationRule
        {
            Name = "Result Pattern",
            Description = "Use a result pattern (e.g., ApiResult<T>) for operation outcomes, including validation errors and success/failure states.",
            Severity = RuleSeverity.Guideline,
            Scope = RuleScope.File
        },
        new CodeGenerationRule
        {
            Name = "Immutability with Records",
            Description = "Use C# record types for request and response models to ensure immutability and thread safety.",
            Severity = RuleSeverity.Guideline,
            Scope = RuleScope.File
        },
        new CodeGenerationRule
        {
            Name = "Input Validation with FluentValidation",
            Description = "All inputs must be validated using FluentValidation with comprehensive rules, including length, format, and required constraints.",
            Severity = RuleSeverity.Mandatory,
            Scope = RuleScope.File
        },
        new CodeGenerationRule
        {
            Name = "XML Documentation",
            Description = "All public types and members must include comprehensive XML documentation for API consumers.",
            Severity = RuleSeverity.Guideline,
            Scope = RuleScope.File
        }
    ];
}
