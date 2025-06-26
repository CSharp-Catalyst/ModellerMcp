using Modeller.Mcp.Shared;
using Modeller.Mcp.Shared.Models;
using Modeller.McpServer.McpValidatorServer.Services;

using System.Text.RegularExpressions;

using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Modeller.McpServer.McpValidatorServer;

public class YamlSchemaValidator(ModelStructureValidator structureValidator) : IMcpModelValidator
{
    private readonly IDeserializer _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

    private Dictionary<string, AttributeTypeDefinition>? _sharedAttributeTypes;

    private ModelFileType DetermineFileType(string yaml, string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        var extension = Path.GetExtension(fileName);

        // Check filename patterns first
        if (fileName.Equals("_meta.yaml", StringComparison.OrdinalIgnoreCase) ||
            fileName.Equals("_meta.yml", StringComparison.OrdinalIgnoreCase))
        {
            return ModelFileType.Metadata;
        }
        if (fileName.Equals("copilot-instructions.md", StringComparison.OrdinalIgnoreCase))
        {
            return ModelFileType.CopilotInstructions;
        }
        if (extension.Equals(".md", StringComparison.OrdinalIgnoreCase))
        {
            return ModelFileType.UserDocumentation;
        }

        // Check content patterns
        if (yaml.Contains("model:") && (yaml.Contains("attributeUsages:") || yaml.Contains("behaviours:")))
            return ModelFileType.BddModel;

        if (yaml.Contains("attributeTypes:"))
            return ModelFileType.AttributeTypes;

        if (yaml.Contains("enum:") && yaml.Contains("items:"))
            return ModelFileType.Enum;

        if (yaml.Contains("validationProfiles:"))
            return ModelFileType.ValidationProfiles;

        return ModelFileType.Unknown;
    }

    public async Task<ModelValidationResponse> ValidateAsync(string filePath, CancellationToken cancellationToken)
    {
        var results = new List<ValidationResult>();
        ModelDefinition? modelDefinition = null;

        try
        {
            // Determine solution root for loading shared attribute types
            var solutionPath = FindSolutionRoot(filePath);
            if (!string.IsNullOrEmpty(solutionPath))
            {
                await LoadSharedAttributeTypesAsync(solutionPath, cancellationToken);
            }

            if (Directory.Exists(filePath))
            {
                // Validate directory structure
                var structureResults = await structureValidator.ValidateStructureAsync(filePath, cancellationToken);
                results.AddRange(structureResults);

                // Validate all YAML files in the directory
                var yamlFiles = Directory.GetFiles(filePath, "*.yaml", SearchOption.AllDirectories)
                    .Concat(Directory.GetFiles(filePath, "*.yml", SearchOption.AllDirectories));

                foreach (var yamlFile in yamlFiles)
                {
                    var fileResponse = await ValidateFileWithModelAsync(yamlFile, cancellationToken);
                    results.AddRange(fileResponse.Results);
                    if (fileResponse.Model != null)
                        modelDefinition = fileResponse.Model;
                }
            }
            else if (File.Exists(filePath))
            {
                // Validate single file
                var fileResponse = await ValidateFileWithModelAsync(filePath, cancellationToken);
                results.AddRange(fileResponse.Results);
                modelDefinition = fileResponse.Model;
            }
            else
            {
                results.Add(new ValidationResult(filePath, "File or directory does not exist", ValidationSeverity.Error));
            }
        }
        catch (Exception ex)
        {
            results.Add(new ValidationResult(filePath, $"Validation failed: {ex.Message}", ValidationSeverity.Error));
        }

        return new ModelValidationResponse { Results = results, Model = modelDefinition };
    }

