namespace Modeller.McpServer.McpValidatorServer.Models;

public record ValidationRequest(string Path);
public record ValidationResponse(IReadOnlyList<ValidationResult> Results);

public interface IMcpModelValidator
{
    Task<IReadOnlyList<ValidationResult>> ValidateAsync(string filePath, CancellationToken cancellationToken);
}

public record ValidationResult(string File, string Message, ValidationSeverity Severity);

public enum ValidationSeverity
{
    Info,
    Warning,
    Error
}