namespace Modeller.Mcp.Shared.Models;

public class ModelValidationResponse
{
    public List<ValidationResult> Results { get; set; } = [];
    public ModelDefinition? Model { get; set; }
}
