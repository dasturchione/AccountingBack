using Application.Abstractions;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Inv.ProductStocks;

public class ProductStockCalculateService : IProductStockCalculateService
{
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<WarehouseProduct> _warehouseProductQuery;
    private readonly IQueryRepository<ProductTable> _productTableQuery;
    private readonly IQueryRepository<RegisterBalance> _registerBalanceQuery;

    public ProductStockCalculateService(
        IQueryBuilder queryBuilder,
        IQueryRepository<WarehouseProduct> warehouseProductQuery,
        IQueryRepository<ProductTable> productTableQuery,
        IQueryRepository<RegisterBalance> registerBalanceQuery)
    {
        _queryBuilder = queryBuilder;
        _warehouseProductQuery = warehouseProductQuery;
        _productTableQuery = productTableQuery;
        _registerBalanceQuery = registerBalanceQuery;
    }

    public Task<Result<Dictionary<int, (decimal Quantity, decimal Available, decimal Reserved, decimal Blocked)>>> GetProductGroupsAsync(
        int? organizationId,
        int? warehouseId = null,
        DateOnly? choosedDate = null,
        IEnumerable<int>? productIds = null,
        CancellationToken ct = default)
    {
        if (organizationId is null)
            return EmptyAsync();

        var productIdList = NormalizeProductIds(productIds);
        if (productIds is not null && productIdList.Count == 0)
            return EmptyAsync();

        return IsCurrentOrFuture(choosedDate)
            ? GetCurrentProductGroupsAsync(organizationId.Value, warehouseId, productIdList, ct)
            : GetHistoricalProductGroupsAsync(organizationId.Value, warehouseId, choosedDate!.Value, productIdList, ct);
    }

    public Task<Result<Dictionary<int, (decimal Quantity, decimal Available, decimal Reserved, decimal Blocked)>>> GetProductsAsync(
        int? organizationId,
        int? warehouseId = null,
        int? productGroupId = null,
        DateOnly? choosedDate = null,
        IEnumerable<int>? productIds = null,
        CancellationToken ct = default)
    {
        if (organizationId is null)
            return EmptyAsync();

        var productIdList = NormalizeProductIds(productIds);
        if (productIds is not null && productIdList.Count == 0 || 
            productGroupId is null)
            return EmptyAsync();

        return IsCurrentOrFuture(choosedDate)
            ? GetCurrentProductsAsync(organizationId.Value, warehouseId, productGroupId, productIdList, ct)
            : GetHistoricalProductsAsync(organizationId.Value, warehouseId, productGroupId, choosedDate!.Value, productIdList, ct);
    }

    public Task<Result<Dictionary<int, (decimal Quantity, decimal Available, decimal Reserved, decimal Blocked)>>> GetProductTablesAsync(
        int? organizationId,
        int? warehouseId = null,
        int? productGroupId = null,
        DateOnly? choosedDate = null,
        IEnumerable<int>? productIds = null,
        CancellationToken ct = default)
    {
        if (organizationId is null)
            return EmptyAsync();

        var productIdList = NormalizeProductIds(productIds);
        if (productIds is not null && productIdList.Count == 0)
            return EmptyAsync();

        return IsCurrentOrFuture(choosedDate)
            ? GetCurrentProductTablesAsync(organizationId.Value, warehouseId, productGroupId, productIdList, ct)
            : GetHistoricalProductTablesAsync(organizationId.Value, warehouseId, productGroupId, choosedDate!.Value, productIdList, ct);
    }

    private async Task<Result<Dictionary<int, (decimal Quantity, decimal Available, decimal Reserved, decimal Blocked)>>> GetCurrentProductGroupsAsync(
        int organizationId,
        int? warehouseId,
        IReadOnlyCollection<int> productIds,
        CancellationToken ct)
    {
        var query = _queryBuilder.For<WarehouseProduct>()
            .Where(x => x.Product.OrganizationId == organizationId &&
                        x.Product.ProductGroupId.HasValue &&
                        (!warehouseId.HasValue || x.WarehouseId == warehouseId.Value) &&
                        (productIds.Count == 0 || productIds.Contains(x.ProductId)))
            .As(x => new ProductGroupBalanceRow
            {
                ProductGroupId = x.Product.ProductGroupId!.Value,
                Quantity = x.Quantity,
                Available = x.AvailableQuantity,
                Reserved = x.ReservedQuantity,
                Blocked = x.BlockedQuantity
            })
            .Build();

        var rows = await _warehouseProductQuery.GetAllAsync(query, ct);
        return Result.Success(GroupBalances(rows, x => x.ProductGroupId));
    }

