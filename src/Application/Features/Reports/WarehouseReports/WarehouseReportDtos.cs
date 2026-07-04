using Application.Common.Pagination;
using Application.Features.InventoryCounts;
using Application.Features.WarehouseTransfers;

namespace Application.Features.Reports.WarehouseReports;

public sealed class WarehouseTransferReportFilterDto
{
    public WarehouseTransferListFilter Filter { get; set; } = new();
}

public sealed class InventoryCountReportFilterDto
{
    public InventoryCountListFilter Filter { get; set; } = new();
}

public sealed class WarehouseReportRequestDto
{
    public WarehouseTransferReportFilterDto Transfers { get; set; } = new();
    public InventoryCountReportFilterDto Counts { get; set; } = new();
}

public sealed class WarehouseTransferReportListResponseDto
{
    public PagedResponse<WarehouseTransferReportItemDto> Page { get; set; } = new();
}

public sealed class InventoryCountReportListResponseDto
{
    public PagedResponse<InventoryCountReportItemDto> Page { get; set; } = new();
}

public sealed class WarehouseTransferReportItemDto
{
    public long Id { get; set; }
    public string DocNumber { get; set; } = null!;
    public DateTime DocDate { get; set; }
}

public sealed class InventoryCountReportItemDto
{
    public long Id { get; set; }
    public string DocNumber { get; set; } = null!;
    public DateTime DocDate { get; set; }
}
