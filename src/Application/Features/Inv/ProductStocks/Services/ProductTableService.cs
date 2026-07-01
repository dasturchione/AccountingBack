using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.Inv.ProductPrices;
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
    private readonly IQueryRepository<ProductTable> _query;
    private readonly IQueryRepository<Product> _productQuery;
    private readonly IQueryRepository<PurchaseDocTable> _purchaseDocTableQuery;
    private readonly IProductPriceCalculateService _priceCalculateService;
    public ProductStockService(IUserContext userContext,
                               IQueryBuilder queryBuilder,
                               IQueryRepository<ProductTable> query,
                               IQueryRepository<Product> productQuery,
                               IQueryRepository<PurchaseDocTable> purchaseDocTableQuery,
                               IProductPriceCalculateService priceCalculateService)
    {
        _query                  = query;
        _productQuery           = productQuery;
        _userContext            = userContext;
        _queryBuilder           = queryBuilder;
        _priceCalculateService  = priceCalculateService;
        _purchaseDocTableQuery  = purchaseDocTableQuery;
    }

    public async Task<Result<ProductTableByMarkingDto>> GetByMarkingNumberAsync(string markingNumber, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<ProductTable>()
            .Where(x => x.MarkingNumber == markingNumber && x.StateId == StateIdConst.ACTIVE)
            .Build();

        query.AddIncludes(b => b.Include(x => x.Product));

        var entity = await _query.GetAsync(query, ct);

        if (entity is null)
            return Result.Failure<ProductTableByMarkingDto>(
                ProductStockErrors.NotFoundByMarkingNumber(markingNumber, _userContext.LanguageId));

        if (entity.StatusId != ProductTableStatusIdConst.IN_STOCK)
            return Result.Failure<ProductTableByMarkingDto>(
                ProductStockErrors.NotAvailableByMarkingNumber(markingNumber, _userContext.LanguageId));

        return new ProductTableByMarkingDto
        {
            ProductTableId = entity.Id,
            ProductId      = entity.ProductId,
            ProductName    = entity.Product.Name,
            Mxik           = entity.Product.Mxik,
            MarkingNumber  = entity.MarkingNumber,
            SerialNumber   = entity.SerialNumber,
        };
    }

    public async Task<Result<PagedResponse<ProductGroupStockDto>>> GetProductGroupsStockAsync(ProductGroupStockFilter filter, CancellationToken ct = default)
    {
        var inStockEntities = await GetInStockEntitiesAsync(ct);
        var priceMap = await GetPriceMapAsync(inStockEntities, ct);
        var costPriceMap = await GetCostPriceMapAsync(inStockEntities, ct);

        var items = inStockEntities
            .Where(x => x.Product.ProductGroup != null)
            .GroupBy(x => new { x.Product.ProductGroupId, GroupName = x.Product.ProductGroup!.Name })
            .Select(g =>
            {
                var qty = g.Count();
                var totalAmount = g.Sum(x => priceMap.GetValueOrDefault(x.ProductId));
                var totalCostAmount = g.Sum(x => costPriceMap.GetValueOrDefault(x.ProductId)?.CostPrice ?? 0m);
                return new ProductGroupStockDto
                {
                    Id       = g.Key.ProductGroupId ?? 0,
                    Name     = g.Key.GroupName,
                    Quantity = qty,
                    Price    = qty > 0 ? Math.Round(totalAmount / qty, 2) : 0,
                    CostPrice = qty > 0 ? Math.Round(totalCostAmount / qty, 2) : 0,
                    TotalAmount = totalAmount,
                };
            })
            .OrderBy(x => x.Name)
            .ToList();

        var pagedList = new PagedList<ProductGroupStockDto>(items, items.Count);

        return Result.Success(PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize));
    }

    public async Task<Result<PagedResponse<ProductStockDto>>> GetProductsStockAsync(ProductStockFilter filter, CancellationToken ct = default)
    {
        var inStockEntities = await GetInStockEntitiesAsync(ct);
        var priceMap = await GetPriceMapAsync(inStockEntities, ct);
        var costPriceMap = await GetCostPriceMapAsync(inStockEntities, ct);
        var serviceProducts = await GetServiceProductsAsync(filter, ct);

        if (filter.ProductGroupId.HasValue)
            inStockEntities = inStockEntities.Where(x => x.Product.ProductGroupId == filter.ProductGroupId.Value).ToList();

        if (!string.IsNullOrEmpty(filter.Search))
            inStockEntities = inStockEntities
                .Where(x => x.Product.Name.Contains(filter.Search, StringComparison.OrdinalIgnoreCase))
                .ToList();

        var items = inStockEntities
            .GroupBy(x => new { x.ProductId, x.Product.Name, x.Product.Barcode, x.Product.Mxik, x.Product.ProductGroup, x.Product.Unit, x.Product.Unit.Code, x.Product.UnitId, x.Product.IsService })
            .Select(g => 
            {
                var price = priceMap.GetValueOrDefault(g.Key.ProductId);
                var qty = g.Count();
                var totalCostAmount = (costPriceMap.GetValueOrDefault(g.Key.ProductId)?.CostPrice ?? 0m) * qty;
                return new ProductStockDto
                {
                    Id               = g.Key.ProductId,
                    Name             = g.Key.Name,
                    Barcode          = g.Key.Barcode,
                    Mxik             = g.Key.Mxik,
                    ProductGroupName = g.Key.ProductGroup?.Name,
                    UnitName         = g.Key.Unit.Name,
                    UnitCode         = g.Key.Unit.Code,
                    UnitId           = g.Key.UnitId,
                    IsService        = g.Key.IsService,
                    Quantity         = qty,
                    Price            = price,
                    CostPrice        = qty > 0 ? Math.Round(totalCostAmount / qty, 2) : 0,
                };
            })
            .OrderBy(x => x.Name)
            .ToList();

        items.AddRange(serviceProducts);
        items = items.OrderBy(x => x.Name).ToList();

        var pagedList = new PagedList<ProductStockDto>(items, items.Count);

        return Result.Success(PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize));
    }

    public async Task<Result<PagedResponse<ProductTableStockDto>>> GetProductTablesStockAsync(ProductTableStockFilter filter, CancellationToken ct = default)
    {
        var inStockEntities = await GetInStockEntitiesAsync(ct);

        if (filter.ProductGroupId.HasValue)
            inStockEntities = inStockEntities.Where(x => x.Product.ProductGroupId == filter.ProductGroupId.Value).ToList();

        if (filter.ProductId.HasValue)
            inStockEntities = inStockEntities.Where(x => x.ProductId == filter.ProductId.Value).ToList();

        var items = inStockEntities
            .Select(x => new ProductTableStockDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductName = x.Product.Name,
                Mxik = x.Product.Mxik,
                SerialNumber = x.SerialNumber,
                MarkingNumber = x.MarkingNumber,
            })
            .OrderBy(x => x.ProductName)
            .ThenBy(x => x.SerialNumber)
            .ToList();

        var pagedList = new PagedList<ProductTableStockDto>(items, items.Count);

        return Result.Success(PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize));
    }

    private async Task<List<ProductTable>> GetInStockEntitiesAsync(CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return new List<ProductTable>();

        var orgId = _userContext.OrganizationId.Value;

        var query = _queryBuilder.For<ProductTable>()
            .Where(x => x.OrganizationId == orgId &&
                        x.StatusId == ProductTableStatusIdConst.IN_STOCK &&
                        x.StateId == StateIdConst.ACTIVE)
            .Build();

        query.AddIncludes(b => 
        {
            b.Include(x => x.Product).ThenInclude(p => p.ProductGroup);
            b.Include(x => x.Product).ThenInclude(p => p.Unit);
        });

        return await _query.GetAllAsync(query, ct);
    }

    private async Task<List<ProductStockDto>> GetServiceProductsAsync(ProductStockFilter filter, CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return new List<ProductStockDto>();

        var orgId = _userContext.OrganizationId.Value;
        var query = _queryBuilder.For<Product>()
            .Where(x => x.OrganizationId == orgId &&
                        x.IsService &&
                        x.StateId == StateIdConst.ACTIVE &&
                        (!filter.ProductGroupId.HasValue || x.ProductGroupId == filter.ProductGroupId.Value) &&
                        (string.IsNullOrEmpty(filter.Search) || x.Name.Contains(filter.Search)))
            .Build();

        query.AddIncludes(b =>
        {
            b.Include(x => x.ProductGroup);
            b.Include(x => x.Unit);
        });

        var products = await _productQuery.GetAllAsync(query, ct);

        return products.Select(product => new ProductStockDto
        {
            Id = product.Id,
            Name = product.Name,
            Barcode = product.Barcode,
            Mxik = product.Mxik,
            ProductGroupName = product.ProductGroup?.Name,
            UnitId = product.UnitId,
            UnitCode = product.Unit.Code,
            UnitName = product.Unit.Name,
            IsService = true,
            Quantity = 0,
            Price = 0,
            CostPrice = 0,
        }).ToList();
    }

    private async Task<Dictionary<int, decimal>> GetPriceMapAsync(List<ProductTable> entities, CancellationToken ct)
    {
        var productIds = entities.Select(x => x.ProductId).Distinct().ToList();
        var salePriceMap = await _priceCalculateService.GetSalePriceMapAsync(productIds, ct);

        return salePriceMap.ToDictionary(x => x.Key, x => x.Value.SalePrice);
    }

    private async Task<Dictionary<int, ProductCostPriceDto>> GetCostPriceMapAsync(List<ProductTable> entities, CancellationToken ct)
    {
        var productIds = entities.Select(x => x.ProductId).Distinct().ToList();

        if (productIds.Count == 0)
            return new Dictionary<int, ProductCostPriceDto>();

        return await _priceCalculateService.GetCostPriceMapAsync(productIds, ct);
    }
}
