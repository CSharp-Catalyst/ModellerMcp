using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.RegularExpressions;

namespace Modeller.McpServer.CodeGeneration.Security;

/// <summary>
/// Builds secure prompts for LLM interactions with sanitization and injection prevention
/// </summary>
public interface ISecurePromptBuilder
{
    /// <summary>
    /// Builds a secure prompt with sanitization and context injection protection
    /// </summary>
    Task<SecurePrompt> BuildSecurePromptAsync(PromptBuildRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates and sanitizes user input before incorporating into prompts
    /// </summary>
    Task<SanitizationResult> SanitizeInputAsync(string input, SanitizationContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a secure context for prompt execution with appropriate boundaries
    /// </summary>
    SecurePromptContext CreateSecureContext(SecurityLevel securityLevel, string userId, string sessionId);
}

/// <summary>
/// Secure prompt builder implementation with comprehensive injection prevention
/// </summary>
public class SecurePromptBuilder : ISecurePromptBuilder
{
    private readonly ILogger<SecurePromptBuilder> _logger;
    private readonly IPromptSecurityService _securityService;
    private readonly Dictionary<string, string> _secureTemplates;

    // Regex patterns for detecting potential injection attempts
    private static readonly Regex[] InjectionPatterns = {
        new(@"\b(ignore|forget|disregard)\s+(previous|above|earlier|all)\s+(instructions?|prompts?|rules?|context)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"\b(you\s+are\s+now|act\s+as|pretend\s+to\s+be|roleplay\s+as)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"\b(system\s+prompt|admin\s+mode|developer\s+mode|debug\s+mode)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"```\s*(python|javascript|sql|bash|powershell|cmd)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"\b(execute|eval|run|compile|interpret)\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"\b(base64|decode|unescape|html|url|json)\s*(decode|encode|parse)", RegexOptions.IgnoreCase | RegexOptions.Compiled)
    };

    // Dangerous keywords that should be filtered or escaped
    private static readonly HashSet<string> DangerousKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "password", "secret", "token", "key", "credential", "auth", "login",
        "admin", "root", "system", "execute", "eval", "script", "injection",
        "sql", "xss", "csrf", "bypass", "exploit", "hack", "vulnerability"
    };

    public SecurePromptBuilder(ILogger<SecurePromptBuilder> logger, IPromptSecurityService securityService)
    {
        _logger = logger;
        _securityService = securityService;
        _secureTemplates = InitializeSecureTemplates();
    }

    public async Task<SecurePrompt> BuildSecurePromptAsync(PromptBuildRequest request, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            // 1. Validate the request
            await ValidatePromptRequestAsync(request, cancellationToken);

            // 2. Create secure context
            var secureContext = CreateSecureContext(request.SecurityLevel, request.UserId, request.SessionId);

            // 3. Sanitize all input parameters
            var sanitizedInputs = new Dictionary<string, string>();
            foreach (var input in request.Inputs)
            {
                var sanitizationContext = new SanitizationContext
                {
                    InputType = input.Key,
                    SecurityLevel = request.SecurityLevel,
                    AllowCodeBlocks = request.AllowCodeGeneration,
                    AllowMarkdown = true
                };

                var sanitizationResult = await SanitizeInputAsync(input.Value, sanitizationContext, cancellationToken);
                sanitizedInputs[input.Key] = sanitizationResult.SanitizedContent;

                if (sanitizationResult.RiskLevel >= RiskLevel.High)
                {
                    _logger.LogWarning("High-risk content detected in input {InputKey} for user {UserId}: {RiskFactors}",
                        input.Key, request.UserId, string.Join(", ", sanitizationResult.RiskFactors));
                }
            }

            // 4. Build the secure prompt using templates
            var promptTemplate = GetSecureTemplate(request.PromptType);
            var securePromptContent = await BuildPromptFromTemplateAsync(promptTemplate, sanitizedInputs, secureContext, cancellationToken);

            // 5. Apply final security boundaries
            var finalPrompt = ApplySecurityBoundaries(securePromptContent, secureContext);

            // 6. Validate the final prompt
            var securityContext = new SecurityContext
            {
                UserId = request.UserId,
                SessionId = request.SessionId,
                RequiredSecurityLevel = request.SecurityLevel,
                IPAddress = "internal", // Default for service-to-service calls
                UserAgent = "SecurePromptBuilder/1.0"
            };
            var validationResult = await _securityService.ValidatePromptAsync(finalPrompt, securityContext);
            
            // 7. Assess injection risk
            var injectionRisk = await _securityService.AssessPromptInjectionRiskAsync(finalPrompt);

            // 8. Create the secure prompt object
            var securePrompt = new SecurePrompt(finalPrompt, validationResult, injectionRisk)
            {
                Context = secureContext,
                OriginalRequest = request,
                SanitizedInputs = sanitizedInputs,
                BuildTime = DateTime.UtcNow - startTime,
                SecuritySignature = ComputeSecuritySignature(finalPrompt, secureContext)
            };

            _logger.LogInformation("Secure prompt built successfully for user {UserId}, type {PromptType}, duration {Duration}ms",
                request.UserId, request.PromptType, securePrompt.BuildTime.TotalMilliseconds);

            return securePrompt;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build secure prompt for user {UserId}, type {PromptType}", 
                request.UserId, request.PromptType);
            throw new PromptSecurityException("Failed to build secure prompt", ex);
        }
    }

    public async Task<SanitizationResult> SanitizeInputAsync(string input, SanitizationContext context, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(input))
        {
            return new SanitizationResult
            {
                SanitizedContent = string.Empty,
                RiskLevel = RiskLevel.Low,
                RiskFactors = new List<string>(),
                ModificationsApplied = new List<string>()
            };
        }

        var riskFactors = new List<string>();
        var modifications = new List<string>();
        var sanitizedContent = input;
        var riskLevel = RiskLevel.Low;

        // 1. Check for injection patterns
        foreach (var pattern in InjectionPatterns)
        {
            if (pattern.IsMatch(sanitizedContent))
            {
                riskFactors.Add($"Potential injection pattern detected: {pattern}");
                riskLevel = (RiskLevel)Math.Max((int)riskLevel, (int)RiskLevel.Medium);
            }
        }

        // 2. Check for dangerous keywords
        var lowerInput = sanitizedContent.ToLowerInvariant();
        foreach (var keyword in DangerousKeywords)
        {
            if (lowerInput.Contains(keyword))
            {
                riskFactors.Add($"Dangerous keyword detected: {keyword}");
                riskLevel = (RiskLevel)Math.Max((int)riskLevel, (int)RiskLevel.Medium);
            }
        }

        // 3. Sanitize based on context and security level
        if (context.SecurityLevel >= SecurityLevel.Enhanced)
        {
            // Aggressive sanitization for high-security contexts
            sanitizedContent = await ApplyAggressiveSanitizationAsync(sanitizedContent, context, modifications, cancellationToken);
        }
        else
        {
            // Standard sanitization
            sanitizedContent = await ApplyStandardSanitizationAsync(sanitizedContent, context, modifications, cancellationToken);
        }

        // 4. Final risk assessment
        if (riskFactors.Count >= 3)
        {
            riskLevel = (RiskLevel)Math.Max((int)riskLevel, (int)RiskLevel.High);
        }

        return new SanitizationResult
        {
            SanitizedContent = sanitizedContent,
            RiskLevel = riskLevel,
            RiskFactors = riskFactors,
            ModificationsApplied = modifications
        };
    }

    public SecurePromptContext CreateSecureContext(SecurityLevel securityLevel, string userId, string sessionId)
    {
        return new SecurePromptContext
        {
            Id = Guid.NewGuid(),
            SecurityLevel = securityLevel,
            UserId = userId,
            SessionId = sessionId,
            CreatedAt = DateTime.UtcNow,
            AllowedCapabilities = GetAllowedCapabilities(securityLevel),
            SecurityBoundaries = GetSecurityBoundaries(securityLevel),
            MaxTokens = GetMaxTokensForSecurityLevel(securityLevel),
            TimeoutSeconds = GetTimeoutForSecurityLevel(securityLevel)
        };
    }

    private async Task ValidatePromptRequestAsync(PromptBuildRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.UserId))
            throw new ArgumentException("UserId is required for secure prompt building");

        if (string.IsNullOrEmpty(request.SessionId))
            throw new ArgumentException("SessionId is required for secure prompt building");

        if (string.IsNullOrEmpty(request.PromptType))
            throw new ArgumentException("PromptType is required for secure prompt building");

        // Validate against security service
        var securityContext = new SecurityContext
        {
            UserId = request.UserId,
            SessionId = request.SessionId,
            IPAddress = request.IPAddress ?? "unknown",
            UserAgent = request.UserAgent ?? "unknown",
            RequiredSecurityLevel = request.SecurityLevel
        };

        var validationResult = await _securityService.ValidatePromptAsync("", securityContext);
        if (!validationResult.IsValid)
        {
            throw new PromptSecurityException($"Security validation failed: {string.Join(", ", validationResult.Issues)}");
        }
    }

    private async Task<string> ApplyStandardSanitizationAsync(string content, SanitizationContext context, List<string> modifications, CancellationToken cancellationToken)
    {
        var sanitized = content;

        // Remove or escape potentially dangerous content
        if (!context.AllowCodeBlocks)
        {
            // Remove code blocks
            sanitized = Regex.Replace(sanitized, @"```[\s\S]*?```", "[CODE_BLOCK_REMOVED]", RegexOptions.Multiline);
            if (sanitized != content) modifications.Add("Removed code blocks");
        }

        // Escape special characters that could be used for injection
        sanitized = EscapeSpecialCharacters(sanitized);
        if (sanitized != content) modifications.Add("Escaped special characters");

        // Limit length based on security level
        var maxLength = GetMaxContentLength(context.SecurityLevel);
        if (sanitized.Length > maxLength)
        {
            sanitized = sanitized.Substring(0, maxLength) + "...[TRUNCATED]";
            modifications.Add($"Truncated to {maxLength} characters");
        }

        return sanitized;
    }

    private async Task<string> ApplyAggressiveSanitizationAsync(string content, SanitizationContext context, List<string> modifications, CancellationToken cancellationToken)
    {
        var sanitized = await ApplyStandardSanitizationAsync(content, context, modifications, cancellationToken);

        // Additional aggressive measures
        foreach (var keyword in DangerousKeywords)
        {
            var pattern = $@"\b{Regex.Escape(keyword)}\b";
            if (Regex.IsMatch(sanitized, pattern, RegexOptions.IgnoreCase))
            {
                sanitized = Regex.Replace(sanitized, pattern, "[FILTERED]", RegexOptions.IgnoreCase);
                modifications.Add($"Filtered dangerous keyword: {keyword}");
            }
        }

        return sanitized;
    }

    private string EscapeSpecialCharacters(string content)
    {
        return content
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("'", "\\'")
            .Replace("`", "\\`")
            .Replace("${", "\\${")
            .Replace("{{", "\\{{")
            .Replace("}}", "\\}}");
    }

    private Dictionary<string, string> InitializeSecureTemplates()
    {
        return new Dictionary<string, string>
        {
            ["ModelAnalysis"] = @"
                # SECURE CONTEXT: Model Analysis Task
                ## SECURITY BOUNDARY: Analysis of Modeller model definitions only
                ## RESTRICTIONS: No code execution, no external access, no sensitive data processing
                
                You are a specialized assistant for analyzing Modeller model definitions. 
                Your role is strictly limited to:
                1. Analyzing YAML model structure and syntax
                2. Providing recommendations for model improvements
                3. Identifying potential modeling issues
                
                ## INPUT DATA (SANITIZED):
                Model Path: {modelPath}
                Analysis Type: {analysisType}
                
                ## SECURITY CONSTRAINTS:
                - Process only Modeller YAML content
                - No execution of embedded code
                - No access to external systems or files
                - Output only analysis and recommendations
                
                Please analyze the provided model definition and provide structured feedback.
            ",
            
            ["DomainReview"] = @"
                # SECURE CONTEXT: Domain Review Task
                ## SECURITY BOUNDARY: Domain-level model review only
                ## RESTRICTIONS: No code execution, no external access, analysis only
                
                You are analyzing models within a specific domain for consistency and compliance.
                Your analysis is limited to:
                1. Model structure consistency
                2. Naming convention compliance
                3. Domain-specific best practices
                4. Inter-model relationship validation
                
                ## INPUT DATA (SANITIZED):
                Domain Path: {domainPath}
                Include Shared: {includeShared}
                
                ## SECURITY CONSTRAINTS:
                - Analyze only provided model definitions
                - No modification of source files
                - No external system access
                - Output recommendations only
                
                Provide your domain-level analysis and recommendations.
            ",
            
            ["ModelTemplate"] = @"
                # SECURE CONTEXT: Model Template Generation
                ## SECURITY BOUNDARY: Template generation only
                ## RESTRICTIONS: Generate templates only, no code execution
                
                You are generating a template for a new Modeller model based on requirements.
                Your output must be:
                1. Valid YAML structure
                2. Compliant with Modeller conventions
                3. Include only template content
                4. Follow security best practices
                
                ## INPUT DATA (SANITIZED):
                Model Type: {modelType}
                Domain: {domain}
                Description: {description}
                
                ## SECURITY CONSTRAINTS:
                - Generate template content only
                - No executable code in templates
                - Standard Modeller YAML structure
                - No sensitive data in templates
                
                Generate the requested model template.
            "
        };
    }

    private string GetSecureTemplate(string promptType)
    {
        return _secureTemplates.GetValueOrDefault(promptType, _secureTemplates["ModelAnalysis"]);
    }

    private async Task<string> BuildPromptFromTemplateAsync(string template, Dictionary<string, string> inputs, SecurePromptContext context, CancellationToken cancellationToken)
    {
        var prompt = template;

        // Replace template placeholders with sanitized inputs
        foreach (var input in inputs)
        {
            var placeholder = $"{{{input.Key}}}";
            prompt = prompt.Replace(placeholder, input.Value);
        }

        // Add security metadata
        prompt += $"\n\n## SECURITY METADATA:\n";
        prompt += $"- Context ID: {context.Id}\n";
        prompt += $"- Security Level: {context.SecurityLevel}\n";
        prompt += $"- Max Tokens: {context.MaxTokens}\n";
        prompt += $"- Session: {context.SessionId}\n";

        return prompt;
    }

    private string ApplySecurityBoundaries(string prompt, SecurePromptContext context)
    {
        var boundaryHeader = "=== SECURITY BOUNDARY START ===\n";
        var boundaryFooter = "\n=== SECURITY BOUNDARY END ===\n";
        var securityReminder = $"SECURITY REMINDER: This session is limited to {context.SecurityLevel} operations only. No code execution or external access permitted.";

        return boundaryHeader + prompt + boundaryFooter + securityReminder;
    }

    private HashSet<string> GetAllowedCapabilities(SecurityLevel securityLevel)
    {
        return securityLevel switch
        {
            SecurityLevel.Basic => new HashSet<string> { "analysis", "validation" },
            SecurityLevel.Standard => new HashSet<string> { "analysis", "validation", "template_generation" },
            SecurityLevel.Enhanced => new HashSet<string> { "analysis", "validation", "template_generation", "recommendations" },
            SecurityLevel.Maximum => new HashSet<string> { "analysis", "validation", "template_generation", "recommendations", "migration_guidance" },
            _ => new HashSet<string> { "analysis" }
        };
    }

    private List<string> GetSecurityBoundaries(SecurityLevel securityLevel)
    {
        var boundaries = new List<string>
        {
            "NO_CODE_EXECUTION",
            "NO_EXTERNAL_ACCESS",
            "NO_SENSITIVE_DATA",
            "MODELLER_MODELS_ONLY"
        };

        if (securityLevel >= SecurityLevel.Enhanced)
        {
            boundaries.Add("AUDIT_ALL_OPERATIONS");
            boundaries.Add("VALIDATE_ALL_INPUTS");
        }

        return boundaries;
    }

    private int GetMaxTokensForSecurityLevel(SecurityLevel securityLevel)
    {
        return securityLevel switch
        {
            SecurityLevel.Basic => 1000,
            SecurityLevel.Standard => 2000,
            SecurityLevel.Enhanced => 4000,
            SecurityLevel.Maximum => 8000,
            _ => 1000
        };
    }

    private int GetTimeoutForSecurityLevel(SecurityLevel securityLevel)
    {
        return securityLevel switch
        {
            SecurityLevel.Basic => 30,
            SecurityLevel.Standard => 60,
            SecurityLevel.Enhanced => 120,
            SecurityLevel.Maximum => 300,
            _ => 30
        };
    }

    private int GetMaxContentLength(SecurityLevel securityLevel)
    {
        return securityLevel switch
        {
            SecurityLevel.Basic => 1000,
            SecurityLevel.Standard => 5000,
            SecurityLevel.Enhanced => 10000,
            SecurityLevel.Maximum => 50000,
            _ => 1000
        };
    }

    private string ComputeSecuritySignature(string prompt, SecurePromptContext context)
    {
        var content = $"{prompt}|{context.Id}|{context.SecurityLevel}|{context.CreatedAt:O}";
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(hash);
    }
}

/// <summary>
/// Request for building a secure prompt
/// </summary>
public record PromptBuildRequest
{
    public required string UserId { get; init; }
    public required string SessionId { get; init; }
    public required string PromptType { get; init; }
    public SecurityLevel SecurityLevel { get; init; } = SecurityLevel.Standard;
    public Dictionary<string, string> Inputs { get; init; } = new();
    public bool AllowCodeGeneration { get; init; } = false;
    public string? IPAddress { get; init; }
    public string? UserAgent { get; init; }
}

/// <summary>
/// Context for input sanitization
/// </summary>
public record SanitizationContext
{
    public required string InputType { get; init; }
    public SecurityLevel SecurityLevel { get; init; } = SecurityLevel.Standard;
    public bool AllowCodeBlocks { get; init; } = false;
    public bool AllowMarkdown { get; init; } = true;
}

/// <summary>
/// Result of input sanitization
/// </summary>
public record SanitizationResult
{
    public required string SanitizedContent { get; init; }
    public RiskLevel RiskLevel { get; init; }
    public List<string> RiskFactors { get; init; } = new();
    public List<string> ModificationsApplied { get; init; } = new();
}

/// <summary>
/// Security context for prompt execution
/// </summary>
public record SecurePromptContext
{
    public Guid Id { get; init; }
    public SecurityLevel SecurityLevel { get; init; }
    public required string UserId { get; init; }
    public required string SessionId { get; init; }
    public DateTime CreatedAt { get; init; }
    public HashSet<string> AllowedCapabilities { get; init; } = new();
    public List<string> SecurityBoundaries { get; init; } = new();
    public int MaxTokens { get; init; }
    public int TimeoutSeconds { get; init; }
}
