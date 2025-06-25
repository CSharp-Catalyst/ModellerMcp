namespace Modeller.McpServer.McpValidatorServer.Models;

public record ValidationResult(string File, string Message, ValidationSeverity Severity);
