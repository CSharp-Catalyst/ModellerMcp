using Microsoft.Extensions.Logging;

using Modeller.Mcp.Shared.CodeGeneration.LLM;

namespace Modeller.Mcp.Shared.CodeGeneration.Security;

/// <summary>
/// Secure wrapper for LLM services that enforces security policies and audit logging
/// </summary>
public interface ISecureLlmService
{
    /// <summary>
    /// Generates code with comprehensive security controls and audit logging
    /// </summary>
    Task<SecureLlmResponse> GenerateSecureCodeAsync(SecureLlmRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the security context and permissions for LLM operations
    /// </summary>
    Task<SecurityValidationResult> ValidateSecurityContextAsync(SecurityContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs post-generation security validation on LLM responses
    /// </summary>
    Task<PostGenerationValidationResult> ValidateGeneratedContentAsync(string content, SecurityContext context, CancellationToken cancellationToken = default);
}

/// <summary>
/// Production implementation of secure LLM service with comprehensive security controls
/// </summary>
public class SecureLlmService(
    ILlmService llmService,
    IPromptSecurityService securityService,
    ISecurePromptBuilder promptBuilder,
    IPromptAuditLogger auditLogger,
    ILogger<SecureLlmService> logger) : ISecureLlmService
{
    public async Task<SecureLlmResponse> GenerateSecureCodeAsync(SecureLlmRequest request, CancellationToken cancellationToken = default)
    {
        var operationId = Guid.NewGuid();
        var startTime = DateTime.UtcNow;

        logger.LogInformation("Starting secure LLM generation {OperationId} for user {UserId}, model {ModelId}",
            operationId, request.SecurityContext.UserId, request.ModelId);

        try
        {
            // Phase 1: Security Context Validation
            var contextValidation = await ValidateSecurityContextAsync(request.SecurityContext, cancellationToken);
            if (!contextValidation.IsValid)
            {
                return await CreateFailureResponseAsync(request, operationId,
                    $"Security context validation failed: {string.Join(", ", contextValidation.Issues)}",
                    startTime);
            }

            // Phase 2: Prompt Security Validation & Sanitization
            var promptValidationResult = await securityService.ValidatePromptAsync(request.RawPrompt, request.SecurityContext);
            var injectionRisk = await securityService.AssessPromptInjectionRiskAsync(request.RawPrompt);

            // Log prompt validation audit entry
            var promptAuditEntry = new PromptAuditEntry
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                ModelId = request.ModelId,
                OriginalPrompt = request.RawPrompt,
                ValidationResult = promptValidationResult,
                InjectionRisk = injectionRisk,
                SecurityContext = request.SecurityContext,
                ProcessingDuration = DateTime.UtcNow - startTime
            };

            await auditLogger.LogPromptValidationAsync(promptAuditEntry);

            // Reject high-risk prompts based on security level
            if (ShouldRejectPrompt(injectionRisk, request.SecurityContext.RequiredSecurityLevel))
            {
                logger.LogWarning("Rejecting high-risk prompt for user {UserId}: {RiskLevel} - {RiskFactors}",
                    request.SecurityContext.UserId, injectionRisk.Level, string.Join(", ", injectionRisk.RiskFactors));

                return await CreateFailureResponseAsync(request, operationId,
                    $"Prompt rejected due to security risk: {injectionRisk.Level}",
                    startTime);
            }

            // Phase 3: Secure Prompt Building
            var promptBuildRequest = new PromptBuildRequest
            {
                UserId = request.SecurityContext.UserId,
                SessionId = request.SecurityContext.SessionId,
                PromptType = request.PromptType,
                SecurityLevel = request.SecurityContext.RequiredSecurityLevel,
                Inputs = request.PromptInputs,
                AllowCodeGeneration = request.AllowCodeGeneration,
                IPAddress = request.SecurityContext.IPAddress,
                UserAgent = request.SecurityContext.UserAgent
            };

            var securePrompt = await promptBuilder.BuildSecurePromptAsync(promptBuildRequest, cancellationToken);
            promptAuditEntry = promptAuditEntry with { SanitizedPrompt = securePrompt.Content };

            // Phase 4: LLM Generation with Security Parameters
            var llmRequest = new LlmRequest
            {
                Prompt = securePrompt.Content,
                ModelId = request.ModelId,
                Parameters = CreateSecureLlmParameters(request.SecurityContext.RequiredSecurityLevel),
                SecurityContext = request.SecurityContext,
                Metadata = new Dictionary<string, object>
                {
                    ["OperationId"] = operationId,
                    ["SecurePromptId"] = securePrompt.Id,
                    ["SecurityLevel"] = request.SecurityContext.RequiredSecurityLevel.ToString(),
                    ["UserId"] = request.SecurityContext.UserId
                }
            };

            var llmResponse = await llmService.GenerateCodeAsync(llmRequest, cancellationToken);

            if (!llmResponse.IsSuccess)
                return await CreateFailureResponseAsync(request, operationId, $"LLM generation failed: {llmResponse.ErrorMessage}", startTime);

            // Phase 5: Post-Generation Security Validation
            var postValidation = await ValidateGeneratedContentAsync(llmResponse.Content, request.SecurityContext, cancellationToken);

            // Phase 6: Create Immutable Response Snapshot
            var responseSnapshot = await CreateResponseSnapshotAsync(llmResponse, securePrompt, postValidation);

            // Phase 7: Audit Logging
            var llmAuditEntry = new LlmAuditEntry
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                ModelId = request.ModelId,
                PromptId = promptAuditEntry.Id,
                ResponseContent = llmResponse.Content,
                ResponseLength = llmResponse.Content.Length,
                TokensUsed = llmResponse.Usage.TotalTokens,
                GenerationDuration = llmResponse.GenerationTime,
                PostValidationPassed = postValidation.IsValid,
                ValidationErrors = postValidation.Issues,
                SecurityContext = request.SecurityContext
            };

            await auditLogger.LogLlmInteractionAsync(llmAuditEntry);

            // Phase 8: Build Secure Response
            var secureLlmResponse = new SecureLlmResponse
            {
                OperationId = operationId,
                Content = llmResponse.Content,
                IsSuccess = postValidation.IsValid,
                ModelId = request.ModelId,
                Usage = llmResponse.Usage,
                GenerationTime = llmResponse.GenerationTime,
                SecurityContext = request.SecurityContext,
                PostValidationResult = postValidation,
                ResponseSnapshot = responseSnapshot,
                AuditTrail = new AuditTrail
                {
                    PromptAuditId = promptAuditEntry.Id,
                    LlmAuditId = llmAuditEntry.Id,
                    SecurityValidations = new List<string> { "context", "prompt", "injection_risk", "post_generation" },
                    TotalProcessingTime = DateTime.UtcNow - startTime
                },
                ErrorMessage = postValidation.IsValid ? null : string.Join("; ", postValidation.Issues)
            };

            logger.LogInformation("Secure LLM generation {OperationId} completed successfully for user {UserId} in {Duration}ms",
                operationId, request.SecurityContext.UserId, (DateTime.UtcNow - startTime).TotalMilliseconds);

            return secureLlmResponse;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Secure LLM generation {OperationId} failed for user {UserId}", 
                operationId, request.SecurityContext.UserId);

            return await CreateFailureResponseAsync(request, operationId, $"Internal error: {ex.Message}", startTime);
        }
    }

    public Task<SecurityValidationResult> ValidateSecurityContextAsync(SecurityContext context, CancellationToken cancellationToken = default)
    {
        var issues = new List<string>();

        // Validate required fields
        if (string.IsNullOrEmpty(context.UserId))
            issues.Add("UserId is required");

        if (string.IsNullOrEmpty(context.SessionId))
            issues.Add("SessionId is required");

        if (string.IsNullOrEmpty(context.IPAddress))
            issues.Add("IPAddress is required");

        // Validate security level
        if (!Enum.IsDefined(typeof(SecurityLevel), context.RequiredSecurityLevel))
            issues.Add("Invalid security level");

        // Additional security checks can be added here:
        // - Rate limiting per user/session
        // - IP address validation
        // - User permission checks
        // - Session validation

        return Task.FromResult(new SecurityValidationResult
        {
            IsValid = issues.Count == 0,
            Issues = issues,
            ValidatedAt = DateTime.UtcNow
        });
    }

    public Task<PostGenerationValidationResult> ValidateGeneratedContentAsync(string content, SecurityContext context, CancellationToken cancellationToken = default)
    {
        var issues = new List<string>();
        var warnings = new List<string>();

        try
        {
            // 1. Check for sensitive information leakage
            if (ContainsSensitiveInformation(content))
                issues.Add("Generated content contains potentially sensitive information");

            // 2. Validate content is appropriate for security level
            if (context.RequiredSecurityLevel >= SecurityLevel.Enhanced)
            {
                if (ContainsExecutableCode(content) && !IsWhitelistedCodePattern(content))
                    issues.Add("Generated content contains executable code not permitted at this security level");
            }

            // 3. Check for injection attack residuals
            if (ContainsInjectionIndicators(content))
                warnings.Add("Generated content may contain injection attempt indicators");

            // 4. Validate content length and structure
            if (content.Length > GetMaxContentLengthForSecurityLevel(context.RequiredSecurityLevel))
                issues.Add($"Generated content exceeds maximum length for security level {context.RequiredSecurityLevel}");

            // 5. Content quality checks
            if (string.IsNullOrWhiteSpace(content))
                issues.Add("Generated content is empty or whitespace only");

            return Task.FromResult(new PostGenerationValidationResult
            {
                IsValid = issues.Count == 0,
                Issues = issues,
                Warnings = warnings,
                ContentLength = content.Length,
                ValidatedAt = DateTime.UtcNow,
                SecurityLevel = context.RequiredSecurityLevel
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Post-generation validation failed for user {UserId}", context.UserId);

            return Task.FromResult(new PostGenerationValidationResult
            {
                IsValid = false,
                Issues = new List<string> { $"Validation error: {ex.Message}" },
                Warnings = new List<string>(),
                ContentLength = content?.Length ?? 0,
                ValidatedAt = DateTime.UtcNow,
                SecurityLevel = context.RequiredSecurityLevel
            });
        }
    }

    private async Task<SecureLlmResponse> CreateFailureResponseAsync(SecureLlmRequest request, Guid operationId, string errorMessage, DateTime startTime)
    {
        // Log the failure for audit purposes
        var failureAuditEntry = new LlmAuditEntry
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            ModelId = request.ModelId,
            PromptId = Guid.NewGuid(), // Placeholder
            ResponseContent = "[FAILED]",
            ResponseLength = 0,
            TokensUsed = 0,
            GenerationDuration = DateTime.UtcNow - startTime,
            PostValidationPassed = false,
            ValidationErrors = new List<string> { errorMessage },
            SecurityContext = request.SecurityContext
        };

        await auditLogger.LogLlmInteractionAsync(failureAuditEntry);

        return new SecureLlmResponse
        {
            OperationId = operationId,
            Content = string.Empty,
            IsSuccess = false,
            ModelId = request.ModelId,
            Usage = new LlmUsageInfo(),
            GenerationTime = DateTime.UtcNow - startTime,
            SecurityContext = request.SecurityContext,
            PostValidationResult = new PostGenerationValidationResult
            {
                IsValid = false,
                Issues = new List<string> { errorMessage },
                Warnings = new List<string>(),
                ContentLength = 0,
                ValidatedAt = DateTime.UtcNow,
                SecurityLevel = request.SecurityContext.RequiredSecurityLevel
            },
            ResponseSnapshot = null,
            AuditTrail = new AuditTrail
            {
                PromptAuditId = Guid.Empty,
                LlmAuditId = failureAuditEntry.Id,
                SecurityValidations = new List<string> { "failed" },
                TotalProcessingTime = DateTime.UtcNow - startTime
            },
            ErrorMessage = errorMessage
        };
    }

    private LlmParameters CreateSecureLlmParameters(SecurityLevel securityLevel)
    {
        return new LlmParameters
        {
            Temperature = securityLevel switch
            {
                SecurityLevel.Basic => 0.1,      // Very deterministic
                SecurityLevel.Standard => 0.3,   // Low creativity
                SecurityLevel.Enhanced => 0.5,   // Moderate creativity
                SecurityLevel.Maximum => 0.7,    // Higher creativity allowed
                _ => 0.1
            },
            MaxTokens = securityLevel switch
            {
                SecurityLevel.Basic => 1000,
                SecurityLevel.Standard => 2000,
                SecurityLevel.Enhanced => 4000,
                SecurityLevel.Maximum => 8000,
                _ => 1000
            },
            TopP = 0.9,
            FrequencyPenalty = 0.1,
            PresencePenalty = 0.1,
            Stop = new[] { "SECURITY_BOUNDARY_END", "SYSTEM:", "ADMIN:", "ROOT:" },
            Seed = securityLevel >= SecurityLevel.Enhanced ? null : 42 // Reproducible for high security
        };
    }

    private Task<ResponseSnapshot> CreateResponseSnapshotAsync(LlmResponse response, SecurePrompt prompt, PostGenerationValidationResult validation)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var contentHash = Convert.ToHexString(sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(response.Content)));
        var promptHash = Convert.ToHexString(sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(prompt.Content)));

        return Task.FromResult(new ResponseSnapshot
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            ContentHash = contentHash,
            PromptHash = promptHash,
            ModelId = response.ModelId,
            TokensUsed = response.Usage.TotalTokens,
            GenerationTime = response.GenerationTime,
            ValidationPassed = validation.IsValid,
            SecurityLevel = prompt.Context?.SecurityLevel ?? SecurityLevel.Standard,
            IsImmutable = true
        });
    }

    private bool ShouldRejectPrompt(InjectionRiskAssessment risk, SecurityLevel securityLevel)
    {
        return securityLevel switch
        {
            SecurityLevel.Basic => risk.Level >= RiskLevel.High,
            SecurityLevel.Standard => risk.Level >= RiskLevel.High,
            SecurityLevel.Enhanced => risk.Level >= RiskLevel.Medium,
            SecurityLevel.Maximum => risk.Level >= RiskLevel.Medium,
            _ => risk.Level >= RiskLevel.High
        };
    }

    private bool ContainsSensitiveInformation(string content)
    {
        var sensitivePatterns = new[]
        {
            @"\b\d{4}[-\s]?\d{4}[-\s]?\d{4}[-\s]?\d{4}\b", // Credit card numbers
            @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", // Email addresses
            @"\b\d{3}-\d{2}-\d{4}\b", // SSN
            @"\bpassword\s*[:=]\s*\S+", // Password assignments
            @"\bapi[_\s-]?key\s*[:=]\s*\S+", // API keys
            @"\btoken\s*[:=]\s*\S+", // Tokens
        };

        return sensitivePatterns.Any(pattern => 
            System.Text.RegularExpressions.Regex.IsMatch(content, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase));
    }

    private bool ContainsExecutableCode(string content)
    {
        var codePatterns = new[]
        {
            @"<script\b", // JavaScript
            @"\beval\s*\(", // Eval functions
            @"\bexec\s*\(", // Exec functions
            @"(?:^|\n)\s*\$\s", // Shell commands
            @"(?:^|\n)\s*sudo\s", // Sudo commands
            @"\bimport\s+os\b", // OS imports
            @"\bimport\s+subprocess\b", // Subprocess imports
        };

        return codePatterns.Any(pattern => 
            System.Text.RegularExpressions.Regex.IsMatch(content, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Multiline));
    }

    private bool IsWhitelistedCodePattern(string content)
    {
        // Allow specific patterns for Modeller YAML generation
        var whitelistedPatterns = new[]
        {
            @"^\s*name:\s*\w+", // YAML name properties
            @"^\s*type:\s*\w+", // YAML type properties
            @"^\s*description:\s*.+", // YAML descriptions
            @"^\s*properties:", // YAML properties sections
            @"^\s*-\s+\w+:", // YAML array items
        };

        var lines = content.Split('\n');
        return lines.All(line => 
            string.IsNullOrWhiteSpace(line) || 
            whitelistedPatterns.Any(pattern => 
                System.Text.RegularExpressions.Regex.IsMatch(line, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase)));
    }

    private bool ContainsInjectionIndicators(string content)
    {
        var injectionIndicators = new[]
        {
            "ignore previous", "forget previous", "new instructions", "system prompt",
            "admin mode", "developer mode", "debug mode", "override", "bypass"
        };

        return injectionIndicators.Any(indicator => 
            content.Contains(indicator, StringComparison.OrdinalIgnoreCase));
    }

    private int GetMaxContentLengthForSecurityLevel(SecurityLevel securityLevel)
    {
        return securityLevel switch
        {
            SecurityLevel.Basic => 5000,
            SecurityLevel.Standard => 10000,
            SecurityLevel.Enhanced => 25000,
            SecurityLevel.Maximum => 100000,
            _ => 5000
        };
    }
}

