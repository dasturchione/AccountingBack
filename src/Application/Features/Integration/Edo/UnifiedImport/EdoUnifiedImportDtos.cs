using Application.Features.SaleDocs;

namespace Application.Features.Integration.Edo.UnifiedImport;

public sealed class EdoUnifiedImportPlanDto
{
    public string ProviderCode { get; init; } = "EDOCS";
    public bool AllowUnmatchedMarkings { get; init; }
    public int TotalCandidates { get; init; }
    public int SignedCount { get; init; }
    public int WaitingForSignatureCount { get; init; }
    public int BlockedCount { get; init; }
    public int AlreadyImportedCount { get; init; }
    public int WaybillLocalCount { get; init; }
    public string PlanHash { get; init; } = string.Empty;
    public IReadOnlyCollection<EdoUnifiedImportPlanItemDto> Items { get; init; } = [];

    [System.Text.Json.Serialization.JsonIgnore]
    public IReadOnlyCollection<EdoUnifiedImportPlanItemDto> AllItems { get; init; } = [];
}

public sealed class EdoUnifiedImportPlanItemDto
{
    public string ProviderDocumentId { get; init; } = string.Empty;
    public string Direction { get; init; } = string.Empty;
    public string DocumentType { get; init; } = string.Empty;
    public string? DocumentNumber { get; init; }
    public DateOnly? DocumentDate { get; init; }
    public string ProviderStatus { get; init; } = string.Empty;
    public bool SentOverrideApplied { get; init; }
    public string Status { get; init; } = string.Empty;
    public long? EdoDocumentId { get; init; }
    public long? ExistingPurchaseId { get; init; }
    public long? ExistingSaleId { get; init; }
    public bool HasMarking { get; init; }
    public int MarkingCount { get; init; }
    public string? MarkingVerificationState { get; init; }
    public string? MarkingSourceType { get; init; }
    public string CounterpartyMappingStatus { get; init; } = string.Empty;
    public string ContractMappingStatus { get; init; } = string.Empty;
    public string CurrencyMappingStatus { get; init; } = string.Empty;
    public string ProductMappingStatus { get; init; } = string.Empty;
    public string WarehouseMappingStatus { get; init; } = string.Empty;
    public string VatMappingStatus { get; init; } = string.Empty;
    public string CostPriceStatus { get; init; } = string.Empty;
    public string ProductTableMappingStatus { get; init; } = string.Empty;
    public string MarkingMappingStatus { get; init; } = string.Empty;
    public int? CounterpartyId { get; init; }
    public long? ContractId { get; init; }
    public IReadOnlyCollection<long> ContractCandidateIds { get; init; } = [];
    public short? CurrencyId { get; init; }
    public int? WarehouseId { get; init; }
    public IReadOnlyCollection<EdoUnifiedImportPlanLineDto> Lines { get; init; } = [];
    public IReadOnlyCollection<string> SafeErrorCodes { get; init; } = [];
}

public sealed record EdoUnifiedImportPlanLineDto
{
    public int LineNumber { get; init; }
    public string? ProviderProductCode { get; init; }
    public string? ProviderProductName { get; init; }
    public decimal? Quantity { get; init; }
    public decimal? UnitPrice { get; init; }
    public decimal? CostPrice { get; init; }
    public string? CostPriceSource { get; init; }
    public int? ProductId { get; init; }
    public short? UnitId { get; init; }
    public short? VatRateId { get; init; }
    public string ProductMappingStatus { get; init; } = "BLOCKED";
    public string ProductTableMappingStatus { get; init; } = "BLOCKED";
    public string VatMappingStatus { get; init; } = "BLOCKED";
    public string CostPriceStatus { get; init; } = "REQUIRES_SELECTION";
    public bool MarkingRequired { get; init; }
    public string MarkingSource { get; init; } = "NONE";
    public IReadOnlyCollection<int> ProductTableIds { get; init; } = [];
}

public sealed class EdoUnifiedImportApplyRequestDto
{
    public bool Confirm { get; init; }
    public string ExpectedPlanHash { get; init; } = string.Empty;
    public bool AllowSentDocuments { get; init; }
    public bool AllowUnmatchedMarkings { get; init; }
    public IReadOnlyCollection<EdoUnifiedImportApplyItemDto> Items { get; init; } = [];
}

public sealed class EdoUnifiedImportApplyItemDto
{
    public string ProviderDocumentId { get; init; } = string.Empty;
    public string Direction { get; init; } = string.Empty;
    public string DocumentType { get; init; } = "FACTURA";
    public int CounterpartyId { get; init; }
    public long? ContractId { get; init; }
    public short CurrencyId { get; init; }
    public int WarehouseId { get; init; }
    public decimal ExchangeRate { get; init; } = 1m;
    public string? Comment { get; init; }
    public IReadOnlyCollection<EdoUnifiedImportApplyLineDto> Lines { get; init; } = [];
}

public sealed class EdoUnifiedImportApplyLineDto
{
    public int LineNumber { get; init; }
    public int ProductId { get; init; }
    public short UnitId { get; init; }
    public short? VatRateId { get; init; }
    public decimal? Quantity { get; init; }
    public decimal? UnitPrice { get; init; }
    public decimal? CostPrice { get; init; }
    public string? CostPriceSource { get; init; }
    public string MarkingSource { get; init; } = "PROVIDER_SNAPSHOT";
    public IReadOnlyCollection<int> ProductTableIds { get; init; } = [];
}

public sealed class EdoUnifiedImportApplyResponseDto
{
    public long BatchId { get; init; }
    public string PlanHash { get; init; } = string.Empty;
    public int ImportedCount { get; init; }
    public int WaitingForSignatureCount { get; init; }
    public int BlockedCount { get; init; }
    public int FailedCount { get; init; }
    public int AlreadyImportedCount { get; init; }
    public IReadOnlyCollection<EdoUnifiedImportBatchDocumentDto> Documents { get; init; } = [];
}

public sealed class EdoUnifiedImportBatchDto
{
    public long BatchId { get; init; }
    public string ProviderCode { get; init; } = "EDOCS";
    public string PlanHash { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int ImportedCount { get; init; }
    public int WaitingForSignatureCount { get; init; }
    public int BlockedCount { get; init; }
    public int FailedCount { get; init; }
    public int AlreadyImportedCount { get; init; }
    public IReadOnlyCollection<EdoUnifiedImportBatchDocumentDto> Documents { get; init; } = [];
}

public sealed class EdoUnifiedImportBatchDocumentDto
{
    public long BatchDocumentId { get; init; }
    public string ProviderDocumentId { get; init; } = string.Empty;
    public string Direction { get; init; } = string.Empty;
    public string DocumentType { get; init; } = "FACTURA";
    public string Status { get; init; } = string.Empty;
    public string? DocumentNumber { get; init; }
    public DateOnly? DocumentDate { get; init; }
    public long? PurchaseDocumentId { get; init; }
    public long? SaleDocumentId { get; init; }
    public string? SafeErrorCode { get; init; }
    public bool HasMarking { get; init; }
    public int MarkingCount { get; init; }
    public string? MarkingVerificationState { get; init; }
    public string? MarkingSourceType { get; init; }
    public string ProviderStatus { get; init; } = string.Empty;
    public bool SentOverrideApplied { get; init; }
}
