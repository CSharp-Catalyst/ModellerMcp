namespace Modeller.McpServer.McpValidatorServer.Models;

public record ValidationResponse(IReadOnlyList<ValidationResult> Results);
