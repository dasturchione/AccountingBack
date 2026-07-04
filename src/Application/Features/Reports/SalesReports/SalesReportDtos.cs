using Application.Common.Pagination;
using Application.Features.SaleDocs;
using Application.Features.Reports.DTOs;

namespace Application.Features.Reports.SalesReports;

public sealed class SalesReportFilterDto
{
    public SaleDocListFilter Filter { get; set; } = new();
}

public sealed class SalesReportRequestDto
{
    public SalesReportFilterDto Filter { get; set; } = new();
    public PaginationDto Pagination { get; set; } = new();
}

public sealed class SalesReportListResponseDto
{
    public PagedResponse<SalesReportItemDto> Page { get; set; } = new();
}

public sealed class SalesReportItemDto
{
    public long Id { get; set; }
    public string DocNumber { get; set; } = null!;
    public DateTime DocDate { get; set; }
    public int OrganizationId { get; set; }
    public string Organization { get; set; } = null!;
}

public sealed class SalesReportDetailResponseDto
{
    public long Id { get; set; }
    public string DocNumber { get; set; } = null!;
    public DateTime DocDate { get; set; }
    public int OrganizationId { get; set; }
    public string Organization { get; set; } = null!;
}
