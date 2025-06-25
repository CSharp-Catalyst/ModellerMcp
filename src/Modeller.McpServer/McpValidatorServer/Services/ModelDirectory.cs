namespace Modeller.McpServer.McpValidatorServer.Services;

public class ModelDirectory
{
    public string Path { get; set; } = string.Empty;
    public bool IsRoot { get; set; }
    public List<ModelFileGroup> ModelGroups { get; set; } = [];
}
