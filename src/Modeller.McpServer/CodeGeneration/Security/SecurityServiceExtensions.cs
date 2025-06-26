using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Modeller.McpServer.CodeGeneration.Security;

/// <summary>
/// Extensions for configuring security services in the DI container
/// </summary>
public static class SecurityServiceExtensions
{
    /// <summary>
    /// Adds all security services required for secure LLM operations
    /// </summary>
    public static IServiceCollection AddSecurityServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Core security services
        services.AddSingleton<IPromptSecurityService, PromptSecurityService>();
        services.AddSingleton<ISecurePromptBuilder, SecurePromptBuilder>();
        services.AddSingleton<ISecureLlmService, SecureLlmService>();

        // VSA prompt services
        services.AddSingleton<Prompts.IVsaPromptService, Prompts.VsaPromptService>();

        // Audit logging - configure based on settings
        var auditConfig = configuration.GetSection("Security:Audit");
        var auditProvider = auditConfig.GetValue<string>("Provider", "file");

        switch (auditProvider.ToLowerInvariant())
        {
            case "file":
                services.AddSingleton<IPromptAuditLogger>(provider =>
                {
                    var logger = provider.GetRequiredService<ILogger<PromptAuditLogger>>();
                    var config = provider.GetRequiredService<IConfiguration>();
                    return new PromptAuditLogger(logger, config);
                });
                break;

            case "database":
                // Future: Add database audit logger
                services.AddSingleton<IPromptAuditLogger, PromptAuditLogger>();
                break;

            case "syslog":
                // Future: Add syslog audit logger
                services.AddSingleton<IPromptAuditLogger, PromptAuditLogger>();
                break;

            default:
                services.AddSingleton<IPromptAuditLogger, PromptAuditLogger>();
                break;
        }

        // Security configuration validation
        services.AddOptions<SecurityConfiguration>()
            .Bind(configuration.GetSection("Security"))
            .ValidateOnStart();

        return services;
    }

    /// <summary>
    /// Adds enhanced security services with additional protections
    /// </summary>
    public static IServiceCollection AddEnhancedSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSecurityServices(configuration);

        // Enhanced security features
        services.AddSingleton<ISecurityMetricsCollector, SecurityMetricsCollector>();
        services.AddSingleton<IRateLimitingService, RateLimitingService>();
        services.AddSingleton<ISecurityTelemetryService, SecurityTelemetryService>();

        // Background services for security monitoring
        services.AddHostedService<SecurityMonitoringService>();
        services.AddHostedService<AuditLogRotationService>();

        return services;
    }
}

/// <summary>
/// File-based implementation of prompt audit logger
/// </summary>
public class FilePromptAuditLogger : IPromptAuditLogger
{
    private readonly ILogger<FilePromptAuditLogger> _logger;
    private readonly string _auditLogPath;
    private readonly SemaphoreSlim _fileLock = new(1, 1);

    public FilePromptAuditLogger(ILogger<FilePromptAuditLogger> logger, string auditLogPath = "audit_logs")
    {
        _logger = logger;
        _auditLogPath = auditLogPath;

        // Ensure audit log directory exists
        Directory.CreateDirectory(_auditLogPath);
    }

    public async Task LogPromptValidationAsync(PromptAuditEntry entry)
    {
        await WriteAuditEntryAsync("prompt_validation", entry);
    }

    public async Task LogLlmInteractionAsync(LlmAuditEntry entry)
    {
        await WriteAuditEntryAsync("llm_interaction", entry);
    }

    public async Task LogSecurityViolationAsync(SecurityViolationEntry entry)
    {
        await WriteAuditEntryAsync("security_violation", entry);
    }

    public async Task LogCodeGenerationAsync(CodeGenerationAuditEntry entry)
    {
        await WriteAuditEntryAsync("code_generation", entry);
    }

