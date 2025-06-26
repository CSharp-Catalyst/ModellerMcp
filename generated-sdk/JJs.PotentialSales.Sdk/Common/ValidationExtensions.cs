using FluentValidation;

namespace JJs.PotentialSales.Sdk.Common;

/// <summary>
/// Extension methods for validation
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Converts FluentValidation results to ApiResult validation errors
    /// </summary>
    public static Dictionary<string, string[]> ToValidationErrors(this FluentValidation.Results.ValidationResult validationResult)
    {
        return validationResult.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );
    }

    /// <summary>
    /// Validates a request and returns ApiResult
    /// </summary>
    public static async Task<ApiResult<T>> ValidateAsync<T>(this IValidator<T> validator, T request)
    {
        var validationResult = await validator.ValidateAsync(request);
        
        if (!validationResult.IsValid)
        {
            return ApiResult<T>.ValidationFailure(validationResult.ToValidationErrors());
        }

        return ApiResult<T>.Success(request);
    }
}
