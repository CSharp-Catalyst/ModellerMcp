namespace Modeller.McpServer.McpValidatorServer.Services;

public class ModelFileInfo
{
    public string Path { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ModelFileType Type { get; set; }

    public string Extension => System.IO.Path.GetExtension(Name);
    public bool IsYaml => Extension.Equals(".yaml", StringComparison.OrdinalIgnoreCase) ||
                          Extension.Equals(".yml", StringComparison.OrdinalIgnoreCase);
}
