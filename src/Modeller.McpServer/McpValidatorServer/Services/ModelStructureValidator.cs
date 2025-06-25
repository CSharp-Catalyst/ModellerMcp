using Modeller.McpServer.McpValidatorServer.Models;

using System.Text.RegularExpressions;

namespace Modeller.McpServer.McpValidatorServer.Services;

public class ModelStructureValidator
{
    private readonly List<ValidationResult> _results = [];

    public async Task<IReadOnlyList<ValidationResult>> ValidateStructureAsync(string rootPath, CancellationToken cancellationToken = default)
    {
        _results.Clear();

        if (!Directory.Exists(rootPath))
        {
            _results.Add(new ValidationResult(rootPath, "Root path does not exist", ValidationSeverity.Error));
            return _results;
        }

        await ValidateProjectStructureAsync(rootPath, cancellationToken);

        return _results;
    }

    private async Task ValidateProjectStructureAsync(string rootPath, CancellationToken cancellationToken)
    {
        var modelDirectories = Directory.GetDirectories(rootPath, "*", SearchOption.AllDirectories)
            .Where(dir => ContainsModelFiles(dir))
            .ToList();

        if (!modelDirectories.Any())
        {
            _results.Add(new ValidationResult(rootPath, "No model directories found", ValidationSeverity.Warning));
            return;
        }

        foreach (var modelDir in modelDirectories)
        {
            await ValidateModelDirectoryAsync(modelDir, cancellationToken);
        }
    }

    private bool ContainsModelFiles(string directory) =>
        Directory.GetFiles(directory, "*.yaml", SearchOption.TopDirectoryOnly).Any() ||
        Directory.GetFiles(directory, "*.yml", SearchOption.TopDirectoryOnly).Any();

    private async Task ValidateModelDirectoryAsync(string modelDir, CancellationToken cancellationToken)
    {
        var yamlFiles = Directory.GetFiles(modelDir, "*.yaml", SearchOption.TopDirectoryOnly)
            .Concat(Directory.GetFiles(modelDir, "*.yml", SearchOption.TopDirectoryOnly))
            .ToList();

        // Check for folder metadata
        var metaFile = Path.Combine(modelDir, "_meta.yaml");
        if (File.Exists(metaFile))
            await ValidateMetadataFileAsync(metaFile, cancellationToken);

        // Validate naming conventions
        foreach (var file in yamlFiles)
        {
            ValidateFileNamingConventions(file);
        }

        // Check for recommended structure
        ValidateRecommendedStructure(modelDir, yamlFiles);
    }

