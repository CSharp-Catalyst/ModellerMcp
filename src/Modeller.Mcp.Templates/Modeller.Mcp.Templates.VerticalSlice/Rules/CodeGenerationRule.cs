namespace Modeller.Mcp.Templates.VerticalSlice.Rules;

public record CodeGenerationRule
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public RuleSeverity Severity { get; set; } = RuleSeverity.Guideline;
    public RuleScope Scope { get; set; } = RuleScope.File;
    public string? Format { get; set; }
    public string? GoodExample { get; set; }
    public string? BadExample { get; set; }
}

public enum RuleScope
{
    Global,
    Domain,
    File
}

[Flags]
public enum RuleSeverity
{
    Critical,
    Mandatory,
    Guideline,
    Optional
}

public static class RuleSeverityExtensions
{
    public static bool IsCritical(this RuleSeverity severity) => severity.HasFlag(RuleSeverity.Critical);
    public static bool IsMandatory(this RuleSeverity severity) => severity.HasFlag(RuleSeverity.Mandatory);
    public static bool IsGuideline(this RuleSeverity severity) => severity.HasFlag(RuleSeverity.Guideline);
    public static bool IsOptional(this RuleSeverity severity) => severity.HasFlag(RuleSeverity.Optional);
}

