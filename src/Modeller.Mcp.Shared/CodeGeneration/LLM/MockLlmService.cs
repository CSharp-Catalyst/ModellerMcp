using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Modeller.Mcp.Shared.CodeGeneration.LLM;

/// <summary>
/// Mock implementation of LLM service for testing and development
/// This can be replaced with actual LLM provider implementations (OpenAI, Azure OpenAI, etc.)
/// </summary>
public class MockLlmService : ILlmService
{
    private readonly ILogger<MockLlmService> _logger;
    private readonly IConfiguration _configuration;
    private readonly bool _simulateLatency;
    private readonly int _minLatencyMs;
    private readonly int _maxLatencyMs;

    public MockLlmService(ILogger<MockLlmService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _simulateLatency = _configuration.GetValue("LLM:Mock:SimulateLatency", true);
        _minLatencyMs = _configuration.GetValue("LLM:Mock:MinLatencyMs", 1000);
        _maxLatencyMs = _configuration.GetValue("LLM:Mock:MaxLatencyMs", 3000);
    }

    public async Task<LlmResponse> GenerateCodeAsync(LlmRequest request, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogInformation("Generating code for model {ModelId} with prompt length {PromptLength}",
                request.ModelId, request.Prompt.Length);

            // Simulate processing latency
            if (_simulateLatency)
            {
                var latency = Random.Shared.Next(_minLatencyMs, _maxLatencyMs);
                await Task.Delay(latency, cancellationToken);
            }

            // Generate mock code based on prompt analysis
            var generatedCode = GenerateMockCode(request.Prompt, request.ModelId);
            var generationTime = DateTime.UtcNow - startTime;

            var response = new LlmResponse
            {
                Content = generatedCode,
                ModelId = request.ModelId,
                Usage = new LlmUsageInfo
                {
                    PromptTokens = EstimateTokenCount(request.Prompt),
                    CompletionTokens = EstimateTokenCount(generatedCode),
                    TotalTokens = EstimateTokenCount(request.Prompt) + EstimateTokenCount(generatedCode),
                    EstimatedCost = 0.002m, // Mock cost
                    CostCurrency = "USD"
                },
                GenerationTime = generationTime,
                IsSuccess = true,
                Metadata = new Dictionary<string, object>
                {
                    ["provider"] = "Mock",
                    ["version"] = "1.0",
                    ["temperature"] = request.Parameters.Temperature,
                    ["maxTokens"] = request.Parameters.MaxTokens
                }
            };

            _logger.LogInformation("Code generation completed for model {ModelId} in {Duration}ms",
                request.ModelId, generationTime.TotalMilliseconds);

            return response;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Code generation cancelled for model {ModelId}", request.ModelId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Code generation failed for model {ModelId}", request.ModelId);

