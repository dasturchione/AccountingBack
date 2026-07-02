using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.Inv.ProductPrices;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.QueryResults;
using SharedKernel.Results;

namespace Application.Features.Inv.ProductStocks;

public class ProductStockService : IProductStockService
{
    private readonly IUserContext _userContext;
    private readonly IInventoryReadDbContext _inventoryReadDbContext;
    private readonly IProductPriceCalculateService _priceCalculateService;

    public ProductStockService(
        IUserContext userContext,
        IInventoryReadDbContext inventoryReadDbContext,
        IProductPriceCalculateService priceCalculateService)
    {
        _userContext = userContext;
        _inventoryReadDbContext = inventoryReadDbContext;
        _priceCalculateService = priceCalculateService;
    }

    public async Task<Result<ProductTableByMarkingDto>> GetByMarkingNumberAsync(string markingNumber, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is null)
            return Result.Failure<ProductTableByMarkingDto>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var entity = await _inventoryReadDbContext.ProductTables
            .AsNoTracking()
            .Where(x => x.OrganizationId == _userContext.OrganizationId.Value
                        && x.MarkingNumber == markingNumber
                        && x.StateId == StateIdConst.ACTIVE)
            .Select(x => new ProductTableByMarkingDto
            {
                ProductTableId = x.Id,
                ProductId = x.ProductId,
                ProductName = x.Product.Name,
                Mxik = x.Product.Mxik,
                MarkingNumber = x.MarkingNumber,
                SerialNumber = x.SerialNumber,
                CurrentWarehouseId = x.CurrentWarehouseId,
                CurrentWarehouseName = x.CurrentWarehouse != null ? x.CurrentWarehouse.Name : null
            })
            .FirstOrDefaultAsync(ct);

        if (entity is null)
            return Result.Failure<ProductTableByMarkingDto>(ProductStockErrors.NotFoundByMarkingNumber(markingNumber, _userContext.LanguageId));

