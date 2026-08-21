namespace Application.Features.SaleDocs.EdoSalePreflight;

public sealed class EdoSalePreflightPlanDto
{
    public string ProviderCode { get; init; } = "EDOCS";
    public string PlanHash { get; init; } = string.Empty;
    public string SourceLinkStatus { get; init; } = "BLOCKED";
    public int TotalCandidates { get; init; }
    public int ReadyCount { get; init; }
    public int RequiresSelectionCount { get; init; }
    public int BlockedCount { get; init; }
    public int DuplicateCount { get; init; }
    public IReadOnlyCollection<string> SafeErrorCodes { get; init; } = [];
    public IReadOnlyCollection<EdoSalePreflightCandidateDto> Candidates { get; init; } = [];
}

public sealed class EdoSalePreflightCandidateDto
{
    public string? ProviderDocumentId { get; init; }
    public string? DocumentType { get; init; }
    public string? DocumentNumber { get; init; }
    public DateOnly? DocumentDate { get; init; }
    public string Status { get; init; } = "BLOCKED";
    public long? ExistingSaleDocId { get; init; }
    public decimal? NetAmount { get; init; }
    public decimal? VatAmount { get; init; }
    public decimal? TotalAmount { get; init; }
    public string? CurrencyCode { get; init; }
    public EdoSaleMappingStatusDto Counterparty { get; init; } = new();
    public EdoSaleMappingStatusDto Contract { get; init; } = new();
    public EdoSaleMappingStatusDto Warehouse { get; init; } = new();
    public EdoSaleMappingStatusDto Currency { get; init; } = new();
    public EdoSaleMarkingStatusDto Marking { get; init; } = new();
    public IReadOnlyCollection<EdoSalePreflightLineDto> Lines { get; init; } = [];
    public IReadOnlyCollection<string> SafeErrorCodes { get; init; } = [];
}

public sealed class EdoSalePreflightLineDto
{
    public int Number { get; init; }
    public string? ProviderProductCode { get; init; }
    public string? ProviderProductName { get; init; }
    public decimal? Quantity { get; init; }
    public decimal? UnitPrice { get; init; }
    public decimal? NetAmount { get; init; }
    public decimal? VatRate { get; init; }
    public decimal? VatAmount { get; init; }
    public decimal? TotalWithVat { get; init; }
    public EdoSaleMappingStatusDto Product { get; init; } = new();
    public EdoSaleMappingStatusDto ProductTable { get; init; } = new();
    public EdoSaleMappingStatusDto Unit { get; init; } = new();
    public EdoSaleMappingStatusDto Vat { get; init; } = new();
    public EdoSaleMappingStatusDto CostPrice { get; init; } = new();
    public EdoSaleMarkingStatusDto Marking { get; init; } = new();
}

public sealed class EdoSaleMappingStatusDto
{
    public string Status { get; init; } = "BLOCKED";
    public string? SafeErrorCode { get; init; }
    public long? SelectedId { get; init; }
    public string? ProviderCode { get; init; }
    public string? ProviderContractNumber { get; init; }
    public DateOnly? ProviderContractDate { get; init; }
    public IReadOnlyCollection<long> CandidateIds { get; init; } = [];
    public string? Description { get; init; }
}

public sealed class EdoSaleMarkingStatusDto
{
    public bool HasMarkings { get; init; }
    public int MarkingCount { get; init; }
    public bool MarkingRequired { get; init; }
    public string Status { get; init; } = "NOT_REQUIRED";
    public string? SafeErrorCode { get; init; }
}
