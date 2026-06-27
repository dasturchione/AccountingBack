using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.PricingConditions;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;

namespace Application.Features.ProductPrices;

public class ProductSalePriceService : IProductSalePriceService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<PricingCondition> _pricingConditionQuery;
    private readonly IQueryRepository<ProductPrice> _productPriceQuery;
    private readonly IQueryRepository<PurchaseDocTable> _purchaseDocTableQuery;

    public ProductSalePriceService(IUserContext userContext,
                                   IQueryBuilder queryBuilder,
                                   IQueryRepository<PricingCondition> pricingConditionQuery,
                                   IQueryRepository<ProductPrice> productPriceQuery,
                                   IQueryRepository<PurchaseDocTable> purchaseDocTableQuery)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _pricingConditionQuery = pricingConditionQuery;
        _productPriceQuery = productPriceQuery;
        _purchaseDocTableQuery = purchaseDocTableQuery;
    }

    public async Task<Dictionary<int, decimal>> GetSalePriceMapAsync(IEnumerable<int> productIds, CancellationToken ct = default)
    {
        var ids = productIds.Distinct().ToList();
        if (ids.Count == 0 || _userContext.OrganizationId is null)
            return new Dictionary<int, decimal>();

        var now = DateTime.Now;
        var pricingCondition = await GetCurrentPricingConditionAsync(now, ct);
        var currentProductPrices = await GetCurrentProductPricesAsync(ids, now, ct);
        var fallbackCostPrices = await GetFallbackCostPricesAsync(ids, ct);

        var result = new Dictionary<int, decimal>(ids.Count);

        foreach (var productId in ids)
        {
            var hasFixedSalePrice = currentProductPrices.TryGetValue((productId, PriceTypeIdConst.FIXED_SALE_PRICE), out var fixedSalePrice);
            var hasAverageCostPrice = currentProductPrices.TryGetValue((productId, PriceTypeIdConst.AVERAGE_COST_PRICE), out var averageCostPrice);

            if (!hasAverageCostPrice && fallbackCostPrices.TryGetValue(productId, out var purchaseCostPrice))
            {
                averageCostPrice = purchaseCostPrice;
                hasAverageCostPrice = true;
            }

            var price = CalculatePrice(pricingCondition, hasFixedSalePrice, fixedSalePrice, hasAverageCostPrice, averageCostPrice);
            result[productId] = ApplyRounding(price, pricingCondition);
        }

        return result;
    }

    private async Task<PricingConditionDto?> GetCurrentPricingConditionAsync(DateTime now, CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return null;

        var query = _queryBuilder.For<PricingCondition>()
            .Where(x => x.OrganizationId == _userContext.OrganizationId.Value &&
                        x.StateId == StateIdConst.ACTIVE &&
                        x.StartDate <= now &&
                        (x.EndDate == null || x.EndDate >= now))
            .As<PricingConditionDto>()
            .Build();

        var items = await _pricingConditionQuery.GetAllAsync(query, ct);
        return items
            .OrderByDescending(x => x.StartDate)
            .ThenByDescending(x => x.Id)
            .FirstOrDefault();
    }

    private async Task<Dictionary<(int ProductId, short PriceTypeId), decimal>> GetCurrentProductPricesAsync(
        IReadOnlyCollection<int> productIds,
        DateTime now,
        CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return new Dictionary<(int ProductId, short PriceTypeId), decimal>();

        var query = _queryBuilder.For<ProductPrice>()
                .Where(x => x.OrganizationId == _userContext.OrganizationId.Value &&
                        productIds.Contains(x.ProductId) &&
                        x.StateId == StateIdConst.ACTIVE &&
                        x.StartDate <= now &&
                        (x.EndDate == null || x.EndDate >= now) &&
                        (x.PriceTypeId == PriceTypeIdConst.AVERAGE_COST_PRICE || x.PriceTypeId == PriceTypeIdConst.FIXED_SALE_PRICE))
            .As(x => new ProductPriceSnapshot
            {
                Id = x.Id,
                ProductId = x.ProductId,
                PriceTypeId = x.PriceTypeId,
                Price = x.Price,
                StartDate = x.StartDate
            })
            .Build();

        var items = await _productPriceQuery.GetAllAsync(query, ct);

        return items
            .GroupBy(x => (x.ProductId, x.PriceTypeId))
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.StartDate).ThenByDescending(x => x.Id).First().Price);
    }

    private async Task<Dictionary<int, decimal>> GetFallbackCostPricesAsync(
        IReadOnlyCollection<int> productIds,
        CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return new Dictionary<int, decimal>();

        var query = _queryBuilder.For<PurchaseDocTable>()
            .Where(x => productIds.Contains(x.Owner.ProductId))
            .As(x => new PurchaseCostSnapshot
            {
                Id = x.Id,
                ProductId = x.Owner.ProductId,
                DocDate = x.Owner.Owner.DocDate,
                CostPrice = x.TotalAmount
            })
            .Build();

        var items = await _purchaseDocTableQuery.GetAllAsync(query, ct);

        return items
            .GroupBy(x => x.ProductId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.DocDate).ThenByDescending(x => x.Id).First().CostPrice);
    }

    private static decimal CalculatePrice(
        PricingConditionDto? pricingCondition,
        bool hasFixedSalePrice,
        decimal fixedSalePrice,
        bool hasAverageCostPrice,
        decimal averageCostPrice)
    {
        if (pricingCondition is null)
            return hasFixedSalePrice
                ? fixedSalePrice
                : hasAverageCostPrice
                    ? averageCostPrice
                    : 0m;

        return pricingCondition.PricingMethodId switch
        {
            PricingMethodIdConst.FIXED_PRICE => hasFixedSalePrice
                ? fixedSalePrice
                : hasAverageCostPrice
                    ? averageCostPrice
                    : 0m,

            PricingMethodIdConst.COST_PLUS_PERCENT => hasAverageCostPrice
                ? Math.Round(averageCostPrice * (1 + pricingCondition.PricingValue / 100m), 8)
                : hasFixedSalePrice
                    ? fixedSalePrice
                    : 0m,

            PricingMethodIdConst.COST_PLUS_AMOUNT => hasAverageCostPrice
                ? Math.Round(averageCostPrice + pricingCondition.PricingValue, 8)
                : hasFixedSalePrice
                    ? fixedSalePrice
                    : 0m,

            _ => hasFixedSalePrice
                ? fixedSalePrice
                : hasAverageCostPrice
                    ? averageCostPrice
                    : 0m
        };
    }

    private static decimal ApplyRounding(decimal price, PricingConditionDto? pricingCondition)
    {
        if (pricingCondition is null)
            return price;

        var step = pricingCondition.RoundingPrecision > 0 ? pricingCondition.RoundingPrecision : 1m;

        return pricingCondition.RoundingMethodId switch
        {
            PriceRoundingMethodIdConst.NONE => price,
            PriceRoundingMethodIdConst.UP => Math.Ceiling(price / step) * step,
            PriceRoundingMethodIdConst.DOWN => Math.Floor(price / step) * step,
            PriceRoundingMethodIdConst.NEAREST => Math.Round(price / step, 0, MidpointRounding.AwayFromZero) * step,
            _ => price
        };
    }

    private sealed class ProductPriceSnapshot
    {
        public long Id { get; set; }
        public int ProductId { get; set; }
        public short PriceTypeId { get; set; }
        public decimal Price { get; set; }
        public DateTime StartDate { get; set; }
    }

    private sealed class PurchaseCostSnapshot
    {
        public long Id { get; set; }
        public int ProductId { get; set; }
        public DateTime DocDate { get; set; }
        public decimal CostPrice { get; set; }
    }
}