    private async Task<ModelValidationResponse> ValidateFileWithModelAsync(string filePath, CancellationToken cancellationToken)
    {
        var results = new List<ValidationResult>();
        ModelDefinition? modelDefinition = null;

        try
        {
            var yaml = await File.ReadAllTextAsync(filePath, cancellationToken);

            if (string.IsNullOrWhiteSpace(yaml))
            {
                results.Add(new ValidationResult(filePath, "File is empty", ValidationSeverity.Warning));
                return new ModelValidationResponse { Results = results };
            }

            // Basic YAML parsing
            var yamlStream = new YamlStream();
            yamlStream.Load(new StringReader(yaml));

            if (yamlStream.Documents.Count == 0)
            {
                results.Add(new ValidationResult(filePath, "Empty YAML document", ValidationSeverity.Warning));
                return new ModelValidationResponse { Results = results };
            }

            // Determine file type and validate accordingly
            var fileType = DetermineFileType(yaml, filePath);
            switch (fileType)
            {
                case ModelFileType.BddModel:
                    modelDefinition = ValidateModelWithReturn(filePath, yaml, results);
                    break;
                case ModelFileType.AttributeTypes:
                    ValidateAttributeTypes(filePath, yaml, results);
                    break;
                case ModelFileType.Enum:
                    ValidateEnum(filePath, yaml, results);
                    break;
                case ModelFileType.ValidationProfiles:
                    ValidateValidationProfiles(filePath, yaml, results);
                    break;
                case ModelFileType.Metadata:
                    ValidateMetadata(filePath, yaml, results);
                    break;
                case ModelFileType.CopilotInstructions:
                    results.Add(new ValidationResult(filePath, "Copilot instructions file detected.", ValidationSeverity.Info));
                    break;
                case ModelFileType.UserDocumentation:
                    results.Add(new ValidationResult(filePath, "User-supplied documentation (.md) detected. This file is static and not generated.", ValidationSeverity.Info));
                    break;
                default:
                    results.Add(new ValidationResult(filePath, "Unable to determine file type", ValidationSeverity.Warning));
                    break;
            }

            // Validate naming conventions
            ValidateNamingConventions(filePath, yaml, results);
        }
        catch (YamlException yamlEx)
        {
            results.Add(new ValidationResult(filePath, $"YAML parsing error: {yamlEx.Message}", ValidationSeverity.Error));
        }
        catch (Exception ex)
        {
            results.Add(new ValidationResult(filePath, $"Validation error: {ex.Message}", ValidationSeverity.Error));
        }

        return new ModelValidationResponse { Results = results, Model = modelDefinition };
    }

    private ModelDefinition? ValidateModelWithReturn(string filePath, string yaml, List<ValidationResult> results)
    {
        try
        {
            var model = _yamlDeserializer.Deserialize<ModelDefinition>(yaml);

            // Validate required fields
            if (string.IsNullOrWhiteSpace(model.Model))
                results.Add(new ValidationResult(filePath, "Model name is required", ValidationSeverity.Error));

            // Validate model name matches file name
            var actualFileName = Path.GetFileNameWithoutExtension(filePath);
            if (!actualFileName.StartsWith(model.Model, StringComparison.OrdinalIgnoreCase))
            {
                results.Add(new ValidationResult(filePath, $"Model name '{model.Model}' should match start of file name", ValidationSeverity.Warning));
            }

            // Validate attribute usages
            ValidateAttributeUsages(filePath, model.Model, model.AttributeUsages, results);

            // Validate behaviours
            ValidateBehaviours(filePath, model.Behaviours, results);

            // Validate scenarios
            ValidateScenarios(filePath, model.Scenarios, results);

            return model;
        }
        catch (Exception ex)
        {
            results.Add(new ValidationResult(filePath, $"BDD model validation error: {ex.Message}", ValidationSeverity.Error));
            return null;
        }
    }

    private void ValidateAttributeTypes(string filePath, string yaml, List<ValidationResult> results)
    {
        try
        {
            // Try to deserialize as wrapper object with attributeTypes property first
            var attributeTypesWrapper = _yamlDeserializer.Deserialize<AttributeTypesWrapper>(yaml);
            List<AttributeTypeDefinition> attributeTypes;

            if (attributeTypesWrapper?.AttributeTypes != null)
            {
                attributeTypes = attributeTypesWrapper.AttributeTypes;
            }
            else
            {
                // Fallback: try to deserialize as direct list
                attributeTypes = _yamlDeserializer.Deserialize<List<AttributeTypeDefinition>>(yaml);
            }

            foreach (var attributeType in attributeTypes)
            {
                if (string.IsNullOrWhiteSpace(attributeType.Name))
                    results.Add(new ValidationResult(filePath, "Attribute type name is required", ValidationSeverity.Error));

                if (string.IsNullOrWhiteSpace(attributeType.Type))
                    results.Add(new ValidationResult(filePath, $"Attribute type '{attributeType.Name}' must specify a type", ValidationSeverity.Error));

                // Validate camelCase naming
                if (!IsCamelCase(attributeType.Name))
                {
                    results.Add(new ValidationResult(filePath,
                        $"Attribute type name '{attributeType.Name}' should be camelCase",
                        ValidationSeverity.Warning));
                }
            }
        }
        catch (Exception ex)
        {
            results.Add(new ValidationResult(filePath, $"Attribute types validation error: {ex.Message}", ValidationSeverity.Error));
        }
    }

