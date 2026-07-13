using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.PricingConditions;
using Application.Features.SaleDocs;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Inv.ProductPrices;

public class ProductPriceCalculateService : IProductPriceCalculateService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<ProductPrice> _productPriceQuery;
    private readonly IQueryRepository<PricingCondition> _pricingConditionQuery;
    private readonly IQueryRepository<PurchaseDocTable> _purchaseDocTableQuery;
    private readonly IQueryRepository<SaleCondition> _saleConditionQuery;
    public ProductPriceCalculateService(IUserContext userContext,
                                        IQueryBuilder queryBuilder,
                                        IQueryRepository<ProductPrice> productPriceQuery,
                                        IQueryRepository<PricingCondition> pricingConditionQuery,
                                        IQueryRepository<PurchaseDocTable> purchaseDocTableQuery,
                                        IQueryRepository<SaleCondition> saleConditionQuery)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _productPriceQuery = productPriceQuery;
        _pricingConditionQuery = pricingConditionQuery;
        _purchaseDocTableQuery = purchaseDocTableQuery;
        _saleConditionQuery = saleConditionQuery;
    }

    public async Task<Dictionary<int, ProductSalePriceDto>> GetSalePriceMapAsync(IEnumerable<int> productIds, CancellationToken ct = default)
    {
        var ids = productIds.Distinct().ToList();
        if (ids.Count == 0 || _userContext.OrganizationId is null)
            return new Dictionary<int, ProductSalePriceDto>();

        var now = DateTime.Now;
        var pricingCondition = await GetCurrentPricingConditionAsync(now, ct);
        var costingMethodId = await GetCurrentCostingMethodIdAsync(ct);
        var currentProductPrices = await GetCurrentProductPricesAsync(ids, now, ct);
        var fallbackCostPrices = await GetFallbackCostPricesAsync(ids, ct);
        var purchaseBatches = await GetPurchaseBatchesAsync(ids, ct);

        var result = new Dictionary<int, ProductSalePriceDto>(ids.Count);

        foreach (var productId in ids)
        {
            var hasFixedSalePrice = currentProductPrices.TryGetValue((productId, PriceTypeIdConst.FIXED_SALE_PRICE), out var fixedSalePrice);
            var hasAverageCostPrice = currentProductPrices.TryGetValue((productId, PriceTypeIdConst.AVERAGE_COST_PRICE), out var averageCostPrice);

            if (!hasAverageCostPrice && fallbackCostPrices.TryGetValue(productId, out var fallbackCostPrice))
            {
                averageCostPrice = fallbackCostPrice;
                hasAverageCostPrice = true;
            }

            purchaseBatches.TryGetValue(productId, out var costBatches);
            var salePrices = BuildSalePrices(
                costBatches,
                costingMethodId,
                pricingCondition,
                hasFixedSalePrice,
                fixedSalePrice,
                hasAverageCostPrice,
                averageCostPrice);

            var salePrice = salePrices.Count > 0
                ? CalculateWeightedAverageSalePrice(salePrices)
                : CalculateDefaultSalePrice(pricingCondition, hasFixedSalePrice, fixedSalePrice, hasAverageCostPrice, averageCostPrice);

            result[productId] = new ProductSalePriceDto
            {
                SalePrice = ApplyRounding(salePrice, pricingCondition),
                SalePrices = salePrices
            };
        }

        return result;
    }

    public async Task<Dictionary<int, ProductCostPriceDto>> GetCostPriceMapAsync(IEnumerable<int> productIds, CancellationToken ct = default)
    {
        var ids = productIds.Distinct().ToList();
        if (ids.Count == 0 || _userContext.OrganizationId is null)
            return new Dictionary<int, ProductCostPriceDto>();

        var costingMethodId = await GetCurrentCostingMethodIdAsync(ct);
        var now = DateTime.Now;

        return costingMethodId == CostingMethodIdConst.AVERAGE
            ? await GetAverageCostPriceDetailsMapAsync(ids, now, ct)
            : await GetStockCostPriceDetailsMapAsync(ids, costingMethodId, ct);
    }

    public async Task<Result<List<ProductTableSelectionDto>>> SelectInventoryAsync(
        int organizationId,
        int warehouseId,
        IReadOnlyCollection<ProductTableSelectionRequestDto> productLines,
        IReadOnlyCollection<int> selectedProductTableIds,
        CancellationToken ct = default)
    {
        if (productLines.Count == 0)
            return Result.Success(new List<ProductTableSelectionDto>());

        var costingMethodId = await GetCurrentCostingMethodIdAsync(organizationId, ct);
        if (costingMethodId is not CostingMethodIdConst.FIFO
            and not CostingMethodIdConst.LIFO
            and not CostingMethodIdConst.AVERAGE)
            return Result.Failure<List<ProductTableSelectionDto>>(SaleDocErrors.InvalidInventoryValuationMethod(costingMethodId.ToString(), _userContext.LanguageId));

        foreach (var productLine in productLines)
        {
            if (productLine.Quantity <= 0 ||
                (productLine.IsPieceTracked && productLine.Quantity != decimal.Truncate(productLine.Quantity)))
                return Result.Failure<List<ProductTableSelectionDto>>(SaleDocErrors.InvalidProductQuantity(productLine.LineId, productLine.Quantity, _userContext.LanguageId));
        }

        var pieceTrackedLines = productLines
            .Where(x => x.IsPieceTracked)
            .ToList();

        if (pieceTrackedLines.Count == 0)
        {
            if (selectedProductTableIds.Count > 0)
                return Result.Failure<List<ProductTableSelectionDto>>(SaleDocErrors.InvalidInventorySelection(_userContext.LanguageId));

            return Result.Success(new List<ProductTableSelectionDto>());
        }

        var productIds = pieceTrackedLines.Select(x => x.ProductId).Distinct().ToList();
        var candidates = await GetInventoryCandidatesAsync(organizationId, warehouseId, productIds, ct);

        var candidateByTableId = candidates.ToDictionary(x => x.ProductTableId);
        var candidatesByProductId = candidates
            .GroupBy(x => x.ProductId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var selectedProductTableId in selectedProductTableIds)
        {
            if (!candidateByTableId.ContainsKey(selectedProductTableId))
                return Result.Failure<List<ProductTableSelectionDto>>(SaleDocErrors.ProductTableNotAvailable(selectedProductTableId, _userContext.LanguageId));
        }

        var groupedSelections = selectedProductTableIds
            .GroupBy(productTableId => candidateByTableId[productTableId].ProductId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var result = new List<ProductTableSelectionDto>(selectedProductTableIds.Count);

        foreach (var productGroup in pieceTrackedLines.GroupBy(x => x.ProductId))
        {
            var productId = productGroup.Key;
            var lines = productGroup.ToList();
            var requiredQuantity = lines.Sum(x => x.Quantity);
            var requiredCount = (int)requiredQuantity;
            var productCandidates = candidatesByProductId.GetValueOrDefault(productId) ?? new List<InventoryCandidateSnapshot>();
            var orderedCandidates = OrderInventoryCandidates(
                productCandidates,
                costingMethodId == CostingMethodIdConst.LIFO);

            if (orderedCandidates.Count < requiredQuantity)
                return Result.Failure<List<ProductTableSelectionDto>>(SaleDocErrors.InsufficientStock(
                    productId,
                    requiredCount,
                    orderedCandidates.Count,
                    _userContext.LanguageId));

            var selectedIdsForProduct = groupedSelections.GetValueOrDefault(productId) ?? new List<int>();
            if (selectedIdsForProduct.Count != requiredQuantity || selectedIdsForProduct.Count != selectedIdsForProduct.Distinct().Count())
                return Result.Failure<List<ProductTableSelectionDto>>(SaleDocErrors.InvalidInventorySelection(_userContext.LanguageId));

            var selectedCandidatesForProduct = selectedIdsForProduct
                .Select(id => candidateByTableId[id])
                .ToList();

            var expectedBatches = orderedCandidates
                .Take(requiredCount)
                .GroupBy(x => (x.PurchaseDate, x.PurchaseDocId))
                .Select(g => new
                {
                    g.Key.PurchaseDate,
                    g.Key.PurchaseDocId,
                    Count = g.Count()
                })
                .ToList();

            if (costingMethodId != CostingMethodIdConst.AVERAGE)
            {
                var selectedBatchCounts = selectedCandidatesForProduct
                    .GroupBy(x => (x.PurchaseDate, x.PurchaseDocId))
                    .ToDictionary(
                        g => g.Key,
                        g => g.Count());

                foreach (var expectedBatch in expectedBatches)
                {
                    if (!selectedBatchCounts.TryGetValue((expectedBatch.PurchaseDate, expectedBatch.PurchaseDocId), out var selectedCount) ||
                        selectedCount != expectedBatch.Count)
                    {
                        return Result.Failure<List<ProductTableSelectionDto>>(SaleDocErrors.InvalidInventorySelection(_userContext.LanguageId));
                    }
                }
            }

            var averageCost = costingMethodId == CostingMethodIdConst.AVERAGE
                ? CalculateWeightedAverageCost(orderedCandidates)
                : 0m;

            var orderedSelectedIds = costingMethodId == CostingMethodIdConst.AVERAGE
                ? selectedIdsForProduct.OrderBy(x => x).ToList()
                : selectedIdsForProduct
                    .Where(id => orderedCandidates.Any(candidate => candidate.ProductTableId == id))
                    .OrderBy(id => orderedCandidates.FindIndex(candidate => candidate.ProductTableId == id))
                    .ToList();

            var selectedIndex = 0;

            foreach (var line in lines)
            {
                var lineQuantity = (int)line.Quantity;
                var lineSelectedIds = orderedSelectedIds.Skip(selectedIndex).Take(lineQuantity).ToList();
                selectedIndex += lineQuantity;

                if (lineSelectedIds.Count != lineQuantity)
                    return Result.Failure<List<ProductTableSelectionDto>>(SaleDocErrors.InvalidInventorySelection(_userContext.LanguageId));

                foreach (var selectedId in lineSelectedIds)
                {
                    var candidate = candidateByTableId[selectedId];
                    result.Add(new ProductTableSelectionDto
                    {
                        LineId = line.LineId,
                        ProductId = productId,
                        ProductTableId = selectedId,
                        CostPrice = costingMethodId == CostingMethodIdConst.AVERAGE
                            ? averageCost
                            : candidate.CostAmount
                    });
                }
            }
        }

        return Result.Success(result);
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

    private async Task<short> GetCurrentCostingMethodIdAsync(CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return CostingMethodIdConst.FIFO;

        return await GetCurrentCostingMethodIdAsync(_userContext.OrganizationId.Value, ct);
    }

    private async Task<short> GetCurrentCostingMethodIdAsync(int organizationId, CancellationToken ct)
    {
        var now = DateTime.Now;
        var query = _queryBuilder.For<SaleCondition>()
            .Where(x => x.OrganizationId == organizationId &&
                        x.StateId == StateIdConst.ACTIVE &&
                        x.StartDate <= now &&
                        (x.EndDate == null || x.EndDate >= now))
            .As(x => new SaleConditionCostingMethodSnapshot
            {
                Id = x.Id,
                StartDate = x.StartDate,
                CostingMethodId = x.CostingMethodId
            })
            .OrderBy(x => x.StartDate)
            .Desc()
            .Build();

        var items = await _saleConditionQuery.GetAllAsync(query, ct);
        var current = items
            .OrderByDescending(x => x.StartDate)
            .ThenByDescending(x => x.Id)
            .FirstOrDefault();

        return NormalizeCostingMethodId(current?.CostingMethodId);
    }

    private static short NormalizeCostingMethodId(short? costingMethodId)
    {
        return costingMethodId switch
        {
            CostingMethodIdConst.LIFO => CostingMethodIdConst.LIFO,
            CostingMethodIdConst.AVERAGE => CostingMethodIdConst.AVERAGE,
            _ => CostingMethodIdConst.FIFO
        };
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

    private async Task<Dictionary<int, ProductCostPriceDto>> GetAverageCostPriceDetailsMapAsync(
        IReadOnlyCollection<int> productIds,
        DateTime now,
        CancellationToken ct)
    {
        var currentProductPrices = await GetCurrentProductPricesAsync(productIds, now, ct);
        var fallbackCostPrices = await GetFallbackCostPricesAsync(productIds, ct);
        var purchaseBatches = await GetPurchaseBatchesAsync(productIds, ct);

        var result = new Dictionary<int, ProductCostPriceDto>(productIds.Count);

        foreach (var productId in productIds)
        {
            purchaseBatches.TryGetValue(productId, out var batches);
            var orderedBatches = OrderPurchaseBatches(batches, descending: true);

            var costPrice = currentProductPrices.TryGetValue((productId, PriceTypeIdConst.AVERAGE_COST_PRICE), out var averageCostPrice)
                ? averageCostPrice
                : orderedBatches.Count > 0
                    ? GetBatchUnitCost(orderedBatches[0])
                    : fallbackCostPrices.GetValueOrDefault(productId);

            result[productId] = new ProductCostPriceDto
            {
                CostPrice = costPrice,
                Purchases = orderedBatches
            };
        }

        return result;
    }

    private async Task<Dictionary<int, ProductCostPriceDto>> GetStockCostPriceDetailsMapAsync(
        IReadOnlyCollection<int> productIds,
        short costingMethodId,
        CancellationToken ct)
    {
        var fallbackCostPrices = await GetFallbackCostPricesAsync(productIds, ct);
        var purchaseBatches = await GetPurchaseBatchesAsync(productIds, ct);
        var descending = costingMethodId == CostingMethodIdConst.LIFO;

        var result = new Dictionary<int, ProductCostPriceDto>(productIds.Count);

        foreach (var productId in productIds)
        {
            purchaseBatches.TryGetValue(productId, out var batches);
            var orderedBatches = OrderPurchaseBatches(batches, descending);
            var selectedBatch = orderedBatches.FirstOrDefault();

            var costPrice = selectedBatch is not null
                ? GetBatchUnitCost(selectedBatch)
                : fallbackCostPrices.GetValueOrDefault(productId);

            result[productId] = new ProductCostPriceDto
            {
                CostPrice = costPrice,
                Purchases = orderedBatches
            };
        }

        return result;
    }

    private static List<ProductSalePriceTableDto> BuildSalePrices(
        IReadOnlyCollection<ProductCostPriceTableDto>? batches,
        short costingMethodId,
        PricingConditionDto? pricingCondition,
        bool hasFixedSalePrice,
        decimal fixedSalePrice,
        bool hasAverageCostPrice,
        decimal averageCostPrice)
    {
        if (batches is null || batches.Count == 0)
            return new List<ProductSalePriceTableDto>();

        var descending = costingMethodId != CostingMethodIdConst.FIFO;
        var orderedBatches = OrderPurchaseBatches(batches.ToList(), descending);
        var result = new List<ProductSalePriceTableDto>(orderedBatches.Count);

        foreach (var batch in orderedBatches)
        {
            var baseCostPrice = costingMethodId == CostingMethodIdConst.AVERAGE && hasAverageCostPrice
                ? averageCostPrice
                : batch.UnitPrice;

            var saleUnitPrice = CalculatePrice(
                pricingCondition,
                hasFixedSalePrice,
                fixedSalePrice,
                true,
                baseCostPrice);

            result.Add(new ProductSalePriceTableDto
            {
                PurchaseId = batch.PurchaseId,
                PurchaseDate = batch.Date,
                UnitPrice = saleUnitPrice,
                ProductTableIds = batch.ProductTableIds.ToList()
            });
        }

        return result;
    }

    private static decimal CalculateWeightedAverageSalePrice(IReadOnlyCollection<ProductSalePriceTableDto> salePrices)
    {
        var totalQuantity = salePrices.Sum(x => x.Quantity);

        if (totalQuantity <= 0)
            return 0m;

        var totalAmount = salePrices.Sum(x => x.UnitPrice * x.Quantity);
        return Math.Round(totalAmount / totalQuantity, 8);
    }

    private static decimal CalculateDefaultSalePrice(
        PricingConditionDto? pricingCondition,
        bool hasFixedSalePrice,
        decimal fixedSalePrice,
        bool hasAverageCostPrice,
        decimal averageCostPrice)
    {
        return CalculatePrice(
            pricingCondition,
            hasFixedSalePrice,
            fixedSalePrice,
            hasAverageCostPrice,
            averageCostPrice);
    }

    private async Task<Dictionary<int, decimal>> GetFallbackCostPricesAsync(
        IReadOnlyCollection<int> productIds,
        CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return new Dictionary<int, decimal>();

        var query = _queryBuilder.For<PurchaseDocTable>()
            .Where(x => productIds.Contains(x.Owner.ProductId) &&
                        x.Owner.Owner.OrganizationId == _userContext.OrganizationId.Value)
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

    private async Task<Dictionary<int, List<ProductCostPriceTableDto>>> GetPurchaseBatchesAsync(
        IReadOnlyCollection<int> productIds,
        CancellationToken ct)
    {
        if (_userContext.OrganizationId is null || productIds.Count == 0)
            return new Dictionary<int, List<ProductCostPriceTableDto>>();

        var query = _queryBuilder.For<PurchaseDocTable>()
            .Where(x => productIds.Contains(x.Owner.ProductId) &&
                        x.ProductTable.OrganizationId == _userContext.OrganizationId.Value &&
                        x.ProductTable.StatusId == ProductTableStatusIdConst.IN_STOCK &&
                        x.ProductTable.StateId == StateIdConst.ACTIVE)
            .As(x => new PurchaseBatchSnapshot
            {
                ProductId = x.Owner.ProductId,
                PurchaseId = x.Owner.OwnerId,
                DocNumber = x.Owner.Owner.DocNumber,
                DocDate = x.Owner.Owner.DocDate,
                ProductTableId = x.ProductTableId,
                TotalAmount = x.TotalAmount
            })
            .Build();

        var items = await _purchaseDocTableQuery.GetAllAsync(query, ct);

        return items
            .GroupBy(x => x.ProductId)
            .ToDictionary(
                g => g.Key,
                g => g.GroupBy(x => new { x.PurchaseId, x.DocNumber, x.DocDate })
                    .Select(batch => new ProductCostPriceTableDto
                    {
                        Date = batch.Key.DocDate,
                        DocNumber = batch.Key.DocNumber,
                        PurchaseId = batch.Key.PurchaseId,
                        UnitPrice = batch.Count() > 0
                            ? Math.Round(batch.Sum(x => x.TotalAmount) / batch.Count(), 8)
                            : 0m,
                        ProductTableIds = batch.Select(x => x.ProductTableId).Distinct().OrderBy(id => id).ToList()
                    })
                    .ToList());
    }

    private async Task<List<InventoryCandidateSnapshot>> GetInventoryCandidatesAsync(
        int organizationId,
        int warehouseId,
        IReadOnlyCollection<int> productIds,
        CancellationToken ct)
    {
        if (productIds.Count == 0)
            return new List<InventoryCandidateSnapshot>();

        var query = _queryBuilder.For<PurchaseDocTable>()
            .Where(x => productIds.Contains(x.ProductTable.ProductId) &&
                        x.ProductTable.OrganizationId == organizationId &&
                        x.ProductTable.StatusId == ProductTableStatusIdConst.IN_STOCK &&
                        x.ProductTable.StateId == StateIdConst.ACTIVE &&
                        x.ProductTable.CurrentWarehouseId == warehouseId &&
                        x.Owner.Owner.OrganizationId == organizationId)
            .As(x => new InventoryCandidateSnapshot
            {
                ProductTableId = x.ProductTableId,
                ProductId = x.ProductTable.ProductId,
                PurchaseDocId = x.Owner.OwnerId,
                PurchaseDate = x.Owner.Owner.DocDate,
                CostAmount = x.TotalAmount
            })
            .Build();

        return await _purchaseDocTableQuery.GetAllAsync(query, ct);
    }

    private static List<InventoryCandidateSnapshot> OrderInventoryCandidates(List<InventoryCandidateSnapshot>? candidates, bool descending)
    {
        if (candidates is null || candidates.Count == 0)
            return new List<InventoryCandidateSnapshot>();

        return descending
            ? candidates.OrderByDescending(x => x.PurchaseDate).ThenByDescending(x => x.PurchaseDocId).ThenByDescending(x => x.ProductTableId).ToList()
            : candidates.OrderBy(x => x.PurchaseDate).ThenBy(x => x.PurchaseDocId).ThenBy(x => x.ProductTableId).ToList();
    }

    private static decimal CalculateWeightedAverageCost(List<InventoryCandidateSnapshot> candidates)
    {
        var quantity = candidates.Count;
        if (quantity <= 0)
            return 0m;

        return Math.Round(candidates.Sum(x => x.CostAmount) / quantity, 8);
    }

    private static List<ProductCostPriceTableDto> OrderPurchaseBatches(List<ProductCostPriceTableDto>? batches, bool descending)
    {
        if (batches is null || batches.Count == 0)
            return new List<ProductCostPriceTableDto>();

        return descending
            ? batches.OrderByDescending(x => x.Date).ThenByDescending(x => x.PurchaseId).ToList()
            : batches.OrderBy(x => x.Date).ThenBy(x => x.PurchaseId).ToList();
    }

    private static decimal GetBatchUnitCost(ProductCostPriceTableDto batch) =>
        batch.UnitPrice;

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

    private sealed class PurchaseBatchSnapshot
    {
        public int ProductId { get; set; }
        public long PurchaseId { get; set; }
        public string DocNumber { get; set; } = null!;
        public DateTime DocDate { get; set; }
        public int ProductTableId { get; set; }
        public decimal TotalAmount { get; set; }
    }

    private sealed class InventoryCandidateSnapshot
    {
        public int ProductTableId { get; set; }
        public int ProductId { get; set; }
        public long PurchaseDocId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal CostAmount { get; set; }
    }

    private sealed class SaleConditionCostingMethodSnapshot
    {
        public long Id { get; set; }
        public DateTime StartDate { get; set; }
        public short CostingMethodId { get; set; }
    }
}
