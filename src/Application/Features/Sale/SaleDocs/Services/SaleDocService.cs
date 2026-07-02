using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.Inv.ProductPrices;
using Application.Features.AuditLogs;
using Application.Features.CounterpartyCards;
using Application.Features.InventoryCounts;
using Application.Features.SaleDocTables;
using Application.Features.Warehouses;
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
    private readonly IProductPriceCalculateService _priceCalculateService;
    private readonly IProductTableReservationService _reservationService;
    private readonly IActiveInventoryCountGuardService _activeInventoryCountGuardService;
    private readonly IDocNumberGenerator _docNumberGenerator;

    public SaleDocService(IUserContext userContext,
                          IQueryBuilder queryBuilder,
                          IAuditLogService auditLogService,
                          ISaleLifecycleService saleLifecycleService,
                          IDocumentPostingLock postingLock,
                          IDocNumberGenerator docNumberGenerator,
                          IQueryRepository<SaleDoc> query,
                          IQueryRepository<VatRate> vatRateQuery,
                          ICommandRepository<SaleDoc> command,
                          ICommandRepository<SaleDocProduct> productLineCommand,
                          IQueryRepository<SaleDocProduct> productLineQuery,
                          ICommandRepository<SaleDocTable> lineCommand,
                          IQueryRepository<Warehouse> warehouseQuery,
                          IQueryRepository<CounterpartyCard> counterpartyQuery,
                          IQueryRepository<Product> productQuery,
                          IProductPriceCalculateService priceCalculateService,
                          IProductTableReservationService reservationService,
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
        _priceCalculateService = priceCalculateService;
        _reservationService = reservationService;
        _activeInventoryCountGuardService = activeInventoryCountGuardService;
        _docNumberGenerator = docNumberGenerator;
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
    /// Bosqich 1: Bugalter sotuv hujjatini yaratadi.
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

            var docNumber = await _docNumberGenerator.GenerateAsync(orgId, "SAL", docDate, ct);

            var warehouseQuery = _queryBuilder.For<Warehouse>().Where(x => x.Id == dto.WarehouseId).Build();
            var warehouse = await _warehouseQuery.GetAsync(warehouseQuery, ct);
            if (warehouse is null)
                return Result.Failure<long>(WarehouseErrors.NotFound(dto.WarehouseId, _userContext.LanguageId));

            var counterpartyQuery = _queryBuilder.For<CounterpartyCard>().Where(x => x.Id == dto.CounterpartyId).Build();
            var counterparty = await _counterpartyQuery.GetAsync(counterpartyQuery, ct);
            if (counterparty is null)
                return Result.Failure<long>(CounterpartyCardErrors.NotFound(dto.CounterpartyId, _userContext.LanguageId));

            var productsResult = await BuildProductLinesAsync(dto.Lines, ct);
            if (!productsResult.IsSuccess)
                return Result.Failure<long>(productsResult.Error);

            var productLines = productsResult.Value;

            var doc = new SaleDoc
            {
                OrganizationId = orgId,
                DocNumber = docNumber,
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
            };

            await _command.CreateAsync(doc, ct);

            var docDto = await GetByIdInternalAsync(doc.Id, ct);
            if (docDto != null)
            {
                _auditLogService.SetNewValues(docDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.SaleDoc, doc.Id.ToString(), AuditLogOperationTypeConst.Create);
            }

            return Result.Success(doc.Id);
        }, ct);

    /// <summary>
    /// Bosqich 2: Skladchik tasdiqlaydi — aniq ProductTable elementlarini tanlaydi.
    /// Faqat productTableId lar keladi, sistema ProductId bo'yicha SaleDocProduct ga moslashtiradi.
    /// SaleDocTable yaratiladi, ProductTable → RESERVED, SaleDoc → PENDING.
    /// </summary>
    public Task<Result> WarehouseConfirmAsync(long id, SaleDocWarehouseConfirmDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(WarehouseConfirmAsync), async () =>
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

            var productLinesQuery = _queryBuilder.For<SaleDocProduct>().Where(x => x.OwnerId == id).Build();
            productLinesQuery.AddIncludes(b => b.Include(x => x.Product));
            productLinesQuery.AddIncludes(b => b.Include(x => x.SaleDocTables).ThenInclude(t => t.ProductTable));
            var productLines = await _productLineQuery.GetAllAsync(productLinesQuery, ct);

            if (productLines.Count == 0)
                return Result.Failure(SaleDocErrors.EmptyProducts(id, _userContext.LanguageId));

            var goodsProductLines = productLines
                .Where(x => !x.Product.IsService)
                .ToList();

            if (doc.StatusId == DocumentStatusIdConst.PENDING)
                return ValidateWarehouseConfirmedLines(id, productLines);

            if (doc.StatusId == DocumentStatusIdConst.POSTED)
                return Result.Success();

            if (doc.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Failure(SaleDocErrors.AlreadyCancelled(id, _userContext.LanguageId));

            if (doc.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(SaleDocErrors.NotDraft(id, _userContext.LanguageId));

            var countGuard = await _activeInventoryCountGuardService.EnsureWarehouseIsNotBlockedAsync(doc.OrganizationId, doc.WarehouseId, "SaleWarehouseConfirm", ct: ct);
            if (!countGuard.IsSuccess)
                return countGuard;

            if (goodsProductLines.Count == 0 && dto.Items.Count > 0)
                return Result.Failure(SaleDocErrors.InvalidInventorySelection(_userContext.LanguageId));

            if (goodsProductLines.Count == 0)
                return Result.Success();

            if (productLines.Any(x => x.SaleDocTables.Count > 0))
                return Result.Failure(SaleDocErrors.InvalidDraftInventoryState(id, _userContext.LanguageId));

            var selectionRequests = goodsProductLines
                .Select(x => new ProductTableSelectionRequestDto
                {
                    LineId = x.Id,
                    ProductId = x.ProductId,
                    Quantity = x.Quantity
                })
                .ToList();

            var selectedProductTableIds = dto.Items.Select(x => x.ProductTableId).ToList();
            var selectionResult = await _priceCalculateService.SelectInventoryAsync(
                doc.OrganizationId,
                doc.WarehouseId,
                selectionRequests,
                selectedProductTableIds,
                ct);
            if (!selectionResult.IsSuccess)
                return Result.Failure(selectionResult.Error);

            var selectedItems = selectionResult.Value;

            var reserved = await _reservationService.TryReserveAsync(doc.WarehouseId, selectedProductTableIds, ct);
            if (!reserved)
                return Result.Failure(SaleDocErrors.InventoryReservationConflict(_userContext.LanguageId));

            var selectedByLineId = selectedItems
                .GroupBy(x => x.LineId)
                .ToDictionary(x => x.Key, x => x.ToList());

            var allNewLines = new List<SaleDocTable>();

            foreach (var productLine in goodsProductLines)
            {
                var matchedPts = selectedByLineId.GetValueOrDefault(productLine.Id) ?? new List<ProductTableSelectionDto>();

                foreach (var pt in matchedPts)
                {
                    var vatAmount = 0m;
                    if (productLine.VatRateId.HasValue && productLine.VatAmount > 0 && productLine.Quantity > 0)
                        vatAmount = Math.Round(productLine.VatAmount / productLine.Quantity, 2);

                    allNewLines.Add(new SaleDocTable
                    {
                        OwnerId = productLine.Id,
                        ProductTableId = pt.ProductTableId,
                        CostPrice = pt.CostPrice,
                        Amount = productLine.UnitPrice,
                        VatRateId = productLine.VatRateId,
                        VatAmount = vatAmount,
                        TotalAmount = productLine.UnitPrice + vatAmount,
                    });
                }

                productLine.CostPrice = matchedPts.Sum(pt => pt.CostPrice);
                await _productLineCommand.UpdateAsync(productLine, ct);
            }

            if (allNewLines.Count > 0)
                await _lineCommand.CreateAsync(allNewLines, ct);

            doc.StatusId = DocumentStatusIdConst.PENDING;
            await _command.UpdateAsync(doc, ct);

            var docDto = await GetByIdInternalAsync(id, ct);
            if (docDto != null)
            {
                _auditLogService.SetNewValues(docDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.SaleDoc, id.ToString(), AuditLogOperationTypeConst.Update, "WarehouseConfirm");
            }

            return Result.Success();
        }, ct);

    /// <summary>
    /// Bosqich 3: Bugalter tasdiqlaydi — har bir SaleDocTable uchun sotuv narxini belgilaydi.
    /// SaleDoc → POSTED, ProductTable → SOLD, provodka yaratiladi.
    /// </summary>
    public Task<Result> ConfirmAsync(long id, SaleDocConfirmDto dto, CancellationToken ct = default) =>
        _saleLifecycleService.ConfirmAsync(id, dto, ct);


    /// <summary>
    /// Bekor qilish — istalgan bosqichdan. ProductTable → IN_STOCK ga qaytariladi.
    /// </summary>
    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        _saleLifecycleService.CancelAsync(id, ct);

    /// <summary>
    /// O'zgartirish — faqat DRAFT va PENDING bosqichlarda.
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

    // ──────────────── Private helpers ────────────────

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

    private Result ValidateWarehouseConfirmedLines(long documentId, List<SaleDocProduct> productLines)
    {
        foreach (var line in productLines)
        {
            if (line.Product.IsService)
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

            var hasInvalidItem = line.SaleDocTables.Any(x =>
                x.ProductTable.ProductId != line.ProductId ||
                x.ProductTable.StatusId != ProductTableStatusIdConst.RESERVED ||
                x.ProductTable.StateId != StateIdConst.ACTIVE);

            if (hasInvalidItem)
                return Result.Failure(SaleDocErrors.InvalidDraftInventoryState(documentId, _userContext.LanguageId));
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

            if (p.Quantity <= 0 || (!product.IsService && p.Quantity != decimal.Truncate(p.Quantity)))
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
                CostPrice = product.IsService ? p.CostPrice : 0m,
                Amount = amount,
                VatRateId = p.VatRateId,
                VatAmount = vatAmount,
                TotalAmount = amount + vatAmount,
            });
        }

        return lines;
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

            if (p.Quantity <= 0 || (!product.IsService && p.Quantity != decimal.Truncate(p.Quantity)))
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
                CostPrice = product.IsService ? p.CostPrice : 0m,
                Amount = amount,
                VatRateId = p.VatRateId,
                VatAmount = vatAmount,
                TotalAmount = amount + vatAmount,
            });
        }

        return lines;
    }

}
