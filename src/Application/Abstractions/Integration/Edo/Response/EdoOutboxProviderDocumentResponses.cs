namespace Application.Abstractions.Integration.Edo;

using System.Text.Json.Serialization;

public sealed class EdoOutboxProviderDocumentDetailDto
{
    public string ProviderDocumentId { get; init; } = string.Empty;
    public EdoProviderCode ProviderCode { get; init; }
    public EdoDirection Direction { get; init; } = EdoDirection.OUTBOX;
    public EdoDocumentCategory Category { get; init; } = EdoDocumentCategory.OUTBOX;
    public string DocumentType { get; init; } = string.Empty;
    public string DocumentNumber { get; init; } = string.Empty;
    public DateOnly DocumentDate { get; init; }
    public EdoDocumentStatusDto Status { get; init; } = new();
    public EdoPartyDto Seller { get; init; } = new();
    public EdoPartyDto Buyer { get; init; } = new();
    public string? ContractNumber { get; init; }
    public DateOnly? ContractDate { get; init; }
    public decimal? NetAmount { get; init; }
    public decimal? VatAmount { get; init; }
    public decimal TotalAmount { get; init; }
    public string? CurrencyCode { get; init; }
    public IReadOnlyCollection<EdoOutboxProviderDocumentLineDto> Lines { get; init; } = [];
}

public sealed class EdoOutboxProviderDocumentLineDto
{
    public int Number { get; init; }
    public string? ProviderProductCode { get; init; }
    public string? ProviderProductName { get; init; }
    public string? PackageCode { get; init; }
    public string? PackageName { get; init; }
    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal? VatRate { get; init; }
    public decimal NetAmount { get; init; }
    public decimal VatAmount { get; init; }
    public decimal TotalWithVat { get; init; }
    public EdoProviderMarkingMetadataDto Marking { get; init; } = new();
}

public sealed class EdoProviderMarkingMetadataDto
{
    public bool HasMarkings { get; init; }
    public int Count { get; init; }
}

// Internal-only mapping source. Marking values are used for server-side
// validation and are never returned from a controller or written to logs.
public sealed class EdoOutboxProviderDocumentMappingSourceDto
{
    public EdoOutboxProviderDocumentDetailDto Document { get; init; } = new();

    [JsonIgnore]
    public IReadOnlyDictionary<int, IReadOnlyCollection<string>> MarkingCodesByLine { get; init; }
        = new Dictionary<int, IReadOnlyCollection<string>>();
}
