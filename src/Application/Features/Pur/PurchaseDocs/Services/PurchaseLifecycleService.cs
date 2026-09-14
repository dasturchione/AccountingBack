using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.Acc.AccountingPeriods;
using Application.Features.AuditLogs;
using Application.Features.CounterpartyRegisterBalances;
using Application.Features.InventoryCounts;
using Application.Features.InventoryMovements;
using Application.Features.Inv.WarehouseProducts;
using Application.Features.PurchaseDocTables;
using Application.Features.Register.AccountingRegisterEntries;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.PurchaseDocs;

public class PurchaseLifecycleService : BaseService, IPurchaseLifecycleService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IDocumentPostingLock _postingLock;
    private readonly IAccountingPeriodValidator _periodValidator;
    private readonly IAuditLogService _auditLogService;
    private readonly IAccountingDispatcher _dispatcher;
    private readonly IInventoryDispatcher _inventoryDispatcher;
    private readonly IWarehouseProductBalanceService _warehouseProductBalanceService;
    private readonly IActiveInventoryCountGuardService _activeInventoryCountGuardService;
    private readonly IPurchaseCounterpartyRegisterService _purchaseCounterpartyRegisterService;
    private readonly IQueryRepository<PurchaseDoc> _query;
    private readonly ICommandRepository<PurchaseDoc> _command;
    private readonly IQueryRepository<ProductTable> _productTableQuery;
    private readonly ICommandRepository<ProductTable> _productTableCommand;
    private readonly IQueryRepository<PurchaseDocTable> _purchaseDocTableQuery;
    private readonly ICommandRepository<PurchaseDocTable> _purchaseDocTableCommand;
    private readonly IQueryRepository<ProductPrice> _productPriceQuery;
    private readonly ICommandRepository<ProductPrice> _productPriceCommand;
    private readonly IQueryRepository<PostingBatch> _postingBatchQuery;
    private readonly ICommandRepository<PostingBatch> _postingBatchCommand;
    private readonly IQueryRepository<AccountingRegisterEntry> _accountingRegisterQuery;
    private readonly ICommandRepository<AccountingRegisterEntry> _accountingRegisterCommand;
    private readonly IQueryRepository<WarehouseProductMovement> _warehouseMovementQuery;
    private readonly IQueryRepository<CounterpartyRegisterBalance> _counterpartyRegisterQuery;
    private readonly IQueryRepository<SaleCondition> _saleConditionQuery;

    public PurchaseLifecycleService(IUserContext userContext,
                                    IQueryBuilder queryBuilder,
                                    IDocumentPostingLock postingLock,
                                    IAccountingPeriodValidator periodValidator,
                                    IAuditLogService auditLogService,
                                    IAccountingDispatcher dispatcher,
                                    IInventoryDispatcher inventoryDispatcher,
                                    IWarehouseProductBalanceService warehouseProductBalanceService,
                                    IActiveInventoryCountGuardService activeInventoryCountGuardService,
                                    IPurchaseCounterpartyRegisterService purchaseCounterpartyRegisterService,
                                    IQueryRepository<PurchaseDoc> query,
                                    ICommandRepository<PurchaseDoc> command,
                                    IQueryRepository<ProductTable> productTableQuery,
                                    ICommandRepository<ProductTable> productTableCommand,
                                    IQueryRepository<PurchaseDocTable> purchaseDocTableQuery,
                                    ICommandRepository<PurchaseDocTable> purchaseDocTableCommand,
                                    IQueryRepository<ProductPrice> productPriceQuery,
                                    ICommandRepository<ProductPrice> productPriceCommand,
                                    IQueryRepository<PostingBatch> postingBatchQuery,
                                    ICommandRepository<PostingBatch> postingBatchCommand,
                                    IQueryRepository<AccountingRegisterEntry> accountingRegisterQuery,
                                    ICommandRepository<AccountingRegisterEntry> accountingRegisterCommand,
                                    IQueryRepository<WarehouseProductMovement> warehouseMovementQuery,
                                    IQueryRepository<CounterpartyRegisterBalance> counterpartyRegisterQuery,
                                    IQueryRepository<SaleCondition> saleConditionQuery,
                                    ILogger<PurchaseLifecycleService> logger,
                                    IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _postingLock = postingLock;
        _periodValidator = periodValidator;
        _auditLogService = auditLogService;
        _dispatcher = dispatcher;
        _inventoryDispatcher = inventoryDispatcher;
        _warehouseProductBalanceService = warehouseProductBalanceService;
        _activeInventoryCountGuardService = activeInventoryCountGuardService;
        _purchaseCounterpartyRegisterService = purchaseCounterpartyRegisterService;
        _query = query;
        _command = command;
        _productTableQuery = productTableQuery;
        _productTableCommand = productTableCommand;
        _purchaseDocTableQuery = purchaseDocTableQuery;
        _purchaseDocTableCommand = purchaseDocTableCommand;
        _productPriceQuery = productPriceQuery;
        _productPriceCommand = productPriceCommand;
        _postingBatchQuery = postingBatchQuery;
        _postingBatchCommand = postingBatchCommand;
        _accountingRegisterQuery = accountingRegisterQuery;
        _accountingRegisterCommand = accountingRegisterCommand;
        _warehouseMovementQuery = warehouseMovementQuery;
        _counterpartyRegisterQuery = counterpartyRegisterQuery;
        _saleConditionQuery = saleConditionQuery;
    }

    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(ConfirmAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            await _postingLock.AcquireAsync(DocumentTypeIdConst.PURCHASE, id, ct);

            var doc = await GetPurchaseDocForLifecycleAsync(id, ct);
            if (doc == null)
                return Result.Failure(PurchaseDocErrors.NotFound(id, _userContext.LanguageId));

            if (doc.OrganizationId != _userContext.OrganizationId.Value)
                return Result.Failure(PurchaseDocErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Failure(PurchaseDocErrors.AlreadyCancelled(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.POSTED)
                return await GetActivePostingBatchAsync(id, ct) is not null
                    ? Result.Success()
                    : Result.Failure(PurchaseDocErrors.MissingPostingBatch(id, _userContext.LanguageId));

            if (doc.StatusId != DocumentStatusIdConst.DRAFT && doc.StatusId != DocumentStatusIdConst.PENDING)
                return Result.Failure(PurchaseDocErrors.CannotConfirmInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));

            var periodValidation = await _periodValidator.EnsureOpenAsync(doc.OrganizationId, doc.DocDate, ct);
            if (!periodValidation.IsSuccess)
                return periodValidation;

            var countGuard = await _activeInventoryCountGuardService.EnsureWarehouseIsNotBlockedAsync(doc.OrganizationId, doc.WarehouseId, "PurchaseConfirm", ct: ct);
            if (!countGuard.IsSuccess)
                return countGuard;

            var validation = ValidateForConfirm(doc);
            if (!validation.IsSuccess)
                return validation;

            if (await GetActivePostingBatchAsync(id, ct) != null || await HasBusinessEffectsAsync(id, ct))
                return Result.Failure(PurchaseDocErrors.BusinessEffectsAlreadyExist(id, _userContext.LanguageId));

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto != null)
                _auditLogService.SetOldValues(oldDocDto);

            var postingBatch = await CreatePostingBatchAsync(doc, PostingBatchStatusConst.POSTED, "Purchase confirmed", ct);


            var dispatch = await _dispatcher.ProcessAsync(doc, ct, postingBatch.Id);
            if (!dispatch.IsSuccess)
                return Result.Failure(dispatch.Error);

            var inventoryDispatch = await _inventoryDispatcher.ProcessAsync(doc, ct, postingBatch.Id);
            if (!inventoryDispatch.IsSuccess)
                return Result.Failure(inventoryDispatch.Error);

            var counterpartyDispatch = await _purchaseCounterpartyRegisterService.PostAsync(doc, postingBatch.Id, ct);
            if (!counterpartyDispatch.IsSuccess)
                return Result.Failure(counterpartyDispatch.Error);

            await UpdateProductCostPricesAsync(doc, ct);

            doc.StatusId = DocumentStatusIdConst.POSTED;
            doc.PostedAt ??= DateTime.Now;
            doc.PostedByUserId ??= _userContext.Id;
            await _command.UpdateAsync(doc, ct);

            var newDocDto = await GetByIdInternalAsync(id, ct);
            if (newDocDto != null)
            {
                _auditLogService.SetNewValues(newDocDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.PurchaseDoc, id.ToString(), AuditLogOperationTypeConst.Update, "Confirmed");
            }

            return Result.Success();
        }, ct);

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CancelAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            await _postingLock.AcquireAsync(DocumentTypeIdConst.PURCHASE, id, ct);

            var doc = await GetPurchaseDocForLifecycleAsync(id, ct);
            if (doc == null)
                return Result.Failure(PurchaseDocErrors.NotFound(id, _userContext.LanguageId));

            if (doc.OrganizationId != _userContext.OrganizationId.Value)
                return Result.Failure(PurchaseDocErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Success();

            if (doc.StatusId != DocumentStatusIdConst.DRAFT &&
                doc.StatusId != DocumentStatusIdConst.PENDING &&
                doc.StatusId != DocumentStatusIdConst.POSTED)
                return Result.Failure(PurchaseDocErrors.CannotCancelInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));

            var periodValidation = await _periodValidator.EnsureOpenAsync(doc.OrganizationId, doc.DocDate, ct);
            if (!periodValidation.IsSuccess)
                return periodValidation;

            var reversalPeriodValidation = await _periodValidator.EnsureOpenAsync(doc.OrganizationId, DateTime.Now, ct);
            if (!reversalPeriodValidation.IsSuccess)
                return reversalPeriodValidation;

            var countGuard = await _activeInventoryCountGuardService.EnsureWarehouseIsNotBlockedAsync(doc.OrganizationId, doc.WarehouseId, "PurchaseCancel", ct: ct);
            if (!countGuard.IsSuccess)
                return countGuard;

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto != null)
                _auditLogService.SetOldValues(oldDocDto);

            if (doc.StatusId == DocumentStatusIdConst.POSTED)
            {
                var inventoryValidation = ValidateInventoryCanBeCancelled(doc);
                if (!inventoryValidation.IsSuccess)
                    return inventoryValidation;

                var activePostingBatch = await GetActivePostingBatchAsync(id, ct);
                if (activePostingBatch == null)
                    return Result.Failure(PurchaseDocErrors.MissingPostingBatch(id, _userContext.LanguageId));

                var reversalBatch = await CreatePostingBatchAsync(doc, PostingBatchStatusConst.REVERSAL, "Purchase cancelled", ct);

                var accountingReverse = await ReverseAccountingEntriesAsync(id, reversalBatch.Id, ct);
                if (!accountingReverse.IsSuccess)
                    return accountingReverse;

                var inventoryReverse = await ReverseInventoryEntriesAsync(doc, reversalBatch.Id, ct);
                if (!inventoryReverse.IsSuccess)
                    return inventoryReverse;

                var counterpartyReverse = await _purchaseCounterpartyRegisterService.ReverseAsync(doc, reversalBatch.Id, ct);
                if (!counterpartyReverse.IsSuccess)
                    return Result.Failure(counterpartyReverse.Error);

                if (doc.FinalAmount > 0m && counterpartyReverse.Value.Count == 0)
                    return Result.Failure(PurchaseDocErrors.MissingCounterpartyRegisterEntries(id, _userContext.LanguageId));

                activePostingBatch.Status = PostingBatchStatusConst.REVERSED;
                activePostingBatch.ReversedAt = DateTime.Now;
                activePostingBatch.ReversedByUserId = _userContext.Id;
                await _postingBatchCommand.UpdateAsync(activePostingBatch, ct);

                await RecalculateProductCostPricesAfterCancelAsync(doc, ct);
            }
            else
            {
                await DeleteDraftProductTablesAsync(doc, ct);
            }

            doc.StatusId = DocumentStatusIdConst.CANCELLED;
            doc.CancelledAt ??= DateTime.Now;
            doc.CancelledByUserId ??= _userContext.Id;
            await _command.UpdateAsync(doc, ct);

            var newDocDto = await GetByIdInternalAsync(id, ct);
            if (newDocDto != null)
            {
                _auditLogService.SetNewValues(newDocDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.PurchaseDoc, id.ToString(), AuditLogOperationTypeConst.Update, "Cancelled");
            }

            return Result.Success();
        }, ct);

    private async Task<PurchaseDocDto?> GetByIdInternalAsync(long id, CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return null;

        var query = _queryBuilder.For<PurchaseDoc>()
            .Where(p => p.Id == id && p.OrganizationId == _userContext.OrganizationId.Value)
            .As<PurchaseDocDto>()
            .Build();
        return await _query.GetAsync(query, ct);
    }

    private async Task<PurchaseDoc?> GetPurchaseDocForLifecycleAsync(long id, CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return null;

        var query = _queryBuilder.For<PurchaseDoc>()
            .Where(p => p.Id == id && p.OrganizationId == _userContext.OrganizationId.Value)
            .Build();
        query.AddIncludes(b => b.Include(d => d.PurchaseDocProducts).ThenInclude(l => l.Product));
        query.AddIncludes(b => b.Include(d => d.PurchaseDocProducts).ThenInclude(l => l.PurchaseDocTables).ThenInclude(t => t.ProductTable).ThenInclude(t => t.Product));
        query.AddIncludes(b => b.Include(d => d.PurchaseDocProducts).ThenInclude(l => l.PurchaseDocTables).ThenInclude(t => t.ProductTable).ThenInclude(t => t.WarehouseProductTable));

        return await _query.GetAsync(query, ct);
    }

    private Result ValidateForConfirm(PurchaseDoc doc)
    {
        if (doc.PurchaseDocProducts.Count == 0)
            return Result.Failure(PurchaseDocErrors.LinesRequired(doc.Id, _userContext.LanguageId));

        foreach (var line in doc.PurchaseDocProducts)
        {
            if (line.Product.IsService || !line.Product.IsPieceTracked)
            {
                if (line.PurchaseDocTables.Count > 0)
                    return Result.Failure(PurchaseDocErrors.ServiceItemsNotAllowed(line.ProductId, _userContext.LanguageId));

                continue;
            }

            if (line.PurchaseDocTables.Count > line.Quantity)
            {
                return Result.Failure(PurchaseDocTableErrors.ProductQuantityItemsMismatch(
                    line.ProductId,
                    line.Quantity,
                    line.PurchaseDocTables.Count,
                    _userContext.LanguageId));
            }

            var hasInvalidDraftItem = line.PurchaseDocTables.Any(x => x.ProductTable.ProductId != line.ProductId);

            if (hasInvalidDraftItem)
                return Result.Failure(PurchaseDocErrors.InvalidDraftInventoryState(doc.Id, _userContext.LanguageId));
        }

        return Result.Success();
    }

    private Result ValidateInventoryCanBeCancelled(PurchaseDoc doc)
    {
        var productTables = GetPurchaseProductTables(doc);
        var hasMovedItem = productTables.Any(x =>
            x.WarehouseProductTable?.StatusId != ProductTableStatusIdConst.IN_STOCK ||
            x.Product.StateId != StateIdConst.ACTIVE);

        return hasMovedItem
            ? Result.Failure(PurchaseDocErrors.CannotCancelMovedInventory(doc.Id, _userContext.LanguageId))
            : Result.Success();
    }

    private async Task<PostingBatch> CreatePostingBatchAsync(PurchaseDoc doc, string status, string comment, CancellationToken ct)
    {
        var now = DateTime.Now;
        var batch = new PostingBatch
        {
            OrganizationId = doc.OrganizationId,
            DocumentTypeId = DocumentTypeIdConst.PURCHASE,
            DocumentId = doc.Id,
            Status = status,
            PostedByUserId = _userContext.Id,
            PostedAt = now,
            Comment = comment
        };

        await _postingBatchCommand.CreateAsync(batch, ct);
        return batch;
    }

    private async Task<PostingBatch?> GetActivePostingBatchAsync(long purchaseDocId, CancellationToken ct)
    {
        var query = _queryBuilder.For<PostingBatch>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.PURCHASE &&
                        x.DocumentId == purchaseDocId &&
                        x.Status == PostingBatchStatusConst.POSTED)
            .Build();

        return await _postingBatchQuery.GetAsync(query, ct);
    }

    private async Task<bool> HasBusinessEffectsAsync(long purchaseDocId, CancellationToken ct)
    {
        var hasAccounting = await _accountingRegisterQuery.AnyAsync(x =>
            x.DocumentTypeId == DocumentTypeIdConst.PURCHASE &&
            x.DocumentId == purchaseDocId &&
            x.ReversalEntryId == null, ct);

        if (hasAccounting)
            return true;

        var hasInventory = await _warehouseMovementQuery.AnyAsync(x =>
            x.DocumentTypeId == DocumentTypeIdConst.PURCHASE &&
            x.DocumentId == purchaseDocId, ct);

        if (hasInventory)
            return true;

        return await _counterpartyRegisterQuery.AnyAsync(x =>
            x.DocumentTypeId == DocumentTypeIdConst.PURCHASE &&
            x.DocumentId == purchaseDocId &&
            x.ReversalEntryId == null, ct);
    }

    private async Task DeleteDraftProductTablesAsync(PurchaseDoc doc, CancellationToken ct)
    {
        var purchaseDocLineIds = doc.PurchaseDocProducts
            .Select(x => x.Id)
            .Where(x => x > 0)
            .Distinct()
            .ToList();
        var productTableIds = GetPurchaseProductTables(doc).Select(x => x.Id).ToList();
        if (purchaseDocLineIds.Count == 0 || productTableIds.Count == 0)
            return;

        await _purchaseDocTableCommand.DeleteAsync(x => purchaseDocLineIds.Contains(x.OwnerId), ct);
        await _productTableCommand.DeleteAsync(x => productTableIds.Contains(x.Id), ct);
    }

    private async Task<Result> ReverseAccountingEntriesAsync(long purchaseDocId, long reversalBatchId, CancellationToken ct)
    {
        var query = _queryBuilder.For<AccountingRegisterEntry>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.PURCHASE &&
                        x.DocumentId == purchaseDocId &&
                        x.ReversalEntryId == null)
            .Build();
        query.AddIncludes(b => b.Include(x => x.RegisterEntrySubkontos));

        var entries = await _accountingRegisterQuery.GetAllAsync(query, ct);
        if (entries.Count == 0)
            return Result.Failure(PurchaseDocErrors.MissingAccountingRegisterEntries(purchaseDocId, _userContext.LanguageId));

        var reversalEntries = AccountingRegisterEntryReversalFactory.Create(
            entries,
            reversalBatchId);

        await _accountingRegisterCommand.CreateAsync(reversalEntries, ct);
        return Result.Success();
    }

    private Task<Result> ReverseInventoryEntriesAsync(PurchaseDoc doc, long reversalBatchId, CancellationToken ct) =>
        _inventoryDispatcher.ReverseAsync(doc, ct);

    private async Task UpdateProductCostPricesAsync(PurchaseDoc doc, CancellationToken ct)
    {
        var lines = doc.PurchaseDocProducts
            .Where(x => !x.Product.IsService && x.PurchaseDocTables.Count > 0)
            .ToList();

        if (lines.Count == 0)
            return;

        var costingMethodId = await GetCurrentCostingMethodIdAsync(doc.OrganizationId, ct);
        var now = DateTime.Now;

        foreach (var group in lines.GroupBy(x => new { x.ProductId, x.UnitId }))
        {
            var purchaseTables = group.SelectMany(x => x.PurchaseDocTables).ToList();
            var newProductTableIds = purchaseTables
                .Select(x => x.ProductTableId)
                .Where(x => x > 0)
                .Distinct()
                .ToList();

            if (newProductTableIds.Count == 0)
                continue;

            var newTotalCost = purchaseTables.Sum(x => x.TotalAmount);
            var price = costingMethodId == CostingMethodIdConst.AVERAGE
                ? await CalculateAverageCostPriceAsync(
                    doc.OrganizationId,
                    group.Key.ProductId,
                    doc.CurrencyId,
                    newProductTableIds,
                    newTotalCost,
                    now,
                    ct)
                : CalculateUnitCost(newTotalCost, newProductTableIds.Count);

            await UpsertProductCostPriceAsync(
                doc.OrganizationId,
                group.Key.ProductId,
                doc.CurrencyId,
                group.Key.UnitId,
                price,
                now,
                ct);
        }
    }

    private async Task RecalculateProductCostPricesAfterCancelAsync(PurchaseDoc doc, CancellationToken ct)
    {
        var goods = doc.PurchaseDocProducts
            .Where(x => !x.Product.IsService)
            .GroupBy(x => new { x.ProductId, x.UnitId })
            .ToList();

        var now = DateTime.Now;
        foreach (var group in goods)
        {
            var inStockProductTableIds = await GetAllInStockProductTableIdsAsync(doc.OrganizationId, group.Key.ProductId, ct);
            var totalCost = await GetPurchaseCostTotalAsync(inStockProductTableIds, ct);
            var price = CalculateUnitCost(totalCost, inStockProductTableIds.Count);

            await UpsertProductCostPriceAsync(
                doc.OrganizationId,
                group.Key.ProductId,
                doc.CurrencyId,
                group.Key.UnitId,
                price,
                now,
                ct);
        }
    }

    private async Task<short> GetCurrentCostingMethodIdAsync(int organizationId, CancellationToken ct)
    {
        var now = DateTime.Now;
        var query = _queryBuilder.For<SaleCondition>()
            .Where(x => x.OrganizationId == organizationId &&
                        x.StateId == StateIdConst.ACTIVE &&
                        x.StartDate <= now &&
                        (x.EndDate == null || x.EndDate >= now))
            .As(x => new SaleConditionValuationMethod
            {
                Id = x.Id,
                StartDate = x.StartDate,
                CostingMethodId = x.CostingMethodId
            })
            .OrderBy(x => x.StartDate)
            .Desc()
            .Build();

        var conditions = await _saleConditionQuery.GetAllAsync(query, ct);
        var current = conditions
            .OrderByDescending(x => x.StartDate)
            .ThenByDescending(x => x.Id)
            .FirstOrDefault();

        return current?.CostingMethodId switch
        {
            CostingMethodIdConst.LIFO => CostingMethodIdConst.LIFO,
            CostingMethodIdConst.AVERAGE => CostingMethodIdConst.AVERAGE,
            _ => CostingMethodIdConst.FIFO
        };
    }

    private async Task<decimal> CalculateAverageCostPriceAsync(
        int organizationId,
        int productId,
        short currencyId,
        IReadOnlyCollection<int> newProductTableIds,
        decimal newTotalCost,
        DateTime now,
        CancellationToken ct)
    {
        var oldProductTableIds = await GetOldInStockProductTableIdsAsync(organizationId, productId, newProductTableIds, ct);
        var newQuantity = newProductTableIds.Count;

        if (oldProductTableIds.Count == 0)
            return CalculateUnitCost(newTotalCost, newQuantity);

        var currentPrice = await GetCurrentProductCostPriceAsync(organizationId, productId, currencyId, now, ct);
        var oldTotalCost = currentPrice is not null
            ? currentPrice.Price * oldProductTableIds.Count
            : await GetPurchaseCostTotalAsync(oldProductTableIds, ct);

        return CalculateUnitCost(oldTotalCost + newTotalCost, oldProductTableIds.Count + newQuantity);
    }

    private async Task<List<int>> GetOldInStockProductTableIdsAsync(
        int organizationId,
        int productId,
        IReadOnlyCollection<int> newProductTableIds,
        CancellationToken ct)
    {
        var query = _queryBuilder.For<ProductTable>()
            .Where(x => x.Product.OrganizationId == organizationId &&
                        x.ProductId == productId &&
                        x.WarehouseProductTable != null &&
                        x.WarehouseProductTable.StatusId == ProductTableStatusIdConst.IN_STOCK &&
                        x.Product.StateId == StateIdConst.ACTIVE &&
                        !newProductTableIds.Contains(x.Id))
            .As(x => x.Id)
            .Build();

        return await _productTableQuery.GetAllAsync(query, ct);
    }

    private async Task<List<int>> GetAllInStockProductTableIdsAsync(int organizationId, int productId, CancellationToken ct)
    {
        var query = _queryBuilder.For<ProductTable>()
            .Where(x => x.Product.OrganizationId == organizationId &&
                        x.ProductId == productId &&
                        x.WarehouseProductTable != null &&
                        x.WarehouseProductTable.StatusId == ProductTableStatusIdConst.IN_STOCK &&
                        x.Product.StateId == StateIdConst.ACTIVE)
            .As(x => x.Id)
            .Build();

        return await _productTableQuery.GetAllAsync(query, ct);
    }

    private async Task<decimal> GetPurchaseCostTotalAsync(IReadOnlyCollection<int> productTableIds, CancellationToken ct)
    {
        if (productTableIds.Count == 0)
            return 0;

        var query = _queryBuilder.For<PurchaseDocTable>()
            .Where(x => productTableIds.Contains(x.ProductTableId))
            .As(x => new ProductTablePurchaseCost
            {
                ProductTableId = x.ProductTableId,
                DocDate = x.Owner.Owner.DocDate,
                TotalAmount = x.TotalAmount
            })
            .Build();

        var costs = await _purchaseDocTableQuery.GetAllAsync(query, ct);

        return costs
            .GroupBy(x => x.ProductTableId)
            .Sum(x => x.OrderByDescending(c => c.DocDate).First().TotalAmount);
    }

    private async Task<ProductPrice?> GetCurrentProductCostPriceAsync(
        int organizationId,
        int productId,
        short currencyId,
        DateTime now,
        CancellationToken ct)
    {
        var query = _queryBuilder.For<ProductPrice>()
            .Where(x => x.OrganizationId == organizationId &&
                        x.ProductId == productId &&
                        x.CurrencyId == currencyId &&
                        x.PriceTypeId == PriceTypeIdConst.AVERAGE_COST_PRICE &&
                        x.StateId == StateIdConst.ACTIVE &&
                        x.StartDate <= now &&
                        (x.EndDate == null || x.EndDate >= now))
            .OrderBy(x => x.StartDate)
            .Desc()
            .Build();

        var prices = await _productPriceQuery.GetAllAsync(query, ct);
        return prices
            .OrderByDescending(x => x.StartDate)
            .ThenByDescending(x => x.Id)
            .FirstOrDefault();
    }

    private async Task UpsertProductCostPriceAsync(
        int organizationId,
        int productId,
        short currencyId,
        short unitId,
        decimal price,
        DateTime now,
        CancellationToken ct)
    {
        var current = await GetCurrentProductCostPriceAsync(organizationId, productId, currencyId, now, ct);

        if (current is null)
        {
            await _productPriceCommand.CreateAsync(new ProductPrice
            {
                OrganizationId = organizationId,
                ProductId = productId,
                CurrencyId = currencyId,
                PriceTypeId = PriceTypeIdConst.AVERAGE_COST_PRICE,
                UnitId = unitId,
                Price = price,
                StartDate = now,
                EndDate = null,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = now
            }, ct);

            return;
        }

        current.UnitId = unitId;
        current.Price = price;
        current.EndDate = null;
        current.StateId = StateIdConst.ACTIVE;

        await _productPriceCommand.UpdateAsync(current, ct);
    }

    private static List<ProductTable> GetPurchaseProductTables(PurchaseDoc doc) =>
        doc.PurchaseDocProducts
            .Where(x => !x.Product.IsService)
            .SelectMany(x => x.PurchaseDocTables)
            .Select(x => x.ProductTable)
            .GroupBy(x => x.Id)
            .Select(x => x.First())
            .ToList();

    private static decimal CalculateUnitCost(decimal totalCost, int quantity) =>
        quantity > 0 ? Math.Round(totalCost / quantity, 2) : 0;

    private sealed class SaleConditionValuationMethod
    {
        public long Id { get; set; }
        public DateTime StartDate { get; set; }
        public short CostingMethodId { get; set; }
    }

    private sealed class ProductTablePurchaseCost
    {
        public int ProductTableId { get; set; }
        public DateTime DocDate { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
