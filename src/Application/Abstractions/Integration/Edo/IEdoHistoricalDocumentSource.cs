namespace Application.Abstractions.Integration.Edo;

public interface IEdoHistoricalDocumentSource
{
    EdoProviderCode ProviderCode { get; }

    ValueTask<EdoHistoricalSourceReadinessDto> CheckReadinessAsync(
        EdoHistoricalExecutionContextDto context,
        CancellationToken ct = default);

    Task<EdoHistoricalPageResultDto> ReadInboxPageAsync(
        EdoHistoricalExecutionContextDto context,
        EdoHistoricalPageRequestDto request,
        CancellationToken ct = default);

    Task<EdoHistoricalDetailResultDto> ReadDetailAsync(
        EdoHistoricalExecutionContextDto context,
        EdoHistoricalDetailRequestDto request,
        CancellationToken ct = default);
}

public sealed record EdoHistoricalExecutionContextDto(int OrganizationId, int? InitiatedByUserId = null);

public sealed class EdoHistoricalPageRequestDto
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 100;
    public int? PreviousProviderTotal { get; init; }
    public IReadOnlyCollection<string> PreviousPageProviderDocumentIds { get; init; } = [];
}

public sealed class EdoHistoricalDetailRequestDto
{
    public EdoHistoricalDocumentSummaryDto Item { get; init; } = new();
    public DateOnly DateFrom { get; init; }
    public DateOnly DateTo { get; init; }
}

public enum EdoHistoricalReadState
{
    COMPLETE,
    PARTIAL,
    WAITING_AUTH,
    TRANSIENT_FAILURE,
    TERMINAL_PROVIDER_FAILURE,
    VALIDATION_FAILURE
}

public sealed class EdoHistoricalSourceReadinessDto
{
    public EdoProviderCode ProviderCode { get; init; }
    public bool IsProviderRegistered { get; init; }
    public bool IsSessionReady { get; init; }
    public EdoHistoricalReadState State { get; init; }
    public string? SafeFailureCode { get; init; }
}

public sealed class EdoHistoricalPageResultDto
{
    public EdoProviderCode ProviderCode { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int? ProviderTotal { get; init; }
    public bool? HasNextPage { get; init; }
    public int? NextPage { get; init; }
    public bool IsCompletenessConfirmed { get; init; }
    public bool RequiresOverlapRescan { get; init; }
    public bool IsRepeatedPage { get; init; }
    public bool ProviderTotalChanged { get; init; }
    public int DuplicateItemCount { get; init; }
    public EdoHistoricalReadState State { get; init; }
    public string? SafeFailureCode { get; init; }
    public IReadOnlyCollection<EdoHistoricalDocumentSummaryDto> Items { get; init; } = [];
}

public sealed class EdoHistoricalDocumentSummaryDto
{
    public string ProviderDocumentId { get; init; } = string.Empty;
    public EdoDirection Direction { get; init; }
    public EdoDocumentStatusCode Status { get; init; }
    public string DocumentType { get; init; } = string.Empty;
    public string? DocumentNumber { get; init; }
    public DateOnly? DocumentDate { get; init; }
    public DateTime? DocumentDateTime { get; init; }
    public string? SellerTin { get; init; }
    public string? BuyerTin { get; init; }
    public string? SellerName { get; init; }
    public decimal? Total { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

public sealed class EdoHistoricalDetailResultDto
{
    public EdoProviderCode ProviderCode { get; init; }
    public EdoHistoricalReadState State { get; init; }
    public bool IsImportReady { get; init; }
    public string? SafeFailureCode { get; init; }
    public EdoHistoricalDocumentDetailDto? Document { get; init; }
}

public sealed class EdoHistoricalDocumentDetailDto
{
    public string ProviderDocumentId { get; init; } = string.Empty;
    public EdoDirection Direction { get; init; }
    public EdoDocumentStatusCode Status { get; init; }
    public string DocumentType { get; init; } = string.Empty;
    public string? DocumentNumber { get; init; }
    public DateOnly? DocumentDate { get; init; }
    public DateTime? DocumentDateTime { get; init; }
    public EdoHistoricalPartyDto? Seller { get; init; }
    public EdoHistoricalPartyDto? Buyer { get; init; }
    public string? ContractNumber { get; init; }
    public DateOnly? ContractDate { get; init; }
    public decimal? NetAmount { get; init; }
    public decimal? VatAmount { get; init; }
    public decimal? TotalAmount { get; init; }
    public IReadOnlyCollection<EdoHistoricalDocumentLineDto> Lines { get; init; } = [];
    public IReadOnlyCollection<string> MarkingNumbers { get; init; } = [];
}

public sealed class EdoHistoricalPartyDto
{
    public string Name { get; init; } = string.Empty;
    public string Tin { get; init; } = string.Empty;
}

public sealed class EdoHistoricalDocumentLineDto
{
    public int Number { get; init; }
    public string? CatalogCode { get; init; }
    public string? CatalogName { get; init; }
    public string? PackageCode { get; init; }
    public string? PackageName { get; init; }
    public bool IsService { get; init; }
    public decimal? Quantity { get; init; }
    public decimal? UnitPrice { get; init; }
    public decimal? NetAmount { get; init; }
    public decimal? VatRate { get; init; }
    public decimal? VatAmount { get; init; }
    public decimal? TotalAmount { get; init; }
    public IReadOnlyCollection<string> MarkingNumbers { get; init; } = [];
}
