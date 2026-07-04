using SharedKernel.Filters;

namespace Application.Features.AccountingReports;

public abstract class AccountingReportFilterBase
{
    public int? PeriodId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public short? CurrencyId { get; set; }
}

public class BalanceSheetFilter : AccountingReportFilterBase;

public class IncomeStatementFilter : AccountingReportFilterBase;

public class CashFlowFilter : AccountingReportFilterBase;

public class AccountTurnoverFilter : AccountingReportFilterBase
{
    public int? AccountId { get; set; }
    public bool IncludeZeroBalance { get; set; }
}

public class AccountCardFilter : IPaginationFilter
{
    public int AccountId { get; set; }
    public int? PeriodId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public short? CurrencyId { get; set; }
    public int? CounterpartyId { get; set; }
    public int? WarehouseId { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; } = 50;
}

public class JournalFilter : AccountingReportFilterBase, IPaginationFilter
{
    public short? DocumentTypeId { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; } = 50;
}
