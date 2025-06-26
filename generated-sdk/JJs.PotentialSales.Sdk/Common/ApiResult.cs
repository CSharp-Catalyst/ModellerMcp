namespace JJs.PotentialSales.Sdk.Common;

/// <summary>
/// Base result pattern for API responses
/// </summary>
/// <typeparam name="T">The type of data returned on success</typeparam>
public record ApiResult<T>
{
    /// <summary>
    /// Indicates if the operation was successful
    /// </summary>
    public bool IsSuccess { get; init; }
    
    /// <summary>
    /// The data returned on success
    /// </summary>
    public T? Data { get; init; }
    
    /// <summary>
    /// Error message if the operation failed
    /// </summary>
    public string? ErrorMessage { get; init; }
    
    /// <summary>
    /// Validation errors if applicable
    /// </summary>
    public Dictionary<string, string[]>? ValidationErrors { get; init; }

    /// <summary>
    /// Creates a successful result with data
    /// </summary>
    public static ApiResult<T> Success(T data) => new()
    {
        IsSuccess = true,
        Data = data
    };

    /// <summary>
    /// Creates a failed result with error message
    /// </summary>
    public static ApiResult<T> Failure(string errorMessage) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage
    };

    /// <summary>
    /// Creates a failed result with validation errors
    /// </summary>
    public static ApiResult<T> ValidationFailure(Dictionary<string, string[]> validationErrors) => new()
    {
        IsSuccess = false,
        ValidationErrors = validationErrors
    };
}
