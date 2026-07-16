using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.Warehouses;
using Domain.Entities;
using SharedKernel.Results;
using SharedKernel.Query;

namespace Application.Features.Inv.WarehouseProducts;

public sealed class WarehouseInventoryService : IWarehouseInventoryService
{
    private const string ProductTableName = "inv_product";
    private const string ProductGroupTableName = "inv_product_group";
    private const string UnitTableName = "cmn_unit";
    private const string NameColumn = "name";

    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<Warehouse> _warehouseQuery;
    private readonly IQueryRepository<WarehouseProduct> _warehouseProductQuery;
    private readonly IQueryRepository<WarehouseProductBatch> _warehouseProductBatchQuery;
    private readonly IQueryRepository<Translation> _translationQuery;

    public WarehouseInventoryService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IQueryRepository<Warehouse> warehouseQuery,
        IQueryRepository<WarehouseProduct> warehouseProductQuery,
        IQueryRepository<WarehouseProductBatch> warehouseProductBatchQuery,
        IQueryRepository<Translation> translationQuery)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _warehouseQuery = warehouseQuery;
        _warehouseProductQuery = warehouseProductQuery;
        _warehouseProductBatchQuery = warehouseProductBatchQuery;
        _translationQuery = translationQuery;
    }

    public async Task<Result<IReadOnlyList<WarehouseProductDto>>> GetWarehouseProductsAsync(WarehouseProductFilter filter, CancellationToken cancellationToken = default)
    {
        if (_userContext.OrganizationId is null)
            return Result.Failure<IReadOnlyList<WarehouseProductDto>>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var organizationId = _userContext.OrganizationId.Value;

        var requestedProductIds = NormalizeProductIds(filter.ProductIds);
        if (filter.ProductIds is not null && requestedProductIds.Count == 0)
            return Result.Success<IReadOnlyList<WarehouseProductDto>>(Array.Empty<WarehouseProductDto>());

        var productRows = await GetProductRowsAsync(organizationId, filter.WarehouseId, filter.ProductGroupId, requestedProductIds, cancellationToken);
        if (productRows.Count == 0)
            return Result.Success<IReadOnlyList<WarehouseProductDto>>(Array.Empty<WarehouseProductDto>());

        var productIds = productRows.Select(row => row.ProductId).Distinct().ToList();
        var batches = await GetBatchRowsAsync(organizationId, filter.WarehouseId, productIds, cancellationToken);
        var translations = await GetTranslationsAsync(productRows, cancellationToken);
        var batchesByProductId = batches
            .GroupBy(batch => batch.ProductId)
            .ToDictionary(group => group.Key, group => group.ToList());

        var result = productRows
            .Select(row => new WarehouseProductDto
            {
                ProductId = row.ProductId,
                ProductName = GetLocalizedName(translations, ProductTableName, row.ProductId, row.ProductName),
                ProductMxik = row.ProductMxik,
                ProductGroupId = row.ProductGroupId,
                ProductGroupName = row.ProductGroupId.HasValue
                    ? GetLocalizedName(translations, ProductGroupTableName, row.ProductGroupId.Value, row.ProductGroupName)
                    : null,
                UnitId = row.UnitId,
                UnitName = GetLocalizedName(translations, UnitTableName, row.UnitId, row.UnitName),
                UnitCode = row.UnitCode,
                Quantity = row.Quantity,
                ReservedQuantity = row.ReservedQuantity,
                BlockedQuantity = row.BlockedQuantity,
                AvailableQuantity = CalculateAvailableQuantity(row.Quantity, row.ReservedQuantity, row.BlockedQuantity),
                Batches = BuildBatches(
                    batchesByProductId.GetValueOrDefault(row.ProductId) ?? [],
                    row.ReservedQuantity,
                    row.BlockedQuantity)
            })
            .OrderBy(item => item.ProductName)
            .ToList();

        return Result.Success<IReadOnlyList<WarehouseProductDto>>(result);
    }

    private async Task<List<WarehouseProductRow>> GetProductRowsAsync(int organizationId, int? warehouseId, int? productGroupId, IReadOnlyCollection<int> productIds, CancellationToken cancellationToken)
    {
        var query = _queryBuilder.For<WarehouseProduct>()
            .Where(item => (!warehouseId.HasValue || item.WarehouseId == warehouseId) &&
                           item.Product.OrganizationId == organizationId &&
                           (!productGroupId.HasValue || item.Product.ProductGroupId == productGroupId.Value) &&
                           (productIds.Count == 0 || productIds.Contains(item.ProductId)))
            .As(item => new WarehouseProductRow
            {
                ProductId = item.ProductId,
                ProductName = item.Product.Name,
                ProductMxik = item.Product.Mxik,
                ProductGroupId = item.Product.ProductGroupId,
                ProductGroupName = item.Product.ProductGroup == null ? null : item.Product.ProductGroup.Name,
                UnitId = item.UnitId,
                UnitName = item.Unit.Name,
                UnitCode = item.Unit.Code,
                Quantity = item.Quantity,
                ReservedQuantity = item.ReservedQuantity,
                BlockedQuantity = item.BlockedQuantity
            })
            .Build();

        return await _warehouseProductQuery.GetAllAsync(query, cancellationToken);
    }

    private async Task<List<WarehouseProductBatchRow>> GetBatchRowsAsync(int organizationId, int? warehouseId, IReadOnlyCollection<int> productIds, CancellationToken cancellationToken)
    {
        var query = _queryBuilder.For<WarehouseProductBatch>()
            .Where(batch => (!warehouseId.HasValue || batch.WarehouseId == warehouseId) &&
                            batch.OrganizationId == organizationId &&
                            productIds.Contains(batch.ProductId) &&
                            batch.RemainingQuantity > 0m)
            .As(batch => new WarehouseProductBatchRow
            {
                BatchId = batch.Id,
                ProductId = batch.ProductId,
                BatchNumber = batch.BatchNumber,
                ReceivedDate = batch.ReceivedDate,
                DocumentId = batch.ReceiptMovement.DocumentId,
                Quantity = batch.RemainingQuantity,
                UnitCost = batch.UnitCost ?? 0m
            })
            .Build();

        return await _warehouseProductBatchQuery.GetAllAsync(query, cancellationToken);
    }

    private async Task<Dictionary<(string TableName, long RecordId), string>> GetTranslationsAsync(IReadOnlyCollection<WarehouseProductRow> products, CancellationToken cancellationToken)
    {
        if (!_userContext.LanguageId.HasValue)
            return new Dictionary<(string TableName, long RecordId), string>();

        var productIds = products.Select(product => (long)product.ProductId).Distinct().ToList();
        var productGroupIds = products
            .Where(product => product.ProductGroupId.HasValue)
            .Select(product => (long)product.ProductGroupId!.Value)
            .Distinct()
            .ToList();
        var unitIds = products.Select(product => (long)product.UnitId).Distinct().ToList();
        var languageId = _userContext.LanguageId.Value;

        var query = _queryBuilder.For<Translation>()
            .Where(translation => translation.LanguageId == languageId &&
                                  translation.ColumnName == NameColumn &&
                                  ((translation.TableName == ProductTableName && productIds.Contains(translation.RecordId)) ||
                                   (translation.TableName == ProductGroupTableName && productGroupIds.Contains(translation.RecordId)) ||
                                   (translation.TableName == UnitTableName && unitIds.Contains(translation.RecordId))))
            .As(translation => new TranslationRow
            {
                TableName = translation.TableName,
                RecordId = translation.RecordId,
                Value = translation.Value
            })
            .Build();

        return (await _translationQuery.GetAllAsync(query, cancellationToken))
            .GroupBy(item => (item.TableName, item.RecordId))
            .ToDictionary(group => group.Key, group => group.Last().Value);
    }

    private static IReadOnlyList<WarehouseProductBatchDto> BuildBatches(IReadOnlyCollection<WarehouseProductBatchRow> batches, decimal reservedQuantity, decimal blockedQuantity)
    {
        var remainingReserved = Math.Max(reservedQuantity, 0m);
        var remainingBlocked = Math.Max(blockedQuantity, 0m);
        var result = new List<WarehouseProductBatchDto>(batches.Count);

        foreach (var batch in batches.OrderBy(item => item.ReceivedDate).ThenBy(item => item.BatchId))
        {
            var quantity = Math.Max(batch.Quantity, 0m);
            var batchReservedQuantity = Math.Min(quantity, remainingReserved);
            remainingReserved -= batchReservedQuantity;

            var batchBlockedQuantity = Math.Min(quantity - batchReservedQuantity, remainingBlocked);
            remainingBlocked -= batchBlockedQuantity;

            var availableQuantity = CalculateAvailableQuantity(quantity, batchReservedQuantity, batchBlockedQuantity);
            if (availableQuantity <= 0m)
                continue;

            result.Add(new WarehouseProductBatchDto
            {
                BatchId = batch.BatchId,
                BatchNumber = string.IsNullOrWhiteSpace(batch.BatchNumber) ? $"BATCH-{batch.BatchId}" : batch.BatchNumber,
                ReceivedDate = batch.ReceivedDate,
                DocumentId = batch.DocumentId,
                Quantity = quantity,
                ReservedQuantity = batchReservedQuantity,
                BlockedQuantity = batchBlockedQuantity,
                AvailableQuantity = availableQuantity,
                UnitCost = batch.UnitCost
            });
        }

        return result;
    }

    private static decimal CalculateAvailableQuantity(decimal quantity, decimal reservedQuantity, decimal blockedQuantity) =>
        Math.Max(quantity - Math.Max(reservedQuantity, 0m) - Math.Max(blockedQuantity, 0m), 0m);

    private static string GetLocalizedName(
        IReadOnlyDictionary<(string TableName, long RecordId), string> translations,
        string tableName,
        long recordId,
        string? fallback) =>
        translations.GetValueOrDefault((tableName, recordId)) ?? fallback ?? string.Empty;

    private static List<int> NormalizeProductIds(IReadOnlyCollection<int>? productIds) =>
        productIds?.Where(productId => productId > 0).Distinct().ToList() ?? [];

    private sealed class WarehouseProductRow
    {
        public int ProductId { get; init; }
        public string ProductName { get; init; } = null!;
        public string? ProductMxik { get; init; }
        public int? ProductGroupId { get; init; }
        public string? ProductGroupName { get; init; }
        public short UnitId { get; init; }
        public string UnitName { get; init; } = null!;
        public string UnitCode { get; init; } = null!;
        public decimal Quantity { get; init; }
        public decimal ReservedQuantity { get; init; }
        public decimal BlockedQuantity { get; init; }
    }

    private sealed class WarehouseProductBatchRow
    {
        public long BatchId { get; init; }
        public int ProductId { get; init; }
        public string? BatchNumber { get; init; }
        public DateTime ReceivedDate { get; init; }
        public long DocumentId { get; init; }
        public decimal Quantity { get; init; }
        public decimal UnitCost { get; init; }
    }

    private sealed class TranslationRow
    {
        public string TableName { get; init; } = null!;
        public long RecordId { get; init; }
        public string Value { get; init; } = null!;
    }
}
