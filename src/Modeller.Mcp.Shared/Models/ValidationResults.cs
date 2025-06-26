namespace Modeller.Mcp.Shared.Models;

public record ValidationResult(string File, string Message, ValidationSeverity Severity);
