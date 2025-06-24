namespace Modeller.McpServer.McpValidatorServer.Models;

// BDD Model Definition Types (New Format)
public record ModelDefinition
{
    public string Model { get; set; } = string.Empty;
    public List<AttributeUsage> AttributeUsages { get; set; } = [];
    public List<Behaviour> Behaviours { get; set; } = [];
    public List<Scenario> Scenarios { get; set; } = [];
    public string? OwnedBy { get; set; }
    public string? Summary { get; set; }
    public string? Description { get; set; }
}

public record AttributeTypeDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Extends { get; set; }
    public string? Format { get; set; }
    public AttributeConstraints? Constraints { get; set; }
    public string? Summary { get; set; }
    public string? Description { get; set; }
}

public record AttributeTypesWrapper
{
    public List<AttributeTypeDefinition> AttributeTypes { get; set; } = [];
}

public record AttributeUsage
{
    public string? Name { get; set; }
    public string Type { get; set; } = string.Empty;
    public bool Required { get; set; }
    public object? Default { get; set; }
    public string? Summary { get; set; }
    public string? Remarks { get; set; }
    public AttributeConstraints? Constraints { get; set; }
}

public record AttributeConstraints
{
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public object? Minimum { get; set; }
    public object? Maximum { get; set; }
    public string? Pattern { get; set; }
    public int? DecimalPlaces { get; set; }
    public List<object>? Enum { get; set; }
    public bool? Nullable { get; set; }
    public string? Unit { get; set; }
    public object? Example { get; set; }
}

public record Behaviour
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> Entities { get; set; } = [];
    public List<string> Preconditions { get; set; } = [];
    public List<string> Effects { get; set; } = [];
}

public record Scenario
{
    public string Name { get; set; } = string.Empty;
    public List<string> Given { get; set; } = [];
    public List<string> When { get; set; } = [];
    public List<string> Then { get; set; } = [];
}

public record ValidationProfile
{
    public string Name { get; set; } = string.Empty;
    public List<Claim> Claims { get; set; } = [];
    public Dictionary<string, AttributeRule> AttributeRules { get; set; } = [];
    public Dictionary<string, BehaviourRule> BehaviourRules { get; set; } = [];
}

public record Claim
{
    public string Action { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
}

public record AttributeRule
{
    public bool? Required { get; set; }
    public object? Default { get; set; }
}

public record BehaviourRule
{
    public bool Allowed { get; set; }
}

public record EnumDefinition
{
    public string Enum { get; set; } = string.Empty;
    public List<EnumItem> Items { get; set; } = [];
    public string? Summary { get; set; }
    public string? Description { get; set; }
}

public record EnumItem
{
    public string Name { get; set; } = string.Empty;
    public string Display { get; set; } = string.Empty;
    public int Value { get; set; }
}

public record FolderMetadata
{
    public string Name { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Description { get; set; }
    public List<string> Owners { get; set; } = [];
    public List<string> Tags { get; set; } = [];
    public List<string> Dependencies { get; set; } = [];
    public string? Version { get; set; }
    public string? Status { get; set; }
    public DateTime? LastReviewed { get; set; }
}