    private async Task<Result<Dictionary<int, (decimal Quantity, decimal Available, decimal Reserved, decimal Blocked)>>> GetCurrentProductsAsync(
        int organizationId,
        int? warehouseId,
        int? productGroupId,
        IReadOnlyCollection<int> productIds,
        CancellationToken ct)
    {
        var query = _queryBuilder.For<WarehouseProduct>()
            .Where(x => x.Product.OrganizationId == organizationId &&
                        (warehouseId == null || x.WarehouseId == warehouseId.Value) && 
                        (productGroupId == null || x.Product.ProductGroupId == productGroupId.Value) && 
                        (productIds.Count == 0 || productIds.Contains(x.ProductId)))
            .As(x => new ProductBalanceRow
            {
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                Available = x.AvailableQuantity,
                Reserved = x.ReservedQuantity,
                Blocked = x.BlockedQuantity
            })
            .Build();

        var rows = await _warehouseProductQuery.GetAllAsync(query, ct);
        return Result.Success(GroupBalances(rows, x => x.ProductId));
    }

    private async Task<Result<Dictionary<int, (decimal Quantity, decimal Available, decimal Reserved, decimal Blocked)>>> GetCurrentProductTablesAsync(
        int organizationId,
        int? warehouseId,
        int? productGroupId,
        IReadOnlyCollection<int> productIds,
        CancellationToken ct)
    {
        var query = _queryBuilder.For<ProductTable>()
            .Where(x => x.Product.OrganizationId == organizationId &&
                        x.CurrentWarehouseId.HasValue &&
                        x.StateId == StateIdConst.ACTIVE &&
                        (x.StatusId == ProductTableStatusIdConst.IN_STOCK ||
                         x.StatusId == ProductTableStatusIdConst.RESERVED ||
                         x.StatusId == ProductTableStatusIdConst.BLOCKED) &&
                        (!warehouseId.HasValue || x.CurrentWarehouseId == warehouseId.Value) && 
                        (productGroupId == null || x.Product.ProductGroupId == productGroupId.Value) && 
                        (productIds.Count == 0 || productIds.Contains(x.ProductId)))
            .As(x => new ProductTableStatusBalanceRow
            {
                ProductTableId = x.Id,
                StatusId = x.StatusId
            })
            .Build();

        var rows = await _productTableQuery.GetAllAsync(query, ct);
        return Result.Success(rows.ToDictionary(
            x => x.ProductTableId,
            x => x.StatusId switch
            {
                ProductTableStatusIdConst.IN_STOCK => (Quantity: 1m, Available: 1m, Reserved: 0m, Blocked: 0m),
                ProductTableStatusIdConst.RESERVED => (Quantity: 1m, Available: 0m, Reserved: 1m, Blocked: 0m),
                ProductTableStatusIdConst.BLOCKED => (Quantity: 1m, Available: 0m, Reserved: 0m, Blocked: 1m),
                _ => (Quantity: 0m, Available: 0m, Reserved: 0m, Blocked: 0m)
            }));
    }

    private async Task<Result<Dictionary<int, (decimal Quantity, decimal Available, decimal Reserved, decimal Blocked)>>> GetHistoricalProductGroupsAsync(
        int organizationId,
        int? warehouseId,
        DateOnly choosedDate,
        IReadOnlyCollection<int> productIds,
        CancellationToken ct)
    {
        var rows = await GetHistoricalRegisterRowsAsync(organizationId, warehouseId, null, choosedDate, productIds, ct);
        return Result.Success(rows
            .Where(x => x.ProductGroupId.HasValue)
            .GroupBy(x => x.ProductGroupId!.Value)
            .ToDictionary(x => x.Key, x => ToHistoricalBalance(x.Sum(r => r.QuantityDelta))));
    }

    private async Task<Result<Dictionary<int, (decimal Quantity, decimal Available, decimal Reserved, decimal Blocked)>>> GetHistoricalProductsAsync(
        int organizationId,
        int? warehouseId,
        int? productGroupId,
        DateOnly choosedDate,
        IReadOnlyCollection<int> productIds,
        CancellationToken ct)
    {
        var rows = await GetHistoricalRegisterRowsAsync(organizationId, warehouseId, productGroupId, choosedDate, productIds, ct);
        return Result.Success(rows
            .GroupBy(x => x.ProductId)
            .ToDictionary(x => x.Key, x => ToHistoricalBalance(x.Sum(r => r.QuantityDelta))));
    }

