namespace Modeller.McpServer.McpValidatorServer.Services;

public class ModelDirectory
{
    public required string Path { get; set; }
    public bool IsRoot { get; set; }
    public List<ModelFileGroup> ModelGroups { get; set; } = [];
}
