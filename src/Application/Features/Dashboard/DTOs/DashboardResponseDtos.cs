namespace Application.Features.Dashboard.DTOs;

public abstract class DashboardSourceDto
{
    public string SourceStatus { get; set; } = "AVAILABLE";
}

public sealed class DashboardOverviewDto
{
    public OverviewFilterDto Filters { get; set; } = new();
    public DashboardCashDto Cash { get; set; } = new();
    public DashboardRelationshipsDto Relationships { get; set; } = new();
    public TaskCalendarDto Tasks { get; set; } = new();
    public DashboardDebtDto Receivables { get; set; } = new();
    public DashboardDebtDto Payables { get; set; } = new();
    public DashboardTaxSummaryDto Tax { get; set; } = new();
    public DashboardElectronicDocumentsDto ElectronicDocuments { get; set; } = new();
}

public sealed class DashboardCashDto : DashboardSourceDto
{
    public List<DashboardCashItemDto> Items { get; set; } = [];
    public DashboardCashTotalsDto Totals { get; set; } = new();
}

public sealed class DashboardCashItemDto
{
    public int AccountId { get; set; }
    public string SourceType { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public short CurrencyId { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal OpeningBalance { get; set; }
    public decimal Inflow { get; set; }
    public decimal Outflow { get; set; }
    public decimal ClosingBalance { get; set; }
}

public sealed class DashboardCashTotalsDto
{
    public decimal OpeningBalance { get; set; }
    public decimal Inflow { get; set; }
    public decimal Outflow { get; set; }
    public decimal ClosingBalance { get; set; }
}

public sealed class DashboardRelationshipsDto : DashboardSourceDto
{
    public DashboardRelationshipSideDto Incoming { get; set; } = new();
    public DashboardRelationshipSideDto Outgoing { get; set; } = new();
}

public sealed class DashboardReceivablesPayablesDto : DashboardSourceDto
{
    public DashboardDebtDto Receivables { get; set; } = new();
    public DashboardDebtDto Payables { get; set; } = new();
}

public sealed class DashboardRelationshipSideDto : DashboardSourceDto
{
    public int Total { get; set; }
    public List<DashboardStatusCountDto> StatusCounts { get; set; } = [];
}

public sealed class DashboardStatusCountDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal? Amount { get; set; }
    public short? CurrencyId { get; set; }
}

public sealed class DashboardDebtDto : DashboardSourceDto
{
    public decimal Current { get; set; }
    public decimal? Overdue { get; set; }
    public List<DashboardAgingBucketDto> Buckets { get; set; } = [];
    public List<DashboardCounterpartyDebtDto> Counterparties { get; set; } = [];
}

public sealed class DashboardAgingBucketDto
{
    public string Bucket { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Amount { get; set; }
}

public sealed class DashboardCounterpartyDebtDto
{
    public int CounterpartyId { get; set; }
    public string CounterpartyName { get; set; } = string.Empty;
    public decimal CurrentAmount { get; set; }
    public decimal? OverdueAmount { get; set; }
    public short CurrencyId { get; set; }
}

public sealed class TaskCalendarDto
{
    public string SourceStatus { get; set; } = "NOT_AVAILABLE";
    public int Total { get; set; }
    public int Completed { get; set; }
    public int Pending { get; set; }
    public int Overdue { get; set; }
    public List<TaskCalendarItemDto> Items { get; set; } = [];
}

public sealed class TaskCalendarItemDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateOnly? Date { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsOverdue { get; set; }
}

public sealed class DashboardTaxSummaryDto : DashboardSourceDto
{
    public decimal Total { get; set; }
    public bool? IsVatPayer { get; set; }
    public List<DashboardTaxSummaryItemDto> Items { get; set; } = [];
}

public sealed class DashboardTaxSummaryItemDto
{
    public string DocumentType { get; set; } = string.Empty;
    public short CurrencyId { get; set; }
    public int Count { get; set; }
    public decimal VatAmount { get; set; }
}

public sealed class DashboardElectronicDocumentsDto : DashboardSourceDto
{
    public List<DashboardStatusCountDto> StatusCounts { get; set; } = [];
    public List<DashboardStatusCountDto> TypeCounts { get; set; } = [];
    public List<DashboardStatusCountDto> DirectionCounts { get; set; } = [];
    public List<DashboardCurrencyTotalDto> CurrencyTotals { get; set; } = [];
    public List<DashboardAmountSeriesPointDto> AmountSeries { get; set; } = [];
}

public sealed class DashboardCurrencyTotalDto
{
    public short CurrencyId { get; set; }
    public int Count { get; set; }
    public decimal? Amount { get; set; }
}

public sealed class DashboardAmountSeriesPointDto
{
    public DateOnly Date { get; set; }
    public decimal Amount { get; set; }
    public short? CurrencyId { get; set; }
}