    private async Task<Result<Dictionary<int, (decimal Quantity, decimal Available, decimal Reserved, decimal Blocked)>>> GetHistoricalProductTablesAsync(
        int organizationId,
        int? warehouseId,
        int? productGroupId,
        DateOnly choosedDate,
        IReadOnlyCollection<int> productIds,
        CancellationToken ct)
    {
        var rows = await GetHistoricalRegisterRowsAsync(organizationId, warehouseId, productGroupId, choosedDate, productIds, ct);
        return Result.Success(rows
            .Where(x => x.ProductTableId.HasValue)
            .GroupBy(x => x.ProductTableId!.Value)
            .ToDictionary(x => x.Key, x => ToHistoricalBalance(x.Sum(r => r.QuantityDelta))));
    }

    private async Task<List<HistoricalRegisterBalanceRow>> GetHistoricalRegisterRowsAsync(
        int organizationId,
        int? warehouseId,
        int? productGroupId,
        DateOnly choosedDate,
        IReadOnlyCollection<int> productIds,
        CancellationToken ct)
    {
        var endDate = choosedDate.ToDateTime(TimeOnly.MaxValue);
        var query = _queryBuilder.For<RegisterBalance>()
            .Where(x => x.Product.OrganizationId == organizationId &&
                        x.DocDate <= endDate &&
                        (!warehouseId.HasValue || x.WarehouseId == warehouseId.Value) &&
                        (productIds.Count == 0 || productIds.Contains(x.ProductId)) &&
                        (productGroupId == null || x.Product.ProductGroupId == productGroupId.Value) &&
                        (x.OperationTypeId == OperationTypeIdConst.IN || x.OperationTypeId == OperationTypeIdConst.OUT))
            .As(x => new HistoricalRegisterBalanceRow
            {
                ProductId = x.ProductId,
                ProductGroupId = x.Product.ProductGroupId,
                ProductTableId = x.ProductTableId,
                QuantityDelta = x.OperationTypeId == OperationTypeIdConst.IN ? x.Quantity : -x.Quantity
            })
            .Build();

        return await _registerBalanceQuery.GetAllAsync(query, ct);
    }

    private static Dictionary<int, (decimal Quantity, decimal Available, decimal Reserved, decimal Blocked)> GroupBalances<T>(
        IEnumerable<T> rows,
        Func<T, int> keySelector)
        where T : BalanceRow
    {
        return rows
            .GroupBy(keySelector)
            .ToDictionary(
                x => x.Key,
                x => (
                    Quantity: x.Sum(r => r.Quantity),
                    Available: x.Sum(r => r.Available),
                    Reserved: x.Sum(r => r.Reserved),
                    Blocked: x.Sum(r => r.Blocked)));
    }

    private static (decimal Quantity, decimal Available, decimal Reserved, decimal Blocked) ToHistoricalBalance(decimal quantity) =>
        (quantity, quantity, 0m, 0m);

    private static bool IsCurrentOrFuture(DateOnly? choosedDate) =>
        !choosedDate.HasValue || choosedDate.Value >= DateOnly.FromDateTime(DateTime.Today);

    private static List<int> NormalizeProductIds(IEnumerable<int>? productIds) =>
        productIds?.Distinct().ToList() ?? [];

    private static Task<Result<Dictionary<int, (decimal Quantity, decimal Available, decimal Reserved, decimal Blocked)>>> EmptyAsync() =>
        Task.FromResult(Result.Success(new Dictionary<int, (decimal Quantity, decimal Available, decimal Reserved, decimal Blocked)>()));

    private abstract class BalanceRow
    {
        public decimal Quantity { get; set; }
        public decimal Available { get; set; }
        public decimal Reserved { get; set; }
        public decimal Blocked { get; set; }
    }

    private sealed class ProductGroupBalanceRow : BalanceRow
    {
        public int ProductGroupId { get; set; }
    }

    private sealed class ProductBalanceRow : BalanceRow
    {
        public int ProductId { get; set; }
    }

    private sealed class ProductTableStatusBalanceRow
    {
        public int ProductTableId { get; set; }
        public short StatusId { get; set; }
    }

    private sealed class HistoricalRegisterBalanceRow
    {
        public int ProductId { get; set; }
        public int? ProductGroupId { get; set; }
        public int? ProductTableId { get; set; }
        public decimal QuantityDelta { get; set; }
    }
}
