namespace Application.Features.Ledger;

public class LedgerReadRequest
{
    public int AccountId { get; set; }
    public int? PeriodId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public short? CurrencyId { get; set; }
    public int? CounterpartyId { get; set; }
    public int? WarehouseId { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class LedgerReadResult
{
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public decimal PageOpeningBalance { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public int TotalCount { get; set; }
    public List<LedgerReadTransaction> Transactions { get; set; } = [];
}

public class LedgerReadTransaction
{
    public long Id { get; set; }
    public DateTime PostingDate { get; set; }
    public string? JournalNumber { get; set; }
    public string? DocumentNumber { get; set; }
    public short DocumentTypeId { get; set; }
    public string DocumentType { get; set; } = null!;
    public string? Reference { get; set; }
    public string? Description { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public short CurrencyId { get; set; }
    public string Currency { get; set; } = null!;
    public int OrganizationId { get; set; }
    public string Organization { get; set; } = null!;
    public int? CounterpartyId { get; set; }
    public string? Counterparty { get; set; }
    public int? WarehouseId { get; set; }
    public string? Warehouse { get; set; }
}
