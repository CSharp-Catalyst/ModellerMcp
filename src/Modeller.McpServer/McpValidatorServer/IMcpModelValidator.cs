using Modeller.McpServer.McpValidatorServer.Models;

namespace Modeller.McpServer.McpValidatorServer;

public interface IMcpModelValidator
{
    Task<ModelValidationResponse> ValidateAsync(string filePath, CancellationToken cancellationToken);
}