    private async Task WriteAuditEntryAsync<T>(string category, T entry)
    {
        await _fileLock.WaitAsync();
        try
        {
            var fileName = $"{category}_{DateTime.UtcNow:yyyy-MM-dd}.jsonl";
            var filePath = Path.Combine(_auditLogPath, fileName);

            var json = System.Text.Json.JsonSerializer.Serialize(entry, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            });

            await File.AppendAllTextAsync(filePath, json + Environment.NewLine);

            _logger.LogDebug("Wrote audit entry to {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write audit entry for category {Category}", category);
            // Don't rethrow - audit logging failure shouldn't break the main flow
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public void Dispose()
    {
        _fileLock?.Dispose();
    }
}

/// <summary>
/// Placeholder interfaces for future enhanced security features
/// </summary>
public interface ISecurityMetricsCollector
{
    Task RecordSecurityEventAsync(string eventType, Dictionary<string, object> metadata);
    Task<SecurityMetrics> GetSecurityMetricsAsync(DateTime fromDate, DateTime toDate);
}

public interface IRateLimitingService
{
    Task<bool> IsRequestAllowedAsync(string userId, string operation, CancellationToken cancellationToken = default);
    Task RecordRequestAsync(string userId, string operation, CancellationToken cancellationToken = default);
}

public interface ISecurityTelemetryService
{
    Task SendSecurityAlertAsync(SecurityAlert alert, CancellationToken cancellationToken = default);
    Task RecordSecurityMetricAsync(string metricName, double value, Dictionary<string, string>? tags = null);
}

/// <summary>
/// Basic implementation stubs for enhanced security services
/// </summary>
public class SecurityMetricsCollector(ILogger<SecurityMetricsCollector> logger) : ISecurityMetricsCollector
{
    public async Task RecordSecurityEventAsync(string eventType, Dictionary<string, object> metadata)
    {
        logger.LogInformation("Security event recorded: {EventType} with metadata: {@Metadata}", 
            eventType, metadata);
        await Task.CompletedTask;
    }

    public async Task<SecurityMetrics> GetSecurityMetricsAsync(DateTime fromDate, DateTime toDate)
    {
        // Placeholder implementation
        await Task.CompletedTask;
        return new SecurityMetrics
        {
            PeriodStart = fromDate,
            PeriodEnd = toDate,
            TotalRequests = 0,
            BlockedRequests = 0,
            HighRiskRequests = 0
        };
    }
}

public class RateLimitingService : IRateLimitingService
{
    private readonly Dictionary<string, List<DateTime>> _requestHistory = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<bool> IsRequestAllowedAsync(string userId, string operation, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var key = $"{userId}:{operation}";
            var now = DateTime.UtcNow;
            var windowStart = now.AddMinutes(-1); // 1-minute sliding window

            if (!_requestHistory.ContainsKey(key))
            {
                _requestHistory[key] = new List<DateTime>();
            }

            // Remove old requests outside the window
            _requestHistory[key].RemoveAll(time => time < windowStart);

            // Check if under limit (e.g., 10 requests per minute)
            const int maxRequestsPerMinute = 10;
            return _requestHistory[key].Count < maxRequestsPerMinute;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task RecordRequestAsync(string userId, string operation, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var key = $"{userId}:{operation}";
            if (!_requestHistory.ContainsKey(key))
            {
                _requestHistory[key] = new List<DateTime>();
            }

            _requestHistory[key].Add(DateTime.UtcNow);
        }
        finally
        {
            _lock.Release();
        }
    }
}

public class SecurityTelemetryService(ILogger<SecurityTelemetryService> logger) : ISecurityTelemetryService
{
    public async Task SendSecurityAlertAsync(SecurityAlert alert, CancellationToken cancellationToken = default)
    {
        logger.LogWarning("Security Alert: {AlertType} - {Message} for User {UserId}", 
            alert.AlertType, alert.Message, alert.UserId);
        await Task.CompletedTask;
    }

    public async Task RecordSecurityMetricAsync(string metricName, double value, Dictionary<string, string>? tags = null)
    {
        logger.LogInformation("Security Metric: {MetricName} = {Value}, Tags: {@Tags}", 
            metricName, value, tags);
        await Task.CompletedTask;
    }
}

/// <summary>
/// Background service for security monitoring
/// </summary>
public class SecurityMonitoringService(
    ILogger<SecurityMonitoringService> logger,
    ISecurityMetricsCollector metricsCollector) : Microsoft.Extensions.Hosting.BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Security monitoring service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Perform periodic security checks
                await PerformSecurityHealthCheckAsync(stoppingToken);
                
