using Application.Common.Pagination;
using Application.Features.CounterpartyRegisterBalances;

namespace Application.Features.Reports.ReceivableReports;

public sealed class ReceivableReportFilterDto
{
    public CounterpartyRegisterBalanceListFilter Filter { get; set; } = new();
}

public sealed class ReceivableReportRequestDto
{
    public ReceivableReportFilterDto Filter { get; set; } = new();
}

public sealed class ReceivableReportListResponseDto
{
    public PagedResponse<ReceivableReportItemDto> Page { get; set; } = new();
}

public sealed class ReceivableReportItemDto
{
    public long Id { get; set; }
    public int CounterpartyId { get; set; }
    public string CounterpartyName { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTime DocDate { get; set; }
}