/// <summary>
/// Request for secure LLM generation
/// </summary>
public record SecureLlmRequest
{
    public required string RawPrompt { get; init; }
    public required string ModelId { get; init; }
    public required string PromptType { get; init; }
    public required SecurityContext SecurityContext { get; init; }
    public Dictionary<string, string> PromptInputs { get; init; } = new();
    public bool AllowCodeGeneration { get; init; } = true;
}

/// <summary>
/// Response from secure LLM generation
/// </summary>
public record SecureLlmResponse
{
    public Guid OperationId { get; init; }
    public required string Content { get; init; }
    public bool IsSuccess { get; init; }
    public required string ModelId { get; init; }
    public LlmUsageInfo Usage { get; init; } = new();
    public TimeSpan GenerationTime { get; init; }
    public required SecurityContext SecurityContext { get; init; }
    public required PostGenerationValidationResult PostValidationResult { get; init; }
    public ResponseSnapshot? ResponseSnapshot { get; init; }
    public required AuditTrail AuditTrail { get; init; }
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Result of security context validation
/// </summary>
public record SecurityValidationResult
{
    public bool IsValid { get; init; }
    public List<string> Issues { get; init; } = new();
    public DateTime ValidatedAt { get; init; }
}

/// <summary>
/// Result of post-generation content validation
/// </summary>
public record PostGenerationValidationResult
{
    public bool IsValid { get; init; }
    public List<string> Issues { get; init; } = new();
    public List<string> Warnings { get; init; } = new();
    public int ContentLength { get; init; }
    public DateTime ValidatedAt { get; init; }
    public SecurityLevel SecurityLevel { get; init; }
}

/// <summary>
/// Immutable snapshot of LLM response for audit purposes
/// </summary>
public record ResponseSnapshot
{
    public Guid Id { get; init; }
    public DateTime CreatedAt { get; init; }
    public required string ContentHash { get; init; }
    public required string PromptHash { get; init; }
    public required string ModelId { get; init; }
    public int TokensUsed { get; init; }
    public TimeSpan GenerationTime { get; init; }
    public bool ValidationPassed { get; init; }
    public SecurityLevel SecurityLevel { get; init; }
    public bool IsImmutable { get; init; }
}

/// <summary>
/// Audit trail for tracking all security validations
/// </summary>
public record AuditTrail
{
    public Guid PromptAuditId { get; init; }
    public Guid LlmAuditId { get; init; }
    public List<string> SecurityValidations { get; init; } = new();
    public TimeSpan TotalProcessingTime { get; init; }
}
