namespace Modeller.McpServer.CodeGeneration.Security;

/// <summary>
/// Represents an audit log entry for prompt validation activities
/// </summary>
public record PromptAuditEntry
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public required string ModelId { get; init; }
    public string? OriginalPrompt { get; init; }
    public string? SanitizedPrompt { get; init; }
    public PromptValidationResult? ValidationResult { get; init; }
    public InjectionRiskAssessment? InjectionRisk { get; init; }
    public SecurityContext? SecurityContext { get; init; }
    public TimeSpan ProcessingDuration { get; init; }
}

/// <summary>
/// Represents an audit log entry for LLM response generation activities
/// </summary>
public record LlmAuditEntry
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public required string ModelId { get; init; }
    public Guid PromptId { get; init; }
    public string? ResponseContent { get; init; }
    public int ResponseLength { get; init; }
    public int TokensUsed { get; init; }
    public TimeSpan GenerationDuration { get; init; }
    public bool PostValidationPassed { get; init; }
    public List<string>? ValidationErrors { get; init; }
    public SecurityContext? SecurityContext { get; init; }
}

/// <summary>
/// Represents an audit log entry for security violations and suspicious activities
/// </summary>
public record SecurityViolationEntry
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public required string ViolationType { get; init; }
    public required string Description { get; init; }
    public RiskLevel RiskLevel { get; init; }
    public required string ModelId { get; init; }
    public SecurityContext? SecurityContext { get; init; }
    public string? AttackVector { get; init; }
    public List<string>? DetectionRules { get; init; }
    public bool ActionTaken { get; init; }
    public string? RemediationAction { get; init; }
    public Dictionary<string, string>? AdditionalData { get; init; }
}

/// <summary>
/// Represents an audit log entry for code generation activities
/// </summary>
public record CodeGenerationAuditEntry
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public required string ModelId { get; init; }
    public Guid PromptId { get; init; }
    public Guid LlmResponseId { get; init; }
    public string? GeneratedCodeType { get; init; }
    public int GeneratedLines { get; init; }
    public bool ValidationPassed { get; init; }
    public List<string>? ValidationErrors { get; init; }
    public bool SecurityScanPassed { get; init; }
    public List<string>? SecurityIssues { get; init; }
    public SecurityContext? SecurityContext { get; init; }
    public TimeSpan GenerationDuration { get; init; }
    public string? TargetFilePath { get; init; }
    public Dictionary<string, string>? Metadata { get; init; }
}

/// <summary>
/// Represents a summary of audit activity for reporting
/// </summary>
public record AuditSummary
{
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public int TotalPrompts { get; init; }
    public int HighRiskPrompts { get; init; }
    public int FailedValidations { get; init; }
    public int LlmResponses { get; init; }
    public int FailedPostValidations { get; init; }
    public Dictionary<string, int> ModelUsage { get; init; } = new();
    public Dictionary<RiskLevel, int> RiskDistribution { get; init; } = new();
    public TimeSpan AverageProcessingTime { get; init; }
}

/// <summary>
/// Configuration for audit logging behavior
/// </summary>
public record AuditConfiguration
{
    public bool EnableFileLogging { get; init; } = true;
    public bool EnableStructuredLogging { get; init; } = true;
    public string AuditLogPath { get; init; } = "logs/audit";
    public int RetentionDays { get; init; } = 90;
    public bool LogPromptContent { get; init; } = false; // Security: don't log actual prompt content by default
    public bool LogResponseContent { get; init; } = false; // Security: don't log actual response content by default
    public RiskLevel MinimumLogLevel { get; init; } = RiskLevel.Low;
    public bool EnableCompression { get; init; } = true;
    public int MaxFileSize { get; init; } = 100 * 1024 * 1024; // 100MB
}
