using ModelContextProtocol.Server;
using Modeller.Mcp.Shared.Models;
using Modeller.Mcp.Shared.Services;
using System.ComponentModel;
using System.Text.Json;

namespace Modeller.Mcp.Shared.Resources;

/// <summary>
/// Provides MCP resources for validated model definitions, enabling them to be referenced
/// as context in chat sessions and used by other tools and prompts.
/// </summary>
[McpServerResourceType]
public class ModelDefinitionResources
{
    private readonly ModelDiscoveryService _modelDiscoveryService;
    private readonly Dictionary<string, ModelDefinition> _validatedModels = new();

    public ModelDefinitionResources(ModelDiscoveryService modelDiscoveryService)
    {
        _modelDiscoveryService = modelDiscoveryService;
    }

    /// <summary>
    /// Stores a validated model definition for later reference as a resource
    /// </summary>
    public void RegisterValidatedModel(string modelName, string domainPath, ModelDefinition modelDefinition)
    {
        var key = $"{domainPath}:{modelName}";
        _validatedModels[key] = modelDefinition;
    }

    /// <summary>
    /// Gets all validated model definitions for a domain
    /// </summary>
    [McpServerResource(UriTemplate = "modeller://models/domain/{domainPath}", Name = "Domain Models", MimeType = "application/json")]
    [Description("Get all validated model definitions for a specific domain")]
    public string GetDomainModels(string domainPath)
    {
        var domainModels = _validatedModels
            .Where(kvp => kvp.Key.StartsWith($"{domainPath}:"))
            .Select(kvp => new { ModelName = kvp.Key.Split(':')[1], Definition = kvp.Value })
            .ToList();

        if (!domainModels.Any())
        {
            // Try to discover models in the domain
            var discoveryResult = _modelDiscoveryService.DiscoverModels(domainPath);
            
            return JsonSerializer.Serialize(new
            {
                DomainPath = domainPath,
                Message = "No validated models found for this domain",
                DiscoveredDirectories = discoveryResult.ModelDirectories.Select(d => d.Path).ToList(),
                LooseFiles = discoveryResult.LooseFiles.Select(f => f.Path).ToList(),
                Errors = discoveryResult.Errors
            }, new JsonSerializerOptions { WriteIndented = true });
        }

        return JsonSerializer.Serialize(new
        {
            DomainPath = domainPath,
            ModelCount = domainModels.Count,
            Models = domainModels
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// Gets a specific validated model definition
    /// </summary>
    [McpServerResource(UriTemplate = "modeller://models/{domainPath}/{modelName}", Name = "Model Definition", MimeType = "application/json")]
    [Description("Get a specific validated model definition by domain and model name")]
    public string GetModelDefinition(string domainPath, string modelName)
    {
        var key = $"{domainPath}:{modelName}";
        
        if (_validatedModels.TryGetValue(key, out var modelDefinition))
        {
            return JsonSerializer.Serialize(new
            {
                DomainPath = domainPath,
                ModelName = modelName,
                Definition = modelDefinition,
                ValidationStatus = "Validated"
            }, new JsonSerializerOptions { WriteIndented = true });
        }

        return JsonSerializer.Serialize(new
        {
            DomainPath = domainPath,
            ModelName = modelName,
            Error = $"Model '{modelName}' not found in domain '{domainPath}' or not yet validated"
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// Gets all currently validated models across all domains
    /// </summary>
    [McpServerResource(UriTemplate = "modeller://models/all", Name = "All Validated Models", MimeType = "application/json")]
    [Description("Get all currently validated model definitions across all domains")]
    public string GetAllValidatedModels()
    {
        var allModels = _validatedModels.Select(kvp => new
        {
            Key = kvp.Key,
            DomainPath = kvp.Key.Split(':')[0],
            ModelName = kvp.Key.Split(':')[1],
            Definition = kvp.Value
        }).ToList();

        return JsonSerializer.Serialize(new
        {
            TotalValidatedModels = allModels.Count,
            Models = allModels
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// Gets the schema/structure information for validated models
    /// </summary>
    [McpServerResource(UriTemplate = "modeller://models/schema/{domainPath}/{modelName}", Name = "Model Schema", MimeType = "application/json")]
    [Description("Get the schema information for a specific validated model")]
    public string GetModelSchema(string domainPath, string modelName)
    {
        var key = $"{domainPath}:{modelName}";
        
        if (_validatedModels.TryGetValue(key, out var modelDefinition))
        {
            var schema = new
            {
                ModelName = modelDefinition.Model,
                Summary = modelDefinition.Summary,
                Attributes = modelDefinition.AttributeUsages?.Select(a => new
                {
                    a.Name,
                    a.Type,
                    a.Required,
                    a.Summary
                }).ToList(),
                Behaviours = modelDefinition.Behaviours?.Select(b => new
                {
                    b.Name,
                    b.Summary,
                    EntityCount = b.Entities?.Count ?? 0
                }).ToList(),
                Domain = domainPath
            };

            return JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
        }

        return JsonSerializer.Serialize(new
        {
            Error = $"Model '{modelName}' not found in domain '{domainPath}'"
        }, new JsonSerializerOptions { WriteIndented = true });
    }
}