    private void ValidateEnum(string filePath, string yaml, List<ValidationResult> results)
    {
        try
        {
            var enumDef = _yamlDeserializer.Deserialize<EnumDefinition>(yaml);

            if (string.IsNullOrWhiteSpace(enumDef.Enum))
                results.Add(new ValidationResult(filePath, "Enum name is required", ValidationSeverity.Error));

            if (enumDef.Items.Count == 0)
                results.Add(new ValidationResult(filePath, "Enum must have at least one item", ValidationSeverity.Error));

            // Check for duplicate values
            var duplicateValues = enumDef.Items
                .GroupBy(i => i.Value)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            foreach (var duplicateValue in duplicateValues)
            {
                results.Add(new ValidationResult(filePath,
                    $"Enum has duplicate value: {duplicateValue}",
                    ValidationSeverity.Error));
            }
        }
        catch (Exception ex)
        {
            results.Add(new ValidationResult(filePath, $"Enum validation error: {ex.Message}", ValidationSeverity.Error));
        }
    }

    private void ValidateValidationProfiles(string filePath, string yaml, List<ValidationResult> results)
    {
        try
        {
            var profiles = _yamlDeserializer.Deserialize<List<ValidationProfile>>(yaml);

            foreach (var profile in profiles)
            {
                if (string.IsNullOrWhiteSpace(profile.Name))
                    results.Add(new ValidationResult(filePath, "Validation profile name is required", ValidationSeverity.Error));

                if (profile.Claims.Count == 0)
                {
                    results.Add(new ValidationResult(filePath,
                        $"Validation profile '{profile.Name}' should have at least one claim",
                        ValidationSeverity.Warning));
                }
            }
        }
        catch (Exception ex)
        {
            results.Add(new ValidationResult(filePath, $"Validation profiles error: {ex.Message}", ValidationSeverity.Error));
        }
    }

    private void ValidateMetadata(string filePath, string yaml, List<ValidationResult> results)
    {
        try
        {
            var metadata = _yamlDeserializer.Deserialize<FolderMetadata>(yaml);

            if (string.IsNullOrWhiteSpace(metadata.Name))
                results.Add(new ValidationResult(filePath, "Metadata name is required", ValidationSeverity.Error));

            // Check lastReviewed date
            if (metadata.LastReviewed.HasValue)
            {
                var daysSinceReview = (DateTime.Now - metadata.LastReviewed.Value).TotalDays;
                if (daysSinceReview > 90)
                {
                    results.Add(new ValidationResult(filePath,
                        $"Metadata has not been reviewed for {daysSinceReview:F0} days (threshold: 90 days)",
                        ValidationSeverity.Warning));
                }
            }
        }
        catch (Exception ex)
        {
            results.Add(new ValidationResult(filePath, $"Metadata validation error: {ex.Message}", ValidationSeverity.Error));
        }
    }

