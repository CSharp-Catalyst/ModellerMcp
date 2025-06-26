namespace Modeller.Mcp.Shared.Services;

public class ModelFileInfo
{
    public required string Path { get; set; }
    public required string Name { get; set; }
    public ModelFileType Type { get; set; }

    public string Extension => System.IO.Path.GetExtension(Name);
    public bool IsYaml => Extension.Equals(".yaml", StringComparison.OrdinalIgnoreCase) ||
                          Extension.Equals(".yml", StringComparison.OrdinalIgnoreCase);
}
