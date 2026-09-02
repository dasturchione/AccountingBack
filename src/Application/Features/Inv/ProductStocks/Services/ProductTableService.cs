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
    private const string ProductTableName = "inv_product";
    private const string WarehouseTableName = "inv_warehouse";
    private const string NameColumn = "name";

    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<ProductTable> _productTableQuery;
    private readonly IQueryRepository<Translation> _translationQuery;
    private readonly IProductPriceCalculateService _priceCalculateService;
    private readonly IWarehouseInventoryService _warehouseInventoryService;

    public ProductStockService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IQueryRepository<ProductTable> productTableQuery,
        IQueryRepository<Translation> translationQuery,
        IProductPriceCalculateService priceCalculateService,
        IWarehouseInventoryService warehouseInventoryService)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _productTableQuery = productTableQuery;
        _translationQuery = translationQuery;
        _priceCalculateService = priceCalculateService;
        _warehouseInventoryService = warehouseInventoryService;
    }

    public async Task<Result<ProductTableByMarkingDto>> GetByMarkingNumberAsync(string markingNumber, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is not int organizationId || organizationId <= 0)
            return Result.Failure<ProductTableByMarkingDto>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var query = _queryBuilder.For<ProductTable>()
            .Where(x => x.Product.OrganizationId == organizationId
                        && x.MarkingNumber == markingNumber
                        && x.Product.StateId == StateIdConst.ACTIVE
                        && (x.WarehouseProductTable == null ||
                            x.WarehouseProductTable.Warehouse.OrganizationId == organizationId))
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

        await LocalizeProductTablesAsync([entity], ct);

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
        if (_userContext.OrganizationId is not int organizationId || organizationId <= 0)
            return Result.Failure<PagedResponse<ProductTableStockDto>>(
                CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var pageSize = filter.PageSize;
        var skip = pageSize.HasValue ? (filter.Page - 1) * pageSize.Value : 0;
        var query = _queryBuilder.For<ProductTable>()
            .Where(x => x.Product.OrganizationId == organizationId
                        && x.WarehouseProductTable != null
                        && x.WarehouseProductTable.Warehouse.OrganizationId == organizationId
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
            .OrderBy(items => items
                .OrderBy(x => x.ProductName)
                .ThenBy(x => x.SerialNumber)
                .ThenBy(x => x.Id))
            .Skip(skip)
            .Take(pageSize)
            .BuildPaged();

        var rows = await _productTableQuery.GetPagedAsync(query, ct);
        await LocalizeProductTablesAsync(rows.Items, ct);
        return Result.Success(PagedResponseFactory.Create(rows, filter.Page, filter.PageSize));
    }

    private async Task LocalizeProductTablesAsync(
        IReadOnlyCollection<ProductTableByMarkingDto> rows,
        CancellationToken ct)
    {
        if (!_userContext.LanguageId.HasValue || rows.Count == 0)
            return;

        var productIds = rows.Select(row => (long)row.ProductId).Distinct().ToList();
        var warehouseIds = rows
            .Where(row => row.CurrentWarehouseId.HasValue)
            .Select(row => (long)row.CurrentWarehouseId!.Value)
            .Distinct()
            .ToList();
        var translations = await GetTranslationsAsync(productIds, warehouseIds, ct);

        foreach (var row in rows)
        {
            row.ProductName = translations.GetValueOrDefault((ProductTableName, row.ProductId)) ?? row.ProductName;
            if (row.CurrentWarehouseId.HasValue)
                row.CurrentWarehouseName = translations.GetValueOrDefault((WarehouseTableName, row.CurrentWarehouseId.Value)) ?? row.CurrentWarehouseName;
        }
    }

    private async Task LocalizeProductTablesAsync(
        IReadOnlyCollection<ProductTableStockDto> rows,
        CancellationToken ct)
    {
        if (!_userContext.LanguageId.HasValue || rows.Count == 0)
            return;

        var productIds = rows.Select(row => (long)row.ProductId).Distinct().ToList();
        var warehouseIds = rows
            .Where(row => row.CurrentWarehouseId.HasValue)
            .Select(row => (long)row.CurrentWarehouseId!.Value)
            .Distinct()
            .ToList();
        var translations = await GetTranslationsAsync(productIds, warehouseIds, ct);

        foreach (var row in rows)
        {
            row.ProductName = translations.GetValueOrDefault((ProductTableName, row.ProductId)) ?? row.ProductName;
            if (row.CurrentWarehouseId.HasValue)
                row.CurrentWarehouseName = translations.GetValueOrDefault((WarehouseTableName, row.CurrentWarehouseId.Value)) ?? row.CurrentWarehouseName;
        }
    }

    private async Task<Dictionary<(string TableName, long RecordId), string>> GetTranslationsAsync(
        IReadOnlyCollection<long> productIds,
        IReadOnlyCollection<long> warehouseIds,
        CancellationToken ct)
    {
        var languageId = _userContext.LanguageId!.Value;
        var query = _queryBuilder.For<Translation>()
            .Where(translation => translation.LanguageId == languageId &&
                                  translation.ColumnName == NameColumn &&
                                  ((translation.TableName == ProductTableName && productIds.Contains(translation.RecordId)) ||
                                   (translation.TableName == WarehouseTableName && warehouseIds.Contains(translation.RecordId))))
            .As(translation => new ProductStockTranslationRow
            {
                TableName = translation.TableName,
                RecordId = translation.RecordId,
                Value = translation.Value
            })
            .Build();

        return (await _translationQuery.GetAllAsync(query, ct))
            .ToDictionary(row => (row.TableName, row.RecordId), row => row.Value);
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

    private sealed class ProductStockTranslationRow
    {
        public string TableName { get; set; } = null!;
        public long RecordId { get; set; }
        public string Value { get; set; } = null!;
    }
}
