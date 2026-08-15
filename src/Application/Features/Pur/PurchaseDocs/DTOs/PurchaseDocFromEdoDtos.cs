namespace Application.Features.PurchaseDocs;

public sealed class PurchaseDocFromEdoRequestDto
{
    public string DocumentIdentity { get; init; } = string.Empty;
    public int CounterpartyId { get; init; }
    public long? ContractId { get; init; }
    public int WarehouseId { get; init; }
    public short CurrencyId { get; init; }
    public string? Comment { get; init; }
    public List<PurchaseDocFromEdoLineDto> Lines { get; init; } = [];
}

public sealed class PurchaseDocFromEdoLineDto
{
    public int LineNumber { get; init; }
    public int ProductId { get; init; }
    public short UnitId { get; init; }
    public short? VatRateId { get; init; }
    public int? DebitAccountId { get; init; }
    public int? VatAccountId { get; init; }
    public List<PurchaseDocLineItemDto> Items { get; init; } = [];
}
