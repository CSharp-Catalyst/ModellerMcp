using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using System.Text.Json;

namespace Modeller.Mcp.Shared.CodeGeneration.Security;

/// <summary>
/// Interface for secure audit logging of prompt validation and LLM interactions
/// </summary>
public interface IPromptAuditLogger
{
    Task LogPromptValidationAsync(PromptAuditEntry entry);
    Task LogLlmInteractionAsync(LlmAuditEntry entry);
    Task LogSecurityViolationAsync(SecurityViolationEntry entry);
    Task LogCodeGenerationAsync(CodeGenerationAuditEntry entry);
}

/// <summary>
/// Provides secure audit logging for prompt validation and LLM interactions
/// </summary>
public class PromptAuditLogger : IPromptAuditLogger
{
    private readonly ILogger<PromptAuditLogger> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _auditLogPath;
    private readonly bool _enableFileLogging;
    private readonly bool _enableStructuredLogging;
    private readonly SemaphoreSlim _fileLock = new(1, 1);

    public PromptAuditLogger(ILogger<PromptAuditLogger> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _auditLogPath = _configuration.GetValue("Security:AuditLogPath", "logs/audit");
        _enableFileLogging = _configuration.GetValue("Security:EnableFileAuditLogging", true);
        _enableStructuredLogging = _configuration.GetValue("Security:EnableStructuredAuditLogging", true);
        
        EnsureAuditDirectoryExists();
    }

