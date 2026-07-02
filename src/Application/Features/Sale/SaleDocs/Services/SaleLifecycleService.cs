using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.Acc.AccountingPeriods;
using Application.Features.AuditLogs;
using Application.Features.CounterpartyRegisterBalances;
using Application.Features.InventoryCounts;
using Application.Features.InventoryRegisterBalances;
using Application.Features.MoneyRegisterBalances;
using Application.Features.Register.AccountingRegisterEntries;
using Application.Features.SaleDocTables;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.SaleDocs;

public class SaleLifecycleService : BaseService, ISaleLifecycleService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IDocumentPostingLock _postingLock;
    private readonly IAccountingPeriodValidator _periodValidator;
    private readonly IAuditLogService _auditLogService;
    private readonly IAccountingDispatcher _dispatcher;
    private readonly IInventoryDispatcher _inventoryDispatcher;
    private readonly IActiveInventoryCountGuardService _activeInventoryCountGuardService;
    private readonly ISaleCounterpartyRegisterService _saleCounterpartyRegisterService;
    private readonly ISaleMoneyRegisterService _saleMoneyRegisterService;
    private readonly IQueryRepository<SaleDoc> _query;
    private readonly ICommandRepository<SaleDoc> _command;
    private readonly ICommandRepository<SaleDocProduct> _productLineCommand;
    private readonly ICommandRepository<SaleDocTable> _lineCommand;
    private readonly IQueryRepository<VatRate> _vatRateQuery;
    private readonly ICommandRepository<ProductTable> _productTableCommand;
    private readonly IQueryRepository<PostingBatch> _postingBatchQuery;
    private readonly ICommandRepository<PostingBatch> _postingBatchCommand;
    private readonly IQueryRepository<AccountingRegisterEntry> _accountingRegisterQuery;
    private readonly ICommandRepository<AccountingRegisterEntry> _accountingRegisterCommand;
    private readonly IQueryRepository<RegisterBalance> _inventoryRegisterQuery;
    private readonly ICommandRepository<RegisterBalance> _inventoryRegisterCommand;
    private readonly IQueryRepository<CounterpartyRegisterBalance> _counterpartyRegisterQuery;
    private readonly IQueryRepository<MoneyRegisterBalance> _moneyRegisterQuery;
    private readonly IQueryRepository<PurchaseDocTable> _purchaseDocTableQuery;

    public SaleLifecycleService(IUserContext userContext,
                                IQueryBuilder queryBuilder,
                                IDocumentPostingLock postingLock,
                                IAccountingPeriodValidator periodValidator,
                                IAuditLogService auditLogService,
                                IAccountingDispatcher dispatcher,
                                IInventoryDispatcher inventoryDispatcher,
                                IActiveInventoryCountGuardService activeInventoryCountGuardService,
                                ISaleCounterpartyRegisterService saleCounterpartyRegisterService,
                                ISaleMoneyRegisterService saleMoneyRegisterService,
                                IQueryRepository<SaleDoc> query,
                                ICommandRepository<SaleDoc> command,
                                ICommandRepository<SaleDocProduct> productLineCommand,
                                ICommandRepository<SaleDocTable> lineCommand,
                                IQueryRepository<VatRate> vatRateQuery,
                                ICommandRepository<ProductTable> productTableCommand,
                                IQueryRepository<PostingBatch> postingBatchQuery,
                                ICommandRepository<PostingBatch> postingBatchCommand,
                                IQueryRepository<AccountingRegisterEntry> accountingRegisterQuery,
                                ICommandRepository<AccountingRegisterEntry> accountingRegisterCommand,
                                IQueryRepository<RegisterBalance> inventoryRegisterQuery,
                                ICommandRepository<RegisterBalance> inventoryRegisterCommand,
                                IQueryRepository<CounterpartyRegisterBalance> counterpartyRegisterQuery,
                                IQueryRepository<MoneyRegisterBalance> moneyRegisterQuery,
                                IQueryRepository<PurchaseDocTable> purchaseDocTableQuery,
                                ILogger<SaleLifecycleService> logger,
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
        _activeInventoryCountGuardService = activeInventoryCountGuardService;
        _saleCounterpartyRegisterService = saleCounterpartyRegisterService;
        _saleMoneyRegisterService = saleMoneyRegisterService;
        _query = query;
        _command = command;
        _productLineCommand = productLineCommand;
        _lineCommand = lineCommand;
        _vatRateQuery = vatRateQuery;
        _productTableCommand = productTableCommand;
        _postingBatchQuery = postingBatchQuery;
        _postingBatchCommand = postingBatchCommand;
        _accountingRegisterQuery = accountingRegisterQuery;
        _accountingRegisterCommand = accountingRegisterCommand;
        _inventoryRegisterQuery = inventoryRegisterQuery;
        _inventoryRegisterCommand = inventoryRegisterCommand;
        _counterpartyRegisterQuery = counterpartyRegisterQuery;
        _moneyRegisterQuery = moneyRegisterQuery;
        _purchaseDocTableQuery = purchaseDocTableQuery;
    }

    public Task<Result> ConfirmAsync(long id, SaleDocConfirmDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(ConfirmAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            await _postingLock.AcquireAsync(DocumentTypeIdConst.SALE, id, ct);

            var doc = await GetSaleDocForLifecycleAsync(id, ct);
            if (doc == null)
                return Result.Failure(SaleDocErrors.NotFound(id, _userContext.LanguageId));

            if (doc.OrganizationId != _userContext.OrganizationId.Value)
                return Result.Failure(SaleDocErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Failure(SaleDocErrors.AlreadyCancelled(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.POSTED)
                return await GetActivePostingBatchAsync(id, ct) is not null
                    ? Result.Success()
                    : Result.Failure(SaleDocErrors.MissingPostingBatch(id, _userContext.LanguageId));

            if (doc.StatusId != DocumentStatusIdConst.DRAFT && doc.StatusId != DocumentStatusIdConst.PENDING)
                return Result.Failure(SaleDocErrors.CannotConfirmInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));

            var periodValidation = await _periodValidator.EnsureOpenAsync(doc.OrganizationId, doc.DocDate, ct);
            if (!periodValidation.IsSuccess)
                return periodValidation;

            var countGuard = await _activeInventoryCountGuardService.EnsureWarehouseIsNotBlockedAsync(doc.OrganizationId, doc.WarehouseId, "SaleConfirm", ct: ct);
            if (!countGuard.IsSuccess)
                return countGuard;

            if (await GetActivePostingBatchAsync(id, ct) != null || await HasBusinessEffectsAsync(id, ct))
                return Result.Failure(SaleDocErrors.BusinessEffectsAlreadyExist(id, _userContext.LanguageId));

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto != null)
                _auditLogService.SetOldValues(oldDocDto);

            var pricingResult = await ApplyConfirmAmountsAsync(doc, dto, ct);
            if (!pricingResult.IsSuccess)
                return pricingResult;

            var validation = ValidateForConfirm(doc);
            if (!validation.IsSuccess)
                return validation;

            var finalInventoryValidation = await ReloadAndValidateReservedProductTablesAsync(doc, ct);
            if (!finalInventoryValidation.IsSuccess)
                return finalInventoryValidation;

            var costSourceValidation = await ValidateCostSourcesAsync(doc, ct);
            if (!costSourceValidation.IsSuccess)
                return costSourceValidation;

            var postingBatch = await CreatePostingBatchAsync(doc, PostingBatchStatusConst.POSTED, "Sale confirmed", ct);

            await UpdateSaleProductTablesAsync(doc, ProductTableStatusIdConst.SOLD, StateIdConst.ACTIVE, ct);

            var dispatch = await _dispatcher.ProcessAsync(doc, ct, postingBatch.Id);
            if (!dispatch.IsSuccess)
                return Result.Failure(dispatch.Error);

            var inventoryDispatch = await _inventoryDispatcher.ProcessAsync(doc, ct, postingBatch.Id);
            if (!inventoryDispatch.IsSuccess)
                return Result.Failure(inventoryDispatch.Error);

            var counterpartyDispatch = await _saleCounterpartyRegisterService.PostAsync(doc, postingBatch.Id, ct);
            if (!counterpartyDispatch.IsSuccess)
                return Result.Failure(counterpartyDispatch.Error);

            var moneyDispatch = await _saleMoneyRegisterService.PostAsync(doc, postingBatch.Id, ct);
            if (!moneyDispatch.IsSuccess)
                return Result.Failure(moneyDispatch.Error);

            doc.StatusId = DocumentStatusIdConst.POSTED;
            doc.PostedAt ??= DateTime.Now;
            doc.PostedByUserId ??= _userContext.Id;
            await _command.UpdateAsync(doc, ct);

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
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            await _postingLock.AcquireAsync(DocumentTypeIdConst.SALE, id, ct);

            var doc = await GetSaleDocForLifecycleAsync(id, ct);
            if (doc == null)
                return Result.Failure(SaleDocErrors.NotFound(id, _userContext.LanguageId));

            if (doc.OrganizationId != _userContext.OrganizationId.Value)
                return Result.Failure(SaleDocErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Success();

            if (doc.StatusId != DocumentStatusIdConst.DRAFT &&
                doc.StatusId != DocumentStatusIdConst.PENDING &&
                doc.StatusId != DocumentStatusIdConst.POSTED)
                return Result.Failure(SaleDocErrors.CannotCancelInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));

            var periodValidation = await _periodValidator.EnsureOpenAsync(doc.OrganizationId, doc.DocDate, ct);
            if (!periodValidation.IsSuccess)
                return periodValidation;

            var reversalPeriodValidation = await _periodValidator.EnsureOpenAsync(doc.OrganizationId, DateTime.Now, ct);
            if (!reversalPeriodValidation.IsSuccess)
                return reversalPeriodValidation;

            var countGuard = await _activeInventoryCountGuardService.EnsureWarehouseIsNotBlockedAsync(doc.OrganizationId, doc.WarehouseId, "SaleCancel", ct: ct);
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
                    return Result.Failure(SaleDocErrors.MissingPostingBatch(id, _userContext.LanguageId));

                var reversalBatch = await CreatePostingBatchAsync(doc, PostingBatchStatusConst.REVERSAL, "Sale cancelled", ct);

                var accountingReverse = await ReverseAccountingEntriesAsync(id, reversalBatch.Id, ct);
                if (!accountingReverse.IsSuccess)
                    return accountingReverse;

                var inventoryReverse = await ReverseInventoryEntriesAsync(doc, reversalBatch.Id, ct);
                if (!inventoryReverse.IsSuccess)
                    return inventoryReverse;

                var counterpartyReverse = await _saleCounterpartyRegisterService.ReverseAsync(doc, reversalBatch.Id, ct);
                if (!counterpartyReverse.IsSuccess)
                    return Result.Failure(counterpartyReverse.Error);

                if (doc.FinalAmount > 0m && counterpartyReverse.Value.Count == 0)
                    return Result.Failure(SaleDocErrors.MissingCounterpartyRegisterEntries(id, _userContext.LanguageId));

                var moneyReverse = await _saleMoneyRegisterService.ReverseAsync(doc, reversalBatch.Id, ct);
                if (!moneyReverse.IsSuccess)
                    return Result.Failure(moneyReverse.Error);

                activePostingBatch.Status = PostingBatchStatusConst.REVERSED;
                activePostingBatch.ReversedAt = DateTime.Now;
                activePostingBatch.ReversedByUserId = _userContext.Id;
                await _postingBatchCommand.UpdateAsync(activePostingBatch, ct);

                await UpdateSaleProductTablesAsync(doc, ProductTableStatusIdConst.IN_STOCK, StateIdConst.ACTIVE, ct);
            }
            else
            {
                await ReleaseReservedProductTablesAsync(doc, ct);
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

    private async Task<SaleDocDto?> GetByIdInternalAsync(long id, CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return null;

        var query = _queryBuilder.For<SaleDoc>()
            .Where(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value)
            .As<SaleDocDto>()
            .Build();
        return await _query.GetAsync(query, ct);
    }

    private async Task<SaleDoc?> GetSaleDocForLifecycleAsync(long id, CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return null;

        var query = _queryBuilder.For<SaleDoc>()
            .Where(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value)
            .Build();
        query.AddIncludes(b => b.Include(d => d.SaleDocProducts).ThenInclude(l => l.Product));
        query.AddIncludes(b => b.Include(d => d.SaleDocProducts).ThenInclude(l => l.SaleDocTables).ThenInclude(t => t.ProductTable));

        return await _query.GetAsync(query, ct);
    }

    private async Task<Result> ApplyConfirmAmountsAsync(SaleDoc doc, SaleDocConfirmDto dto, CancellationToken ct)
    {
        if (dto.Lines.Count == 0)
        {
            RecalculateDocumentTotals(doc);
            return Result.Success();
        }

        var lineById = doc.SaleDocProducts.ToDictionary(x => x.Id);

        foreach (var lineDto in dto.Lines)
        {
            if (!lineById.TryGetValue(lineDto.Id, out var productLine))
                return Result.Failure(SaleDocErrors.LineNotFound(lineDto.Id, _userContext.LanguageId));

            if (lineDto.UnitPrice < 0)
                return Result.Failure(SaleDocErrors.InvalidProductUnitPrice(productLine.Id, lineDto.UnitPrice, _userContext.LanguageId));

            if (lineDto.CostPrice < 0)
                return Result.Failure(SaleDocErrors.InvalidProductCostPrice(productLine.Id, lineDto.CostPrice, _userContext.LanguageId));

            if (productLine.Product.IsService)
            {
                var amount = productLine.Quantity * lineDto.UnitPrice;
                var vatAmountResult = await CalculateVatAsync(amount, productLine.VatRateId, ct);
                if (!vatAmountResult.IsSuccess)
                    return Result.Failure(vatAmountResult.Error);

                productLine.UnitPrice = lineDto.UnitPrice;
                productLine.CostPrice = lineDto.CostPrice;
                productLine.Amount = amount;
                productLine.VatAmount = vatAmountResult.Value;
                productLine.TotalAmount = amount + vatAmountResult.Value;
                await _productLineCommand.UpdateAsync(productLine, ct);

                continue;
            }

            if (productLine.SaleDocTables.Count == 0)
                return Result.Failure(SaleDocErrors.InvalidDraftInventoryState(doc.Id, _userContext.LanguageId));

            foreach (var table in productLine.SaleDocTables)
            {
                var vatAmountResult = await CalculateVatAsync(lineDto.UnitPrice, table.VatRateId, ct);
                if (!vatAmountResult.IsSuccess)
                    return Result.Failure(vatAmountResult.Error);

                table.Amount = lineDto.UnitPrice;
                table.VatAmount = vatAmountResult.Value;
                table.TotalAmount = table.Amount + table.VatAmount;
            }

            await _lineCommand.UpdateAsync(productLine.SaleDocTables, ct);

            productLine.UnitPrice = lineDto.UnitPrice;
            productLine.CostPrice = productLine.SaleDocTables.Sum(x => x.CostPrice);
            productLine.Amount = productLine.SaleDocTables.Sum(x => x.Amount);
            productLine.VatAmount = productLine.SaleDocTables.Sum(x => x.VatAmount);
            productLine.TotalAmount = productLine.SaleDocTables.Sum(x => x.TotalAmount);
            await _productLineCommand.UpdateAsync(productLine, ct);
        }

        RecalculateDocumentTotals(doc);
        await _command.UpdateAsync(doc, ct);

        return Result.Success();
    }

    private async Task<Result<decimal>> CalculateVatAsync(decimal amount, short? vatRateId, CancellationToken ct)
    {
        if (!vatRateId.HasValue)
            return Result.Success(0m);

        var vatQuery = _queryBuilder.For<VatRate>().Where(v => v.Id == vatRateId.Value).Build();
        var vatRate = await _vatRateQuery.GetAsync(vatQuery, ct);

        return vatRate == null
            ? Result.Failure<decimal>(SaleDocTableErrors.VatRateNotFound(vatRateId.Value, _userContext.LanguageId))
            : Result.Success(Math.Round(amount * vatRate.Rate / 100, 8));
    }

    private Result ValidateForConfirm(SaleDoc doc)
    {
        if (doc.SaleDocProducts.Count == 0)
            return Result.Failure(SaleDocErrors.EmptyProducts(doc.Id, _userContext.LanguageId));

        foreach (var line in doc.SaleDocProducts)
        {
            if (line.Quantity <= 0)
                return Result.Failure(SaleDocErrors.InvalidProductQuantity(line.Id, line.Quantity, _userContext.LanguageId));

            if (line.Product.IsService)
            {
                if (line.SaleDocTables.Count > 0)
                    return Result.Failure(SaleDocErrors.ServiceItemsNotAllowed(line.ProductId, _userContext.LanguageId));

                continue;
            }

            if (line.Quantity != decimal.Truncate(line.Quantity))
                return Result.Failure(SaleDocErrors.InvalidProductQuantity(line.Id, line.Quantity, _userContext.LanguageId));

            if (line.SaleDocTables.Count != (int)line.Quantity)
                return Result.Failure(SaleDocErrors.QuantityMismatch(line.Id, line.Quantity, line.SaleDocTables.Count, _userContext.LanguageId));

            if (line.SaleDocTables.Select(x => x.ProductTableId).Distinct().Count() != line.SaleDocTables.Count)
                return Result.Failure(SaleDocErrors.InvalidInventorySelection(_userContext.LanguageId));

            var hasInvalidDraftItem = line.SaleDocTables.Any(x =>
                x.ProductTable.ProductId != line.ProductId ||
                x.ProductTable.OrganizationId != doc.OrganizationId ||
                x.ProductTable.CurrentWarehouseId != doc.WarehouseId ||
                x.ProductTable.StatusId != ProductTableStatusIdConst.RESERVED ||
                x.ProductTable.StateId != StateIdConst.ACTIVE);

            if (hasInvalidDraftItem)
                return Result.Failure(SaleDocErrors.InvalidDraftInventoryState(doc.Id, _userContext.LanguageId));
        }

        return Result.Success();
    }

    private async Task<Result> ValidateCostSourcesAsync(SaleDoc doc, CancellationToken ct)
    {
        var goodsRows = doc.SaleDocProducts
            .Where(x => !x.Product.IsService)
            .SelectMany(x => x.SaleDocTables)
            .ToList();

        if (goodsRows.Count == 0)
            return Result.Success();

        var productTableIds = goodsRows.Select(x => x.ProductTableId).Distinct().ToList();
        var query = _queryBuilder.For<PurchaseDocTable>()
            .Where(x => productTableIds.Contains(x.ProductTableId) &&
                        x.Owner.Owner.OrganizationId == doc.OrganizationId &&
                        x.Owner.Owner.StatusId == DocumentStatusIdConst.POSTED &&
                        x.Owner.Owner.StateId == StateIdConst.ACTIVE &&
                        x.Owner.Owner.DocDate <= doc.DocDate)
            .As(x => x.ProductTableId)
            .Build();

        var purchasedProductTableIds = (await _purchaseDocTableQuery.GetAllAsync(query, ct)).ToHashSet();
        var missingRow = goodsRows.FirstOrDefault(x => !purchasedProductTableIds.Contains(x.ProductTableId));

        return missingRow is null
            ? Result.Success()
            : Result.Failure(SaleDocErrors.CostPriceNotFound(missingRow.ProductTable.ProductId, _userContext.LanguageId));
    }

    private Result ValidateInventoryCanBeCancelled(SaleDoc doc)
    {
        var productTables = GetSaleProductTables(doc);
        var hasMovedItem = productTables.Any(x =>
            x.StatusId != ProductTableStatusIdConst.SOLD ||
            x.StateId != StateIdConst.ACTIVE);

        return hasMovedItem
            ? Result.Failure(SaleDocErrors.CannotCancelMovedInventory(doc.Id, _userContext.LanguageId))
            : Result.Success();
    }

    private async Task<PostingBatch> CreatePostingBatchAsync(SaleDoc doc, string status, string comment, CancellationToken ct)
    {
        var now = DateTime.Now;
        var batch = new PostingBatch
        {
            OrganizationId = doc.OrganizationId,
            DocumentTypeId = DocumentTypeIdConst.SALE,
            DocumentId = doc.Id,
            Status = status,
            PostedByUserId = _userContext.Id,
            PostedAt = now,
            Comment = comment
        };

        await _postingBatchCommand.CreateAsync(batch, ct);
        return batch;
    }

    private async Task<PostingBatch?> GetActivePostingBatchAsync(long saleDocId, CancellationToken ct)
    {
        var query = _queryBuilder.For<PostingBatch>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.SALE &&
                        x.DocumentId == saleDocId &&
                        x.Status == PostingBatchStatusConst.POSTED)
            .Build();

        return await _postingBatchQuery.GetAsync(query, ct);
    }

    private async Task<bool> HasBusinessEffectsAsync(long saleDocId, CancellationToken ct)
    {
        var hasAccounting = await _accountingRegisterQuery.AnyAsync(x =>
            x.DocumentTypeId == DocumentTypeIdConst.SALE &&
            x.DocumentId == saleDocId &&
            x.ReversalEntryId == null, ct);

        if (hasAccounting)
            return true;

        var hasInventory = await _inventoryRegisterQuery.AnyAsync(x =>
            x.DocumentTypeId == DocumentTypeIdConst.SALE &&
            x.DocumentId == saleDocId &&
            x.ReversalEntryId == null, ct);

        if (hasInventory)
            return true;

        var hasCounterparty = await _counterpartyRegisterQuery.AnyAsync(x =>
            x.DocumentTypeId == DocumentTypeIdConst.SALE &&
            x.DocumentId == saleDocId &&
            x.ReversalEntryId == null, ct);

        if (hasCounterparty)
            return true;

        return await _moneyRegisterQuery.AnyAsync(x =>
            x.DocumentTypeId == DocumentTypeIdConst.SALE &&
            x.DocumentId == saleDocId &&
            x.ReversalEntryId == null, ct);
    }

    private async Task UpdateSaleProductTablesAsync(SaleDoc doc, short statusId, short stateId, CancellationToken ct)
    {
        var productTables = GetSaleProductTables(doc);
        if (productTables.Count == 0)
            return;

        foreach (var productTable in productTables)
        {
            productTable.StatusId = statusId;
            productTable.StateId = stateId;
        }

        await _productTableCommand.UpdateAsync(productTables, ct);
    }

    private async Task ReleaseReservedProductTablesAsync(SaleDoc doc, CancellationToken ct)
    {
        var productTables = GetSaleProductTables(doc)
            .Where(x => x.StatusId == ProductTableStatusIdConst.RESERVED)
            .ToList();

        if (productTables.Count == 0)
            return;

        foreach (var productTable in productTables)
        {
            productTable.StatusId = ProductTableStatusIdConst.IN_STOCK;
            productTable.StateId = StateIdConst.ACTIVE;
        }

        await _productTableCommand.UpdateAsync(productTables, ct);
    }

    private async Task<Result> ReloadAndValidateReservedProductTablesAsync(SaleDoc doc, CancellationToken ct)
    {
        foreach (var productTable in GetSaleProductTables(doc))
            await _productTableCommand.ReloadAsync(productTable, ct);

        return ValidateForConfirm(doc);
    }

    private async Task<Result> ReverseAccountingEntriesAsync(long saleDocId, long reversalBatchId, CancellationToken ct)
    {
        var query = _queryBuilder.For<AccountingRegisterEntry>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.SALE &&
                        x.DocumentId == saleDocId &&
                        x.ReversalEntryId == null)
            .Build();
        query.AddIncludes(b => b.Include(x => x.RegisterEntrySubkontos));

        var entries = await _accountingRegisterQuery.GetAllAsync(query, ct);
        if (entries.Count == 0)
            return Result.Failure(SaleDocErrors.MissingAccountingRegisterEntries(saleDocId, _userContext.LanguageId));

        var now = DateTime.Now;
        var reversalEntries = entries.Select(entry => new AccountingRegisterEntry
        {
            OrganizationId = entry.OrganizationId,
            DocumentTypeId = entry.DocumentTypeId,
            DocumentId = entry.DocumentId,
            DebitAccountId = entry.CreditAccountId,
            CreditAccountId = entry.DebitAccountId,
            CurrencyId = entry.CurrencyId,
            Amount = entry.Amount,
            DocDate = now,
            CreatedDate = now,
            OperationTypeId = entry.OperationTypeId,
            DebitQuantity = entry.CreditQuantity,
            CreditQuantity = entry.DebitQuantity,
            Content = $"Reversal: {entry.Content}",
            JournalNumber = entry.JournalNumber,
            PostingBatchId = reversalBatchId,
            SourceLineId = entry.SourceLineId,
            ReversalEntryId = entry.Id,
            RegisterEntrySubkontos = entry.RegisterEntrySubkontos.Select(subkonto => new RegisterEntrySubkonto
            {
                Side = ReverseSubkontoSide(subkonto.Side),
                SubkontoTypeId = subkonto.SubkontoTypeId,
                SortOrder = subkonto.SortOrder,
                EntityId = subkonto.EntityId,
                DisplayValue = subkonto.DisplayValue,
                CreatedDate = now
            }).ToList()
        }).ToList();

        await _accountingRegisterCommand.CreateAsync(reversalEntries, ct);
        return Result.Success();
    }

    private async Task<Result> ReverseInventoryEntriesAsync(SaleDoc doc, long reversalBatchId, CancellationToken ct)
    {
        var expectedRows = doc.SaleDocProducts
            .Where(x => !x.Product.IsService)
            .SelectMany(x => x.SaleDocTables)
            .Count();

        var query = _queryBuilder.For<RegisterBalance>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.SALE &&
                        x.DocumentId == doc.Id &&
                        x.ReversalEntryId == null &&
                        x.OperationTypeId == OperationTypeIdConst.OUT)
            .Build();

        var entries = await _inventoryRegisterQuery.GetAllAsync(query, ct);
        if (entries.Count != expectedRows)
            return Result.Failure(SaleDocErrors.MissingInventoryRegisterEntries(doc.Id, _userContext.LanguageId));

        if (entries.Count == 0)
            return Result.Success();

        var now = DateTime.Now;
        var reversalEntries = entries.Select(entry => new RegisterBalance
        {
            OrganizationId = entry.OrganizationId,
            DocumentTypeId = entry.DocumentTypeId,
            DocumentId = entry.DocumentId,
            WarehouseId = entry.WarehouseId,
            ProductId = entry.ProductId,
            ProductTableId = entry.ProductTableId,
            OperationTypeId = OperationTypeIdConst.IN,
            Quantity = entry.Quantity,
            Amount = entry.Amount,
            DocDate = now,
            CreatedDate = now,
            PostingBatchId = reversalBatchId,
            SourceLineId = entry.SourceLineId,
            ReversalEntryId = entry.Id
        }).ToList();

        await _inventoryRegisterCommand.CreateAsync(reversalEntries, ct);
        return Result.Success();
    }

    private void RecalculateDocumentTotals(SaleDoc doc)
    {
        doc.TotalAmount = doc.SaleDocProducts.Sum(x => x.Amount);
        doc.VatAmount = doc.SaleDocProducts.Sum(x => x.VatAmount);
        doc.FinalAmount = doc.SaleDocProducts.Sum(x => x.TotalAmount);
    }

    private static List<ProductTable> GetSaleProductTables(SaleDoc doc) =>
        doc.SaleDocProducts
            .Where(x => !x.Product.IsService)
            .SelectMany(x => x.SaleDocTables)
            .Select(x => x.ProductTable)
            .GroupBy(x => x.Id)
            .Select(x => x.First())
            .ToList();

    private static string ReverseSubkontoSide(string side) =>
        side == SubkontoSideConst.DEBIT ? SubkontoSideConst.CREDIT :
        side == SubkontoSideConst.CREDIT ? SubkontoSideConst.DEBIT :
        side;
}