    private void ValidateAttributeUsages(string filePath, string modelName, List<AttributeUsage> attributeUsages, List<ValidationResult> results)
    {
        foreach (var usage in attributeUsages)
        {
            // Create identifier for clearer error messages
            var attributeIdentifier = !string.IsNullOrWhiteSpace(usage.Name) ? $"'{usage.Name}'" : 
                                     !string.IsNullOrWhiteSpace(usage.Type) ? $"(type: {usage.Type})" : 
                                     "(unnamed attribute)";

            if (string.IsNullOrWhiteSpace(usage.Type))
                results.Add(new ValidationResult(filePath, $"Model '{modelName}': Attribute usage {attributeIdentifier} type is required", ValidationSeverity.Error));

            if (string.IsNullOrWhiteSpace(usage.Name))
                results.Add(new ValidationResult(filePath, $"Model '{modelName}': Attribute usage {attributeIdentifier} name is required", ValidationSeverity.Error));

            if (string.IsNullOrWhiteSpace(usage.Summary))
                results.Add(new ValidationResult(filePath, $"Model '{modelName}': Attribute usage {attributeIdentifier} summary is required", ValidationSeverity.Error));

            // Validate that the attribute type exists in shared attribute types
            if (!string.IsNullOrWhiteSpace(usage.Type) && _sharedAttributeTypes != null && !_sharedAttributeTypes.ContainsKey(usage.Type))
            {
                results.Add(new ValidationResult(filePath, $"Model '{modelName}': Attribute usage type '{usage.Type}' is not defined in shared attribute types", ValidationSeverity.Error));
            }

            // Validate camelCase naming for attribute names
            if (!string.IsNullOrWhiteSpace(usage.Name) && !IsCamelCase(usage.Name))
            {
                results.Add(new ValidationResult(filePath,
                    $"Model '{modelName}': Attribute name '{usage.Name}' should be camelCase",
                    ValidationSeverity.Warning));
            }

            // Validate unique constraint logic (info level as it's a new feature)
            if (usage.Unique && !usage.Required)
            {
                results.Add(new ValidationResult(filePath,
                    $"Model '{modelName}': Attribute usage '{usage.Name ?? usage.Type}' is marked as unique but not required. Consider if this is intentional.",
                    ValidationSeverity.Info));
            }
        }
    }

    private void ValidateBehaviours(string filePath, List<Behaviour> behaviours, List<ValidationResult> results)
    {
        foreach (var behaviour in behaviours)
        {
            if (string.IsNullOrWhiteSpace(behaviour.Name))
                results.Add(new ValidationResult(filePath, "Behaviour name is required", ValidationSeverity.Error));

            // Validate camelCase naming
            if (!IsCamelCase(behaviour.Name))
            {
                results.Add(new ValidationResult(filePath,
                    $"Behaviour name '{behaviour.Name}' should be camelCase",
                    ValidationSeverity.Warning));
            }

            if (behaviour.Entities.Count == 0)
            {
                results.Add(new ValidationResult(filePath,
                    $"Behaviour '{behaviour.Name}' should specify at least one entity",
                    ValidationSeverity.Warning));
            }
        }
    }

    private void ValidateScenarios(string filePath, List<Scenario> scenarios, List<ValidationResult> results)
    {
        foreach (var scenario in scenarios)
        {
            if (string.IsNullOrWhiteSpace(scenario.Name))
                results.Add(new ValidationResult(filePath, "Scenario name is required", ValidationSeverity.Error));

            if (scenario.Given.Count == 0)
            {
                results.Add(new ValidationResult(filePath,
                    $"Scenario '{scenario.Name}' should have at least one 'given' condition",
                    ValidationSeverity.Warning));
            }

            if (scenario.When.Count == 0)
            {
                results.Add(new ValidationResult(filePath,
                    $"Scenario '{scenario.Name}' should have at least one 'when' condition",
                    ValidationSeverity.Warning));
            }

            if (scenario.Then.Count == 0)
            {
                results.Add(new ValidationResult(filePath,
                    $"Scenario '{scenario.Name}' should have at least one 'then' condition",
                    ValidationSeverity.Warning));
            }
        }
    }

    private void ValidateNamingConventions(string filePath, string yaml, List<ValidationResult> results)
    {
        // Check for abbreviations (basic heuristic)
        var abbreviationPattern = @"\b[A-Z]{2,}\b";
        var matches = Regex.Matches(yaml, abbreviationPattern);

        var commonAbbreviations = new HashSet<string> { "ID", "URL", "URI", "API", "HTTP", "HTTPS", "JSON", "XML", "HTML", "CSS", "SQL", "UTC" };

        foreach (Match match in matches)
        {
            if (!commonAbbreviations.Contains(match.Value))
            {
                results.Add(new ValidationResult(filePath,
                    $"Potential abbreviation '{match.Value}' found - consider using full words unless domain-specific",
                    ValidationSeverity.Info));
            }
        }
    }

