using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Modeller.Mcp.Shared.CodeGeneration.Security;

/// <summary>
/// Interface for prompt security validation and sanitization services
/// </summary>
public interface IPromptSecurityService
{
    /// <summary>
    /// Validates a prompt for security issues
    /// </summary>
    Task<PromptValidationResult> ValidatePromptAsync(string prompt, SecurityContext context);

    /// <summary>
    /// Assesses the injection risk level of a prompt
    /// </summary>
    Task<InjectionRiskAssessment> AssessPromptInjectionRiskAsync(string prompt);

    /// <summary>
    /// Sanitizes a prompt to remove or neutralize potential security threats
    /// </summary>
    Task<string> SanitizePromptAsync(string prompt, SecurityContext context);
}

/// <summary>
/// Validates and sanitizes prompts to prevent injection attacks and ensure security
/// </summary>
public class PromptSecurityService(
    ILogger<PromptSecurityService> logger,
    IPromptAuditLogger auditLogger,
    IConfiguration configuration) : IPromptSecurityService
{

    /// <summary>
    /// Validates and sanitizes a prompt for secure LLM generation
    /// </summary>
    public async Task<PromptValidationResult> ValidateAndSanitizePromptAsync(
        string rawPrompt, 
        string modelId,
        SecurityContext context)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            // 1. Sanitize user-provided content
            var sanitizedPrompt = await SanitizeUserInputAsync(rawPrompt);
            
            // 2. Validate prompt structure and boundaries
            var validationResult = await ValidatePromptStructureAsync(sanitizedPrompt);
            
            // 3. Check for potential prompt injection attempts
            var injectionRisk = await DetectPromptInjectionAsync(sanitizedPrompt);
            
            // 4. Log the prompt for audit trail
            await auditLogger.LogPromptValidationAsync(new PromptAuditEntry
            {
                Id = Guid.NewGuid(),
                OriginalPrompt = rawPrompt,
                SanitizedPrompt = sanitizedPrompt,
                ModelId = modelId,
                ValidationResult = validationResult,
                InjectionRisk = injectionRisk,
                SecurityContext = context,
                Timestamp = startTime,
                ProcessingDuration = DateTime.UtcNow - startTime
            });
            
            // 5. Check risk threshold
            var maxRiskLevel = configuration.GetValue("Security:MaxPromptRiskLevel", RiskLevel.Medium);
            if (injectionRisk.Level > maxRiskLevel)
            {
                logger.LogWarning("High injection risk detected for model {ModelId}: {Reason}", 
                    modelId, injectionRisk.Reason);
                throw new PromptSecurityException($"Prompt injection risk too high: {injectionRisk.Reason}");
            }
            
            return new PromptValidationResult
            {
                IsValid = validationResult.IsValid && injectionRisk.Level <= maxRiskLevel,
                Issues = validationResult.Issues.Concat(injectionRisk.Level > maxRiskLevel ? 
                    new[] { $"Injection risk level {injectionRisk.Level} exceeds maximum {maxRiskLevel}" } : 
                    Array.Empty<string>()).ToList(),
                Warnings = validationResult.Warnings,
                ProcessedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to validate prompt for model {ModelId}", modelId);
            throw;
        }
    }

    /// <summary>
    /// Validates a prompt for security issues
    /// </summary>
    public async Task<PromptValidationResult> ValidatePromptAsync(string prompt, SecurityContext context)
    {
        // Check structural issues first
        var structuralResult = await ValidatePromptStructureAsync(prompt);
        
        // Check injection risks
        var injectionRisk = await DetectPromptInjectionAsync(prompt);
        
        // Combine results - prompt is invalid if either structural issues or high injection risk
        var allIssues = structuralResult.Issues.ToList();
        if (injectionRisk.Level == RiskLevel.High)
            allIssues.Add($"High injection risk detected: {injectionRisk.Reason ?? "Multiple risk factors identified"}");
        
        return new PromptValidationResult
        {
            IsValid = structuralResult.IsValid && injectionRisk.Level != RiskLevel.High,
            Issues = allIssues,
            Warnings = structuralResult.Warnings,
            ProcessedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Assesses the injection risk level of a prompt
    /// </summary>
    public async Task<InjectionRiskAssessment> AssessPromptInjectionRiskAsync(string prompt)
    {
        // Delegate to the existing risk assessment logic
        var assessment = await DetectPromptInjectionAsync(prompt);
        return assessment;
    }

    /// <summary>
    /// Sanitizes a prompt to remove or neutralize potential security threats
    /// </summary>
    public async Task<string> SanitizePromptAsync(string prompt, SecurityContext context)
    {
        // Delegate to the existing sanitization logic
        var sanitized = await SanitizeUserInputAsync(prompt);
        return sanitized;
    }

    private Task<string> SanitizeUserInputAsync(string prompt)
    {
        if (string.IsNullOrEmpty(prompt))
            return Task.FromResult(prompt);

        // Remove or escape potential prompt injection attempts
        var sanitized = prompt
            // Escape prompt delimiters and control sequences
            .Replace("```", "\\`\\`\\`")
            .Replace("---", "\\-\\-\\-")
            .Replace("{{", "\\{\\{")
            .Replace("}}", "\\}\\}")
            // Filter obvious injection attempts
            .Replace("Ignore previous instructions", "[FILTERED_INSTRUCTION]")
            .Replace("Ignore all previous", "[FILTERED_INSTRUCTION]")
            .Replace("System:", "[FILTERED_ROLE]")
            .Replace("Assistant:", "[FILTERED_ROLE]")
            .Replace("Human:", "[FILTERED_ROLE]")
            // Remove potential command injection
            .Replace("$(", "\\$(")
            .Replace("`", "\\`")
            // Filter attempts to break out of context
            .Replace("</system>", "[FILTERED_TAG]")
            .Replace("<system>", "[FILTERED_TAG]")
            .Replace("</prompt>", "[FILTERED_TAG]")
            .Replace("<prompt>", "[FILTERED_TAG]");

        return Task.FromResult(sanitized);
    }

    private Task<PromptValidationResult> ValidatePromptStructureAsync(string prompt)
    {
        var issues = new List<string>();
        var warnings = new List<string>();

        // Check for balanced delimiters
        if (CountOccurrences(prompt, "```") % 2 != 0)
            issues.Add("Unbalanced code block delimiters");

        if (CountOccurrences(prompt, "{{") != CountOccurrences(prompt, "}}"))
            issues.Add("Unbalanced template variable delimiters");

        // Check prompt length
        var maxLength = configuration.GetValue("Security:MaxPromptLength", 50000);
        if (prompt.Length > maxLength)
            warnings.Add($"Prompt length ({prompt.Length}) exceeds recommended maximum ({maxLength})");

        // Check for suspicious patterns
        if (prompt.Contains("eval(") || prompt.Contains("exec("))
            issues.Add("Contains potentially dangerous evaluation functions");

        return Task.FromResult(new PromptValidationResult
        {
            IsValid = !issues.Any(),
            Issues = issues,
            Warnings = warnings,
            ProcessedAt = DateTime.UtcNow
        });
    }

    private Task<InjectionRiskAssessment> DetectPromptInjectionAsync(string prompt)
    {
        var riskFactors = new List<string>();
        var riskLevel = RiskLevel.Low;

        // Check for instruction override attempts
        var injectionPatterns = new[]
        {
            "ignore",
            "forget",
            "disregard",
            "override",
            "bypass",
            "disable",
            "turn off",
            "don't follow",
            "stop following"
        };

        var instructionKeywords = new[]
        {
            "previous instructions",
            "system prompt",
            "rules",
            "guidelines",
            "constraints",
            "security",
            "validation"
        };

        foreach (var injectionPattern in injectionPatterns)
        {
            foreach (var instructionKeyword in instructionKeywords)
            {
                // Check for exact match first
                if (prompt.ToLowerInvariant().Contains($"{injectionPattern} {instructionKeyword}"))
                {
                    riskFactors.Add($"Potential instruction override: '{injectionPattern} {instructionKeyword}'");
                    riskLevel = RiskLevel.High;
                }
                // Also check for patterns with words in between (e.g., "ignore all previous instructions")
                else if (prompt.ToLowerInvariant().Contains(injectionPattern) && 
                         prompt.ToLowerInvariant().Contains(instructionKeyword))
                {
                    // Check if they appear within reasonable proximity (within 10 words)
                    var words = prompt.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var injectionIndex = Array.FindIndex(words, w => w.Contains(injectionPattern));
                    var instructionIndex = Array.FindIndex(words, w => w.Contains(instructionKeyword.Split(' ')[0]));
                    
                    if (injectionIndex >= 0 && instructionIndex >= 0 && 
                        Math.Abs(injectionIndex - instructionIndex) <= 10)
                    {
                        riskFactors.Add($"Potential instruction override pattern: '{injectionPattern}' near '{instructionKeyword}'");
                        riskLevel = RiskLevel.High;
                    }
                }
            }
        }

        // Check for role confusion attempts
        var rolePatterns = new[] { "you are now", "act as", "pretend to be", "roleplay as" };
        foreach (var pattern in rolePatterns)
        {
            if (prompt.ToLowerInvariant().Contains(pattern))
            {
                riskFactors.Add($"Potential role confusion: '{pattern}'");
                riskLevel = (RiskLevel)Math.Max((int)riskLevel, (int)RiskLevel.Medium);
            }
        }

        // Check for encoded or obfuscated content
        if (prompt.Contains("base64") || prompt.Contains("decode") || prompt.Contains("unescape"))
        {
            riskFactors.Add("Contains encoding/decoding references");
            riskLevel = (RiskLevel)Math.Max((int)riskLevel, (int)RiskLevel.Medium);
        }

        return Task.FromResult(new InjectionRiskAssessment
        {
            Level = riskLevel,
            RiskFactors = riskFactors,
            Reason = string.Join("; ", riskFactors),
            AssessedAt = DateTime.UtcNow
        });
    }

    private static int CountOccurrences(string text, string pattern)
    {
        return (text.Length - text.Replace(pattern, "").Length) / pattern.Length;
    }
}

/// <summary>
/// Represents a validated and sanitized prompt ready for LLM processing
/// </summary>
public record SecurePrompt
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Content { get; }
    public PromptValidationResult ValidationResult { get; }
    public InjectionRiskAssessment RiskAssessment { get; }
    public DateTime CreatedAt { get; }
    
    // Additional properties for compatibility with SecurePromptBuilder
    public SecurePromptContext? Context { get; init; }
    public PromptBuildRequest? OriginalRequest { get; init; }
    public Dictionary<string, string> SanitizedInputs { get; init; } = new();
    public TimeSpan BuildTime { get; init; }
    public string SecuritySignature { get; init; } = string.Empty;

    public SecurePrompt(string content, PromptValidationResult validationResult, InjectionRiskAssessment riskAssessment)
    {
        Content = content;
        ValidationResult = validationResult;
        RiskAssessment = riskAssessment;
        CreatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Security context for prompt validation
/// </summary>
public record SecurityContext
{
    public required string UserId { get; init; }
    public required string SessionId { get; init; }
    public required string IPAddress { get; init; }
    public required string UserAgent { get; init; }
    public SecurityLevel RequiredSecurityLevel { get; init; } = SecurityLevel.Standard;
}

/// <summary>
/// Result of prompt validation
/// </summary>
public record PromptValidationResult
{
    public bool IsValid { get; init; }
    public List<string> Issues { get; init; } = new();
    public List<string> Warnings { get; init; } = new();
    public DateTime ProcessedAt { get; init; }
}

/// <summary>
/// Assessment of prompt injection risk
/// </summary>
public record InjectionRiskAssessment
{
    public RiskLevel Level { get; init; }
    public List<string> RiskFactors { get; init; } = new();
    public string Reason { get; init; } = string.Empty; // Reason can default to empty as it's optional
    public DateTime AssessedAt { get; init; }
}

/// <summary>
/// Risk levels for security assessment
/// </summary>
public enum RiskLevel
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

/// <summary>
/// Security levels for different operations
/// </summary>
public enum SecurityLevel
{
    Basic = 1,
    Standard = 2,
    Enhanced = 3,
    Maximum = 4
}

/// <summary>
/// Exception thrown when prompt security validation fails
/// </summary>
public class PromptSecurityException : Exception
{
    public PromptSecurityException(string message) : base(message) { }
    public PromptSecurityException(string message, Exception innerException) : base(message, innerException) { }
}