    private async Task ValidateMetadataFileAsync(string metaFile, CancellationToken cancellationToken)
    {
        try
        {
            var content = await File.ReadAllTextAsync(metaFile, cancellationToken);
            // Basic validation - check if it's valid YAML and has required fields
            if (string.IsNullOrWhiteSpace(content))
            {
                _results.Add(new ValidationResult(metaFile, "Metadata file is empty", ValidationSeverity.Warning));
                return;
            }

            // Check for lastReviewed date
            if (content.Contains("lastReviewed"))
            {
                var lastReviewedMatch = Regex.Match(content, @"lastReviewed:\s*(.+)");
                if (lastReviewedMatch.Success)
                {
                    if (DateTime.TryParse(lastReviewedMatch.Groups[1].Value, out var lastReviewed))
                    {
                        if (DateTime.Now.Subtract(lastReviewed).TotalDays > 90)
                        {
                            _results.Add(new ValidationResult(metaFile,
                                $"Metadata has not been reviewed for {DateTime.Now.Subtract(lastReviewed).Days} days (threshold: 90 days)",
                                ValidationSeverity.Warning));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _results.Add(new ValidationResult(metaFile, $"Error reading metadata file: {ex.Message}", ValidationSeverity.Error));
        }
    }

    private void ValidateFileNamingConventions(string file)
    {
        var fileName = Path.GetFileNameWithoutExtension(file);

        if(fileName is not null and "_meta")
        {
            return;
        }

        // Check for PascalCase
        if (!IsPascalCase(fileName))
        {
            _results.Add(new ValidationResult(file,
                $"File name '{fileName}' should be in PascalCase",
                ValidationSeverity.Warning));
        }

        // Check if file is in Enums directory (should not have .Type/.Behaviour suffix)
        var fileDirectory = Path.GetDirectoryName(file);
        var dirName = Path.GetFileName(fileDirectory);
        
        // Check if this is an Enums directory (direct or under Shared)
        var isEnums = dirName?.Equals("Enums", StringComparison.OrdinalIgnoreCase) == true;
        
        // Check for recommended suffixes (but skip for enum files)
        if (!isEnums && fileName != null &&
            !fileName.EndsWith(".Type") && !fileName.EndsWith(".Behaviour") &&
            !fileName.EndsWith(".Behavior") && !fileName.Contains("_meta"))
        {
            _results.Add(new ValidationResult(file,
                $"File name '{fileName}' should end with '.Type' or '.Behaviour' for clarity",
                ValidationSeverity.Info));
        }
    }

    private void ValidateRecommendedStructure(string modelDir, List<string> yamlFiles)
    {
        // Identify special folders: Shared, AttributeTypes, Enums (case-insensitive)
        var dirName = Path.GetFileName(modelDir);
        var parentDir = Path.GetFileName(Path.GetDirectoryName(modelDir));
        var isShared = dirName.Equals("Shared", StringComparison.OrdinalIgnoreCase);
        var isAttributeTypes = dirName.Equals("AttributeTypes", StringComparison.OrdinalIgnoreCase);
        var isEnums = dirName.Equals("Enums", StringComparison.OrdinalIgnoreCase);

        // Also handle Shared/AttributeTypes and Shared/Enums, with null check for parentDir
        var isSharedAttributeTypes = isShared || (parentDir != null && parentDir.Equals("Shared", StringComparison.OrdinalIgnoreCase) && dirName.Equals("AttributeTypes", StringComparison.OrdinalIgnoreCase));
        var isSharedEnums = isShared || (parentDir != null && parentDir.Equals("Shared", StringComparison.OrdinalIgnoreCase) && dirName.Equals("Enums", StringComparison.OrdinalIgnoreCase));

        if (isAttributeTypes || isEnums || isSharedAttributeTypes || isSharedEnums)
        {
            // Warn if subdirectories are present (should be flat)
            var subDirs = Directory.GetDirectories(modelDir, "*", SearchOption.TopDirectoryOnly);
            if (subDirs.Length > 0)
            {
                _results.Add(new ValidationResult(modelDir,
                    $"{dirName} directory should not contain subdirectories.",
                    ValidationSeverity.Info));
            }

            foreach (var file in yamlFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                // PascalCase check
                if (!IsPascalCase(fileName))
                {
                    _results.Add(new ValidationResult(file,
                        $"File name '{fileName}' should be in PascalCase",
                        ValidationSeverity.Warning));
                }
                if (isAttributeTypes || isSharedAttributeTypes)
                {
                    // Warn if file name does not contain 'Type' (optional, for clarity)
                    if (!fileName.Contains("Type"))
                    {
                        _results.Add(new ValidationResult(file,
                            $"AttributeTypes file '{fileName}' should include 'Type' in the name for clarity.",
                            ValidationSeverity.Info));
                    }
                }
                if (isEnums || isSharedEnums)
                {
                    // Warn if file name contains .Type or .Behaviour (should just be the enum name)
                    if (fileName.Contains(".Type") || fileName.Contains(".Behaviour") || fileName.Contains(".Behavior"))
                    {
                        _results.Add(new ValidationResult(file,
                            $"Enum file '{fileName}' should not include '.Type' or '.Behaviour' in the name.",
                            ValidationSeverity.Info));
                    }
                }
            }
            return;
        }

        var hasTypeFile = yamlFiles.Any(f => Path.GetFileNameWithoutExtension(f).EndsWith(".Type"));
        var hasBehaviourFile = yamlFiles.Any(f => Path.GetFileNameWithoutExtension(f).EndsWith(".Behaviour") ||
                                                  Path.GetFileNameWithoutExtension(f).EndsWith(".Behavior"));

        if (!hasTypeFile)
        {
            _results.Add(new ValidationResult(modelDir,
                "Directory should contain at least one .Type.yaml file",
                ValidationSeverity.Info));
        }

        if (yamlFiles.Count > 1 && !hasBehaviourFile)
        {
            _results.Add(new ValidationResult(modelDir,
                "Directory with multiple files should separate behaviors into .Behaviour.yaml files",
                ValidationSeverity.Info));
        }
    }

    private static bool IsPascalCase(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        // Allow dots in the name for Type/Behaviour files
        var parts = input.Split('.');
        return parts.All(part =>
            !string.IsNullOrEmpty(part) &&
            char.IsUpper(part[0]) &&
            part.All(c => char.IsLetterOrDigit(c)));
    }
}
