namespace Application.Features.AccountingReports;

public class BalanceSheetDto
{
    public int? PeriodId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public short? CurrencyId { get; set; }
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal TotalEquity { get; set; }
    public decimal TotalLiabilitiesAndEquity => TotalLiabilities + TotalEquity;
    public List<BalanceSheetSectionDto> Sections { get; set; } = [];
}

public class BalanceSheetSectionDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public decimal Total { get; set; }
    public List<BalanceSheetRowDto> Rows { get; set; } = [];
}

public class BalanceSheetRowDto
{
    public int? AccountId { get; set; }
    public string AccountCode { get; set; } = null!;
    public string AccountName { get; set; } = null!;
    public decimal Balance { get; set; }
}

public class IncomeStatementDto
{
    public int? PeriodId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public short? CurrencyId { get; set; }
    public decimal RevenueTotal { get; set; }
    public decimal CostOfSalesTotal { get; set; }
    public decimal OperatingExpenseTotal { get; set; }
    public decimal OtherIncomeTotal { get; set; }
    public decimal OtherExpenseTotal { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal OperatingProfit { get; set; }
    public decimal NetProfit { get; set; }
    public List<IncomeStatementSectionDto> Sections { get; set; } = [];
}

public class IncomeStatementSectionDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public decimal Total { get; set; }
    public List<IncomeStatementRowDto> Rows { get; set; } = [];
}

public class IncomeStatementRowDto
{
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = null!;
    public string AccountName { get; set; } = null!;
    public decimal Amount { get; set; }
}

public class CashFlowDto
{
    public int? PeriodId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public short? CurrencyId { get; set; }
    public decimal OpeningCashBalance { get; set; }
    public decimal ClosingCashBalance { get; set; }
    public List<CashFlowSectionDto> Sections { get; set; } = [];
}

public class CashFlowSectionDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public decimal Inflow { get; set; }
    public decimal Outflow { get; set; }
    public decimal Net => Inflow - Outflow;
    public List<CashFlowRowDto> Rows { get; set; } = [];
}

public class CashFlowRowDto
{
    public string CounterpartAccountCode { get; set; } = null!;
    public string CounterpartAccountName { get; set; } = null!;
    public decimal Inflow { get; set; }
    public decimal Outflow { get; set; }
    public decimal Net => Inflow - Outflow;
}

public class AccountTurnoverDto
{
    public int? PeriodId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public short? CurrencyId { get; set; }
    public bool IncludeZeroBalance { get; set; }
    public decimal OpeningDebitTotal { get; set; }
    public decimal OpeningCreditTotal { get; set; }
    public decimal PeriodDebitTotal { get; set; }
    public decimal PeriodCreditTotal { get; set; }
    public decimal ClosingDebitTotal { get; set; }
    public decimal ClosingCreditTotal { get; set; }
    public List<AccountTurnoverRowDto> Items { get; set; } = [];
}

public class AccountTurnoverRowDto
{
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = null!;
    public string AccountName { get; set; } = null!;
    public string AccountNumber { get; set; } = null!;
    public decimal OpeningDebit { get; set; }
    public decimal OpeningCredit { get; set; }
    public decimal PeriodDebit { get; set; }
    public decimal PeriodCredit { get; set; }
    public decimal ClosingDebit { get; set; }
    public decimal ClosingCredit { get; set; }
}

public class AccountCardDto
{
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = null!;
    public string AccountName { get; set; } = null!;
    public int? PeriodId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public short? CurrencyId { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
    public List<AccountCardTransactionDto> Transactions { get; set; } = [];
}

public class AccountCardTransactionDto
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
    public decimal RunningBalance { get; set; }
    public short CurrencyId { get; set; }
    public string Currency { get; set; } = null!;
    public int OrganizationId { get; set; }
    public string Organization { get; set; } = null!;
    public int? CounterpartyId { get; set; }
    public string? Counterparty { get; set; }
    public int? WarehouseId { get; set; }
    public string? Warehouse { get; set; }
}

public class JournalDto
{
    public int? PeriodId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public short? CurrencyId { get; set; }
    public short? DocumentTypeId { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
    public List<JournalEntryDto> Entries { get; set; } = [];
}

public class JournalEntryDto
{
    public long Id { get; set; }
    public DateTime PostingDate { get; set; }
    public string? JournalNumber { get; set; }
    public string? DocumentNumber { get; set; }
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
}
