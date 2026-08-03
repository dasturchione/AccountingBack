using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.AuditLogs;
using Application.Features.CounterpartyCards;
using Application.Features.DocumentNumbers;
using Application.Features.Inv.ProductPrices;
using Application.Features.Inv.WarehouseProducts;
using Application.Features.InventoryCounts;
using Application.Features.SaleDocTables;
using Application.Features.SaleShipments;
using Application.Features.Warehouses;
using DocumentFormat.OpenXml.Office2010.Excel;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.SaleDocs;

public class SaleDocService : BaseService, ISaleDocService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IAuditLogService _auditLogService;
    private readonly ISaleLifecycleService _saleLifecycleService;
    private readonly IDocumentPostingLock _postingLock;
    private readonly IQueryRepository<SaleDoc> _query;
    private readonly IQueryRepository<VatRate> _vatRateQuery;
    private readonly ICommandRepository<SaleDoc> _command;
    private readonly ICommandRepository<SaleDocProduct> _productLineCommand;
    private readonly IQueryRepository<SaleDocProduct> _productLineQuery;
    private readonly ICommandRepository<SaleDocTable> _lineCommand;
    private readonly IQueryRepository<Warehouse> _warehouseQuery;
    private readonly IQueryRepository<CounterpartyCard> _counterpartyQuery;
    private readonly IQueryRepository<Product> _productQuery;
    private readonly IQueryRepository<WarehouseProductBatchTable> _warehouseProductBatchTableQuery;
    private readonly IQueryRepository<SaleShipmentDoc> _shipmentDocQuery;
    private readonly ICommandRepository<SaleShipmentDoc> _shipmentDocCommand;
    private readonly IQueryRepository<SaleShipmentProduct> _shipmentProductQuery;
    private readonly ICommandRepository<SaleShipmentProduct> _shipmentProductCommand;
    private readonly IProductPriceCalculateService _priceCalculateService;
    private readonly IProductTableReservationService _reservationService;
    private readonly IWarehouseProductBalanceService _warehouseProductBalanceService;
    private readonly IWarehouseInventoryService _warehouseInventoryService;
    private readonly IActiveInventoryCountGuardService _activeInventoryCountGuardService;
    private readonly IDocumentNumberService _documentNumberService;

    public SaleDocService(IUserContext userContext,
                          IQueryBuilder queryBuilder,
                          IAuditLogService auditLogService,
                          ISaleLifecycleService saleLifecycleService,
                          IDocumentPostingLock postingLock,
                          IDocumentNumberService documentNumberService,
                          IQueryRepository<SaleDoc> query,
                          IQueryRepository<VatRate> vatRateQuery,
                          ICommandRepository<SaleDoc> command,
                          ICommandRepository<SaleDocProduct> productLineCommand,
                          IQueryRepository<SaleDocProduct> productLineQuery,
                          ICommandRepository<SaleDocTable> lineCommand,
                          IQueryRepository<Warehouse> warehouseQuery,
                          IQueryRepository<CounterpartyCard> counterpartyQuery,
                          IQueryRepository<Product> productQuery,
                          IQueryRepository<WarehouseProductBatchTable> warehouseProductBatchTableQuery,
                          IQueryRepository<SaleShipmentDoc> shipmentDocQuery,
                          ICommandRepository<SaleShipmentDoc> shipmentDocCommand,
                          IQueryRepository<SaleShipmentProduct> shipmentProductQuery,
                          ICommandRepository<SaleShipmentProduct> shipmentProductCommand,
                          IProductPriceCalculateService priceCalculateService,
                          IProductTableReservationService reservationService,
                          IWarehouseProductBalanceService warehouseProductBalanceService,
                          IWarehouseInventoryService warehouseInventoryService,
                          IActiveInventoryCountGuardService activeInventoryCountGuardService,
                          ILogger<SaleDocService> logger,
                          IUnitOfWork unitOfWork)
            : base(logger, unitOfWork)
    {
        _query = query;
        _command = command;
        _lineCommand = lineCommand;
        _productLineCommand = productLineCommand;
        _productLineQuery = productLineQuery;
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _auditLogService = auditLogService;
        _saleLifecycleService = saleLifecycleService;
        _postingLock = postingLock;
        _vatRateQuery = vatRateQuery;
        _warehouseQuery = warehouseQuery;
        _counterpartyQuery = counterpartyQuery;
        _productQuery = productQuery;
        _warehouseProductBatchTableQuery = warehouseProductBatchTableQuery;
        _shipmentDocQuery = shipmentDocQuery;
        _shipmentDocCommand = shipmentDocCommand;
        _shipmentProductQuery = shipmentProductQuery;
        _shipmentProductCommand = shipmentProductCommand;
        _priceCalculateService = priceCalculateService;
        _reservationService = reservationService;
        _warehouseProductBalanceService = warehouseProductBalanceService;
        _warehouseInventoryService = warehouseInventoryService;
        _activeInventoryCountGuardService = activeInventoryCountGuardService;
        _documentNumberService = documentNumberService;
    }

    public Task<Result<PagedResponse<SaleDocListDto>>> GetAllAsync(SaleDocListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            var query = _queryBuilder.BuildPaged<SaleDoc, SaleDocListDto, SaleDocListFilter>(filter);
            var pagedList = await _query.GetPagedAsync(query, ct);
            return Result.Success(PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize));
        });

    public Task<Result<SaleDocDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<SaleDocDto>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.For<SaleDoc>()
                .Where(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value)
                .As<SaleDocDto>()
                .Build();
            var entity = await _query.GetAsync(query, ct);

            if (entity == null)
                return Result.Failure<SaleDocDto>(SaleDocErrors.NotFound(id, _userContext.LanguageId));

            return Result.Success(entity);
        });

    /// <summary>
    /// Bosqich 1: Buxgalter sotuv hujjatini yaratadi.
    /// SaleDoc (DRAFT) + SaleDocProduct yaratiladi. ProductTable hali o'zgarmaydi.
    /// </summary>
    public Task<Result<long>> CreateAsync(SaleDocCreateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<long>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var orgId = _userContext.OrganizationId.Value;
            var now = DateTime.Now;
            var docDate = dto.DocDate.HasValue
                ? DateTime.SpecifyKind(dto.DocDate.Value, DateTimeKind.Unspecified)
                : now;

            var warehouseQuery = _queryBuilder.For<Warehouse>().Where(x => x.Id == dto.WarehouseId).Build();
            var warehouse = await _warehouseQuery.GetAsync(warehouseQuery, ct);
            if (warehouse is null)
                return Result.Failure<long>(WarehouseErrors.NotFound(dto.WarehouseId, _userContext.LanguageId));

            var counterpartyQuery = _queryBuilder.For<CounterpartyCard>().Where(x => x.Id == dto.CounterpartyId).Build();
            var counterparty = await _counterpartyQuery.GetAsync(counterpartyQuery, ct);
            if (counterparty is null)
                return Result.Failure<long>(CounterpartyCardErrors.NotFound(dto.CounterpartyId, _userContext.LanguageId));

            var shipmentLinkResult = await ValidateShipmentLinkAsync(dto, orgId, ct);
            if (!shipmentLinkResult.IsSuccess)
                return Result.Failure<long>(shipmentLinkResult.Error);

            var productsResult = await BuildProductLinesAsync(dto.Lines, ct);
            if (!productsResult.IsSuccess)
                return Result.Failure<long>(productsResult.Error);

            var productLines = productsResult.Value;

            var batchSelectionValidation = AttachProductBatchSelections(productLines, dto.Lines);
            if (!batchSelectionValidation.IsSuccess)
                return Result.Failure<long>(batchSelectionValidation.Error);

            var documentNumberResult = await _documentNumberService.GetNextAsync(
                orgId,
                DocumentTypeIdConst.SALE,
                docDate,
                ct);
            if (!documentNumberResult.IsSuccess)
                return Result.Failure<long>(documentNumberResult.Error);

            var doc = new SaleDoc
            {
                OrganizationId = orgId,
                DocNumber = documentNumberResult.Value.DocumentNumber,
                DocDate = docDate,
                CurrencyId = dto.CurrencyId,
                ExchangeRate = dto.ExchangeRate == 0 ? 1m : dto.ExchangeRate,
                SaleDocProducts = productLines,
                TotalAmount = productLines.Sum(l => l.Amount),
                VatAmount = productLines.Sum(l => l.VatAmount),
                FinalAmount = productLines.Sum(l => l.TotalAmount),
                StatusId = DocumentStatusIdConst.DRAFT,
                Comment = dto.Comment,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = now,
                WarehouseId = dto.WarehouseId,
                CounterpartyId = dto.CounterpartyId,
                ContractId = dto.ContractId,
                CustomerAccountId = dto.CustomerAccountId,
                VatAccountId = dto.VatAccountId,
            };

            await _command.CreateAsync(doc, ct);

            if (shipmentLinkResult.Value is not null)
            {
                var shipmentLinkingResult = await LinkShipmentAsync(shipmentLinkResult.Value, doc.Id, productLines, ct);
                if (!shipmentLinkingResult.IsSuccess)
                    return Result.Failure<long>(shipmentLinkingResult.Error);
            }

            if (dto.ProcessingMode == SaleProcessingMode.Immediate)
            {
                var assemblyProductLines = await GetProductLinesForAssemblyAsync(doc.Id, ct);
                var assemblyDtos = BuildAssemblyDtosFromCreate(assemblyProductLines, dto.Lines);
                var assemblyResult = await AssemblyAsync(doc.Id, assemblyDtos, ct);
                if (!assemblyResult.IsSuccess)
                    return Result.Failure<long>(assemblyResult.Error);
            }

            var docDto = await GetByIdInternalAsync(doc.Id, ct);
            if (docDto != null)
            {
                _auditLogService.SetNewValues(docDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.SaleDoc, doc.Id.ToString(), AuditLogOperationTypeConst.Create);
            }

            return Result.Success(doc.Id);
        }, ct);

    /// <summary>
    /// Bosqich 2: Skladchik yig'adi вЂ” aniq ProductTable elementlarini tanlaydi.
    /// IsPieceTracked=false bo'lgan tovarlar avtomatik yig'ilgan hisoblanadi.
    /// </summary>
    public Task<Result> AssemblyAsync(long id, List<SaleDocProductAssemblyDto> dtos, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(AssemblyAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            await _postingLock.AcquireAsync(DocumentTypeIdConst.SALE, id, ct);

            var docQuery = _queryBuilder.For<SaleDoc>().Where(x => x.Id == id).Build();
            var doc = await _query.GetAsync(docQuery, ct);

            if (doc == null)
                return Result.Failure(SaleDocErrors.NotFound(id, _userContext.LanguageId));

            if (doc.OrganizationId != _userContext.OrganizationId.Value)
                return Result.Failure(SaleDocErrors.NotFound(id, _userContext.LanguageId));

            var productLines = await GetProductLinesForAssemblyAsync(id, ct);
            return await ApplyAssemblyAsync(doc, productLines, dtos ?? new List<SaleDocProductAssemblyDto>(), ct);
        }, ct);

    /// <summary>
    /// Bosqich 3: Buxgalter tasdiqlaydi вЂ” har bir SaleDocTable uchun sotuv narxini belgilaydi.
    /// SaleDoc в†’ POSTED, ProductTable в†’ SOLD, provodka yaratiladi.
    /// </summary>
    public Task<Result> ConfirmAsync(long id, SaleDocConfirmDto dto, CancellationToken ct = default) =>
        _saleLifecycleService.ConfirmAsync(id, dto, ct);


    /// <summary>
    /// Bekor qilish вЂ” istalgan bosqichdan. ProductTable в†’ IN_STOCK ga qaytariladi.
    /// </summary>
    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        _saleLifecycleService.CancelAsync(id, ct);

    /// <summary>
    /// O'zgartirish вЂ” faqat DRAFT va PENDING bosqichlarda.
    /// DRAFT: header + mahsulot liniyalari o'zgaradi.
    /// PENDING: header o'zgaradi (skladchik tanlagan tovarlar o'zgarmaydi).
    /// </summary>
    public Task<Result> UpdateAsync(long id, SaleDocUpdateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.For<SaleDoc>()
                .Where(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value)
                .Build();
            var doc = await _query.GetAsync(query, ct);

            if (doc == null)
                return Result.Failure(SaleDocErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId != DocumentStatusIdConst.DRAFT && doc.StatusId != DocumentStatusIdConst.PENDING)
                return Result.Failure(SaleDocErrors.CannotUpdateInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto != null)
                _auditLogService.SetOldValues(oldDocDto);

            doc.DocDate = DateTime.SpecifyKind(dto.DocDate, DateTimeKind.Unspecified);
            doc.CounterpartyId = dto.CounterpartyId;
            doc.WarehouseId = dto.WarehouseId;
            doc.CurrencyId = dto.CurrencyId;
            doc.ExchangeRate = dto.ExchangeRate == 0 ? 1m : dto.ExchangeRate;
            doc.ContractId = dto.ContractId;
            doc.CustomerAccountId = dto.CustomerAccountId;
            doc.VatAccountId = dto.VatAccountId;
            doc.Comment = dto.Comment;
            doc.StateId = dto.StateId;

            // DRAFT da mahsulot liniyalarini ham o'zgartirish mumkin
            if (doc.StatusId == DocumentStatusIdConst.DRAFT && dto.Lines.Count > 0)
            {
                var existingProductsQuery = _queryBuilder.For<SaleDocProduct>().Where(x => x.OwnerId == id).Build();
                var existingProducts = await _productLineQuery.GetAllAsync(existingProductsQuery, ct);

                // Mavjud liniyalarni o'chirishs
                foreach (var existing in existingProducts)
                    await _productLineCommand.DeleteAsync(existing, ct);

                // Yangi liniyalarni yaratish
                var productsResult = await BuildProductLinesFromUpdateAsync(dto.Lines, ct);
                if (!productsResult.IsSuccess)
                    return Result.Failure(productsResult.Error);

                var newProducts = productsResult.Value;
                foreach (var p in newProducts)
                    p.OwnerId = id;

                await _productLineCommand.CreateAsync(newProducts, ct);

                doc.TotalAmount = newProducts.Sum(p => p.Amount);
                doc.VatAmount = newProducts.Sum(p => p.VatAmount);
                doc.FinalAmount = newProducts.Sum(p => p.TotalAmount);
            }

            await _command.UpdateAsync(doc, ct);

            var newDocDto = await GetByIdInternalAsync(id, ct);
            if (newDocDto != null)
            {
                _auditLogService.SetNewValues(newDocDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.SaleDoc, id.ToString(), AuditLogOperationTypeConst.Update, dto.Comment);
            }

            return Result.Success();
        }, ct);

    public Task<Result> DeleteAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(DeleteAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.For<SaleDoc>()
                .Where(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value)
                .Build();
            var doc = await _query.GetAsync(query, ct);

            if (doc == null)
                return Result.Failure(SaleDocErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(SaleDocErrors.NotDraft(id, _userContext.LanguageId));

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto != null)
                _auditLogService.SetOldValues(oldDocDto);

            // Product liniyalarini o'chirish
            var productLinesQuery = _queryBuilder.For<SaleDocProduct>().Where(x => x.OwnerId == id).Build();
            var productLines = await _productLineQuery.GetAllAsync(productLinesQuery, ct);
            foreach (var pl in productLines)
                await _productLineCommand.DeleteAsync(pl, ct);

            doc.StateId = StateIdConst.PASSIVE;
            await _command.UpdateAsync(doc, ct);

            var newDocDto = await GetByIdInternalAsync(id, ct);
            if (newDocDto != null)
            {
                _auditLogService.SetNewValues(newDocDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.SaleDoc, id.ToString(), AuditLogOperationTypeConst.Delete);
            }

            return Result.Success();
        }, ct);

    public async Task<Result<List<SaleDocAvailableProductDto>>> GetAvailableProductsAsync(long id, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is null)
            return Result.Failure<List<SaleDocAvailableProductDto>>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var organizationId = _userContext.OrganizationId.Value;
        var documentQuery = _queryBuilder.For<SaleDoc>()
            .Where(x => x.Id == id && x.OrganizationId == organizationId)
            .Build();
        documentQuery.AddIncludes(b => b.Include(x => x.SaleDocProducts).ThenInclude(x => x.Product));
        documentQuery.AddIncludes(b => b.Include(x => x.SaleDocProducts).ThenInclude(x => x.SaleDocProductBatches));

        var document = await _query.GetAsync(documentQuery, ct);
        if (document is null)
            return Result.Failure<List<SaleDocAvailableProductDto>>(SaleDocErrors.NotFound(id, _userContext.LanguageId));

        var productLines = document.SaleDocProducts
            .Where(x => !x.Product.IsService && x.Product.IsPieceTracked)
            .ToList();
        if (productLines.Count == 0)
            return Result.Success(new List<SaleDocAvailableProductDto>());

        var productIds = productLines.Select(x => x.ProductId).Distinct().ToList();
        var inventoryResult = await _warehouseInventoryService.GetWarehouseProductsAsync(
            new WarehouseProductFilter
            {
                WarehouseId = document.WarehouseId,
                ProductIds = productIds
            },
            ct);
        if (!inventoryResult.IsSuccess)
            return Result.Failure<List<SaleDocAvailableProductDto>>(inventoryResult.Error);

        var availableBatchesByProductId = inventoryResult.Value
            .ToDictionary(x => x.ProductId, x => x.Batches);
        var availableBatchIds = availableBatchesByProductId.Values
            .SelectMany(x => x)
            .Select(x => x.BatchId)
            .Distinct()
            .ToList();

        var productTablesByBatchId = new Dictionary<long, List<AvailableProductTableRow>>();
        if (availableBatchIds.Count > 0)
        {
            var productTableQuery = _queryBuilder.For<WarehouseProductBatchTable>()
                .Where(x => availableBatchIds.Contains(x.BatchId) &&
                            x.Batch.OrganizationId == organizationId &&
                            x.Batch.WarehouseId == document.WarehouseId &&
                            x.Batch.ProductId == x.ProductTable.ProductId &&
                            x.ProductTable.WarehouseProductTable != null &&
                            x.ProductTable.WarehouseProductTable.WarehouseId == document.WarehouseId &&
                            x.ProductTable.WarehouseProductTable.StatusId == ProductTableStatusIdConst.IN_STOCK)
                .As(x => new AvailableProductTableRow
                {
                    BatchId = x.BatchId,
                    ProductId = x.ProductTable.ProductId,
                    ProductTableId = x.ProductTableId,
                    MarkingNumber = x.ProductTable.MarkingNumber
                })
                .Build();

            productTablesByBatchId = (await _warehouseProductBatchTableQuery.GetAllAsync(productTableQuery, ct))
                .GroupBy(x => x.BatchId)
                .ToDictionary(x => x.Key, x => x.OrderBy(item => item.ProductTableId).ToList());
        }

        var result = productLines.Select(line =>
        {
            var requiredBatchIds = line.SaleDocProductBatches
                .Select(x => x.WarehouseProductBatchId)
                .ToHashSet();
            var availableBatches = availableBatchesByProductId.GetValueOrDefault(line.ProductId) ?? [];

            return new SaleDocAvailableProductDto
            {
                SaleDocProductId = line.Id,
                ProductId = line.ProductId,
                Batches = availableBatches
                    .OrderBy(x => x.ReceivedDate)
                    .ThenBy(x => x.BatchId)
                    .Select(batch => new SaleDocAvailableProductBatchDto
                    {
                        BatchId = batch.BatchId,
                        BatchNumber = batch.BatchNumber,
                        BatchDate = batch.ReceivedDate,
                        IsRequired = requiredBatchIds.Contains(batch.BatchId),
                        ProductTables = (productTablesByBatchId.GetValueOrDefault(batch.BatchId) ?? [])
                            .Where(x => x.ProductId == line.ProductId)
                            .Take(GetProductTableLimit(batch.AvailableQuantity))
                            .Select(x => new SaleDocAvailableProductTableDto
                            {
                                ProductTableId = x.ProductTableId,
                                MarkingNumber = x.MarkingNumber
                            })
                            .ToList()
                    })
                    .Where(x => x.ProductTables?.Count > 0)
                    .ToList()
            };
        }).ToList();

        return Result.Success(result);
    }

    public async Task<Result<List<SaleDocAvailableProductDto>>> GetAvailableProductsAsync(int productId, int warehouseId, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is null)
            return Result.Failure<List<SaleDocAvailableProductDto>>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var organizationId = _userContext.OrganizationId.Value;
        var product = await _productQuery.GetAsync(
            _queryBuilder.For<Product>()
                .Where(x => x.Id == productId && x.OrganizationId == organizationId)
                .Build(),
            ct);
        if (product is null)
            return Result.Failure<List<SaleDocAvailableProductDto>>(SaleDocErrors.ProductNotFound(productId, _userContext.LanguageId));

        if (product.IsService)
            return Result.Success(new List<SaleDocAvailableProductDto>());

        var inventoryResult = await _warehouseInventoryService.GetWarehouseProductsAsync(
            new WarehouseProductFilter
            {
                WarehouseId = warehouseId,
                ProductIds = [productId]
            },
            ct);
        if (!inventoryResult.IsSuccess)
            return Result.Failure<List<SaleDocAvailableProductDto>>(inventoryResult.Error);

        var availableBatches = inventoryResult.Value
            .SingleOrDefault(x => x.ProductId == productId)
            ?.Batches ?? [];
        var batchIds = availableBatches.Select(x => x.BatchId).ToList();
        var productTablesByBatchId = new Dictionary<long, List<AvailableProductTableRow>>();
        if (product.IsPieceTracked && batchIds.Count > 0)
        {
            var productTableQuery = _queryBuilder.For<WarehouseProductBatchTable>()
                .Where(x => batchIds.Contains(x.BatchId) &&
                            x.Batch.OrganizationId == organizationId &&
                            x.Batch.WarehouseId == warehouseId &&
                            x.Batch.ProductId == productId &&
                            x.ProductTable.ProductId == productId &&
                            x.ProductTable.WarehouseProductTable != null &&
                            x.ProductTable.WarehouseProductTable.WarehouseId == warehouseId &&
                            x.ProductTable.WarehouseProductTable.StatusId == ProductTableStatusIdConst.IN_STOCK)
                .As(x => new AvailableProductTableRow
                {
                    BatchId = x.BatchId,
                    ProductId = x.ProductTable.ProductId,
                    ProductTableId = x.ProductTableId,
                    MarkingNumber = x.ProductTable.MarkingNumber
                })
                .Build();

            productTablesByBatchId = (await _warehouseProductBatchTableQuery.GetAllAsync(productTableQuery, ct))
                .GroupBy(x => x.BatchId)
                .ToDictionary(x => x.Key, x => x.OrderBy(item => item.ProductTableId).ToList());
        }

        var result = new SaleDocAvailableProductDto
        {
            ProductId = productId,
            Batches = availableBatches
                .OrderBy(x => x.ReceivedDate)
                .ThenBy(x => x.BatchId)
                .Select(batch => new SaleDocAvailableProductBatchDto
                {
                    BatchId = batch.BatchId,
                    BatchNumber = batch.BatchNumber,
                    BatchDate = batch.ReceivedDate,
                    IsRequired = false,
                    ProductTables = product.IsPieceTracked
                        ? (productTablesByBatchId.GetValueOrDefault(batch.BatchId) ?? [])
                        .Take(GetProductTableLimit(batch.AvailableQuantity))
                        .Select(x => new SaleDocAvailableProductTableDto
                        {
                            ProductTableId = x.ProductTableId,
                            MarkingNumber = x.MarkingNumber
                        })
                        .ToList()
                        : new()
                })
                .Where(x => !product.IsPieceTracked || x.ProductTables?.Count > 0)
                .ToList()
        };

        return Result.Success(new List<SaleDocAvailableProductDto> { result });
    }

    private async Task<Result<SaleShipmentLink?>> ValidateShipmentLinkAsync(
        SaleDocCreateDto dto,
        int organizationId,
        CancellationToken ct)
    {
        if (!dto.ShipmentId.HasValue)
            return Result.Success<SaleShipmentLink?>(null);

        var shipment = await _shipmentDocQuery.GetAsync(
            _queryBuilder.For<SaleShipmentDoc>()
                .Where(x => x.Id == dto.ShipmentId.Value && x.OrganizationId == organizationId)
                .Build(),
            ct);
        if (shipment is null)
            return Result.Failure<SaleShipmentLink?>(SaleShipmentErrors.NotFound(dto.ShipmentId.Value, _userContext.LanguageId));
        if (shipment.SaleDocId.HasValue)
            return Result.Failure<SaleShipmentLink?>(SaleShipmentErrors.AlreadyLinkedToSale(shipment.Id, _userContext.LanguageId));

        var requestedLinks = dto.Lines
            .Select((line, index) => new ShipmentLineLink(index, line.ShipmentProductId, line.ProductId, line.Quantity))
            .Where(link => link.ShipmentProductId.HasValue)
            .ToList();
        if (requestedLinks.GroupBy(link => link.ShipmentProductId!.Value).Any(group => group.Count() > 1))
            return Result.Failure<SaleShipmentLink?>(SaleShipmentErrors.DuplicateShipmentProductLink(_userContext.LanguageId));
        if (requestedLinks.Count == 0)
            return Result.Success<SaleShipmentLink?>(new SaleShipmentLink(shipment, []));

        var shipmentProductIds = requestedLinks
            .Select(link => link.ShipmentProductId!.Value)
            .ToList();
        var shipmentProducts = await _shipmentProductQuery.GetAllAsync(
            _queryBuilder.For<SaleShipmentProduct>()
                .Where(x => shipmentProductIds.Contains(x.Id) && x.OwnerId == shipment.Id)
                .Build(),
            ct);
        var shipmentProductsById = shipmentProducts.ToDictionary(x => x.Id);
        var links = new List<SaleShipmentProductLink>(requestedLinks.Count);

        foreach (var requestedLink in requestedLinks)
        {
            var shipmentProductId = requestedLink.ShipmentProductId!.Value;
            if (!shipmentProductsById.TryGetValue(shipmentProductId, out var shipmentProduct))
            {
                return Result.Failure<SaleShipmentLink?>(
                    SaleShipmentErrors.ShipmentProductNotFound(shipmentProductId, _userContext.LanguageId));
            }
            if (shipmentProduct.SaleDocProductId.HasValue)
            {
                return Result.Failure<SaleShipmentLink?>(
                    SaleShipmentErrors.ShipmentProductAlreadyLinked(shipmentProductId, _userContext.LanguageId));
            }
            if (shipmentProduct.ProductId != requestedLink.ProductId || shipmentProduct.Quantity != requestedLink.Quantity)
            {
                return Result.Failure<SaleShipmentLink?>(
                    SaleShipmentErrors.ShipmentProductMismatch(
                        shipmentProductId,
                        requestedLink.ProductId,
                        requestedLink.Quantity,
                        _userContext.LanguageId));
            }

            links.Add(new SaleShipmentProductLink(requestedLink.LineIndex, shipmentProduct));
        }

        return Result.Success<SaleShipmentLink?>(new SaleShipmentLink(shipment, links));
    }

    private async Task<Result> LinkShipmentAsync(
        SaleShipmentLink shipmentLink,
        long saleDocId,
        IReadOnlyList<SaleDocProduct> saleDocProducts,
        CancellationToken ct)
    {
        if (shipmentLink.Shipment.SaleDocId.HasValue)
            return Result.Failure(SaleShipmentErrors.AlreadyLinkedToSale(shipmentLink.Shipment.Id, _userContext.LanguageId));

        shipmentLink.Shipment.SaleDocId = saleDocId;
        await _shipmentDocCommand.UpdateAsync(shipmentLink.Shipment, ct);

        foreach (var productLink in shipmentLink.ProductLinks)
        {
            if (productLink.LineIndex < 0 || productLink.LineIndex >= saleDocProducts.Count)
                return Result.Failure(SaleShipmentErrors.ShipmentProductNotFound(productLink.ShipmentProduct.Id, _userContext.LanguageId));

            productLink.ShipmentProduct.SaleDocProductId = saleDocProducts[productLink.LineIndex].Id;
        }

        if (shipmentLink.ProductLinks.Count > 0)
            await _shipmentProductCommand.UpdateAsync(shipmentLink.ProductLinks.Select(x => x.ShipmentProduct), ct);

        return Result.Success();
    }
    private async Task<SaleDocDto?> GetByIdInternalAsync(long id, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is null)
            return null;

        var query = _queryBuilder.For<SaleDoc>()
            .Where(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value)
            .As<SaleDocDto>()
            .Build();
        return await _query.GetAsync(query, ct);
    }

    private async Task<List<SaleDocProduct>> GetProductLinesForAssemblyAsync(long saleDocId, CancellationToken ct)
    {
        var q = _queryBuilder.For<SaleDocProduct>().Where(x => x.OwnerId == saleDocId).Build();
        q.AddIncludes(b => b.Include(x => x.Product));
        q.AddIncludes(b => b.Include(x => x.SaleDocTables).ThenInclude(t => t.ProductTable).ThenInclude(t => t.Product));
        q.AddIncludes(b => b.Include(x => x.SaleDocTables).ThenInclude(t => t.ProductTable).ThenInclude(t => t.WarehouseProductTable));
        return await _productLineQuery.GetAllAsync(q, ct);
    }

    private async Task<Result> ApplyAssemblyAsync(SaleDoc doc, List<SaleDocProduct> productLines, IReadOnlyCollection<SaleDocProductAssemblyDto> dtos, CancellationToken ct)
    {
        if (productLines.Count == 0)
            return Result.Failure(SaleDocErrors.EmptyProducts(doc.Id, _userContext.LanguageId));

        var dtoByLineId = new Dictionary<long, SaleDocProductAssemblyDto>();
        foreach (var dto in dtos)
            if (!dtoByLineId.TryAdd(dto.Id, dto))
                return Result.Failure(SaleDocErrors.InvalidInventorySelection(_userContext.LanguageId));

        var lineIds = productLines.Select(x => x.Id).ToHashSet();
        var unknown = dtos.FirstOrDefault(x => !lineIds.Contains(x.Id));
        if (unknown != null)
            return Result.Failure(SaleDocErrors.LineNotFound(unknown.Id, _userContext.LanguageId));

        if (doc.StatusId == DocumentStatusIdConst.PENDING)
            return ValidateWarehouseConfirmedLines(doc, productLines);
        if (doc.StatusId == DocumentStatusIdConst.POSTED)
            return Result.Success();
        if (doc.StatusId == DocumentStatusIdConst.CANCELLED)
            return Result.Failure(SaleDocErrors.AlreadyCancelled(doc.Id, _userContext.LanguageId));
        if (doc.StatusId != DocumentStatusIdConst.DRAFT)
            return Result.Failure(SaleDocErrors.NotDraft(doc.Id, _userContext.LanguageId));

        var countGuard = await _activeInventoryCountGuardService.EnsureWarehouseIsNotBlockedAsync(doc.OrganizationId, doc.WarehouseId, "SaleAssembly", ct: ct);
        if (!countGuard.IsSuccess)
            return countGuard;

        foreach (var line in productLines.Where(x => x.Product.IsService || !x.Product.IsPieceTracked))
            if (dtoByLineId.TryGetValue(line.Id, out var dto) && dto.Items.Count > 0)
                return Result.Failure(SaleDocErrors.InvalidInventorySelection(_userContext.LanguageId));

        var goodsLines = productLines.Where(x => !x.Product.IsService).ToList();
        var pieceLines = goodsLines.Where(x => x.Product.IsPieceTracked).ToList();
        foreach (var line in pieceLines)
            if (!dtoByLineId.TryGetValue(line.Id, out var dto) || !dto.Assembled)
                return Result.Failure(SaleDocErrors.InvalidInventorySelection(_userContext.LanguageId));

        if (productLines.Any(x => x.SaleDocTables.Count > 0))
            return Result.Failure(SaleDocErrors.InvalidDraftInventoryState(doc.Id, _userContext.LanguageId));

        var selectedProductTableIds = pieceLines.SelectMany(x => dtoByLineId[x.Id].Items.Select(i => i.ProductTableId)).ToList();
        var selectionResult = await _priceCalculateService.SelectInventoryAsync(doc.OrganizationId, doc.WarehouseId, goodsLines.Select(x => new ProductTableSelectionRequestDto { LineId = x.Id, ProductId = x.ProductId, Quantity = x.Quantity, IsPieceTracked = x.Product.IsPieceTracked }).ToList(), selectedProductTableIds, ct);
        if (!selectionResult.IsSuccess)
            return Result.Failure(selectionResult.Error);

        var selectedItems = selectionResult.Value;
        var reserve = await _warehouseProductBalanceService.ReserveAsync(
            doc.WarehouseId,
            BuildWarehouseProductBalanceItems(goodsLines, selectedItems),
            selectedProductTableIds,
            ct);
        if (!reserve.IsSuccess)
            return Result.Failure(reserve.Error);
        var selectedByLineId = selectedItems.GroupBy(x => x.LineId).ToDictionary(x => x.Key, x => x.ToList());
        var rows = new List<SaleDocTable>();
        foreach (var line in pieceLines)
        {
            var matched = selectedByLineId.GetValueOrDefault(line.Id) ?? new List<ProductTableSelectionDto>();
            foreach (var item in matched)
            {
                var vat = line.VatRateId.HasValue && line.VatAmount > 0 && line.Quantity > 0 ? Math.Round(line.VatAmount / line.Quantity, 2) : 0m;
                rows.Add(new SaleDocTable { OwnerId = line.Id, ProductTableId = item.ProductTableId, CostPrice = line.CostPrice, Amount = line.UnitPrice, VatRateId = line.VatRateId, VatAmount = vat, TotalAmount = line.UnitPrice + vat });
            }
        }

        if (rows.Count > 0)
            await _lineCommand.CreateAsync(rows, ct);

        var reservedLines = await GetProductLinesForAssemblyAsync(doc.Id, ct);
        var reservedLinesValidation = ValidateWarehouseConfirmedLines(doc, reservedLines);
        if (!reservedLinesValidation.IsSuccess)
            return reservedLinesValidation;

        doc.StatusId = DocumentStatusIdConst.PENDING;
        await _command.UpdateAsync(doc, ct);
        await CreateAssemblyAuditAsync(doc.Id, ct);
        return Result.Success();
    }

    private async Task CreateAssemblyAuditAsync(long id, CancellationToken ct)
    {
        var dto = await GetByIdInternalAsync(id, ct);
        if (dto != null)
        {
            _auditLogService.SetNewValues(dto);
            await _auditLogService.CreateAsync(AuditLogTableConst.SaleDoc, id.ToString(), AuditLogOperationTypeConst.Update, "Assembly");
        }
    }

    private static List<SaleDocProductAssemblyDto> BuildAssemblyDtosFromCreate(IReadOnlyList<SaleDocProduct> productLines, IReadOnlyList<SaleDocCreateProductDto> lineDtos)
    {
        var count = Math.Min(productLines.Count, lineDtos.Count);
        var result = new List<SaleDocProductAssemblyDto>(count);
        for (var i = 0; i < count; i++)
            result.Add(new SaleDocProductAssemblyDto { Id = productLines[i].Id, Assembled = lineDtos[i].Assembled || !productLines[i].Product.IsPieceTracked, Items = lineDtos[i].Items.Select(x => new SaleDocAssemblyItemDto { ProductTableId = x.ProductTableId }).ToList() });
        return result;
    }
    
    private Result ValidateWarehouseConfirmedLines(SaleDoc document, List<SaleDocProduct> productLines)
    {
        foreach (var line in productLines)
        {
            if (line.Product.IsService || !line.Product.IsPieceTracked)
            {
                if (line.SaleDocTables.Count > 0)
                    return Result.Failure(SaleDocErrors.ServiceItemsNotAllowed(line.ProductId, _userContext.LanguageId));

                continue;
            }

            if (line.Quantity != decimal.Truncate(line.Quantity) ||
                line.SaleDocTables.Count != (int)line.Quantity)
            {
                return Result.Failure(SaleDocErrors.QuantityMismatch(
                    line.Id,
                    line.Quantity,
                    line.SaleDocTables.Count,
                    _userContext.LanguageId));
            }

            if (line.SaleDocTables.Select(x => x.ProductTableId).Distinct().Count() != line.SaleDocTables.Count)
                return Result.Failure(SaleDocErrors.InvalidInventorySelection(_userContext.LanguageId));

            var hasInvalidItem = line.SaleDocTables.Any(x =>
                x.ProductTable.ProductId != line.ProductId ||
                x.ProductTable.Product.OrganizationId != document.OrganizationId ||
                x.ProductTable.WarehouseProductTable == null ||
                x.ProductTable.WarehouseProductTable.WarehouseId != document.WarehouseId ||
                x.ProductTable.WarehouseProductTable.StatusId != ProductTableStatusIdConst.RESERVED ||
                x.ProductTable.Product.StateId != StateIdConst.ACTIVE);

            if (hasInvalidItem)
                return Result.Failure(SaleDocErrors.InvalidDraftInventoryState(document.Id, _userContext.LanguageId));
        }

        return Result.Success();
    }

    private async Task<Result<List<SaleDocProduct>>> BuildProductLinesAsync(List<SaleDocCreateProductDto> products, CancellationToken ct)
    {
        var lines = new List<SaleDocProduct>(products.Count);
        var productIds = products.Select(x => x.ProductId).Distinct().ToList();
        var vatRateIds = products.Where(x => x.VatRateId.HasValue).Select(x => x.VatRateId!.Value).Distinct().ToList();

        var productsQuery = _queryBuilder.For<Product>()
            .Where(x => productIds.Contains(x.Id))
            .Build();
        var productById = (await _productQuery.GetAllAsync(productsQuery, ct)).ToDictionary(x => x.Id);

        var vatRateById = new Dictionary<short, VatRate>();
        if (vatRateIds.Count > 0)
        {
            var vatRatesQuery = _queryBuilder.For<VatRate>()
                .Where(x => vatRateIds.Contains(x.Id))
                .Build();
            vatRateById = (await _vatRateQuery.GetAllAsync(vatRatesQuery, ct)).ToDictionary(x => x.Id);
        }

        foreach (var p in products)
        {
            if (!productById.TryGetValue(p.ProductId, out var product))
                return Result.Failure<List<SaleDocProduct>>(SaleDocErrors.ProductNotFound(p.ProductId, _userContext.LanguageId));

            if (p.Quantity <= 0 || (product.IsPieceTracked && p.Quantity != decimal.Truncate(p.Quantity)))
                return Result.Failure<List<SaleDocProduct>>(SaleDocErrors.InvalidProductQuantity(p.ProductId, p.Quantity, _userContext.LanguageId));

            if (p.UnitPrice < 0)
                return Result.Failure<List<SaleDocProduct>>(SaleDocErrors.InvalidProductUnitPrice(p.ProductId, p.UnitPrice, _userContext.LanguageId));

            if (p.CostPrice < 0)
                return Result.Failure<List<SaleDocProduct>>(SaleDocErrors.InvalidProductCostPrice(p.ProductId, p.CostPrice, _userContext.LanguageId));

            var vatAmount = 0m;
            if (p.VatRateId.HasValue)
            {
                if (!vatRateById.TryGetValue(p.VatRateId.Value, out var vatRate))
                    return Result.Failure<List<SaleDocProduct>>(SaleDocTableErrors.VatRateNotFound(p.VatRateId.Value, _userContext.LanguageId));

                vatAmount = Math.Round(p.Quantity * p.UnitPrice * vatRate.Rate / 100, 8);
            }

            var amount = p.Quantity * p.UnitPrice;
            var unitId = p.UnitId > 0 ? p.UnitId : product.UnitId;

            lines.Add(new SaleDocProduct
            {
                ProductId = p.ProductId,
                Quantity = p.Quantity,
                UnitId = unitId,
                UnitPrice = p.UnitPrice,
                CostPrice = p.CostPrice,
                Amount = amount,
                VatRateId = p.VatRateId,
                InventoryAccountId = p.InventoryAccountId,
                IncomeAccountId = p.IncomeAccountId,
                CostAccountId = p.CostAccountId,
                VatAmount = vatAmount,
                TotalAmount = amount + vatAmount,
            });
        }

        return lines;
    }

    private Result AttachProductBatchSelections(
        IReadOnlyList<SaleDocProduct> productLines,
        IReadOnlyList<SaleDocCreateProductDto> lineDtos)
    {
        if (productLines.Count != lineDtos.Count)
            return Result.Failure(SaleDocErrors.InvalidInventorySelection(_userContext.LanguageId));

        for (var index = 0; index < productLines.Count; index++)
        {
            var productLine = productLines[index];
            var selections = lineDtos[index].ProductBatches;
            if (selections.Count == 0)
                continue;

            if (selections.Any(item => item.BatchId <= 0 || item.Quantity <= 0m) ||
                selections.GroupBy(item => item.BatchId).Any(group => group.Count() > 1) ||
                selections.Sum(item => item.Quantity) > productLine.Quantity)
            {
                return Result.Failure(SaleDocErrors.InvalidInventorySelection(_userContext.LanguageId));
            }

            foreach (var selection in selections)
            {
                productLine.SaleDocProductBatches.Add(new SaleDocProductBatch
                {
                    WarehouseProductBatchId = selection.BatchId,
                    Quantity = selection.Quantity
                });
            }
        }

        return Result.Success();
    }
    private async Task<Result<List<SaleDocProduct>>> BuildProductLinesFromUpdateAsync(List<SaleDocUpdateProductDto> products, CancellationToken ct)
    {
        var lines = new List<SaleDocProduct>(products.Count);
        var productIds = products.Select(x => x.ProductId).Distinct().ToList();
        var vatRateIds = products.Where(x => x.VatRateId.HasValue).Select(x => x.VatRateId!.Value).Distinct().ToList();

        var productsQuery = _queryBuilder.For<Product>()
            .Where(x => productIds.Contains(x.Id))
            .Build();
        var productById = (await _productQuery.GetAllAsync(productsQuery, ct)).ToDictionary(x => x.Id);

        var vatRateById = new Dictionary<short, VatRate>();
        if (vatRateIds.Count > 0)
        {
            var vatRatesQuery = _queryBuilder.For<VatRate>()
                .Where(x => vatRateIds.Contains(x.Id))
                .Build();
            vatRateById = (await _vatRateQuery.GetAllAsync(vatRatesQuery, ct)).ToDictionary(x => x.Id);
        }

        foreach (var p in products)
        {
            if (!productById.TryGetValue(p.ProductId, out var product))
                return Result.Failure<List<SaleDocProduct>>(SaleDocErrors.ProductNotFound(p.ProductId, _userContext.LanguageId));

            if (p.Quantity <= 0 || (product.IsPieceTracked && p.Quantity != decimal.Truncate(p.Quantity)))
                return Result.Failure<List<SaleDocProduct>>(SaleDocErrors.InvalidProductQuantity(p.Id ?? p.ProductId, p.Quantity, _userContext.LanguageId));

            if (p.UnitPrice < 0)
                return Result.Failure<List<SaleDocProduct>>(SaleDocErrors.InvalidProductUnitPrice(p.Id ?? p.ProductId, p.UnitPrice, _userContext.LanguageId));

            if (p.CostPrice < 0)
                return Result.Failure<List<SaleDocProduct>>(SaleDocErrors.InvalidProductCostPrice(p.Id ?? p.ProductId, p.CostPrice, _userContext.LanguageId));

            var vatAmount = 0m;
            if (p.VatRateId.HasValue)
            {
                if (!vatRateById.TryGetValue(p.VatRateId.Value, out var vatRate))
                    return Result.Failure<List<SaleDocProduct>>(SaleDocTableErrors.VatRateNotFound(p.VatRateId.Value, _userContext.LanguageId));

                vatAmount = Math.Round(p.Quantity * p.UnitPrice * vatRate.Rate / 100, 2);
            }

            var amount = p.Quantity * p.UnitPrice;
            var unitId = p.UnitId > 0 ? p.UnitId : product.UnitId;

            lines.Add(new SaleDocProduct
            {
                ProductId = p.ProductId,
                Quantity = p.Quantity,
                UnitId = unitId,
                UnitPrice = p.UnitPrice,
                CostPrice = p.CostPrice,
                Amount = amount,
                VatRateId = p.VatRateId,
                InventoryAccountId = p.InventoryAccountId,
                IncomeAccountId = p.IncomeAccountId,
                CostAccountId = p.CostAccountId,
                VatAmount = vatAmount,
                TotalAmount = amount + vatAmount,
            });
        }

        return lines;
    }

    private sealed record ShipmentLineLink(int LineIndex, long? ShipmentProductId, int ProductId, decimal Quantity);

    private sealed record SaleShipmentProductLink(int LineIndex, SaleShipmentProduct ShipmentProduct);

    private sealed record SaleShipmentLink(
        SaleShipmentDoc Shipment,
        IReadOnlyList<SaleShipmentProductLink> ProductLinks);
    private sealed class AvailableProductTableRow
    {
        public long BatchId { get; init; }
        public int ProductId { get; init; }
        public int ProductTableId { get; init; }
        public string? MarkingNumber { get; init; }
    }

    private static int GetProductTableLimit(decimal availableQuantity) =>
        availableQuantity <= 0m
            ? 0
            : availableQuantity >= int.MaxValue
                ? int.MaxValue
                : (int)decimal.Floor(availableQuantity);

    private static List<WarehouseProductBalanceItem> BuildWarehouseProductBalanceItems(
        IReadOnlyCollection<SaleDocProduct> productLines,
        IReadOnlyCollection<ProductTableSelectionDto> selectedItems)
    {
        var unitIdByProductId = productLines
            .GroupBy(x => x.ProductId)
            .ToDictionary(x => x.Key, x => x.First().UnitId);

        var pieceTrackedItems = selectedItems
            .GroupBy(x => x.ProductId)
            .Select(x => new WarehouseProductBalanceItem(
                x.Key,
                unitIdByProductId[x.Key],
                x.Count()))
            .ToList();

        var nonPieceTrackedItems = productLines
            .Where(x => !x.Product.IsService && !x.Product.IsPieceTracked)
            .Select(x => new WarehouseProductBalanceItem(
                x.ProductId,
                x.UnitId,
                x.Quantity))
            .ToList();

        return pieceTrackedItems
            .Concat(nonPieceTrackedItems)
            .Where(x => x.Quantity > 0m)
            .GroupBy(x => new { x.ProductId, x.UnitId })
            .Select(x => new WarehouseProductBalanceItem(
                x.Key.ProductId,
                x.Key.UnitId,
                x.Sum(i => i.Quantity)))
            .ToList();
    }
}
