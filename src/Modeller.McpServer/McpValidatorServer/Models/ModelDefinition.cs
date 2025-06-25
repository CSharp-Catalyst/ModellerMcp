namespace Modeller.McpServer.McpValidatorServer.Models;

public record ModelDefinition
{
    public required string Model { get; set; }
    public List<AttributeUsage> AttributeUsages { get; set; } = [];
    public List<Behaviour> Behaviours { get; set; } = [];
    public List<Scenario> Scenarios { get; set; } = [];
    public string? OwnedBy { get; set; }
    public required string Summary { get; set; }
    public string? Remarks { get; set; }
}

public record AttributeTypeDefinition
{
    public required string Name { get; set; }
    public required string Type { get; set; }
    public string? Extends { get; set; }
    public string? Format { get; set; }
    public AttributeConstraints? Constraints { get; set; }
    public required string Summary { get; set; }
    public string? Remarks { get; set; }
}

public record AttributeTypesWrapper
{
    public List<AttributeTypeDefinition> AttributeTypes { get; set; } = [];
}

public record AttributeUsage
{
    public required string Name { get; set; }
    public required string Type { get; set; }
    public bool Required { get; set; }
    public bool Unique { get; set; }
    public object? Default { get; set; }
    public required string Summary { get; set; }
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
    public required string Name { get; set; }
    public required string Summary { get; set; }
    public string? Remarks { get; set; }
    public List<string> Entities { get; set; } = [];
    public List<string> Preconditions { get; set; } = [];
    public List<string> Effects { get; set; } = [];
}

public record Scenario
{
    public required string Name { get; set; }
    public List<string> Given { get; set; } = [];
    public List<string> When { get; set; } = [];
    public List<string> Then { get; set; } = [];
}

public record ValidationProfile
{
    public required string Name { get; set; }
    public List<Claim> Claims { get; set; } = [];
    public Dictionary<string, AttributeRule> AttributeRules { get; set; } = [];
    public Dictionary<string, BehaviourRule> BehaviourRules { get; set; } = [];
}

public record Claim
{
    public required string Action { get; set; }
    public required string Resource { get; set; }
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
    public required string Enum { get; set; }
    public List<EnumItem> Items { get; set; } = [];
    public required string Summary { get; set; }
    public string? Remarks { get; set; }
}

public record EnumItem
{
    public required string Name { get; set; }
    public required string Display { get; set; }
    public int Value { get; set; }
}

public record FolderMetadata
{
    public required string Name { get; set; }
    public required string Summary { get; set; }
    public string? Remarks { get; set; }
    public List<string> Owners { get; set; } = [];
    public List<string> Tags { get; set; } = [];
    public List<string> Dependencies { get; set; } = [];
    public string? Version { get; set; }
    public string? Status { get; set; }
    public DateTime? LastReviewed { get; set; }
}
