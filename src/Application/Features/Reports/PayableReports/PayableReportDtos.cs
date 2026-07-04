using Application.Common.Pagination;
using Application.Features.CounterpartyRegisterBalances;

namespace Application.Features.Reports.PayableReports;

public sealed class PayableReportFilterDto
{
    public CounterpartyRegisterBalanceListFilter Filter { get; set; } = new();
}

public sealed class PayableReportRequestDto
{
    public PayableReportFilterDto Filter { get; set; } = new();
}

public sealed class PayableReportListResponseDto
{
    public PagedResponse<PayableReportItemDto> Page { get; set; } = new();
}

public sealed class PayableReportItemDto
{
    public long Id { get; set; }
    public int CounterpartyId { get; set; }
    public string CounterpartyName { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTime DocDate { get; set; }
}
