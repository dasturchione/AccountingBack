using Application.Abstractions;
using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.ProductTables;

public class ProductTableService : IProductTableService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<ProductTable> _query;
    private readonly IQueryRepository<ProductPrice> _priceQuery;

    public ProductTableService(IUserContext userContext,
                               IQueryBuilder queryBuilder,
                               IQueryRepository<ProductTable> query,
                               IQueryRepository<ProductPrice> priceQuery)
    {
        _userContext   = userContext;
        _queryBuilder  = queryBuilder;
        _query         = query;
        _priceQuery    = priceQuery;
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
                ProductTableErrors.NotFoundByMarkingNumber(markingNumber, _userContext.LanguageId));

        if (entity.StatusId != ProductTableStatusIdConst.IN_STOCK)
            return Result.Failure<ProductTableByMarkingDto>(
                ProductTableErrors.NotAvailableByMarkingNumber(markingNumber, _userContext.LanguageId));

        return new ProductTableByMarkingDto
        {
            ProductTableId = entity.Id,
            ProductId      = entity.ProductId,
            ProductName    = entity.Product.Name,
            MarkingNumber  = entity.MarkingNumber,
            SerialNumber   = entity.SerialNumber,
        };
    }

    public async Task<Result<List<ProductTableGroupSummaryDto>>> GetProductGroupSummaryAsync(CancellationToken ct = default)
    {
        var inStockEntities = await GetInStockEntitiesAsync(ct);
        var priceMap = await GetPriceMapAsync(inStockEntities, ct);

        var result = inStockEntities
            .Where(x => x.Product.ProductGroup != null)
            .GroupBy(x => new { x.Product.ProductGroupId, GroupName = x.Product.ProductGroup!.Name })
            .Select(g =>
            {
                var qty = g.Count();
                var totalAmount = g.Sum(x => priceMap.GetValueOrDefault(x.ProductId));
                return new ProductTableGroupSummaryDto
                {
                    Id       = g.Key.ProductGroupId ?? 0,
                    Name     = g.Key.GroupName,
                    Quantity = qty,
                    Price    = qty > 0 ? Math.Round(totalAmount / qty, 2) : 0,
                    TotalAmount = totalAmount,
                };
            })
            .OrderBy(x => x.Name)
            .ToList();

        return Result.Success(result);
    }

    public async Task<Result<List<ProductTableProductSummaryDto>>> GetProductSummaryAsync(ProductTableGroupFilter filter, CancellationToken ct = default)
    {
        var inStockEntities = await GetInStockEntitiesAsync(ct);
        var priceMap = await GetPriceMapAsync(inStockEntities, ct);

        if (filter.ProductGroupId.HasValue)
            inStockEntities = inStockEntities.Where(x => x.Product.ProductGroupId == filter.ProductGroupId.Value).ToList();

        if (!string.IsNullOrEmpty(filter.Search))
            inStockEntities = inStockEntities
                .Where(x => x.Product.Name.Contains(filter.Search, StringComparison.OrdinalIgnoreCase))
                .ToList();

        var result = inStockEntities
            .GroupBy(x => new { x.ProductId, x.Product.Name, x.Product.Barcode, x.Product.ProductGroup, x.Product.Unit })
            .Select(g =>
            {
                var price = priceMap.GetValueOrDefault(g.Key.ProductId);
                return new ProductTableProductSummaryDto
                {
                    Id               = g.Key.ProductId,
                    Name             = g.Key.Name,
                    Barcode          = g.Key.Barcode,
                    ProductGroupName = g.Key.ProductGroup?.Name,
                    UnitName         = g.Key.Unit.Name,
                    Quantity         = g.Count(),
                    Price            = price,
                };
            })
            .OrderBy(x => x.Name)
            .ToList();

        return Result.Success(result);
    }

    public async Task<Result<List<ProductTableItemDto>>> GetProductTableSummaryAsync(int? productGroupId, int? productId, CancellationToken ct = default)
    {
        var inStockEntities = await GetInStockEntitiesAsync(ct);

        if (productGroupId.HasValue)
            inStockEntities = inStockEntities.Where(x => x.Product.ProductGroupId == productGroupId.Value).ToList();

        if (productId.HasValue)
            inStockEntities = inStockEntities.Where(x => x.ProductId == productId.Value).ToList();

        var result = inStockEntities
            .Select(x => new ProductTableItemDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductName = x.Product.Name,
                SerialNumber = x.SerialNumber,
                MarkingNumber = x.MarkingNumber,
            })
            .OrderBy(x => x.ProductName)
            .ThenBy(x => x.SerialNumber)
            .ToList();

        return Result.Success(result);
    }

    private async Task<List<ProductTable>> GetInStockEntitiesAsync(CancellationToken ct)
    {
        var query = _queryBuilder.For<ProductTable>()
            .Where(x => x.StatusId == ProductTableStatusIdConst.IN_STOCK && x.StateId == StateIdConst.ACTIVE)
            .Build();

        query.AddIncludes(b => b.Include(x => x.Product).ThenInclude(p => p.ProductGroup));
        query.AddIncludes(b => b.Include(x => x.Product).ThenInclude(p => p.Unit));

        return await _query.GetAllAsync(query, ct);
    }

    private async Task<Dictionary<int, decimal>> GetPriceMapAsync(List<ProductTable> entities, CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return new Dictionary<int, decimal>();

        var orgId = _userContext.OrganizationId.Value;
        var now = DateTime.Now;
        var productIds = entities.Select(x => x.ProductId).Distinct().ToList();

        var priceQuery = _queryBuilder.For<ProductPrice>()
            .Where(x => productIds.Contains(x.ProductId)
                     && x.OrganizationId == orgId
                     && x.StateId == StateIdConst.ACTIVE
                     && x.StartDate <= now
                     && (x.EndDate == null || x.EndDate >= now))
            .Build();

        var prices = await _priceQuery.GetAllAsync(priceQuery, ct);
        return prices.GroupBy(p => p.ProductId)
                     .ToDictionary(g => g.Key, g => g.First().Price);
    }
}
