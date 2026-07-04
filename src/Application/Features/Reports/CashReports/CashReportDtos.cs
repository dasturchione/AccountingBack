using Application.Common.Pagination;
using Application.Features.CashOperations;

namespace Application.Features.Reports.CashReports;

public sealed class CashReportFilterDto
{
    public CashOperationListFilter Filter { get; set; } = new();
}

public sealed class CashReportRequestDto
{
    public CashReportFilterDto Filter { get; set; } = new();
}

public sealed class CashReportListResponseDto
{
    public PagedResponse<CashReportItemDto> Page { get; set; } = new();
}

public sealed class CashReportItemDto
{
    public long Id { get; set; }
    public string DocNumber { get; set; } = null!;
    public DateTime DocDate { get; set; }
}
