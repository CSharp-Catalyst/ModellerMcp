namespace JJs.PotentialSales.Sdk.Prospects;

/// <summary>
/// Request model for retrieving a prospect by its unique number
/// </summary>
public record GetProspectRequest
{
    /// <summary>
    /// The unique potential sale number to retrieve
    /// </summary>
    public required string PotentialSaleNumber { get; init; }
}
