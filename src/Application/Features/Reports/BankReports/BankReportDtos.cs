using Application.Common.Pagination;
using Application.Features.BankOperations;

namespace Application.Features.Reports.BankReports;

public sealed class BankReportFilterDto
{
    public BankOperationListFilter Filter { get; set; } = new();
}

public sealed class BankReportRequestDto
{
    public BankReportFilterDto Filter { get; set; } = new();
}

public sealed class BankReportListResponseDto
{
    public PagedResponse<BankReportItemDto> Page { get; set; } = new();
}

public sealed class BankReportItemDto
{
    public long Id { get; set; }
    public string DocNumber { get; set; } = null!;
    public DateTime DocDate { get; set; }
}
