using Application.Features.AccountingReports;
using Application.Features.Reports.DTOs;
using Application.Features.Reports.Models;

namespace Application.Features.Reports.FinancialReports;

public sealed class FinancialReportFilterDto : ReportFilterDto
{
    public int? PeriodId { get; set; }
}

public sealed class FinancialReportRequestDto
{
    public FinancialReportFilterDto Filter { get; set; } = new();
    public PaginationDto Pagination { get; set; } = new();
    public SortDto? Sort { get; set; }
}

public sealed class FinancialAccountCardRequestDto
{
    public AccountCardFilter Filter { get; set; } = new();
}

public sealed class FinancialJournalRequestDto
{
    public JournalFilter Filter { get; set; } = new();
}

public sealed class FinancialReportResponseDto
{
    public BalanceSheetDto BalanceSheet { get; set; } = new();
    public IncomeStatementDto IncomeStatement { get; set; } = new();
    public CashFlowDto CashFlow { get; set; } = new();
}

public sealed class FinancialTurnoverResponseDto : ReportResponseDto<FinancialTurnoverItemDto>
{
    public int? PeriodId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public short? CurrencyId { get; set; }
    public bool IncludeZeroBalance { get; set; }
    public ReportTotalsDto TurnoverTotals { get; set; } = new();
}

public sealed class FinancialTurnoverItemDto : ReportItemDto
{
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = null!;
    public string AccountName { get; set; } = null!;
    public decimal OpeningDebit { get; set; }
    public decimal OpeningCredit { get; set; }
    public decimal PeriodDebit { get; set; }
    public decimal PeriodCredit { get; set; }
    public decimal ClosingDebit { get; set; }
    public decimal ClosingCredit { get; set; }
}

public sealed class FinancialCardResponseDto : ReportResponseDto<FinancialCardTransactionDto>
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
}

public sealed class FinancialCardTransactionDto : ReportItemDto
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

public sealed class FinancialJournalResponseDto : ReportResponseDto<FinancialJournalItemDto>
{
    public int? PeriodId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public short? CurrencyId { get; set; }
    public short? DocumentTypeId { get; set; }
}

public sealed class FinancialJournalItemDto : ReportItemDto
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
