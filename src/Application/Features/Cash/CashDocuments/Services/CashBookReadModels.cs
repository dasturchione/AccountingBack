namespace Application.Features.CashDocuments;

public sealed class CashBookReadRequest
{
    public int CashBoxId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public sealed class CashBookReadResult
{
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public decimal PageOpeningBalance { get; set; }
    public decimal TotalReceipt { get; set; }
    public decimal TotalPayment { get; set; }
    public int TotalCount { get; set; }
    public IReadOnlyCollection<CashBookReadEntry> Entries { get; set; } = [];
}

public sealed class CashBookReadEntry
{
    public long MoneyRegisterEntryId { get; set; }
    public long CashOperationId { get; set; }
    public DateTime DocDate { get; set; }
    public string DocNumber { get; set; } = null!;
    public string DocumentKind { get; set; } = null!;
    public short PaymentPurposeId { get; set; }
    public string PaymentPurposeName { get; set; } = null!;
    public int? CounterpartyId { get; set; }
    public string? CounterpartyName { get; set; }
    public string? Comment { get; set; }
    public short CurrencyId { get; set; }
    public string CurrencyName { get; set; } = null!;
    public decimal Receipt { get; set; }
    public decimal Payment { get; set; }
}
