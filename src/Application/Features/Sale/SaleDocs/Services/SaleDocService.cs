using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.Inv.ProductPrices;
using Application.Features.AuditLogs;
using Application.Features.CounterpartyCards;
using Application.Features.InventoryRegisterBalances;
using Application.Features.Register.AccountingRegisterEntries;
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
    private readonly IAccountingDispatcher _dispatcher;
    private readonly IInventoryDispatcher _inventoryDispatcher;
    private readonly IQueryRepository<SaleDoc> _query;
    private readonly IQueryRepository<VatRate> _vatRateQuery;
    private readonly ICommandRepository<SaleDoc> _command;
    private readonly ICommandRepository<SaleDocProduct> _productLineCommand;
    private readonly IQueryRepository<SaleDocProduct> _productLineQuery;
    private readonly ICommandRepository<SaleDocTable> _lineCommand;
    private readonly IQueryRepository<SaleDocTable> _lineQuery;
    private readonly IQueryRepository<Warehouse> _warehouseQuery;
    private readonly IQueryRepository<CounterpartyCard> _counterpartyQuery;
    private readonly IQueryRepository<ProductTable> _productTableQuery;
    private readonly ICommandRepository<ProductTable> _productTableCommand;
    private readonly IProductPriceCalculateService _priceCalculateService;
    private readonly IProductTableReservationService _reservationService;
    private readonly IDocNumberGenerator _docNumberGenerator;

    public SaleDocService(IUserContext userContext,
                          IQueryBuilder queryBuilder,
                          IAuditLogService auditLogService,
                          IAccountingDispatcher dispatcher,
                          IInventoryDispatcher inventoryDispatcher,
                          IDocNumberGenerator docNumberGenerator,
                          IQueryRepository<SaleDoc> query,
                          IQueryRepository<VatRate> vatRateQuery,
                          ICommandRepository<SaleDoc> command,
                          ICommandRepository<SaleDocProduct> productLineCommand,
                          IQueryRepository<SaleDocProduct> productLineQuery,
                          ICommandRepository<SaleDocTable> lineCommand,
                          IQueryRepository<SaleDocTable> lineQuery,
                          IQueryRepository<Warehouse> warehouseQuery,
                          IQueryRepository<CounterpartyCard> counterpartyQuery,
                          IQueryRepository<ProductTable> productTableQuery,
                          ICommandRepository<ProductTable> productTableCommand,
                          IProductPriceCalculateService priceCalculateService,
                          IProductTableReservationService reservationService,
                          ILogger<SaleDocService> logger,
                          IUnitOfWork unitOfWork)
            : base(logger, unitOfWork)
    {
        _query = query;
        _command = command;
        _dispatcher = dispatcher;
        _lineCommand = lineCommand;
        _lineQuery = lineQuery;
        _productLineCommand = productLineCommand;
        _productLineQuery = productLineQuery;
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _auditLogService = auditLogService;
        _vatRateQuery = vatRateQuery;
        _warehouseQuery = warehouseQuery;
        _counterpartyQuery = counterpartyQuery;
        _productTableQuery = productTableQuery;
        _productTableCommand = productTableCommand;
        _priceCalculateService = priceCalculateService;
        _reservationService = reservationService;
        _inventoryDispatcher = inventoryDispatcher;
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
            var query = _queryBuilder.For<SaleDoc>().Where(x => x.Id == id).As<SaleDocDto>().Build();
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

            var docNumber = await _docNumberGenerator.GenerateAsync(orgId, "SAL", now, ct);

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
                DocDate = now,
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

            var docQuery = _queryBuilder.For<SaleDoc>().Where(x => x.Id == id).Build();
            var doc = await _query.GetAsync(docQuery, ct);

            if (doc == null)
                return Result.Failure(SaleDocErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(SaleDocErrors.NotDraft(id, _userContext.LanguageId));

            var productLinesQuery = _queryBuilder.For<SaleDocProduct>().Where(x => x.OwnerId == id).Build();
            var productLines = await _productLineQuery.GetAllAsync(productLinesQuery, ct);

            if (productLines.Count == 0)
                return Result.Failure(SaleDocErrors.EmptyProducts(id, _userContext.LanguageId));

            var selectionRequests = productLines
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

            var reserved = await _reservationService.TryReserveAsync(selectedProductTableIds, ct);
            if (!reserved)
                return Result.Failure(SaleDocErrors.InventoryReservationConflict(_userContext.LanguageId));

            var selectedByLineId = selectedItems
                .GroupBy(x => x.LineId)
                .ToDictionary(x => x.Key, x => x.ToList());

            var allNewLines = new List<SaleDocTable>();

            foreach (var productLine in productLines)
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
        ExecuteInTransactionAsync(nameof(ConfirmAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var docQuery = _queryBuilder.For<SaleDoc>().Where(x => x.Id == id).Build();
            var doc = await _query.GetAsync(docQuery, ct);

            if (doc == null)
                return Result.Failure(SaleDocErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId != DocumentStatusIdConst.PENDING)
                return Result.Failure(SaleDocErrors.NotPending(id, _userContext.LanguageId));

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto != null)
                _auditLogService.SetOldValues(oldDocDto);

            // SaleDocTable larni yangilash — bugalter sotuv narxini belgilaydi
            var productLinesQuery = _queryBuilder.For<SaleDocProduct>().Where(x => x.OwnerId == id).Build();
            var productLines = await _productLineQuery.GetAllAsync(productLinesQuery, ct);
            var productLineIds = productLines.Select(p => p.Id).ToList();

            var tablesQuery = _queryBuilder.For<SaleDocTable>().Where(x => productLineIds.Contains(x.OwnerId)).Build();
            var existingLines = await _lineQuery.GetAllAsync(tablesQuery, ct);

            foreach (var lineDto in dto.Lines)
            {
                var lineTables = existingLines.Where(l => l.OwnerId == lineDto.Id).ToList();
                if (lineTables.Count == 0)
                    return Result.Failure(SaleDocErrors.LineNotFound(lineDto.Id, _userContext.LanguageId));

                foreach (var line in lineTables)
                {
                    line.Amount = lineDto.UnitPrice;
                    line.CostPrice = lineDto.CostPrice;

                    if (line.VatRateId.HasValue)
                    {
                        var vatQuery = _queryBuilder.For<VatRate>().Where(v => v.Id == line.VatRateId.Value).Build();
                        var vatRate = await _vatRateQuery.GetAsync(vatQuery, ct);

                        if (vatRate != null)
                            line.VatAmount = Math.Round(line.Amount * vatRate.Rate / 100, 2);
                    }

                    line.TotalAmount = line.Amount + line.VatAmount;
                    await _lineCommand.UpdateAsync(line, ct);
                }
            }

            // SaleDocProduct summalarini qayta hisoblash
            foreach (var productLine in productLines)
            {
                var lineTables = existingLines.Where(t => t.OwnerId == productLine.Id).ToList();
                productLine.Amount = lineTables.Sum(t => t.Amount);
                productLine.UnitPrice = productLine.Quantity > 0 ? Math.Round(productLine.Amount / productLine.Quantity, 2) : 0;
                productLine.VatAmount = lineTables.Sum(t => t.VatAmount);
                productLine.TotalAmount = lineTables.Sum(t => t.TotalAmount);
                await _productLineCommand.UpdateAsync(productLine, ct);
            }

            // SaleDoc summalarini yangilash
            doc.TotalAmount = existingLines.Sum(l => l.Amount);
            doc.VatAmount = existingLines.Sum(l => l.VatAmount);
            doc.FinalAmount = existingLines.Sum(l => l.TotalAmount);
            doc.StatusId = DocumentStatusIdConst.POSTED;
            doc.PostedAt ??= DateTime.Now;
            doc.PostedByUserId ??= _userContext.Id;
            await _command.UpdateAsync(doc, ct);

            // ProductTable → SOLD
            var productTableIds = existingLines.Select(t => t.ProductTableId).ToList();
            await UpdateProductTableStatusesAsync(productTableIds, ProductTableStatusIdConst.SOLD, ct);

            // Provodka
            var fullDocQuery = _queryBuilder.For<SaleDoc>().Where(d => d.Id == doc.Id).Build();
            fullDocQuery.AddIncludes(b => b.Include(d => d.SaleDocProducts).ThenInclude(p => p.SaleDocTables));
            fullDocQuery.AddIncludes(b => b.Include(d => d.SaleDocProducts).ThenInclude(p => p.SaleDocTables).ThenInclude(t => t.ProductTable));
            var fullDoc = await _query.GetAsync(fullDocQuery, ct) ?? doc;

            var dispatch = await _dispatcher.ProcessAsync(fullDoc, ct);
            if (!dispatch.IsSuccess)
                return Result.Failure(dispatch.Error);

            var inventoryDispatch = await _inventoryDispatcher.ProcessAsync(fullDoc, ct);
            if (!inventoryDispatch.IsSuccess)
                return Result.Failure(inventoryDispatch.Error);

            var newDocDto = await GetByIdInternalAsync(id, ct);
            if (newDocDto != null)
            {
                _auditLogService.SetNewValues(newDocDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.SaleDoc, id.ToString(), AuditLogOperationTypeConst.Update, "Confirmed");
            }

            return Result.Success();
        }, ct);

    /// <summary>
    /// Bekor qilish — istalgan bosqichdan. ProductTable → IN_STOCK ga qaytariladi.
    /// </summary>
    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CancelAsync), async () =>
        {
            var docQuery = _queryBuilder.For<SaleDoc>().Where(x => x.Id == id).Build();
            var doc = await _query.GetAsync(docQuery, ct);

            if (doc == null)
                return Result.Failure(SaleDocErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Failure(SaleDocErrors.AlreadyCancelled(id, _userContext.LanguageId));

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto != null)
                _auditLogService.SetOldValues(oldDocDto);

            // PENDING yoki POSTED bo'lsa ProductTable larni IN_STOCK ga qaytarish
            if (doc.StatusId == DocumentStatusIdConst.PENDING || doc.StatusId == DocumentStatusIdConst.POSTED)
            {
                var productLinesQuery = _queryBuilder.For<SaleDocProduct>().Where(x => x.OwnerId == id).Build();
                var productLines = await _productLineQuery.GetAllAsync(productLinesQuery, ct);
                var productLineIds = productLines.Select(p => p.Id).ToList();

                var tablesQuery = _queryBuilder.For<SaleDocTable>().Where(x => productLineIds.Contains(x.OwnerId)).Build();
                var tables = await _lineQuery.GetAllAsync(tablesQuery, ct);
                var productTableIds = tables.Select(t => t.ProductTableId).ToList();

                if (productTableIds.Count > 0)
                    await UpdateProductTableStatusesAsync(productTableIds, ProductTableStatusIdConst.IN_STOCK, ct);
            }

            doc.StatusId = DocumentStatusIdConst.CANCELLED;
            doc.CancelledAt ??= DateTime.Now;
            doc.CancelledByUserId ??= _userContext.Id;
            await _command.UpdateAsync(doc, ct);

            var newDocDto = await GetByIdInternalAsync(id, ct);
            if (newDocDto != null)
            {
                _auditLogService.SetNewValues(newDocDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.SaleDoc, id.ToString(), AuditLogOperationTypeConst.Update, "Cancelled");
            }

            return Result.Success();
        }, ct);

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

            var query = _queryBuilder.For<SaleDoc>().Where(x => x.Id == id).Build();
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
            var query = _queryBuilder.For<SaleDoc>().Where(x => x.Id == id).Build();
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
        var query = _queryBuilder.For<SaleDoc>().Where(x => x.Id == id).As<SaleDocDto>().Build();
        return await _query.GetAsync(query, ct);
    }

    private async Task UpdateProductTableStatusesAsync(List<int> productTableIds, short statusId, CancellationToken ct)
    {
        var ptQuery = _queryBuilder.For<ProductTable>()
            .Where(x => productTableIds.Contains(x.Id))
            .Build();
        var productTables = await _productTableQuery.GetAllAsync(ptQuery);

        foreach (var pt in productTables)
            pt.StatusId = statusId;

        await _productTableCommand.UpdateAsync(productTables, ct);
    }

    private async Task<Result<List<SaleDocProduct>>> BuildProductLinesAsync(List<SaleDocCreateProductDto> products, CancellationToken ct)
    {
        var lines = new List<SaleDocProduct>(products.Count);

        foreach (var p in products)
        {
            var vatAmount = 0m;
            if (p.VatRateId.HasValue)
            {
                var vatQuery = _queryBuilder.For<VatRate>().Where(v => v.Id == p.VatRateId.Value).Build();
                var vatRate = await _vatRateQuery.GetAsync(vatQuery, ct);
                if (vatRate == null)
                    return Result.Failure<List<SaleDocProduct>>(SaleDocTableErrors.VatRateNotFound(p.VatRateId.Value, _userContext.LanguageId));

                vatAmount = Math.Round(p.Quantity * p.UnitPrice * vatRate.Rate / 100, 8);
            }

            var amount = p.Quantity * p.UnitPrice;

            lines.Add(new SaleDocProduct
            {
                ProductId = p.ProductId,
                Quantity = p.Quantity,
                UnitId = p.UnitId,
                UnitPrice = p.UnitPrice,
                CostPrice = p.CostPrice,
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

        foreach (var p in products)
        {
            var vatAmount = 0m;
            if (p.VatRateId.HasValue)
            {
                var vatQuery = _queryBuilder.For<VatRate>().Where(v => v.Id == p.VatRateId.Value).Build();
                var vatRate = await _vatRateQuery.GetAsync(vatQuery, ct);
                if (vatRate == null)
                    return Result.Failure<List<SaleDocProduct>>(SaleDocTableErrors.VatRateNotFound(p.VatRateId.Value, _userContext.LanguageId));

                vatAmount = Math.Round(p.Quantity * p.UnitPrice * vatRate.Rate / 100, 2);
            }

            var amount = p.Quantity * p.UnitPrice;

            lines.Add(new SaleDocProduct
            {
                ProductId = p.ProductId,
                Quantity = p.Quantity,
                UnitId = p.UnitId,
                UnitPrice = p.UnitPrice,
                CostPrice = p.CostPrice,
                Amount = amount,
                VatRateId = p.VatRateId,
                VatAmount = vatAmount,
                TotalAmount = amount + vatAmount,
            });
        }

        return lines;
    }

}
