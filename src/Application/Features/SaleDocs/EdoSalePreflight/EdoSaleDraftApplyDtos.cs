namespace Application.Features.SaleDocs.EdoSalePreflight;

public sealed class EdoSaleDraftApplyRequestDto
{
    public bool Confirm { get; init; }
    public string ExpectedPlanHash { get; init; } = string.Empty;
    public IReadOnlyCollection<EdoSaleDraftApplyItemDto> Items { get; init; } = [];
}

public sealed class EdoSaleDraftApplyItemDto
{
    public string ProviderDocumentId { get; init; } = string.Empty;
    public string DocumentType { get; init; } = "FACTURA";
    public bool AllowSentDocuments { get; init; }
    public bool AllowUnmatchedMarkings { get; init; }
    public int CounterpartyId { get; init; }
    public long ContractId { get; init; }
    public short CurrencyId { get; init; }
    public int WarehouseId { get; init; }
    public decimal ExchangeRate { get; init; } = 1m;
    public string? Comment { get; init; }
    public IReadOnlyCollection<EdoSaleDraftApplyLineDto> Lines { get; init; } = [];
}

public sealed class EdoSaleDraftApplyLineDto
{
    public int Number { get; init; }
    public int ProductId { get; init; }
    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public short UnitId { get; init; }
    public short? VatRateId { get; init; }
    public decimal CostPrice { get; init; }
    public string CostPriceSource { get; init; } = string.Empty;
    public string MarkingSource { get; init; } = "NONE";
    public IReadOnlyCollection<SaleDocCreateProductTableDto> Items { get; init; } = [];
}

public sealed class EdoSaleDraftApplyResponseDto
{
    public string PlanHash { get; init; } = string.Empty;
    public int CreatedDraftCount { get; init; }
    public int AlreadyImportedCount { get; init; }
    public IReadOnlyCollection<EdoSaleDraftApplyResultItemDto> Items { get; init; } = [];
}

public sealed class EdoSaleDraftApplyResultItemDto
{
    public string ProviderDocumentId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public long? SaleDocId { get; init; }
    public IReadOnlyCollection<string> SafeErrorCodes { get; init; } = [];
}
