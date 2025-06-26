namespace Modeller.Mcp.Shared.Services;

public class ModelDiscoveryResult
{
    public List<ModelDirectory> ModelDirectories { get; set; } = [];
    public List<ModelFileInfo> LooseFiles { get; set; } = [];
    public List<string> Errors { get; set; } = [];

    public bool HasModels => ModelDirectories.Any() || LooseFiles.Any();
    public int TotalFileCount => ModelDirectories.Sum(d => d.ModelGroups.Sum(g => g.Files.Count)) + LooseFiles.Count;
}