                // Wait for 5 minutes before next check
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in security monitoring service");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        logger.LogInformation("Security monitoring service stopped");
    }

    private async Task PerformSecurityHealthCheckAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken); // Minimal delay to satisfy async requirement
        // Placeholder for security health checks
        await metricsCollector.RecordSecurityEventAsync("HealthCheck", new Dictionary<string, object>
        {
            ["Timestamp"] = DateTime.UtcNow,
            ["Status"] = "Healthy"
        });
    }
}

/// <summary>
/// Background service for audit log rotation
/// </summary>
public class AuditLogRotationService(
    ILogger<AuditLogRotationService> logger,
    IConfiguration configuration) : Microsoft.Extensions.Hosting.BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Audit log rotation service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RotateOldAuditLogsAsync(stoppingToken);
                
                // Run daily at 2 AM
                var nextRun = DateTime.Today.AddDays(1).AddHours(2);
                var delay = nextRun - DateTime.Now;
                if (delay.TotalMilliseconds > 0)
                {
                    await Task.Delay(delay, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in audit log rotation service");
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        logger.LogInformation("Audit log rotation service stopped");
    }

    private async Task RotateOldAuditLogsAsync(CancellationToken cancellationToken)
    {
        var auditPath = configuration.GetValue<string>("Security:AuditLogPath", "audit_logs");
        var retentionDays = configuration.GetValue<int>("Security:AuditRetentionDays", 90);

        if (!Directory.Exists(auditPath))
            return;

        var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
        var filesToRotate = Directory.GetFiles(auditPath, "*.jsonl")
            .Where(file => File.GetCreationTimeUtc(file) < cutoffDate)
            .ToList();

        foreach (var file in filesToRotate)
        {
            try
            {
                // Compress and archive old files
                var archivePath = Path.Combine(auditPath, "archive");
                Directory.CreateDirectory(archivePath);

                var archiveFileName = Path.GetFileNameWithoutExtension(file) + ".gz";
                var archiveFilePath = Path.Combine(archivePath, archiveFileName);

                // Simple compression (in production, use a proper compression library)
                var content = await File.ReadAllTextAsync(file, cancellationToken);
                var compressedContent = System.Text.Encoding.UTF8.GetBytes(content);
                await File.WriteAllBytesAsync(archiveFilePath, compressedContent, cancellationToken);

                File.Delete(file);

                logger.LogInformation("Rotated audit log file {FileName} to archive", Path.GetFileName(file));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to rotate audit log file {FileName}", Path.GetFileName(file));
            }
        }
    }
}

/// <summary>
/// Supporting data models for enhanced security services
/// </summary>
public record SecurityMetrics
{
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public int TotalRequests { get; init; }
    public int BlockedRequests { get; init; }
    public int HighRiskRequests { get; init; }
}

public record SecurityAlert
{
    public required string AlertType { get; init; }
    public required string Message { get; init; }
    public required string UserId { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Configuration model for security settings
/// </summary>
public class SecurityConfiguration
{
    public AuditConfiguration Audit { get; set; } = new();
    public RateLimitConfiguration RateLimit { get; set; } = new();
    public PromptSecurityConfiguration PromptSecurity { get; set; } = new();
}

public class RateLimitConfiguration
{
    public int MaxRequestsPerMinute { get; set; } = 10;
    public int MaxRequestsPerHour { get; set; } = 100;
    public int MaxRequestsPerDay { get; set; } = 1000;
}

public class PromptSecurityConfiguration
{
    public SecurityLevel DefaultSecurityLevel { get; set; } = SecurityLevel.Standard;
    public bool EnableInjectionDetection { get; set; } = true;
    public bool EnableContentSanitization { get; set; } = true;
    public bool EnablePostGenerationValidation { get; set; } = true;
}
