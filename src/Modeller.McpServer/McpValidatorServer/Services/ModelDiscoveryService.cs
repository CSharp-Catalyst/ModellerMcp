namespace Modeller.McpServer.McpValidatorServer.Services;

public class ModelDiscoveryService
{
    public ModelDiscoveryResult DiscoverModels(string rootPath)
    {
        var result = new ModelDiscoveryResult();
        if (!Directory.Exists(rootPath))
        {
            result.Errors.Add($"Root path does not exist: {rootPath}");
            return result;
        }

        try
        {
            var potentialModelPaths = new[]
            {
                Path.Combine(rootPath, "models"),
                Path.Combine(rootPath, "src", "models"),
            };

            foreach (var modelPath in potentialModelPaths.Where(d => Directory.Exists(d)))
                ScanDirectory(modelPath, result);

            if (!result.ModelDirectories.Any())
                ScanForYamlFiles(rootPath, result);
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error during model discovery: {ex.Message}");
        }

        return result;
    }

    private void ScanDirectory(string directory, ModelDiscoveryResult result)
    {
        try
        {
            var yamlFiles = Directory.GetFiles(directory, "*.yaml", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(directory, "*.yml", SearchOption.AllDirectories))
                .ToList();

            if (!yamlFiles.Any()) return;

            var modelDir = new ModelDirectory
            {
                Path = directory,
                IsRoot = true
            };

            var directoryGroups = yamlFiles
                .GroupBy(f => Path.GetDirectoryName(f))
                .Where(g => g.Key != null);

            foreach (var group in directoryGroups)
            {
                var dirPath = group.Key!;
                var files = group.ToList();

                var modelFiles = new ModelFileGroup
                {
                    Directory = dirPath,
                    Files = files.Select(f => new ModelFileInfo
                    {
                        Path = f,
                        Name = Path.GetFileName(f),
                        Type = DetermineFileType(f)
                    }).ToList()
                };

                var metaFile = Path.Combine(dirPath, "_meta.yaml");
                if (File.Exists(metaFile))
                {
                    modelFiles.HasMetadata = true;
                    modelFiles.MetadataPath = metaFile;
                }

                modelDir.ModelGroups.Add(modelFiles);
            }

            result.ModelDirectories.Add(modelDir);
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error scanning directory {directory}: {ex.Message}");
        }
    }

    private void ScanForYamlFiles(string rootPath, ModelDiscoveryResult result)
    {
        try
        {
            var yamlFiles = Directory.GetFiles(rootPath, "*.yaml", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(rootPath, "*.yml", SearchOption.AllDirectories))
                .Where(f => !f.Contains("bin") && !f.Contains("obj") && !f.Contains("node_modules"))
                .ToList();

            if (yamlFiles.Any())
            {
                result.LooseFiles.AddRange(yamlFiles.Select(f => new ModelFileInfo
                {
                    Path = f,
                    Name = Path.GetFileName(f),
                    Type = DetermineFileType(f)
                }));
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error scanning for YAML files: {ex.Message}");
        }
    }

    private ModelFileType DetermineFileType(string filePath)
    {
        var fileName = Path.GetFileName(filePath);

        if (fileName.Equals("_meta.yaml", StringComparison.OrdinalIgnoreCase) ||
            fileName.Equals("_meta.yml", StringComparison.OrdinalIgnoreCase))
        {
            return ModelFileType.Metadata;
        }

        try
        {
            var content = File.ReadAllText(filePath);

            if (content.Contains("model:") && (content.Contains("attributeUsages:") || content.Contains("behaviours:")))
                return ModelFileType.BddModel;

            if (content.Contains("attributeTypes:"))
                return ModelFileType.AttributeTypes;

            if (content.Contains("enum:") || content.Contains("items:") && content.Contains("name:") && content.Contains("display:"))
                return ModelFileType.Enum;

            if (content.Contains("validationProfiles:"))
                return ModelFileType.ValidationProfiles;
        }
        catch
        {
            // If we can't read the file, just return unknown
        }

        return ModelFileType.Unknown;
    }
}

public class ModelDiscoveryResult
{
    public List<ModelDirectory> ModelDirectories { get; set; } = [];
    public List<ModelFileInfo> LooseFiles { get; set; } = [];
    public List<string> Errors { get; set; } = [];

    public bool HasModels => ModelDirectories.Any() || LooseFiles.Any();
    public int TotalFileCount => ModelDirectories.Sum(d => d.ModelGroups.Sum(g => g.Files.Count)) + LooseFiles.Count;
}

public class ModelDirectory
{
    public string Path { get; set; } = string.Empty;
    public bool IsRoot { get; set; }
    public List<ModelFileGroup> ModelGroups { get; set; } = [];
}

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

public class ModelFileInfo
{
    public string Path { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ModelFileType Type { get; set; }

    public string Extension => System.IO.Path.GetExtension(Name);
    public bool IsYaml => Extension.Equals(".yaml", StringComparison.OrdinalIgnoreCase) ||
                          Extension.Equals(".yml", StringComparison.OrdinalIgnoreCase);
}
