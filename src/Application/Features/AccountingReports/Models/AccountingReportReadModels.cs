namespace Application.Features.AccountingReports;

public class CashFlowReadRequest
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public short? CurrencyId { get; set; }
}

public class CashFlowReadResult
{
    public decimal OpeningCashBalance { get; set; }
    public decimal ClosingCashBalance { get; set; }
    public List<CashFlowReadRow> Rows { get; set; } = [];
}

public class CashFlowReadRow
{
    public string CounterpartAccountCode { get; set; } = null!;
    public string CounterpartAccountName { get; set; } = null!;
    public decimal Inflow { get; set; }
    public decimal Outflow { get; set; }
}

public class JournalReadRequest
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public short? CurrencyId { get; set; }
    public short? DocumentTypeId { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class JournalReadResult
{
    public int TotalCount { get; set; }
    public List<JournalReadRow> Rows { get; set; } = [];
}

public class JournalReadRow
{
    public long Id { get; set; }
    public DateTime PostingDate { get; set; }
    public string? JournalNumber { get; set; }
    public long DocumentId { get; set; }
    public short DocumentTypeId { get; set; }
    public string DocumentType { get; set; } = null!;
    public string? Description { get; set; }
    public string? DebitAccountCode { get; set; }
    public string? DebitAccountName { get; set; }
    public string? CreditAccountCode { get; set; }
    public string? CreditAccountName { get; set; }
    public decimal Amount { get; set; }
    public short CurrencyId { get; set; }
    public string Currency { get; set; } = null!;
    public int OrganizationId { get; set; }
    public string Organization { get; set; } = null!;
    public string? Counterparty { get; set; }
    public string? Warehouse { get; set; }
    public string? DocumentNumber { get; set; }
}
