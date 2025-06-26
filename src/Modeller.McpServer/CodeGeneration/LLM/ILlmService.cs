using Modeller.McpServer.CodeGeneration.Security;

namespace Modeller.McpServer.CodeGeneration.LLM;

/// <summary>
/// Abstraction for LLM service providers used in code generation
/// </summary>
public interface ILlmService
{
    /// <summary>
    /// Generates code based on the provided prompt and model configuration
    /// </summary>
    Task<LlmResponse> GenerateCodeAsync(LlmRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that the LLM service is available and properly configured
    /// </summary>
    Task<bool> ValidateServiceAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets information about available models and their capabilities
    /// </summary>
    Task<IEnumerable<LlmModelInfo>> GetAvailableModelsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Estimates the cost and token usage for a given request
    /// </summary>
    Task<LlmUsageEstimate> EstimateUsageAsync(string prompt, string modelId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Request for LLM code generation
/// </summary>
public record LlmRequest
{
    public required string Prompt { get; init; }
    public required string ModelId { get; init; }
    public LlmParameters Parameters { get; init; } = new();
    public SecurityContext? SecurityContext { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Response from LLM code generation
/// </summary>
public record LlmResponse
{
    public required string Content { get; init; }
    public required string ModelId { get; init; }
    public LlmUsageInfo Usage { get; init; } = new();
    public TimeSpan GenerationTime { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Parameters for LLM generation
/// </summary>
public record LlmParameters
{
    public double Temperature { get; init; } = 0.7;
    public int MaxTokens { get; init; } = 4000;
    public double TopP { get; init; } = 0.9;
    public double FrequencyPenalty { get; init; } = 0.0;
    public double PresencePenalty { get; init; } = 0.0;
    public string[]? Stop { get; init; }
    public int? Seed { get; init; } // For reproducible generation
}

/// <summary>
/// Information about token usage and costs
/// </summary>
public record LlmUsageInfo
{
    public int PromptTokens { get; init; }
    public int CompletionTokens { get; init; }
    public int TotalTokens { get; init; }
    public decimal? EstimatedCost { get; init; }
    public string CostCurrency { get; init; } = "USD"; // Reasonable default for currency
}

/// <summary>
/// Estimated usage for a request before execution
/// </summary>
public record LlmUsageEstimate
{
    public int EstimatedPromptTokens { get; init; }
    public int EstimatedCompletionTokens { get; init; }
    public int EstimatedTotalTokens { get; init; }
    public decimal EstimatedCost { get; init; }
    public required string CostCurrency { get; init; }
    public bool WithinQuotaLimits { get; init; } = true;
}

/// <summary>
/// Information about an available LLM model
/// </summary>
public record LlmModelInfo
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string Description { get; init; } = string.Empty; // Optional description can default to empty
    public required string Provider { get; init; }
    public int MaxTokens { get; init; }
    public decimal CostPer1kTokens { get; init; }
    public string[] Capabilities { get; init; } = Array.Empty<string>();
    public bool IsAvailable { get; init; } = true;
    public DateTime? LastUpdated { get; init; }
}

/// <summary>
/// Exception thrown when LLM service operations fail
/// </summary>
public class LlmServiceException : Exception
{
    public string? ModelId { get; }
    public LlmErrorType ErrorType { get; }

    public LlmServiceException(string message, LlmErrorType errorType = LlmErrorType.Unknown, string? modelId = null) 
        : base(message)
    {
        ErrorType = errorType;
        ModelId = modelId;
    }

    public LlmServiceException(string message, Exception innerException, LlmErrorType errorType = LlmErrorType.Unknown, string? modelId = null) 
        : base(message, innerException)
    {
        ErrorType = errorType;
        ModelId = modelId;
    }
}

/// <summary>
/// Types of LLM service errors
/// </summary>
public enum LlmErrorType
{
    Unknown,
    Authentication,
    QuotaExceeded,
    ModelUnavailable,
    InvalidRequest,
    ContentFiltered,
    Timeout,
    NetworkError,
    InternalError
}
