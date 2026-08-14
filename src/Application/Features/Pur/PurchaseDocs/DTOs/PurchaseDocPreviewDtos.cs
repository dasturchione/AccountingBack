using Application.Abstractions.Integration.Edo;
using SharedKernel.Text;

namespace Application.Features.PurchaseDocs;

public sealed class PurchaseDocPreviewRequestDto
{
    public string DocumentIdentity { get; init; } = string.Empty;
    public int? CounterpartyId { get; init; }
    public long? ContractId { get; init; }
    public int? WarehouseId { get; init; }
    public short? CurrencyId { get; init; }
    public List<PurchaseDocPreviewLineMappingDto> Lines { get; init; } = [];
}

public sealed class PurchaseDocPreviewLineMappingDto
{
    public int LineNumber { get; init; }
    public int? ProductId { get; init; }
    public short? UnitId { get; init; }
    public short? VatRateId { get; init; }
    public List<PurchaseDocLineItemDto> Items { get; init; } = [];
}

public sealed class PurchaseDocPreviewDto
{
    public EdoProviderCode Provider { get; init; }
    public string DocumentIdentity { get; init; } = string.Empty;
    public EdoDocumentStatusCode Status { get; init; }
    public string? DocumentNumber { get; init; }
    public DateOnly? DocumentDate { get; init; }
    public PurchaseDocPreviewCounterpartyDto Counterparty { get; init; } = new();
    public PurchaseDocPreviewContractDto Contract { get; init; } = new();
    public IReadOnlyCollection<PurchaseDocPreviewLineDto> Lines { get; init; } = [];
    public PurchaseDocPreviewReferenceDto Currency { get; init; } = new();
    public PurchaseDocPreviewReferenceDto Warehouse { get; init; } = new();
    public IReadOnlyCollection<PurchaseDocPreviewValidationErrorDto> ValidationErrors { get; init; } = [];
    public PurchaseDocPreviewDuplicateDto Duplicate { get; init; } = new();
    public bool CanCreateDraft { get; init; }
}

public sealed class PurchaseDocPreviewCounterpartyDto
{
    public int? Id { get; init; }
    public string? Name { get; init; }
    public string? Inn { get; init; }
    public bool IsResolved { get; init; }
    public bool RequiresSelection { get; init; }
}

public sealed class PurchaseDocPreviewContractDto
{
    public long? Id { get; init; }
    public string? Number { get; init; }
    public DateOnly? Date { get; init; }
    public bool IsResolved { get; init; }
    public bool RequiresSelection { get; init; }
}

public sealed class PurchaseDocPreviewLineDto
{
    private string? _packageName;
    private string? _productName;

    public int Number { get; init; }
    public string? CatalogCode { get; init; }
    public string? PackageCode { get; init; }
    public string? PackageName
    {
        get => _packageName;
        init => _packageName = Utf8MojibakeNormalizer.Normalize(value);
    }
    public PurchaseDocPreviewItemType ItemType { get; init; }
    public int? ProductId { get; init; }
    public string? ProductName
    {
        get => _productName;
        init => _productName = Utf8MojibakeNormalizer.Normalize(value);
    }
    public bool? IsService { get; init; }
    public short? UnitId { get; init; }
    public string? UnitName { get; init; }
    public decimal? Quantity { get; init; }
    public decimal? UnitPrice { get; init; }
    public decimal? VatRate { get; init; }
    public short? VatRateId { get; init; }
    public decimal? TotalWithVat { get; init; }
    public bool IsResolved { get; init; }
    public bool RequiresManualMapping { get; init; }
}

public enum PurchaseDocPreviewItemType
{
    UNKNOWN,
    PRODUCT,
    SERVICE
}

public sealed class PurchaseDocPreviewReferenceDto
{
    public int? Id { get; init; }
    public string? Name { get; init; }
    public bool IsResolved { get; init; }
    public bool IsSuggested { get; init; }
    public bool RequiresSelection { get; init; }
}

public sealed class PurchaseDocPreviewValidationErrorDto
{
    public string Code { get; init; } = string.Empty;
    public string Field { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

public sealed class PurchaseDocPreviewDuplicateDto
{
    public bool IsDuplicate { get; init; }
    public long? ExistingPurchaseId { get; init; }
}
