using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
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
    private readonly ICommandRepository<SaleDocTable> _lineCommand;
    private readonly IQueryRepository<SaleDocTable> _lineQuery;
    private readonly IQueryRepository<Warehouse> _warehouseQuery;
    private readonly IQueryRepository<CounterpartyCard> _counterpartyQuery;
    private readonly IQueryRepository<ProductTable> _productTableQuery;
    private readonly ICommandRepository<ProductTable> _productTableCommand;
    private readonly IQueryRepository<PurchaseDocTable> _purchaseDocTableQuery;
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
                          ICommandRepository<SaleDocTable> lineCommand,
                          IQueryRepository<SaleDocTable> lineQuery,
                          IQueryRepository<Warehouse> warehouseQuery,
                          IQueryRepository<CounterpartyCard> counterpartyQuery,
                          IQueryRepository<ProductTable> productTableQuery,
                          ICommandRepository<ProductTable> productTableCommand,
                          IQueryRepository<PurchaseDocTable> purchaseDocTableQuery,
                          ILogger<SaleDocService> logger,
                          IUnitOfWork unitOfWork)
            : base(logger, unitOfWork)
    {
        _query                  = query;
        _command                = command;
        _dispatcher             = dispatcher;
        _lineCommand            = lineCommand;
        _lineQuery              = lineQuery;
        _userContext            = userContext;
        _queryBuilder           = queryBuilder;
        _auditLogService        = auditLogService;
        _vatRateQuery           = vatRateQuery;
        _warehouseQuery         = warehouseQuery;
        _counterpartyQuery      = counterpartyQuery;
        _productTableQuery      = productTableQuery;
        _productTableCommand    = productTableCommand;
        _purchaseDocTableQuery  = purchaseDocTableQuery;
        _inventoryDispatcher    = inventoryDispatcher;
        _docNumberGenerator     = docNumberGenerator;
    }

    public Task<Result<PagedResponse<SaleDocListDto>>> GetAllAsync(SaleDocListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            var query     = _queryBuilder.BuildPaged<SaleDoc, SaleDocListDto, SaleDocListFilter>(filter);
            var pagedList = await _query.GetPagedAsync(query, ct);
            return Result.Success(PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize));
        });

    public Task<Result<SaleDocDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            var query  = _queryBuilder.For<SaleDoc>().Where(x => x.Id == id).As<SaleDocDto>().Build();
            var entity = await _query.GetAsync(query, ct);

            if (entity == null)
                return Result.Failure<SaleDocDto>(SaleDocErrors.NotFound(id, _userContext.LanguageId));

            return Result.Success(entity);
        });

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

            var productTableIds = dto.Lines.Select(l => l.ProductTableId).ToList();
            var linesResult = await BuildDraftLinesAsync(orgId, productTableIds, ct);
            if (!linesResult.IsSuccess)
                return Result.Failure<long>(linesResult.Error);

            var lines = linesResult.Value;

            var doc = new SaleDoc
            {
                OrganizationId = orgId,
                DocNumber      = docNumber,
                DocDate        = now,
                CurrencyId     = dto.CurrencyId,
                SaleDocTables  = lines,
                TotalAmount    = lines.Sum(l => l.Amount),
                VatAmount      = 0,
                FinalAmount    = lines.Sum(l => l.Amount),
                StatusId       = DocumentStatusIdConst.DRAFT,
                Comment        = dto.Comment,
                StateId        = StateIdConst.ACTIVE,
                CreatedDate    = now,
                WarehouseId    = dto.WarehouseId,
                CounterpartyId = dto.CounterpartyId,
            };

            await _command.CreateAsync(doc, ct);

            await UpdateProductTableStatusesAsync(doc.SaleDocTables.Select(s => s.ProductTable).ToList(), ProductTableStatusIdConst.RESERVED, ct);

            var docDto = await GetByIdInternalAsync(doc.Id, ct);
            if (docDto != null)
            {
                _auditLogService.SetNewValues(docDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.SaleDoc, doc.Id.ToString(), AuditLogOperationTypeConst.Create);
            }

            return Result.Success(doc.Id);
        }, ct);

    public Task<Result> ConfirmAsync(long id, SaleDocConfirmDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(ConfirmAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var docQuery = _queryBuilder.For<SaleDoc>().Where(x => x.Id == id).Build();
            var doc = await _query.GetAsync(docQuery, ct);

            if (doc == null)
                return Result.Failure(SaleDocErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.POSTED)
                return Result.Failure(SaleDocErrors.AlreadyPosted(id, _userContext.LanguageId));

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto != null)
                _auditLogService.SetOldValues(oldDocDto);

            var counterpartyQuery = _queryBuilder.For<CounterpartyCard>().Where(x => x.Id == dto.CounterpartyId).Build();
            var counterparty = await _counterpartyQuery.GetAsync(counterpartyQuery, ct);
            if (counterparty is null)
                return Result.Failure(CounterpartyCardErrors.NotFound(dto.CounterpartyId, _userContext.LanguageId));

            var linesQuery = _queryBuilder.For<SaleDocTable>().Where(x => x.OwnerId == id).Build();
            var existingLines = await _lineQuery.GetAllAsync(linesQuery, ct);

            foreach (var lineDto in dto.Lines)
            {
                var line = existingLines.FirstOrDefault(l => l.Id == lineDto.Id);
                if (line == null)
                    return Result.Failure(SaleDocErrors.LineNotFound(lineDto.Id, _userContext.LanguageId));

                line.CostPrice = lineDto.CostPrice;

                if (lineDto.VatRateId.HasValue)
                {
                    var vatQuery = _queryBuilder.For<VatRate>().Where(v => v.Id == lineDto.VatRateId.Value).Build();
                    var vatRate = await _vatRateQuery.GetAsync(vatQuery, ct);

                    if (vatRate == null)
                        return Result.Failure(SaleDocTableErrors.VatRateNotFound(lineDto.VatRateId.Value, _userContext.LanguageId));

                    line.VatRateId = lineDto.VatRateId;
                    line.VatAmount = Math.Round(line.Price * vatRate.Rate / 100, 2);
                }
                else
                {
                    line.VatRateId = null;
                    line.VatAmount = 0;
                }

                line.TotalAmount = line.Amount + line.VatAmount;
                await _lineCommand.UpdateAsync(line, ct);
            }

            doc.CounterpartyId = dto.CounterpartyId;
            doc.DocDate        = DateTime.SpecifyKind(dto.DocDate, DateTimeKind.Unspecified);
            doc.TotalAmount    = existingLines.Sum(l => l.Amount);
            doc.VatAmount      = existingLines.Sum(l => l.VatAmount);
            doc.FinalAmount    = existingLines.Sum(l => l.TotalAmount);
            doc.StatusId       = DocumentStatusIdConst.POSTED;

            await _command.UpdateAsync(doc, ct);

            // ProductTable statuslarini SOLD qilish
            var productTableIds = existingLines.Select(l => l.ProductTableId).ToList();
            await UpdateProductTableStatusesAsync(productTableIds, ProductTableStatusIdConst.SOLD, ct);

            var fullDocQuery = _queryBuilder.For<SaleDoc>().Where(d => d.Id == doc.Id).Build();
            fullDocQuery.AddIncludes(b => b.Include(d => d.SaleDocTables).ThenInclude(l => l.ProductTable));
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

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CancelAsync), async () =>
        {
            var docQuery = _queryBuilder.For<SaleDoc>().Where(x => x.Id == id).Build();
            var doc = await _query.GetAsync(docQuery, ct);

            if (doc == null)
                return Result.Failure(SaleDocErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Failure(SaleDocErrors.AlreadyCancelled(id, _userContext.LanguageId));

            if (doc.StatusId != DocumentStatusIdConst.POSTED)
                return Result.Failure(SaleDocErrors.NotPosted(id, _userContext.LanguageId));

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto != null)
                _auditLogService.SetOldValues(oldDocDto);

            var linesQuery = _queryBuilder.For<SaleDocTable>().Where(x => x.OwnerId == id).Build();
            var existingLines = await _lineQuery.GetAllAsync(linesQuery, ct);

            // ProductTable statuslarini IN_STOCK ga qaytarish
            var productTableIds = existingLines.Select(l => l.ProductTableId).ToList();
            await UpdateProductTableStatusesAsync(productTableIds, ProductTableStatusIdConst.IN_STOCK, ct);

            doc.StatusId = DocumentStatusIdConst.CANCELLED;
            await _command.UpdateAsync(doc, ct);

            var newDocDto = await GetByIdInternalAsync(id, ct);
            if (newDocDto != null)
            {
                _auditLogService.SetNewValues(newDocDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.SaleDoc, id.ToString(), AuditLogOperationTypeConst.Update, "Cancelled");
            }

            return Result.Success();
        }, ct);

    public Task<Result> UpdateAsync(long id, SaleDocUpdateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<long>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.For<SaleDoc>().Where(x => x.Id == id).Build();
            var doc   = await _query.GetAsync(query, ct);

            if (doc == null)
                return Result.Failure(SaleDocErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.POSTED)
                return Result.Failure(SaleDocErrors.AlreadyPosted(id, _userContext.LanguageId));

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto != null)
                _auditLogService.SetOldValues(oldDocDto);

            doc.DocDate        = DateTime.SpecifyKind(dto.DocDate, DateTimeKind.Unspecified);
            doc.CounterpartyId = dto.CounterpartyId;
            doc.WarehouseId    = dto.WarehouseId;
            doc.CurrencyId     = dto.CurrencyId;
            doc.Comment        = dto.Comment;
            doc.StateId        = dto.StateId;

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
            var doc   = await _query.GetAsync(query, ct);

            if (doc == null)
                return Result.Failure(SaleDocErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.POSTED)
                return Result.Failure(SaleDocErrors.AlreadyPosted(id, _userContext.LanguageId));

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto != null)
                _auditLogService.SetOldValues(oldDocDto);

            // DRAFT holatdagi doc o'chirilganda ProductTable larni IN_STOCK ga qaytarish
            var linesQuery = _queryBuilder.For<SaleDocTable>().Where(x => x.OwnerId == id).Build();
            var existingLines = await _lineQuery.GetAllAsync(linesQuery, ct);
            var productTableIds = existingLines.Select(l => l.ProductTableId).ToList();
            await UpdateProductTableStatusesAsync(productTableIds, ProductTableStatusIdConst.IN_STOCK, ct);

            await _lineCommand.DeleteAsync(l => l.OwnerId == id, ct);

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
        {
            pt.StatusId = statusId;
        }

        await _productTableCommand.UpdateAsync(productTables, ct);
    }

    private async Task UpdateProductTableStatusesAsync(List<ProductTable> productTables, short statusId, CancellationToken ct)
    {
        foreach (var pt in productTables)
        {
            pt.StatusId = statusId;
        }

        await _productTableCommand.UpdateAsync(productTables, ct);
    }

    private async Task<Result<List<SaleDocTable>>> BuildDraftLinesAsync(int organizationId, List<int> productTableIds, CancellationToken ct)
    {
        var lines = new List<SaleDocTable>(productTableIds.Count);

        var ptQuery = _queryBuilder.For<ProductTable>()
                            .Where(x => x.OrganizationId == organizationId
                                      && x.StateId == StateIdConst.ACTIVE
                                      && productTableIds.Contains(x.Id))
                            .Build();

        var productTables = await _productTableQuery.GetAllAsync(ptQuery);


        var purchaseQuery = _queryBuilder.For<PurchaseDocTable>()
                                .Where(x => productTableIds.Contains(x.ProductTableId))
                                .As(s => new
                                {
                                    ProductTableId = s.ProductTableId,
                                    PurchaseId = s.OwnerId,
                                    DocDate = s.Owner.DocDate,
                                    CostPrice = s.TotalAmount
                                }).Build();

        var purchases = await _purchaseDocTableQuery.GetAllAsync(purchaseQuery, ct);

        var lastPurchases = purchases
                                .GroupBy(g => g.ProductTableId)
                                .Select(g => g.OrderByDescending(x => x.DocDate).First())
                                .ToList();

        foreach (var ptId in productTableIds)
        {
            var pt = productTables.FirstOrDefault(x => x.Id == ptId);
            if (pt == null)
                return Result.Failure<List<SaleDocTable>>(SaleDocErrors.ProductTableNotFound(ptId, _userContext.LanguageId));

            if (pt.StatusId != ProductTableStatusIdConst.IN_STOCK)
                return Result.Failure<List<SaleDocTable>>(SaleDocErrors.ProductTableNotAvailable(ptId, _userContext.LanguageId));

            var lastPurchase = lastPurchases.FirstOrDefault(f => f.ProductTableId ==  ptId);

            if (lastPurchase == null)
                return Result.Failure<List<SaleDocTable>>(SaleDocErrors.CostPriceNotFound(pt.ProductId, _userContext.LanguageId));

            lines.Add(new SaleDocTable
            {
                ProductTableId = pt.Id,
                Quantity       = 1,
                Price          = lastPurchase.CostPrice,
                CostPrice      = lastPurchase.CostPrice,
                Amount         = lastPurchase.CostPrice,
                VatRateId      = null,
                VatAmount      = 0,
                TotalAmount    = lastPurchase.CostPrice
            });
        }

        return lines;
    }
}
