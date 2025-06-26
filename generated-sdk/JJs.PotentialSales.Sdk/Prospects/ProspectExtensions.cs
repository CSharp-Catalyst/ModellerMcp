using JJs.PotentialSales.Sdk.Common;

namespace JJs.PotentialSales.Sdk.Prospects;

/// <summary>
/// Extension methods for Prospect entity mapping and operations
/// </summary>
public static class ProspectExtensions
{
    /// <summary>
    /// Converts a Prospect entity to a ProspectResponse
    /// </summary>
    public static ProspectResponse ToResponse(this Prospect entity) => new()
    {
        ProspectId = entity.ProspectId.ToString(),
        PotentialSaleNumber = entity.PotentialSaleNumber,
        SiteNumber = entity.SiteNumber,
        Assignee = entity.Assignee,
        ProspectTypeId = entity.ProspectTypeId,
        SourceId = entity.SourceId,
        CustomerStatus = entity.CustomerStatus,
        CustomerNumber = entity.CustomerNumber,
        TradingName = entity.TradingName,
        ProspectStatus = entity.ProspectStatus,
        Interest = entity.Interest,
        SalesFollowUpDate = entity.SalesFollowUpDate,
        SalesFollowUpDescription = entity.SalesFollowUpDescription,
        QuoteProvided = entity.QuoteProvided,
        QuoteProvidedDate = entity.QuoteProvidedDate,
        QuoteProvidedDescription = entity.QuoteProvidedDescription,
        AddressLine = entity.AddressLine,
        ContactFirstName = entity.ContactFirstName,
        ContactLastName = entity.ContactLastName,
        ContactPhone = entity.ContactPhone,
        ContactEmail = entity.ContactEmail,
        Description = entity.Description,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };

    /// <summary>
    /// Converts a CreateProspectRequest to a Prospect entity
    /// </summary>
    public static Prospect ToEntity(this CreateProspectRequest request) => new()
    {
        ProspectId = Guid.NewGuid(),
        PotentialSaleNumber = request.PotentialSaleNumber,
        SiteNumber = request.SiteNumber,
        Assignee = request.Assignee,
        ProspectTypeId = request.ProspectTypeId,
        SourceId = request.SourceId,
        CustomerStatus = request.CustomerStatus,
        CustomerNumber = request.CustomerNumber,
        TradingName = request.TradingName,
        ProspectStatus = request.ProspectStatus,
        Interest = request.Interest,
        SalesFollowUpDate = request.SalesFollowUpDate,
        SalesFollowUpDescription = request.SalesFollowUpDescription,
        QuoteProvided = request.QuoteProvided,
        QuoteProvidedDate = request.QuoteProvidedDate,
        QuoteProvidedDescription = request.QuoteProvidedDescription,
        AddressLine = request.AddressLine,
        ContactFirstName = request.ContactFirstName,
        ContactLastName = request.ContactLastName,
        ContactPhone = request.ContactPhone,
        ContactEmail = request.ContactEmail,
        Description = request.Description,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = null
    };

    /// <summary>
    /// Creates a successful ApiResult with ProspectResponse
    /// </summary>
    public static ApiResult<ProspectResponse> ToApiResult(this Prospect entity)
    {
        return ApiResult<ProspectResponse>.Success(entity.ToResponse());
    }

    /// <summary>
    /// Creates a successful ApiResult with a list of ProspectResponse
    /// </summary>
    public static ApiResult<IEnumerable<ProspectResponse>> ToApiResult(this IEnumerable<Prospect> entities)
    {
        return ApiResult<IEnumerable<ProspectResponse>>.Success(entities.Select(e => e.ToResponse()));
    }
}

/// <summary>
/// Entity representation for mapping (would typically be in a separate domain assembly)
/// </summary>
public record Prospect
{
    public Guid ProspectId { get; init; }
    public required string PotentialSaleNumber { get; init; }
    public required string SiteNumber { get; init; }
    public required string Assignee { get; init; }
    public required string ProspectTypeId { get; init; }
    public required string SourceId { get; init; }
    public required CustomerStatus CustomerStatus { get; init; }
    public string? CustomerNumber { get; init; }
    public required string TradingName { get; init; }
    public required ProspectStatus ProspectStatus { get; init; }
    public required Interest Interest { get; init; }
    public DateTime? SalesFollowUpDate { get; init; }
    public string? SalesFollowUpDescription { get; init; }
    public bool? QuoteProvided { get; init; }
    public DateTime? QuoteProvidedDate { get; init; }
    public string? QuoteProvidedDescription { get; init; }
    public required string AddressLine { get; init; }
    public string? ContactFirstName { get; init; }
    public string? ContactLastName { get; init; }
    public string? ContactPhone { get; init; }
    public string? ContactEmail { get; init; }
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
