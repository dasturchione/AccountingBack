using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.Inv.ProductPrices;
using Application.Features.Inv.WarehouseProducts;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.QueryResults;
using SharedKernel.Results;

namespace Application.Features.Inv.ProductStocks;

public class ProductStockService : IProductStockService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<ProductTable> _productTableQuery;
    private readonly IQueryRepository<Product> _productQuery;
    private readonly IProductPriceCalculateService _priceCalculateService;
    private readonly IWarehouseInventoryService _warehouseInventoryService;

    public ProductStockService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IQueryRepository<ProductTable> productTableQuery,
        IQueryRepository<Product> productQuery,
        IProductPriceCalculateService priceCalculateService,
        IWarehouseInventoryService warehouseInventoryService)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _productTableQuery = productTableQuery;
        _productQuery = productQuery;
        _priceCalculateService = priceCalculateService;
        _warehouseInventoryService = warehouseInventoryService;
    }

    public async Task<Result<ProductTableByMarkingDto>> GetByMarkingNumberAsync(string markingNumber, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is null)
            return Result.Failure<ProductTableByMarkingDto>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var query = _queryBuilder.For<ProductTable>()
            .Where(x => x.Product.OrganizationId == _userContext.OrganizationId.Value
                        && x.MarkingNumber == markingNumber
                        && x.Product.StateId == StateIdConst.ACTIVE)
            .As(x => new ProductTableByMarkingDto
            {
                ProductTableId = x.Id,
                ProductId = x.ProductId,
                ProductName = x.Product.Name,
                Mxik = x.Product.Mxik,
                MarkingNumber = x.MarkingNumber,
                SerialNumber = x.SerialNumber,
                CurrentWarehouseId = x.WarehouseProductTable != null ? x.WarehouseProductTable.WarehouseId : null,
                CurrentWarehouseName = x.WarehouseProductTable != null ? x.WarehouseProductTable.Warehouse.Name : null
            })
            .Build();

        var entity = await _productTableQuery.GetAsync(query, ct);

        if (entity is null)
            return Result.Failure<ProductTableByMarkingDto>(ProductStockErrors.NotFoundByMarkingNumber(markingNumber, _userContext.LanguageId));

        return Result.Success(entity);
    }

    public async Task<Result<PagedResponse<ProductGroupStockDto>>> GetProductGroupsStockAsync(ProductGroupStockFilter filter, CancellationToken ct = default)
    {
        var warehouseProductsResult = await _warehouseInventoryService.GetWarehouseProductsAsync(
            new WarehouseProductFilter
            {
                WarehouseId = filter.WarehouseId
            },
            ct);

        if (!warehouseProductsResult.IsSuccess)
            return Result.Failure<PagedResponse<ProductGroupStockDto>>(warehouseProductsResult.Error);

        var warehouseProducts = warehouseProductsResult.Value;
        var groupRows = warehouseProducts
            .Where(product => product.ProductGroupId.HasValue)
            .GroupBy(product => new
            {
                Id = product.ProductGroupId!.Value,
                Name = product.ProductGroupName ?? string.Empty
            })
            .Select(group => new ProductGroupAggregateRow
            {
                Id = group.Key.Id,
                Name = group.Key.Name,
                Quantity = group.Sum(product => product.Quantity)
            })
            .OrderBy(group => group.Name)
            .ToList();

        var totalCount = groupRows.Count;
        var pageSize = filter.PageSize ?? totalCount;
        var skip = Math.Max(filter.Page - 1, 0) * pageSize;
        var pageRows = groupRows.Skip(skip).Take(pageSize).ToList();

        if (pageRows.Count == 0)
            return Result.Success(PagedResponseFactory.Create(new PagedList<ProductGroupStockDto>([], totalCount), filter.Page, filter.PageSize));

        var pageGroupIds = pageRows.Select(group => group.Id).ToHashSet();
        var perProductRows = warehouseProducts
            .Where(product => product.ProductGroupId.HasValue && pageGroupIds.Contains(product.ProductGroupId.Value))
            .GroupBy(product => new
            {
                GroupId = product.ProductGroupId!.Value,
                product.ProductId
            })
            .Select(group => new GroupProductAggregateRow
            {
                GroupId = group.Key.GroupId,
                ProductId = group.Key.ProductId,
                Quantity = group.Sum(product => product.Quantity)
            })
            .ToList();

        var productIds = perProductRows.Select(product => product.ProductId).Distinct().ToList();
        var priceMap = await _priceCalculateService.GetSalePriceMapAsync(productIds, ct);
        var costPriceMap = await _priceCalculateService.GetCostPriceMapAsync(productIds, ct);
        var rowsByGroup = perProductRows
            .GroupBy(product => product.GroupId)
            .ToDictionary(group => group.Key, group => group.ToList());

        var items = pageRows.Select(group =>
        {
            var products = rowsByGroup.GetValueOrDefault(group.Id) ?? [];
            var totalAmount = products.Sum(product => priceMap.GetValueOrDefault(product.ProductId)?.SalePrice * product.Quantity ?? 0m);
            var totalCostAmount = products.Sum(product => costPriceMap.GetValueOrDefault(product.ProductId)?.CostPrice * product.Quantity ?? 0m);

            return new ProductGroupStockDto
            {
                Id = group.Id,
                Name = group.Name,
                Quantity = group.Quantity,
                Price = group.Quantity > 0m ? Math.Round(totalAmount / group.Quantity, 2) : 0m,
                CostPrice = group.Quantity > 0m ? Math.Round(totalCostAmount / group.Quantity, 2) : 0m,
                TotalAmount = totalAmount
            };
        }).ToList();

        return Result.Success(PagedResponseFactory.Create(new PagedList<ProductGroupStockDto>(items, totalCount), filter.Page, filter.PageSize));
    }
    public async Task<Result<PagedResponse<WarehouseProductDto>>> GetProductsStockAsync(ProductStockFilter filter, CancellationToken ct = default)
    {
        var warehouseProductsResult = await _warehouseInventoryService.GetWarehouseProductsAsync(
            new WarehouseProductFilter
            {
                WarehouseId = filter.WarehouseId,
                ProductGroupId = filter.ProductGroupId
            },
            ct);

        if (!warehouseProductsResult.IsSuccess)
            return Result.Failure<PagedResponse<WarehouseProductDto>>(warehouseProductsResult.Error);

        var products = warehouseProductsResult.Value
            .Where(x => string.IsNullOrWhiteSpace(filter.Search) ||
                        x.ProductName.Contains(filter.Search, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var totalCount = products.Count;
        var pageSize = filter.PageSize ?? totalCount;
        var skip = Math.Max(filter.Page - 1, 0) * pageSize;
        var pageItems = products.Skip(skip).Take(pageSize).ToList();

        return Result.Success(
            PagedResponseFactory.Create(
                new PagedList<WarehouseProductDto>(pageItems, totalCount),
                filter.Page,
                filter.PageSize));
    }
    public async Task<Result<PagedResponse<ProductTableStockDto>>> GetProductTablesStockAsync(ProductTableStockFilter filter, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is null)
            return Result.Success(PagedResponseFactory.Create(new PagedList<ProductTableStockDto>([], 0), filter.Page, filter.PageSize));

        var query = _queryBuilder.For<ProductTable>()
            .Where(x => x.Product.OrganizationId == _userContext.OrganizationId.Value
                        && x.WarehouseProductTable != null
                        && x.WarehouseProductTable.StatusId == ProductTableStatusIdConst.IN_STOCK
                        && x.Product.StateId == StateIdConst.ACTIVE
                        && (!filter.WarehouseId.HasValue || x.WarehouseProductTable.WarehouseId == filter.WarehouseId.Value)
                        && (!filter.ProductGroupId.HasValue || x.Product.ProductGroupId == filter.ProductGroupId.Value)
                        && (!filter.ProductId.HasValue || x.ProductId == filter.ProductId.Value))
            .As(x => new ProductTableStockDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductName = x.Product.Name,
                Mxik = x.Product.Mxik,
                SerialNumber = x.SerialNumber,
                MarkingNumber = x.MarkingNumber,
                CurrentWarehouseId = x.WarehouseProductTable != null ? x.WarehouseProductTable.WarehouseId : null,
                CurrentWarehouseName = x.WarehouseProductTable != null ? x.WarehouseProductTable.Warehouse.Name : null
            })
            .Build();

        var rows = (await _productTableQuery.GetAllAsync(query, ct))
            .OrderBy(x => x.ProductName)
            .ThenBy(x => x.SerialNumber)
            .ToList();

        var totalCount = rows.Count;
        var pageSize = filter.PageSize ?? totalCount;
        var skip = Math.Max(filter.Page - 1, 0) * pageSize;
        var items = rows.Skip(skip).Take(pageSize).ToList();

        return Result.Success(PagedResponseFactory.Create(new PagedList<ProductTableStockDto>(items, totalCount), filter.Page, filter.PageSize));
    }

    private sealed class ProductGroupAggregateRow
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Quantity { get; set; }
    }

    private sealed class GroupProductAggregateRow
    {
        public int GroupId { get; set; }
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
    }

    private sealed class ProductStockAggregateRow
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = null!;
        public string? Barcode { get; set; }
        public string? Mxik { get; set; }
        public bool IsPieceTracked { get; set; }
        public string? ProductGroupName { get; set; }
        public short UnitId { get; set; }
        public string UnitCode { get; set; } = null!;
        public string UnitName { get; set; } = null!;
        public bool IsService { get; set; }
        public int Quantity { get; set; }
    }

    private sealed class ProductStockSourceRow
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = null!;
        public string? Barcode { get; set; }
        public string? Mxik { get; set; }
        public string? ProductGroupName { get; set; }
        public short UnitId { get; set; }
        public string UnitCode { get; set; } = null!;
        public string UnitName { get; set; } = null!;
        public bool IsService { get; set; }
        public bool IsPieceTracked { get; set; }
    }
}
