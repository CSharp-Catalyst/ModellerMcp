namespace JJs.PotentialSales.Sdk.Prospects;

/// <summary>
/// Response model for prospect data
/// </summary>
public record ProspectResponse
{
    /// <summary>
    /// The unique identifier for the prospect
    /// </summary>
    public required string ProspectId { get; init; }

    /// <summary>
    /// A unique identifier assigned to each potential sale, ensuring traceability and differentiation within the system
    /// </summary>
    public required string PotentialSaleNumber { get; init; }

    /// <summary>
    /// The site associated with the potential sale
    /// </summary>
    public required string SiteNumber { get; init; }

    /// <summary>
    /// The individual responsible for managing the potential sale, ensuring accountability and clear ownership within the sales team
    /// </summary>
    public required string Assignee { get; init; }

    /// <summary>
    /// The classification of the potential sale, which could include categories like 'New Business', 'Renewal', or 'Upsell'
    /// </summary>
    public required string ProspectTypeId { get; init; }

    /// <summary>
    /// The origin or channel through which the potential sale was initiated, such as 'Referral', 'Website', or 'Campaign'
    /// </summary>
    public required string SourceId { get; init; }

    /// <summary>
    /// The status of the customer
    /// </summary>
    public required CustomerStatus CustomerStatus { get; init; }

    /// <summary>
    /// A unique identifier assigned to each customer, used to accurately reference and track customer-related information
    /// </summary>
    public string? CustomerNumber { get; init; }

    /// <summary>
    /// The trading name of the customer
    /// </summary>
    public required string TradingName { get; init; }

    /// <summary>
    /// The current state of the potential sale, indicating progress or actions needed
    /// </summary>
    public required ProspectStatus ProspectStatus { get; init; }

    /// <summary>
    /// The interest of the customer in the potential sale, indicating the likelihood of conversion
    /// </summary>
    public required Interest Interest { get; init; }

    /// <summary>
    /// The scheduled date for the sales team to follow up on the potential sale
    /// </summary>
    public DateTime? SalesFollowUpDate { get; init; }

    /// <summary>
    /// A detailed note or plan regarding the follow-up actions to be taken on the specified date
    /// </summary>
    public string? SalesFollowUpDescription { get; init; }

    /// <summary>
    /// A flag indicating whether a quote has been provided to the customer
    /// </summary>
    public bool? QuoteProvided { get; init; }

    /// <summary>
    /// The specific date when a quote was delivered to the customer
    /// </summary>
    public DateTime? QuoteProvidedDate { get; init; }

    /// <summary>
    /// A summary or explanation of the quote provided, including key details or terms
    /// </summary>
    public string? QuoteProvidedDescription { get; init; }

    /// <summary>
    /// The address associated with the customer, this maybe a new address or an existing customer address
    /// </summary>
    public required string AddressLine { get; init; }

    /// <summary>
    /// The first name of the contact person managing the potential sale
    /// </summary>
    public string? ContactFirstName { get; init; }

    /// <summary>
    /// The last name of the contact person managing the potential sale
    /// </summary>
    public string? ContactLastName { get; init; }

    /// <summary>
    /// The phone number of the contact person managing the potential sale
    /// </summary>
    public string? ContactPhone { get; init; }

    /// <summary>
    /// The email address of the contact person managing the potential sale
    /// </summary>
    public string? ContactEmail { get; init; }

    /// <summary>
    /// A detailed description of the potential sale, including key information, requirements, and objectives
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// When the prospect was created
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// When the prospect was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; init; }
}
