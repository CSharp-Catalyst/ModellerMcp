namespace Modeller.McpServer.McpValidatorServer.Services;

public class ModelFileGroup
{
    public string Directory { get; set; } = string.Empty;
    public List<ModelFileInfo> Files { get; set; } = [];
    public bool HasMetadata { get; set; }
    public string? MetadataPath { get; set; }

    public string Name => Path.GetFileName(Directory);
    public bool HasTypeFile => Files.Any(f => f.Type == ModelFileType.BddModel || f.Name.EndsWith(".Type.yaml"));
    public bool HasBehaviourFile => Files.Any(f => f.Name.EndsWith(".Behaviour.yaml") || f.Name.EndsWith(".Behavior.yaml"));
}