    private static bool IsCamelCase(string input) => !string.IsNullOrEmpty(input) && char.IsLower(input[0]) && input.All(c => char.IsLetterOrDigit(c));

    private async Task<Dictionary<string, AttributeTypeDefinition>> LoadSharedAttributeTypesAsync(string solutionPath, CancellationToken cancellationToken)
    {
        if (_sharedAttributeTypes != null)
            return _sharedAttributeTypes;

        var sharedAttributeTypes = new Dictionary<string, AttributeTypeDefinition>();

        // Look for Shared/AttributeTypes directories
        var sharedPaths = Directory.GetDirectories(solutionPath, "*", SearchOption.AllDirectories)
            .Where(d => d.EndsWith("Shared", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var sharedPath in sharedPaths)
        {
            // Load AttributeTypes
            var attributeTypePaths = Directory.GetDirectories(sharedPath, "AttributeTypes", SearchOption.TopDirectoryOnly);
            foreach (var attrTypePath in attributeTypePaths)
            {
                var yamlFiles = Directory.GetFiles(attrTypePath, "*.yaml", SearchOption.AllDirectories)
                    .Concat(Directory.GetFiles(attrTypePath, "*.yml", SearchOption.AllDirectories));

                foreach (var yamlFile in yamlFiles)
                {
                    try
                    {
                        var yaml = await File.ReadAllTextAsync(yamlFile, cancellationToken);
                        if (yaml.Contains("attributeTypes:"))
                        {
                            var attributeTypesWrapper = _yamlDeserializer.Deserialize<AttributeTypesWrapper>(yaml);
                            if (attributeTypesWrapper?.AttributeTypes != null)
                            {
                                foreach (var attrType in attributeTypesWrapper.AttributeTypes)
                                {
                                    if (!string.IsNullOrWhiteSpace(attrType.Name))
                                    {
                                        sharedAttributeTypes[attrType.Name] = attrType;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Skip files that can't be parsed
                        continue;
                    }
                }
            }

            // Load Enums as valid attribute types
            var enumPaths = Directory.GetDirectories(sharedPath, "Enums", SearchOption.TopDirectoryOnly);
            foreach (var enumPath in enumPaths)
            {
                var yamlFiles = Directory.GetFiles(enumPath, "*.yaml", SearchOption.AllDirectories)
                    .Concat(Directory.GetFiles(enumPath, "*.yml", SearchOption.AllDirectories));

                foreach (var yamlFile in yamlFiles)
                {
                    try
                    {
                        var yaml = await File.ReadAllTextAsync(yamlFile, cancellationToken);
                        if (yaml.Contains("enum:") && yaml.Contains("items:"))
                        {
                            var enumDef = _yamlDeserializer.Deserialize<EnumDefinition>(yaml);
                            if (!string.IsNullOrWhiteSpace(enumDef.Enum))
                            {
                                // Create a pseudo attribute type definition for the enum
                                var enumAsAttributeType = new AttributeTypeDefinition
                                {
                                    Name = enumDef.Enum,
                                    Type = "enum",
                                    Summary = enumDef.Summary ?? $"Enum type {enumDef.Enum}"
                                };
                                sharedAttributeTypes[enumDef.Enum] = enumAsAttributeType;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Skip files that can't be parsed
                        continue;
                    }
                }
            }
        }

        _sharedAttributeTypes = sharedAttributeTypes;
        return sharedAttributeTypes;
    }

    private static string FindSolutionRoot(string filePath)
    {
        var directory = Directory.Exists(filePath) ? filePath : Path.GetDirectoryName(filePath);
        
        while (!string.IsNullOrEmpty(directory))
        {
            if (Directory.GetFiles(directory, "*.sln").Any() || 
                Directory.Exists(Path.Combine(directory, "models")))
            {
                return directory;
            }
            directory = Path.GetDirectoryName(directory);
        }
        
        return string.Empty;
    }
}
