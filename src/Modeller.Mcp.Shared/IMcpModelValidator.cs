using Modeller.Mcp.Shared.Models;

namespace Modeller.Mcp.Shared;

public interface IMcpModelValidator
{
    Task<ModelValidationResponse> ValidateAsync(string filePath, CancellationToken cancellationToken);
}
