using JJs.PotentialSales.Sdk.Common;

namespace JJs.PotentialSales.Sdk.Prospects;

/// <summary>
/// Result pattern specific to Prospect operations
/// </summary>
/// <typeparam name="T">The type of data returned</typeparam>
public record ProspectResult<T> : ApiResult<T>
{
    /// <summary>
    /// Creates a successful prospect result
    /// </summary>
    public static new ProspectResult<T> Success(T data) => new()
    {
        IsSuccess = true,
        Data = data
    };

    /// <summary>
    /// Creates a failed prospect result with error message
    /// </summary>
    public static new ProspectResult<T> Failure(string errorMessage) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage
    };

    /// <summary>
    /// Creates a failed prospect result with validation errors
    /// </summary>
    public static new ProspectResult<T> ValidationFailure(Dictionary<string, string[]> validationErrors) => new()
    {
        IsSuccess = false,
        ValidationErrors = validationErrors
    };

    /// <summary>
    /// Creates a result indicating the prospect was not found
    /// </summary>
    public static ProspectResult<T> NotFound(string potentialSaleNumber) => new()
    {
        IsSuccess = false,
        ErrorMessage = $"Prospect with potential sale number '{potentialSaleNumber}' was not found"
    };

    /// <summary>
    /// Creates a result indicating the prospect already exists
    /// </summary>
    public static ProspectResult<T> AlreadyExists(string potentialSaleNumber) => new()
    {
        IsSuccess = false,
        ErrorMessage = $"Prospect with potential sale number '{potentialSaleNumber}' already exists"
    };
}