            return new LlmResponse
            {
                Content = string.Empty,
                ModelId = request.ModelId,
                IsSuccess = false,
                ErrorMessage = ex.Message,
                GenerationTime = DateTime.UtcNow - startTime
            };
        }
    }

    public async Task<bool> ValidateServiceAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validating Mock LLM service availability");

        // Simulate validation check
        await Task.Delay(100, cancellationToken);

        return true; // Mock service is always available
    }

    public async Task<IEnumerable<LlmModelInfo>> GetAvailableModelsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving available Mock LLM models");

        // Simulate API call
        await Task.Delay(50, cancellationToken);

        return
        [
            new LlmModelInfo
            {
                Id = "mock-csharp-code-gen",
                Name = "Mock C# Code Generator",
                Description = "Mock model for generating C# code from natural language descriptions",
                Provider = "Mock",
                MaxTokens = 8192,
                CostPer1kTokens = 0.002m,
                Capabilities = ["code-generation", "csharp", "dotnet"],
                IsAvailable = true,
                LastUpdated = DateTime.UtcNow
            },
            new LlmModelInfo
            {
                Id = "mock-general-purpose",
                Name = "Mock General Purpose Model",
                Description = "Mock general-purpose model for various code generation tasks",
                Provider = "Mock",
                MaxTokens = 4096,
                CostPer1kTokens = 0.001m,
                Capabilities = ["code-generation", "text-generation", "analysis"],
                IsAvailable = true,
                LastUpdated = DateTime.UtcNow
            }
        ];
    }

    public async Task<LlmUsageEstimate> EstimateUsageAsync(string prompt, string modelId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Estimating usage for model {ModelId} with prompt length {PromptLength}",
            modelId, prompt.Length);

        // Simulate estimation calculation
        await Task.Delay(10, cancellationToken);

        var promptTokens = EstimateTokenCount(prompt);
        var estimatedCompletionTokens = Math.Min(promptTokens * 2, 2000); // Estimate 2x prompt length, max 2000
        var totalTokens = promptTokens + estimatedCompletionTokens;

        return new LlmUsageEstimate
        {
            EstimatedPromptTokens = promptTokens,
            EstimatedCompletionTokens = estimatedCompletionTokens,
            EstimatedTotalTokens = totalTokens,
            EstimatedCost = totalTokens * 0.002m / 1000m,
            CostCurrency = "USD",
            WithinQuotaLimits = totalTokens < 10000 // Mock quota limit
        };
    }

    private string GenerateMockCode(string prompt, string modelId)
    {
        // Analyze prompt to determine what kind of code to generate
        var promptLower = prompt.ToLowerInvariant();

        if (promptLower.Contains("class") || promptLower.Contains("type"))
        {
            return GenerateMockClassCode(prompt);
        }
        else if (promptLower.Contains("method") || promptLower.Contains("function"))
        {
            return GenerateMockMethodCode(prompt);
        }
        else if (promptLower.Contains("property") || promptLower.Contains("attribute"))
        {
            return GenerateMockPropertyCode(prompt);
        }
        else
        {
            return promptLower.Contains("interface") ? GenerateMockInterfaceCode(prompt) : GenerateGenericMockCode(prompt);
        }
    }

    private string GenerateMockClassCode(string prompt) => """
        // Generated by Mock LLM Service
        // Based on prompt analysis for class generation

        using System;
        using System.Collections.Generic;
        using System.ComponentModel.DataAnnotations;

        namespace Generated.Models
        {
            /// <summary>
            /// Auto-generated class based on the provided requirements
            /// </summary>
            public class GeneratedEntity
            {
                public Guid Id { get; set; } = Guid.NewGuid();
                
                [Required]
                public string Name { get; set; } = string.Empty;
                
                public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                
                public DateTime? UpdatedAt { get; set; }
                
                // Additional properties would be generated based on specific prompt analysis
                public Dictionary<string, object> Properties { get; set; } = new();
                
                public void UpdateTimestamp()
                {
                    UpdatedAt = DateTime.UtcNow;
                }
            }
        }
        """;

    private string GenerateMockMethodCode(string prompt) => """
        // Generated by Mock LLM Service
        // Based on prompt analysis for method generation

        /// <summary>
        /// Auto-generated method based on the provided requirements
        /// </summary>
        /// <param name="input">Input parameter</param>
        /// <returns>Processed result</returns>
        public async Task<string> ProcessDataAsync(string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException("Input cannot be null or empty", nameof(input));
            
            try
            {
                // Mock processing logic
                await Task.Delay(100); // Simulate async processing
                
                var result = input.ToUpperInvariant();
                return $"Processed: {result}";
            }
            catch (Exception ex)
            {
                // Log error and handle appropriately
                throw new InvalidOperationException("Processing failed", ex);
            }
        }
        """;

    private string GenerateMockPropertyCode(string prompt) => """
        // Generated by Mock LLM Service
        // Based on prompt analysis for property generation

        /// <summary>
        /// Auto-generated property based on the provided requirements
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string GeneratedProperty { get; set; } = string.Empty;

        /// <summary>
        /// Backing field for computed property
        /// </summary>
        private string? _computedValue;

        /// <summary>
        /// Computed property with lazy evaluation
        /// </summary>
        public string ComputedProperty => _computedValue ??= ComputeValue();

        private string ComputeValue()
        {
            // Mock computation logic
            return $"Computed_{DateTime.UtcNow:yyyyMMddHHmmss}";
        }
        """;

    private string GenerateMockInterfaceCode(string prompt) => """
        // Generated by Mock LLM Service
        // Based on prompt analysis for interface generation

        /// <summary>
        /// Auto-generated interface based on the provided requirements
        /// </summary>
        public interface IGeneratedService
        {
            /// <summary>
            /// Processes data asynchronously
            /// </summary>
            Task<string> ProcessAsync(string data, CancellationToken cancellationToken = default);
            
            /// <summary>
            /// Validates input data
            /// </summary>
            bool ValidateData(string data);
            
            /// <summary>
            /// Gets service configuration
            /// </summary>
            Dictionary<string, object> GetConfiguration();
            
            /// <summary>
            /// Event fired when processing completes
            /// </summary>
            event EventHandler<ProcessingCompleteEventArgs> ProcessingComplete;
        }

        /// <summary>
        /// Event arguments for processing completion
        /// </summary>
        public class ProcessingCompleteEventArgs : EventArgs
        {
            public string Result { get; }
            public TimeSpan Duration { get; }
            
            public ProcessingCompleteEventArgs(string result, TimeSpan duration)
            {
                Result = result;
                Duration = duration;
            }
        }
        """;

    private string GenerateGenericMockCode(string prompt) => """
        // Generated by Mock LLM Service
        // Generic code generation based on prompt analysis

        using System;
        using System.Threading.Tasks;
        using Microsoft.Extensions.Logging;

        /// <summary>
        /// Auto-generated code based on the provided requirements
        /// Note: This is mock-generated code for development and testing purposes
        /// </summary>
        public class GeneratedSolution
        {
            private readonly ILogger<GeneratedSolution> _logger;
            
            public GeneratedSolution(ILogger<GeneratedSolution> logger)
            {
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }
            
            /// <summary>
            /// Main processing method
            /// </summary>
            public async Task<bool> ExecuteAsync()
            {
                _logger.LogInformation("Executing generated solution");
                
                try
                {
                    // Mock implementation logic
                    await Task.Delay(100);
                    
                    _logger.LogInformation("Solution executed successfully");
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Solution execution failed");
                    return false;
                }
            }
        }
        """;

    private static int EstimateTokenCount(string text) =>
        // Simple token estimation: roughly 4 characters per token for English text
        // This is a rough approximation - real implementations would use proper tokenizers
        Math.Max(1, text.Length / 4);
}
