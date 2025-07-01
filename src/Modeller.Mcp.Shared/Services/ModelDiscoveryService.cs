namespace Modeller.Mcp.Shared.Services;

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

            if (result.ModelDirectories.Count == 0)
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

            if (yamlFiles.Count == 0) return;

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

            if (yamlFiles.Count != 0)
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

            // Check for BDD models - files containing model definitions
            if (content.Contains("model:"))
            {
                bool hasAttributeUsages = content.Contains("attributeUsages:");
                bool hasBehaviours = content.Contains("behaviours:");
                bool hasScenarios = content.Contains("scenarios:");

                // If it has model: and any of the BDD components, it's a BDD model
                if (hasAttributeUsages || hasBehaviours || hasScenarios)
                {
                    return ModelFileType.BddModel;
                }
            }

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