        return Result.Success(entity);
    }

    public async Task<Result<PagedResponse<ProductGroupStockDto>>> GetProductGroupsStockAsync(ProductGroupStockFilter filter, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is null)
            return Result.Success(PagedResponseFactory.Create(new PagedList<ProductGroupStockDto>([], 0), filter.Page, filter.PageSize));

        var orgId = _userContext.OrganizationId.Value;
        var baseQuery = _inventoryReadDbContext.ProductTables
            .AsNoTracking()
            .Where(x => x.OrganizationId == orgId
                        && x.StatusId == ProductTableStatusIdConst.IN_STOCK
                        && x.StateId == StateIdConst.ACTIVE
                        && (!filter.WarehouseId.HasValue || x.CurrentWarehouseId == filter.WarehouseId.Value)
                        && x.Product.ProductGroupId != null);

        var groupedQuery = baseQuery
            .GroupBy(x => new
            {
                Id = x.Product.ProductGroupId!.Value,
                Name = x.Product.ProductGroup!.Name
            })
            .Select(g => new ProductGroupAggregateRow
            {
                Id = g.Key.Id,
                Name = g.Key.Name,
                Quantity = g.Count()
            })
            .OrderBy(x => x.Name);

        var totalCount = await groupedQuery.CountAsync(ct);
        var pageSize = filter.PageSize ?? totalCount;
        var skip = Math.Max(filter.Page - 1, 0) * pageSize;
        var pageRows = await groupedQuery.Skip(skip).Take(pageSize).ToListAsync(ct);

        if (pageRows.Count == 0)
            return Result.Success(PagedResponseFactory.Create(new PagedList<ProductGroupStockDto>([], totalCount), filter.Page, filter.PageSize));

        var pageGroupIds = pageRows.Select(x => x.Id).ToList();
        var perProductRows = await baseQuery
            .Where(x => pageGroupIds.Contains(x.Product.ProductGroupId!.Value))
            .GroupBy(x => new
            {
                GroupId = x.Product.ProductGroupId!.Value,
                x.ProductId
            })
            .Select(g => new GroupProductAggregateRow
            {
                GroupId = g.Key.GroupId,
                ProductId = g.Key.ProductId,
                Quantity = g.Count()
            })
            .ToListAsync(ct);

        var productIds = perProductRows.Select(x => x.ProductId).Distinct().ToList();
        var priceMap = await _priceCalculateService.GetSalePriceMapAsync(productIds, ct);
        var costPriceMap = await _priceCalculateService.GetCostPriceMapAsync(productIds, ct);
        var rowsByGroup = perProductRows.GroupBy(x => x.GroupId).ToDictionary(x => x.Key, x => x.ToList());

        var items = pageRows.Select(row =>
        {
            var productRows = rowsByGroup.GetValueOrDefault(row.Id) ?? [];
            var totalAmount = productRows.Sum(x => priceMap.GetValueOrDefault(x.ProductId)?.SalePrice * x.Quantity ?? 0m);
            var totalCostAmount = productRows.Sum(x => costPriceMap.GetValueOrDefault(x.ProductId)?.CostPrice * x.Quantity ?? 0m);

            return new ProductGroupStockDto
            {
                Id = row.Id,
                Name = row.Name,
                Quantity = row.Quantity,
                Price = row.Quantity > 0 ? Math.Round(totalAmount / row.Quantity, 2) : 0m,
                CostPrice = row.Quantity > 0 ? Math.Round(totalCostAmount / row.Quantity, 2) : 0m,
                TotalAmount = totalAmount
            };
        }).ToList();

        return Result.Success(PagedResponseFactory.Create(new PagedList<ProductGroupStockDto>(items, totalCount), filter.Page, filter.PageSize));
    }

    public async Task<Result<PagedResponse<ProductStockDto>>> GetProductsStockAsync(ProductStockFilter filter, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is null)
            return Result.Success(PagedResponseFactory.Create(new PagedList<ProductStockDto>([], 0), filter.Page, filter.PageSize));

        var orgId = _userContext.OrganizationId.Value;
        var baseQuery = _inventoryReadDbContext.ProductTables
            .AsNoTracking()
            .Where(x => x.OrganizationId == orgId
                        && x.StatusId == ProductTableStatusIdConst.IN_STOCK
                        && x.StateId == StateIdConst.ACTIVE
                        && (!filter.WarehouseId.HasValue || x.CurrentWarehouseId == filter.WarehouseId.Value)
                        && (!filter.ProductGroupId.HasValue || x.Product.ProductGroupId == filter.ProductGroupId.Value)
                        && (string.IsNullOrEmpty(filter.Search) || x.Product.Name.Contains(filter.Search)));

        var goodsQuery = baseQuery
            .GroupBy(x => new
            {
                x.ProductId,
                x.Product.Name,
                x.Product.Barcode,
                x.Product.Mxik,
                ProductGroupName = x.Product.ProductGroup != null ? x.Product.ProductGroup.Name : null,
                UnitId = x.Product.UnitId,
                UnitCode = x.Product.Unit.Code,
                UnitName = x.Product.Unit.Name,
                x.Product.IsService
            })
            .Select(g => new ProductStockAggregateRow
            {
                ProductId = g.Key.ProductId,
                Name = g.Key.Name,
                Barcode = g.Key.Barcode,
                Mxik = g.Key.Mxik,
                ProductGroupName = g.Key.ProductGroupName,
                UnitId = g.Key.UnitId,
                UnitCode = g.Key.UnitCode,
                UnitName = g.Key.UnitName,
                IsService = g.Key.IsService,
                Quantity = g.Count()
            })
            .OrderBy(x => x.Name);

        var totalGoods = await goodsQuery.CountAsync(ct);
        var pageSize = filter.PageSize ?? totalGoods;
        var skip = Math.Max(filter.Page - 1, 0) * pageSize;
        var goodsPage = await goodsQuery.Skip(skip).Take(pageSize).ToListAsync(ct);

        var goodsProductIds = goodsPage.Select(x => x.ProductId).Distinct().ToList();
        var priceMap = await _priceCalculateService.GetSalePriceMapAsync(goodsProductIds, ct);
        var costPriceMap = await _priceCalculateService.GetCostPriceMapAsync(goodsProductIds, ct);

        var items = goodsPage.Select(row =>
        {
            var salePrice = priceMap.GetValueOrDefault(row.ProductId)?.SalePrice ?? 0m;
            var costPrice = costPriceMap.GetValueOrDefault(row.ProductId)?.CostPrice ?? 0m;

            return new ProductStockDto
            {
                Id = row.ProductId,
                Name = row.Name,
                Barcode = row.Barcode,
                Mxik = row.Mxik,
                ProductGroupName = row.ProductGroupName,
                UnitId = row.UnitId,
                UnitCode = row.UnitCode,
                UnitName = row.UnitName,
                IsService = row.IsService,
                Quantity = row.Quantity,
                Price = salePrice,
                CostPrice = costPrice
            };
        }).ToList();

        if (!filter.WarehouseId.HasValue)
        {
            var serviceProducts = await _inventoryReadDbContext.Products
                .AsNoTracking()
                .Where(x => x.OrganizationId == orgId
                            && x.IsService
                            && x.StateId == StateIdConst.ACTIVE
                            && (!filter.ProductGroupId.HasValue || x.ProductGroupId == filter.ProductGroupId.Value)
                            && (string.IsNullOrEmpty(filter.Search) || x.Name.Contains(filter.Search)))
                .OrderBy(x => x.Name)
                .Select(x => new ProductStockDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Barcode = x.Barcode,
                    Mxik = x.Mxik,
                    ProductGroupName = x.ProductGroup != null ? x.ProductGroup.Name : null,
                    UnitId = x.UnitId,
                    UnitCode = x.Unit.Code,
                    UnitName = x.Unit.Name,
                    IsService = true,
                    Quantity = 0,
                    Price = 0,
                    CostPrice = 0
                })
                .ToListAsync(ct);

            items.AddRange(serviceProducts);
            items = items.OrderBy(x => x.Name).ToList();
        }

        return Result.Success(PagedResponseFactory.Create(new PagedList<ProductStockDto>(items, totalGoods), filter.Page, filter.PageSize));
    }

    public async Task<Result<PagedResponse<ProductTableStockDto>>> GetProductTablesStockAsync(ProductTableStockFilter filter, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is null)
            return Result.Success(PagedResponseFactory.Create(new PagedList<ProductTableStockDto>([], 0), filter.Page, filter.PageSize));

        var query = _inventoryReadDbContext.ProductTables
            .AsNoTracking()
            .Where(x => x.OrganizationId == _userContext.OrganizationId.Value
                        && x.StatusId == ProductTableStatusIdConst.IN_STOCK
                        && x.StateId == StateIdConst.ACTIVE
                        && (!filter.WarehouseId.HasValue || x.CurrentWarehouseId == filter.WarehouseId.Value)
                        && (!filter.ProductGroupId.HasValue || x.Product.ProductGroupId == filter.ProductGroupId.Value)
                        && (!filter.ProductId.HasValue || x.ProductId == filter.ProductId.Value))
            .OrderBy(x => x.Product.Name)
            .ThenBy(x => x.SerialNumber)
            .Select(x => new ProductTableStockDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductName = x.Product.Name,
                Mxik = x.Product.Mxik,
                SerialNumber = x.SerialNumber,
                MarkingNumber = x.MarkingNumber,
                CurrentWarehouseId = x.CurrentWarehouseId,
                CurrentWarehouseName = x.CurrentWarehouse != null ? x.CurrentWarehouse.Name : null
            });

        var totalCount = await query.CountAsync(ct);
        var pageSize = filter.PageSize ?? totalCount;
        var skip = Math.Max(filter.Page - 1, 0) * pageSize;
        var items = await query.Skip(skip).Take(pageSize).ToListAsync(ct);

        return Result.Success(PagedResponseFactory.Create(new PagedList<ProductTableStockDto>(items, totalCount), filter.Page, filter.PageSize));
    }

    private sealed class ProductGroupAggregateRow
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int Quantity { get; set; }
    }

    private sealed class GroupProductAggregateRow
    {
        public int GroupId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    private sealed class ProductStockAggregateRow
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
        public int Quantity { get; set; }
    }
}
