using Application.Common.Pagination;
using Application.Features.PurchaseDocs;
using Application.Features.Reports.DTOs;

namespace Application.Features.Reports.PurchaseReports;

public sealed class PurchaseReportFilterDto
{
    public PurchaseDocListFilter Filter { get; set; } = new();
}

public sealed class PurchaseReportRequestDto
{
    public PurchaseReportFilterDto Filter { get; set; } = new();
    public PaginationDto Pagination { get; set; } = new();
}

public sealed class PurchaseReportListResponseDto
{
    public PagedResponse<PurchaseReportItemDto> Page { get; set; } = new();
}

public sealed class PurchaseReportItemDto
{
    public long Id { get; set; }
    public string DocNumber { get; set; } = null!;
    public DateTime DocDate { get; set; }
    public int OrganizationId { get; set; }
    public string Organization { get; set; } = null!;
}

public sealed class PurchaseReportDetailResponseDto
{
    public long Id { get; set; }
    public string DocNumber { get; set; } = null!;
    public DateTime DocDate { get; set; }
    public int OrganizationId { get; set; }
    public string Organization { get; set; } = null!;
}