    /// <summary>
    /// Logs prompt validation activity for audit trail
    /// </summary>
    public async Task LogPromptValidationAsync(PromptAuditEntry entry)
    {
        try
        {
            // Log to structured logger for integration with logging infrastructure
            if (_enableStructuredLogging)
            {
                using var scope = _logger.BeginScope(new Dictionary<string, object>
                {
                    ["AuditEntryId"] = entry.Id,
                    ["ModelId"] = entry.ModelId,
                    ["UserId"] = entry.SecurityContext?.UserId ?? "Unknown",
                    ["SessionId"] = entry.SecurityContext?.SessionId ?? "Unknown",
                    ["RiskLevel"] = entry.InjectionRisk?.Level.ToString() ?? "Unknown",
                    ["ValidationSuccess"] = entry.ValidationResult?.IsValid ?? false,
                    ["ProcessingDuration"] = entry.ProcessingDuration.TotalMilliseconds
                });

                _logger.LogInformation("Prompt validation completed for model {ModelId} with risk level {RiskLevel}",
                    entry.ModelId, entry.InjectionRisk?.Level.ToString() ?? "Unknown");

                if (entry.ValidationResult?.Issues.Any() == true)
                {
                    _logger.LogWarning("Prompt validation issues detected: {Issues}",
                        string.Join(", ", entry.ValidationResult.Issues));
                }
            }

            // Write to dedicated audit file for compliance and forensics
            if (_enableFileLogging)
                await WriteAuditEntryToFileAsync(entry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log audit entry {AuditEntryId}", entry.Id);
            // Don't rethrow - audit logging failure shouldn't break the main flow
        }
    }

    /// <summary>
    /// Logs LLM response generation activity for audit trail
    /// </summary>
    public async Task LogLlmResponseAsync(LlmAuditEntry entry)
    {
        try
        {
            if (_enableStructuredLogging)
            {
                using var scope = _logger.BeginScope(new Dictionary<string, object>
                {
                    ["AuditEntryId"] = entry.Id,
                    ["ModelId"] = entry.ModelId,
                    ["PromptId"] = entry.PromptId,
                    ["ResponseLength"] = entry.ResponseLength,
                    ["TokensUsed"] = entry.TokensUsed,
                    ["GenerationDuration"] = entry.GenerationDuration.TotalMilliseconds,
                    ["PostValidationPassed"] = entry.PostValidationPassed
                });

                _logger.LogInformation("LLM response generated for model {ModelId} (tokens: {TokensUsed}, duration: {Duration}ms)",
                    entry.ModelId, entry.TokensUsed, entry.GenerationDuration.TotalMilliseconds);

                if (!entry.PostValidationPassed)
                {
                    _logger.LogWarning("LLM response failed post-generation validation: {ValidationErrors}",
                        string.Join(", ", entry.ValidationErrors ?? new List<string>()));
                }
            }

            if (_enableFileLogging)
                await WriteAuditEntryToFileAsync(entry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log LLM audit entry {AuditEntryId}", entry.Id);
        }
    }

    /// <summary>
    /// Logs LLM interaction activity for audit trail
    /// </summary>
    public async Task LogLlmInteractionAsync(LlmAuditEntry entry)
    {
        try
        {
            // Log to structured logger
            if (_enableStructuredLogging)
            {
                using var scope = _logger.BeginScope(new Dictionary<string, object>
                {
                    ["AuditEntryId"] = entry.Id,
                    ["ModelId"] = entry.ModelId,
                    ["PromptId"] = entry.PromptId,
                    ["UserId"] = entry.SecurityContext?.UserId ?? "Unknown",
                    ["SessionId"] = entry.SecurityContext?.SessionId ?? "Unknown",
                    ["ResponseLength"] = entry.ResponseLength,
                    ["TokensUsed"] = entry.TokensUsed,
                    ["GenerationDuration"] = entry.GenerationDuration.TotalMilliseconds,
                    ["PostValidationPassed"] = entry.PostValidationPassed
                });

                _logger.LogInformation("LLM interaction completed for model {ModelId} with {TokensUsed} tokens in {Duration}ms", 
                    entry.ModelId, entry.TokensUsed, entry.GenerationDuration.TotalMilliseconds);
            }

            // Write to audit file
            if (_enableFileLogging)
                await WriteAuditEntryToFileAsync(entry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log LLM interaction audit entry {AuditEntryId}", entry.Id);
        }
    }

    /// <summary>
    /// Logs security violation activity for audit trail
    /// </summary>
    public async Task LogSecurityViolationAsync(SecurityViolationEntry entry)
    {
        try
        {
            // Log to structured logger with high severity
            if (_enableStructuredLogging)
            {
                using var scope = _logger.BeginScope(new Dictionary<string, object>
                {
                    ["AuditEntryId"] = entry.Id,
                    ["ViolationType"] = entry.ViolationType,
                    ["RiskLevel"] = entry.RiskLevel.ToString(),
                    ["ModelId"] = entry.ModelId,
                    ["UserId"] = entry.SecurityContext?.UserId ?? "Unknown",
                    ["SessionId"] = entry.SecurityContext?.SessionId ?? "Unknown",
                    ["AttackVector"] = entry.AttackVector ?? "Unknown",
                    ["ActionTaken"] = entry.ActionTaken
                });

                _logger.LogWarning("SECURITY VIOLATION: {ViolationType} detected for model {ModelId} - Risk Level: {RiskLevel}", 
                    entry.ViolationType, entry.ModelId, entry.RiskLevel);
            }

            // Write to audit file with high priority
            if (_enableFileLogging)
                await WriteAuditEntryToFileAsync(entry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log security violation audit entry {AuditEntryId}", entry.Id);
        }
    }

    /// <summary>
    /// Logs code generation activity for audit trail
    /// </summary>
    public async Task LogCodeGenerationAsync(CodeGenerationAuditEntry entry)
    {
        try
        {
            // Log to structured logger
            if (_enableStructuredLogging)
            {
                using var scope = _logger.BeginScope(new Dictionary<string, object>
                {
                    ["AuditEntryId"] = entry.Id,
                    ["ModelId"] = entry.ModelId,
                    ["PromptId"] = entry.PromptId,
                    ["LlmResponseId"] = entry.LlmResponseId,
                    ["UserId"] = entry.SecurityContext?.UserId ?? "Unknown",
                    ["SessionId"] = entry.SecurityContext?.SessionId ?? "Unknown",
                    ["GeneratedCodeType"] = entry.GeneratedCodeType ?? "Unknown",
                    ["GeneratedLines"] = entry.GeneratedLines,
                    ["ValidationPassed"] = entry.ValidationPassed,
                    ["SecurityScanPassed"] = entry.SecurityScanPassed,
                    ["GenerationDuration"] = entry.GenerationDuration.TotalMilliseconds
                });

                _logger.LogInformation("Code generation completed for model {ModelId} - {GeneratedLines} lines of {CodeType}", 
                    entry.ModelId, entry.GeneratedLines, entry.GeneratedCodeType);
            }

            // Write to audit file
            if (_enableFileLogging)
                await WriteAuditEntryToFileAsync(entry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log code generation audit entry {AuditEntryId}", entry.Id);
        }
    }

    /// <summary>
    /// Retrieves audit entries for a specific time range (for compliance reporting)
    /// </summary>
    public async Task<IEnumerable<PromptAuditEntry>> GetAuditEntriesAsync(
        DateTime fromDate, 
        DateTime toDate, 
        string? modelId = null,
        string? userId = null)
    {
        var entries = new List<PromptAuditEntry>();

        if (!_enableFileLogging)
        {
            _logger.LogWarning("File audit logging is disabled, cannot retrieve historical entries");
            return entries;
        }

        try
        {
            var auditFiles = Directory.GetFiles(_auditLogPath, "prompt-audit-*.json")
                .Where(f => IsFileInDateRange(f, fromDate, toDate))
                .OrderBy(f => f);

            foreach (var file in auditFiles)
            {
                var fileEntries = await ReadAuditEntriesFromFileAsync(file);
                entries.AddRange(fileEntries.Where(e => 
                    e.Timestamp >= fromDate && 
                    e.Timestamp <= toDate &&
                    (modelId == null || e.ModelId == modelId) &&
                    (userId == null || e.SecurityContext?.UserId == userId)));
            }

            return entries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve audit entries for date range {FromDate} to {ToDate}", 
                fromDate, toDate);
            return entries;
        }
    }

    private async Task WriteAuditEntryToFileAsync(PromptAuditEntry entry)
    {
        await _fileLock.WaitAsync();
        try
        {
            var fileName = $"prompt-audit-{DateTime.UtcNow:yyyy-MM-dd}.json";
            var filePath = Path.Combine(_auditLogPath, fileName);
            
            var auditRecord = new
            {
                EntryType = "PromptValidation",
                entry.Id,
                entry.Timestamp,
                entry.ModelId,
                entry.ProcessingDuration,
                SecurityContext = new
                {
                    entry.SecurityContext?.UserId,
                    entry.SecurityContext?.SessionId,
                    entry.SecurityContext?.IPAddress,
                    entry.SecurityContext?.UserAgent,
                    entry.SecurityContext?.RequiredSecurityLevel
                },
                ValidationResult = new
                {
                    entry.ValidationResult?.IsValid,
                    entry.ValidationResult?.Issues,
                    entry.ValidationResult?.Warnings,
                    entry.ValidationResult?.ProcessedAt
                },
                InjectionRisk = new
                {
                    entry.InjectionRisk?.Level,
                    entry.InjectionRisk?.RiskFactors
                }
                // Note: Actual prompt content excluded for security
            };
            
            var json = JsonSerializer.Serialize(auditRecord, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await File.AppendAllTextAsync(filePath, json + Environment.NewLine);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private async Task WriteAuditEntryToFileAsync(LlmAuditEntry entry)
    {
        await _fileLock.WaitAsync();
        try
        {
            var fileName = $"llm-audit-{DateTime.UtcNow:yyyy-MM-dd}.json";
            var filePath = Path.Combine(_auditLogPath, fileName);
            
            var auditRecord = new
            {
                EntryType = "LlmInteraction",
                entry.Id,
                entry.Timestamp,
                entry.ModelId,
                entry.PromptId,
                entry.ResponseLength,
                entry.TokensUsed,
                entry.GenerationDuration,
                entry.PostValidationPassed,
                entry.ValidationErrors,
                SecurityContext = new
                {
                    entry.SecurityContext?.UserId,
                    entry.SecurityContext?.SessionId,
                    entry.SecurityContext?.IPAddress
                }
                // Note: Response content excluded for security by default
            };
            
            var json = JsonSerializer.Serialize(auditRecord, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await File.AppendAllTextAsync(filePath, json + Environment.NewLine);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private async Task WriteAuditEntryToFileAsync(SecurityViolationEntry entry)
    {
        await _fileLock.WaitAsync();
        try
        {
            var fileName = $"security-audit-{DateTime.UtcNow:yyyy-MM-dd}.json";
            var filePath = Path.Combine(_auditLogPath, fileName);
            
            var auditRecord = new
            {
                EntryType = "SecurityViolation",
                entry.Id,
                entry.Timestamp,
                entry.ViolationType,
                entry.Description,
                entry.RiskLevel,
                entry.ModelId,
                entry.AttackVector,
                entry.DetectionRules,
                entry.ActionTaken,
                entry.RemediationAction,
                entry.AdditionalData,
                SecurityContext = new
                {
                    entry.SecurityContext?.UserId,
                    entry.SecurityContext?.SessionId,
                    entry.SecurityContext?.IPAddress,
                    entry.SecurityContext?.UserAgent
                }
            };
            
            var json = JsonSerializer.Serialize(auditRecord, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await File.AppendAllTextAsync(filePath, json + Environment.NewLine);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private async Task WriteAuditEntryToFileAsync(CodeGenerationAuditEntry entry)
    {
        await _fileLock.WaitAsync();
        try
        {
            var fileName = $"codegen-audit-{DateTime.UtcNow:yyyy-MM-dd}.json";
            var filePath = Path.Combine(_auditLogPath, fileName);
            
            var auditRecord = new
            {
                EntryType = "CodeGeneration",
                entry.Id,
                entry.Timestamp,
                entry.ModelId,
                entry.PromptId,
                entry.LlmResponseId,
                entry.GeneratedCodeType,
                entry.GeneratedLines,
                entry.ValidationPassed,
                entry.ValidationErrors,
                entry.SecurityScanPassed,
                entry.SecurityIssues,
                entry.GenerationDuration,
                entry.TargetFilePath,
                entry.Metadata,
                SecurityContext = new
                {
                    entry.SecurityContext?.UserId,
                    entry.SecurityContext?.SessionId,
                    entry.SecurityContext?.IPAddress
                }
            };
            
            var json = JsonSerializer.Serialize(auditRecord, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await File.AppendAllTextAsync(filePath, json + Environment.NewLine);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private async Task<List<PromptAuditEntry>> ReadAuditEntriesFromFileAsync(string filePath)
    {
        var entries = new List<PromptAuditEntry>();
        
        try
        {
            var lines = await File.ReadAllLinesAsync(filePath);
            foreach (var line in lines.Where(l => !string.IsNullOrWhiteSpace(l)))
            {
                try
                {
                    var jsonDoc = JsonDocument.Parse(line);
                    if (jsonDoc.RootElement.TryGetProperty("entryType", out var entryType) && 
                        entryType.GetString() == "PromptValidation")
                    {
                        // Parse the audit entry - simplified parsing for this implementation
                        var entry = ParsePromptAuditEntry(jsonDoc.RootElement);
                        if (entry is not null)
                            entries.Add(entry);
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse audit entry from line in file {FilePath}", filePath);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read audit entries from file {FilePath}", filePath);
        }

        return entries;
    }

    private PromptAuditEntry? ParsePromptAuditEntry(JsonElement element)
    {
        try
        {
            return new PromptAuditEntry
            {
                Id = element.GetProperty("id").GetGuid(),
                Timestamp = element.GetProperty("timestamp").GetDateTime(),
                ModelId = element.GetProperty("modelId").GetString() ?? string.Empty,
                ProcessingDuration = TimeSpan.FromMilliseconds(
                    element.GetProperty("processingDuration").GetDouble()),
                // Note: Original prompts are not stored in audit logs for security
                OriginalPrompt = "[REDACTED_FOR_SECURITY]",
                SanitizedPrompt = "[REDACTED_FOR_SECURITY]"
            };
        }
        catch
        {
            return null;
        }
    }

    private void EnsureAuditDirectoryExists()
    {
        if (_enableFileLogging && !Directory.Exists(_auditLogPath))
        {
            Directory.CreateDirectory(_auditLogPath);
            _logger.LogInformation("Created audit log directory: {AuditLogPath}", _auditLogPath);
        }
    }

    private static bool IsFileInDateRange(string filePath, DateTime fromDate, DateTime toDate)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var datePart = fileName.Split('-').LastOrDefault();
        
        return DateTime.TryParseExact(datePart, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var fileDate) && 
               fileDate >= fromDate.Date && fileDate <= toDate.Date;
    }

    private static string ComputePromptHash(string? content)
    {
        if (string.IsNullOrEmpty(content))
            return string.Empty;

        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(hash);
    }
}
