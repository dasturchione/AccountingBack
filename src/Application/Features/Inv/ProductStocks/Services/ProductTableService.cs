using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.ProductPrices;
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
    private readonly IQueryRepository<PurchaseDocTable> _purchaseDocTableQuery;
    private readonly IProductSalePriceService _productSalePriceService;
    public ProductStockService(IUserContext userContext,
                               IQueryBuilder queryBuilder,
                               IQueryRepository<ProductTable> query,
                               IQueryRepository<PurchaseDocTable> purchaseDocTableQuery,
                               IProductSalePriceService productSalePriceService)
    {
        _userContext           = userContext;
        _queryBuilder          = queryBuilder;
        _query                 = query;
        _purchaseDocTableQuery = purchaseDocTableQuery;
        _productSalePriceService = productSalePriceService;
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
                var totalCostAmount = g.Sum(x => costPriceMap.GetValueOrDefault(x.Id));
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

        if (filter.ProductGroupId.HasValue)
            inStockEntities = inStockEntities.Where(x => x.Product.ProductGroupId == filter.ProductGroupId.Value).ToList();

        if (!string.IsNullOrEmpty(filter.Search))
            inStockEntities = inStockEntities
                .Where(x => x.Product.Name.Contains(filter.Search, StringComparison.OrdinalIgnoreCase))
                .ToList();

        var items = inStockEntities
            .GroupBy(x => new { x.ProductId, x.Product.Name, x.Product.Barcode, x.Product.Mxik, x.Product.ProductGroup, x.Product.Unit })
            .Select(g => 
            {
                var price = priceMap.GetValueOrDefault(g.Key.ProductId);
                var qty = g.Count();
                var totalCostAmount = g.Sum(x => costPriceMap.GetValueOrDefault(x.Id));
                return new ProductStockDto
                {
                    Id               = g.Key.ProductId,
                    Name             = g.Key.Name,
                    Barcode          = g.Key.Barcode,
                    Mxik             = g.Key.Mxik,
                    ProductGroupName = g.Key.ProductGroup?.Name,
                    UnitName         = g.Key.Unit.Name,
                    Quantity         = qty,
                    Price            = price,
                    CostPrice        = qty > 0 ? Math.Round(totalCostAmount / qty, 2) : 0,
                };
            })
            .OrderBy(x => x.Name)
            .ToList();

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

    public async Task<Result<List<ProductStockPurchaseDto>>> GetPurchasesByProductIdAsync(int productId, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is null)
            return Result.Success(new List<ProductStockPurchaseDto>());

        var orgId = _userContext.OrganizationId.Value;

        var query = _queryBuilder.For<ProductTable>()
            .Where(x => x.OrganizationId == orgId &&
                        x.ProductId == productId &&
                        x.StatusId == ProductTableStatusIdConst.IN_STOCK &&
                        x.StateId == StateIdConst.ACTIVE)
            .Build();

        var productTables = await _query.GetAllAsync(query, ct);

        if (productTables.Count == 0)
            return Result.Success(new List<ProductStockPurchaseDto>());

        var productTableIds = productTables.Select(x => x.Id).ToList();

        var purchaseQuery = _queryBuilder.For<PurchaseDocTable>()
            .Where(x => productTableIds.Contains(x.ProductTableId))
            .As(x => new
            {
                x.ProductTableId,
                x.TotalAmount,
                PurchaseId = x.Owner.Owner.Id,
                DocNumber = x.Owner.Owner.DocNumber,
                DocDate = x.Owner.Owner.DocDate,
            })
            .Build();

        var purchases = await _purchaseDocTableQuery.GetAllAsync(purchaseQuery, ct);

        var result = purchases
            .GroupBy(x => new { x.PurchaseId, x.DocNumber, x.DocDate, x.TotalAmount })
            .Select(g => new ProductStockPurchaseDto
            {
                PurchaseId = g.Key.PurchaseId,
                DocNumber = g.Key.DocNumber,
                Date = g.Key.DocDate,
                Qty = g.Count(),
                TotalAmount = g.Key.TotalAmount,
                ProductTableIds = g.Select(x => x.ProductTableId).ToList(),
            })
            .OrderByDescending(x => x.Date)
            .ToList();

        return Result.Success(result);
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

        query.AddIncludes(b => b.Include(x => x.Product).ThenInclude(p => p.ProductGroup));
        query.AddIncludes(b => b.Include(x => x.Product).ThenInclude(p => p.Unit));

        return await _query.GetAllAsync(query, ct);
    }

    private async Task<Dictionary<int, decimal>> GetPriceMapAsync(List<ProductTable> entities, CancellationToken ct)
    {
        var productIds = entities.Select(x => x.ProductId).Distinct().ToList();
        return await _productSalePriceService.GetSalePriceMapAsync(productIds, ct);
    }

    private async Task<Dictionary<int, decimal>> GetCostPriceMapAsync(List<ProductTable> entities, CancellationToken ct)
    {
        var productTableIds = entities.Select(x => x.Id).Distinct().ToList();

        if (productTableIds.Count == 0)
            return new Dictionary<int, decimal>();

        var purchaseQuery = _queryBuilder.For<PurchaseDocTable>()
            .Where(x => productTableIds.Contains(x.ProductTableId))
            .As(x => new
            {
                ProductTableId = x.ProductTableId,
                DocDate = x.Owner.Owner.DocDate,
                CostPrice = x.TotalAmount
            })
            .Build();

        var purchases = await _purchaseDocTableQuery.GetAllAsync(purchaseQuery, ct);

        return purchases
            .GroupBy(x => x.ProductTableId)
            .ToDictionary(
                x => x.Key,
                x => x.OrderByDescending(p => p.DocDate).First().CostPrice);
    }
}
