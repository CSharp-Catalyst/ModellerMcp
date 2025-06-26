namespace JJs.PotentialSales.Sdk.Prospects;

/// <summary>
/// Represents the status of a prospect in the sales pipeline
/// </summary>
public enum ProspectStatus
{
    /// <summary>
    /// The prospect is open and active
    /// </summary>
    Open = 1,

    /// <summary>
    /// The prospect has been cancelled
    /// </summary>
    Cancelled = 2,

    /// <summary>
    /// The prospect was won/converted to a customer
    /// </summary>
    Won = 3,

    /// <summary>
    /// The prospect was lost to competition or declined
    /// </summary>
    Lost = 4
}

/// <summary>
/// Represents the customer's interest level in the potential sale
/// </summary>
public enum Interest
{
    /// <summary>
    /// Customer has no interest
    /// </summary>
    No = 1,

    /// <summary>
    /// Customer is interested
    /// </summary>
    Yes = 2,

    /// <summary>
    /// Customer might be interested
    /// </summary>
    Maybe = 3
}

/// <summary>
/// Represents the status of the customer
/// </summary>
public enum CustomerStatus
{
    /// <summary>
    /// Active customer
    /// </summary>
    Active = 1,

    /// <summary>
    /// Inactive customer
    /// </summary>
    Inactive = 2,

    /// <summary>
    /// Prospective customer
    /// </summary>
    Prospect = 3
}
