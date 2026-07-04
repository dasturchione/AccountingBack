using Application.Common.Pagination;
using Application.Features.InventoryCounts;
using Application.Features.WarehouseTransfers;
using SharedKernel.Results;

namespace Application.Features.Reports.WarehouseReports;

public interface IWarehouseReportService
{
    Task<Result<PagedResponse<WarehouseTransferListDto>>> GetTransfersAsync(WarehouseTransferListFilter filter, CancellationToken ct = default);
    Task<Result<WarehouseTransferDto>> GetTransferAsync(long id, CancellationToken ct = default);
    Task<Result<PagedResponse<InventoryCountListDto>>> GetCountsAsync(InventoryCountListFilter filter, CancellationToken ct = default);
    Task<Result<InventoryCountDto>> GetCountAsync(long id, CancellationToken ct = default);
}

public sealed class WarehouseReportService : IWarehouseReportService
{
    private readonly IWarehouseTransferService _warehouseTransferService;
    private readonly IInventoryCountService _inventoryCountService;

    public WarehouseReportService(IWarehouseTransferService warehouseTransferService, IInventoryCountService inventoryCountService)
    {
        _warehouseTransferService = warehouseTransferService;
        _inventoryCountService = inventoryCountService;
    }

    public Task<Result<InventoryCountDto>> GetCountAsync(long id, CancellationToken ct = default) =>
        _inventoryCountService.GetByIdAsync(id, ct);

    public Task<Result<PagedResponse<InventoryCountListDto>>> GetCountsAsync(InventoryCountListFilter filter, CancellationToken ct = default) =>
        _inventoryCountService.GetAllAsync(filter, ct);

    public Task<Result<PagedResponse<WarehouseTransferListDto>>> GetTransfersAsync(WarehouseTransferListFilter filter, CancellationToken ct = default) =>
        _warehouseTransferService.GetAllAsync(filter, ct);

    public Task<Result<WarehouseTransferDto>> GetTransferAsync(long id, CancellationToken ct = default) =>
        _warehouseTransferService.GetByIdAsync(id, ct);
}
