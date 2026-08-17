using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features;
using Application.Features.DocumentNumbers;
using Application.Features.Inv.WarehouseProducts;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.SaleShipments;

public sealed class SaleShipmentService : BaseService, ISaleShipmentService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IDocumentNumberService _documentNumberService;
    private readonly IWarehouseInventoryService _warehouseInventoryService;
    private readonly IQueryRepository<SaleShipmentDoc> _shipmentQuery;
    private readonly ICommandRepository<SaleShipmentDoc> _shipmentCommand;
    private readonly ICommandRepository<SaleShipmentProduct> _shipmentProductCommand;
    private readonly ICommandRepository<SaleShipmentProductBatch> _shipmentProductBatchCommand;
    private readonly ICommandRepository<SaleShipmentTable> _shipmentTableCommand;
    private readonly IQueryRepository<Warehouse> _warehouseQuery;
    private readonly IQueryRepository<CounterpartyCard> _counterpartyQuery;
    private readonly IQueryRepository<Product> _productQuery;
    private readonly IQueryRepository<WarehouseProductTable> _warehouseProductTableQuery;

    public SaleShipmentService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IDocumentNumberService documentNumberService,
        IWarehouseInventoryService warehouseInventoryService,
        IQueryRepository<SaleShipmentDoc> shipmentQuery,
        ICommandRepository<SaleShipmentDoc> shipmentCommand,
        ICommandRepository<SaleShipmentProduct> shipmentProductCommand,
        ICommandRepository<SaleShipmentProductBatch> shipmentProductBatchCommand,
        ICommandRepository<SaleShipmentTable> shipmentTableCommand,
        IQueryRepository<Warehouse> warehouseQuery,
        IQueryRepository<CounterpartyCard> counterpartyQuery,
        IQueryRepository<Product> productQuery,
        IQueryRepository<WarehouseProductTable> warehouseProductTableQuery,
        ILogger<SaleShipmentService> logger,
        IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _documentNumberService = documentNumberService;
        _warehouseInventoryService = warehouseInventoryService;
        _shipmentQuery = shipmentQuery;
        _shipmentCommand = shipmentCommand;
        _shipmentProductCommand = shipmentProductCommand;
        _shipmentProductBatchCommand = shipmentProductBatchCommand;
        _shipmentTableCommand = shipmentTableCommand;
        _warehouseQuery = warehouseQuery;
        _counterpartyQuery = counterpartyQuery;
        _productQuery = productQuery;
        _warehouseProductTableQuery = warehouseProductTableQuery;
    }

    public Task<Result<PagedResponse<SaleShipmentListDto>>> GetListAsync(SaleShipmentFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetListAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<PagedResponse<SaleShipmentListDto>>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.BuildPaged<SaleShipmentDoc, SaleShipmentListDto, SaleShipmentFilter>(filter);
            var paged = await _shipmentQuery.GetPagedAsync(query, ct);
            return Result.Success(PagedResponseFactory.Create(paged, filter.Page, filter.PageSize));
        });

    public Task<Result<SaleShipmentDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<SaleShipmentDto>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var shipment = await _shipmentQuery.GetAsync(
                _queryBuilder.For<SaleShipmentDoc>()
                    .Where(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value)
                    .As<SaleShipmentDto>()
                    .Build(),
                ct);
            return shipment is null
                ? Result.Failure<SaleShipmentDto>(SaleShipmentErrors.NotFound(id, _userContext.LanguageId))
                : Result.Success(shipment);
        });

    public Task<Result<long>> CreateAsync(SaleShipmentCreateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<long>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));
            if (_userContext.Id is null)
                return Result.Failure<long>(SaleShipmentErrors.CurrentUserNotFound(_userContext.LanguageId));

            var organizationId = _userContext.OrganizationId.Value;
            var headerValidation = await ValidateHeaderAsync(organizationId, dto.WarehouseId, dto.CounterpartyId, ct);
            if (!headerValidation.IsSuccess)
                return Result.Failure<long>(headerValidation.Error);

            var productsResult = await BuildProductsAsync(organizationId, dto.WarehouseId, dto.Products, ct);
            if (!productsResult.IsSuccess)
                return Result.Failure<long>(productsResult.Error);

            var now = DateTime.Now;
            var docDate = dto.DocDate == default
                ? now
                : DateTime.SpecifyKind(dto.DocDate, DateTimeKind.Unspecified);
            var docNumberResult = await ResolveDocNumberAsync(organizationId, dto.DocNumber, docDate, ct);
            if (!docNumberResult.IsSuccess)
                return Result.Failure<long>(docNumberResult.Error);

            var shipment = new SaleShipmentDoc
            {
                OrganizationId = organizationId,
                WarehouseId = dto.WarehouseId,
                CounterpartyId = dto.CounterpartyId,
                DocNumber = docNumberResult.Value,
                DocDate = docDate,
                Comment = dto.Comment,
                StatusId = DocumentStatusIdConst.DRAFT,
                CreatedUserId = _userContext.Id.Value,
                CreatedDate = now,
                SaleShipmentProducts = productsResult.Value
            };

            await _shipmentCommand.CreateAsync(shipment, ct);
            return Result.Success(shipment.Id);
        }, ct);

    public Task<Result> UpdateAsync(long id, SaleShipmentUpdateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var organizationId = _userContext.OrganizationId.Value;
            var shipment = await GetShipmentEntityAsync(id, organizationId, ct);
            if (shipment is null)
                return Result.Failure(SaleShipmentErrors.NotFound(id, _userContext.LanguageId));
            if (shipment.SaleDocId.HasValue)
                return Result.Failure(SaleShipmentErrors.LinkedDocumentCannotBeChanged(id, _userContext.LanguageId));

            var headerValidation = await ValidateHeaderAsync(organizationId, dto.WarehouseId, dto.CounterpartyId, ct);
            if (!headerValidation.IsSuccess)
                return headerValidation;

            var productsResult = await BuildProductsAsync(organizationId, dto.WarehouseId, dto.Products, ct);
            if (!productsResult.IsSuccess)
                return Result.Failure(productsResult.Error);

            if (!string.IsNullOrWhiteSpace(dto.DocNumber) && dto.DocNumber.Trim().Length > 50)
                return Result.Failure(SaleShipmentErrors.InvalidDocNumber(_userContext.LanguageId));

            await _shipmentTableCommand.DeleteAsync(x => x.ShipmentProduct.OwnerId == id, ct);
            await _shipmentProductBatchCommand.DeleteAsync(x => x.ShipmentProduct.OwnerId == id, ct);
            await _shipmentProductCommand.DeleteAsync(x => x.OwnerId == id, ct);

            foreach (var product in productsResult.Value)
                product.OwnerId = id;
            await _shipmentProductCommand.CreateAsync(productsResult.Value, ct);

            shipment.WarehouseId = dto.WarehouseId;
            shipment.CounterpartyId = dto.CounterpartyId;
            shipment.DocNumber = string.IsNullOrWhiteSpace(dto.DocNumber) ? shipment.DocNumber : dto.DocNumber.Trim();
            shipment.DocDate = dto.DocDate == default
                ? shipment.DocDate
                : DateTime.SpecifyKind(dto.DocDate, DateTimeKind.Unspecified);
            shipment.Comment = dto.Comment;
            await _shipmentCommand.UpdateAsync(shipment, ct);

            return Result.Success();
        }, ct);

    public Task<Result> DeleteAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(DeleteAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var shipment = await GetShipmentEntityAsync(id, _userContext.OrganizationId.Value, ct);
            if (shipment is null)
                return Result.Failure(SaleShipmentErrors.NotFound(id, _userContext.LanguageId));
            if (shipment.SaleDocId.HasValue)
                return Result.Failure(SaleShipmentErrors.LinkedDocumentCannotBeChanged(id, _userContext.LanguageId));

            await _shipmentTableCommand.DeleteAsync(x => x.ShipmentProduct.OwnerId == id, ct);
            await _shipmentProductBatchCommand.DeleteAsync(x => x.ShipmentProduct.OwnerId == id, ct);
            await _shipmentProductCommand.DeleteAsync(x => x.OwnerId == id, ct);
            await _shipmentCommand.DeleteAsync(shipment, ct);

            return Result.Success();
        }, ct);

    private async Task<SaleShipmentDoc?> GetShipmentEntityAsync(long id, int organizationId, CancellationToken ct) =>
        await _shipmentQuery.GetAsync(
            _queryBuilder.For<SaleShipmentDoc>()
                .Where(x => x.Id == id && x.OrganizationId == organizationId)
                .Build(),
            ct);

    private async Task<Result> ValidateHeaderAsync(int organizationId, int warehouseId, int? counterpartyId, CancellationToken ct)
    {
        var warehouse = await _warehouseQuery.GetAsync(
            _queryBuilder.For<Warehouse>().Where(x => x.Id == warehouseId).Build(),
            ct);
        if (warehouse is null)
            return Result.Failure(SaleShipmentErrors.WarehouseNotFound(warehouseId, _userContext.LanguageId));
        if (warehouse.OrganizationId != organizationId)
            return Result.Failure(SaleShipmentErrors.WarehouseOrganizationMismatch(warehouseId, organizationId, _userContext.LanguageId));
        if (warehouse.StateId != StateIdConst.ACTIVE)
            return Result.Failure(SaleShipmentErrors.WarehouseInactive(warehouseId, _userContext.LanguageId));

        if (!counterpartyId.HasValue)
            return Result.Success();

        var counterparty = await _counterpartyQuery.GetAsync(
            _queryBuilder.For<CounterpartyCard>().Where(x => x.Id == counterpartyId.Value).Build(),
            ct);
        if (counterparty is null)
            return Result.Failure(SaleShipmentErrors.CounterpartyNotFound(counterpartyId.Value, _userContext.LanguageId));
        if (counterparty.OrganizationId != organizationId)
            return Result.Failure(SaleShipmentErrors.CounterpartyOrganizationMismatch(counterpartyId.Value, organizationId, _userContext.LanguageId));

        return Result.Success();
    }

    private async Task<Result<string?>> ResolveDocNumberAsync(int organizationId, string? docNumber, DateTime docDate, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(docNumber))
        {
            var normalizedDocNumber = docNumber.Trim();
            return normalizedDocNumber.Length > 50
                ? Result.Failure<string?>(SaleShipmentErrors.InvalidDocNumber(_userContext.LanguageId))
                : Result.Success<string?>(normalizedDocNumber);
        }

        var documentNumberResult = await _documentNumberService.GetNextAsync(
            organizationId,
            DocumentTypeIdConst.SALESHIPMENT,
            docDate,
            ct);

        return documentNumberResult.IsSuccess
            ? Result.Success<string?>(documentNumberResult.Value.DocumentNumber)
            : Result.Failure<string?>(documentNumberResult.Error);
    }

    private async Task<Result<List<SaleShipmentProduct>>> BuildProductsAsync(
        int organizationId,
        int warehouseId,
        IReadOnlyList<SaleShipmentProductCreateDto>? productDtos,
        CancellationToken ct)
    {
        var requestedProducts = productDtos ?? [];
        if (requestedProducts.Count == 0)
            return Result.Failure<List<SaleShipmentProduct>>(SaleShipmentErrors.ProductsRequired(_userContext.LanguageId));

        var productIds = requestedProducts.Select(x => x.ProductId).Distinct().ToList();
        var products = await _productQuery.GetAllAsync(
            _queryBuilder.For<Product>()
                .Where(x => productIds.Contains(x.Id) && x.OrganizationId == organizationId)
                .Build(),
            ct);
        var productsById = products.ToDictionary(x => x.Id);
        foreach (var productDto in requestedProducts)
        {
            if (!productsById.TryGetValue(productDto.ProductId, out var product))
                return Result.Failure<List<SaleShipmentProduct>>(SaleShipmentErrors.ProductNotFound(productDto.ProductId, _userContext.LanguageId));
            if (product.IsService)
                return Result.Failure<List<SaleShipmentProduct>>(SaleShipmentErrors.ProductServiceNotAllowed(productDto.ProductId, _userContext.LanguageId));
            if (productDto.UnitId != product.UnitId)
                return Result.Failure<List<SaleShipmentProduct>>(SaleShipmentErrors.ProductUnitMismatch(productDto.ProductId, productDto.UnitId, _userContext.LanguageId));
            if (productDto.Quantity <= 0m || (product.IsPieceTracked && productDto.Quantity != decimal.Truncate(productDto.Quantity)))
                return Result.Failure<List<SaleShipmentProduct>>(SaleShipmentErrors.InvalidQuantity(productDto.ProductId, productDto.Quantity, _userContext.LanguageId));
        }

        var duplicateProductTableId = requestedProducts
            .SelectMany(x => x.ProductTables ?? [])
            .GroupBy(x => x.ProductTableId)
            .FirstOrDefault(group => group.Key <= 0 || group.Count() > 1)
            ?.Key;
        if (duplicateProductTableId.HasValue)
            return Result.Failure<List<SaleShipmentProduct>>(SaleShipmentErrors.DuplicateProductTable(duplicateProductTableId.Value, _userContext.LanguageId));

        var inventoryResult = await _warehouseInventoryService.GetWarehouseProductsAsync(
            new WarehouseProductFilter { WarehouseId = warehouseId, ProductIds = productIds },
            ct);
        if (!inventoryResult.IsSuccess)
            return Result.Failure<List<SaleShipmentProduct>>(inventoryResult.Error);

        var availableBatchesByProductId = inventoryResult.Value.ToDictionary(
            x => x.ProductId,
            x => x.Batches.ToDictionary(batch => batch.BatchId));
        var requestedBatchQuantities = new Dictionary<(int ProductId, long BatchId), decimal>();

        foreach (var productDto in requestedProducts)
        {
            var product = productsById[productDto.ProductId];
            var batches = productDto.Batches ?? [];
            var productTables = productDto.ProductTables ?? [];
            if (batches.Any(x => x.BatchId <= 0 || x.Quantity <= 0m))
                return Result.Failure<List<SaleShipmentProduct>>(SaleShipmentErrors.BatchUnavailable(0, warehouseId, productDto.ProductId, _userContext.LanguageId));
            var duplicateBatch = batches.GroupBy(x => x.BatchId).FirstOrDefault(group => group.Count() > 1);
            if (duplicateBatch is not null)
                return Result.Failure<List<SaleShipmentProduct>>(SaleShipmentErrors.DuplicateBatch(duplicateBatch.Key, _userContext.LanguageId));

            if (product.IsPieceTracked)
            {
                if (productTables.Count != (int)productDto.Quantity)
                {
                    return Result.Failure<List<SaleShipmentProduct>>(
                        SaleShipmentErrors.ProductTableQuantityMismatch(productDto.ProductId, productDto.Quantity, productTables.Count, _userContext.LanguageId));
                }
            }
            else
            {
                if (productTables.Count > 0)
                    return Result.Failure<List<SaleShipmentProduct>>(SaleShipmentErrors.ProductTablesNotAllowed(productDto.ProductId, _userContext.LanguageId));
                var batchQuantity = batches.Sum(x => x.Quantity);
                if (batchQuantity != productDto.Quantity)
                {
                    return Result.Failure<List<SaleShipmentProduct>>(
                        SaleShipmentErrors.BatchQuantityMismatch(productDto.ProductId, productDto.Quantity, batchQuantity, _userContext.LanguageId));
                }
            }

            foreach (var batch in batches)
            {
                if (!availableBatchesByProductId.TryGetValue(productDto.ProductId, out var availableBatches) ||
                    !availableBatches.TryGetValue(batch.BatchId, out var availableBatch))
                {
                    return Result.Failure<List<SaleShipmentProduct>>(
                        SaleShipmentErrors.BatchUnavailable(batch.BatchId, warehouseId, productDto.ProductId, _userContext.LanguageId));
                }

                var key = (productDto.ProductId, batch.BatchId);
                requestedBatchQuantities[key] = requestedBatchQuantities.GetValueOrDefault(key) + batch.Quantity;
                if (requestedBatchQuantities[key] > availableBatch.AvailableQuantity)
                {
                    return Result.Failure<List<SaleShipmentProduct>>(
                        SaleShipmentErrors.BatchQuantityUnavailable(batch.BatchId, requestedBatchQuantities[key], availableBatch.AvailableQuantity, _userContext.LanguageId));
                }
            }
        }

        var productTableIds = requestedProducts
            .SelectMany(x => x.ProductTables ?? [])
            .Select(x => x.ProductTableId)
            .ToList();
        var productTablesById = new Dictionary<int, ShipmentProductTableSnapshot>();
        if (productTableIds.Count > 0)
        {
            var productTables = await _warehouseProductTableQuery.GetAllAsync(
                _queryBuilder.For<WarehouseProductTable>()
                    .Where(x => productTableIds.Contains(x.ProductTableId))
                    .As(x => new ShipmentProductTableSnapshot(
                        x.ProductTableId,
                        x.ProductTable.ProductId,
                        x.WarehouseId,
                        x.StatusId))
                    .Build(),
                ct);
            productTablesById = productTables.ToDictionary(x => x.ProductTableId);
        }

        foreach (var productDto in requestedProducts.Where(x => productsById[x.ProductId].IsPieceTracked))
        {
            foreach (var item in productDto.ProductTables ?? [])
            {
                if (!productTablesById.TryGetValue(item.ProductTableId, out var productTable))
                {
                    return Result.Failure<List<SaleShipmentProduct>>(
                        SaleShipmentErrors.ProductTableNotFound(item.ProductTableId, _userContext.LanguageId));
                }
                if (productTable.ProductId != productDto.ProductId)
                {
                    return Result.Failure<List<SaleShipmentProduct>>(
                        SaleShipmentErrors.ProductTableMismatch(item.ProductTableId, productDto.ProductId, _userContext.LanguageId));
                }
                if (productTable.WarehouseId != warehouseId || productTable.StatusId != ProductTableStatusIdConst.IN_STOCK)
                {
                    return Result.Failure<List<SaleShipmentProduct>>(
                        SaleShipmentErrors.ProductTableUnavailable(item.ProductTableId, warehouseId, _userContext.LanguageId));
                }
            }
        }

        var now = DateTime.Now;
        return Result.Success(requestedProducts.Select(productDto => new SaleShipmentProduct
        {
            ProductId = productDto.ProductId,
            UnitId = productDto.UnitId,
            Quantity = productDto.Quantity,
            CreatedDate = now,
            SaleShipmentProductBatches = (productDto.Batches ?? [])
                .Select(batch => new SaleShipmentProductBatch { BatchId = batch.BatchId, Quantity = batch.Quantity })
                .ToList(),
            SaleShipmentTables = (productDto.ProductTables ?? [])
                .Select(item => new SaleShipmentTable { ProductTableId = item.ProductTableId, CreatedDate = now })
                .ToList()
        }).ToList());
    }

    private sealed record ShipmentProductTableSnapshot(
        int ProductTableId,
        int ProductId,
        int WarehouseId,
        short StatusId);
}
