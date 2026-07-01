using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.AuditLogs;
using Application.Features.Contracts;
using Application.Features.CounterpartyRegisterBalances;
using Application.Features.InventoryRegisterBalances;
using Application.Features.PurchaseDocTables;
using Application.Features.Register.AccountingRegisterEntries;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.PurchaseDocs;

public class PurchaseDocService : BaseService, IPurchaseDocService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IAuditLogService _auditLogService;
    private readonly IAccountingDispatcher _dispatcher;
    private readonly IInventoryDispatcher _inventoryDispatcher;
    private readonly IPurchaseCounterpartyRegisterService _purchaseCounterpartyRegisterService;
    private readonly IQueryRepository<PurchaseDoc> _query;
    private readonly IQueryRepository<VatRate> _vatRateQuery;
    private readonly ICommandRepository<PurchaseDoc> _command;
    private readonly IQueryRepository<Contract> _contractQuery;
    private readonly IQueryRepository<ProductTable> _productTableQuery;
    private readonly ICommandRepository<ProductTable> _productTableCommand;
    private readonly IQueryRepository<PurchaseDocTable> _purchaseDocTableQuery;
    private readonly IQueryRepository<ProductPrice> _productPriceQuery;
    private readonly ICommandRepository<ProductPrice> _productPriceCommand;
    private readonly IQueryRepository<PostingBatch> _postingBatchQuery;
    private readonly ICommandRepository<PostingBatch> _postingBatchCommand;
    private readonly IQueryRepository<AccountingRegisterEntry> _accountingRegisterQuery;
    private readonly ICommandRepository<AccountingRegisterEntry> _accountingRegisterCommand;
    private readonly IQueryRepository<RegisterBalance> _inventoryRegisterQuery;
    private readonly ICommandRepository<RegisterBalance> _inventoryRegisterCommand;
    private readonly IQueryRepository<CounterpartyRegisterBalance> _counterpartyRegisterQuery;
    private readonly IQueryRepository<SaleCondition> _saleConditionQuery;
    private readonly ICommandRepository<PurchaseDocProduct> _productLineCommand;
    private readonly ICommandRepository<PurchaseDocTable> _tableLineCommand;
    private readonly IDocNumberGenerator _docNumberGenerator;

    public PurchaseDocService(IUserContext userContext,
                              IQueryBuilder queryBuilder,
                              IAuditLogService auditLogService,
                              IAccountingDispatcher dispatcher,
                              IInventoryDispatcher inventoryDispatcher,
                              IPurchaseCounterpartyRegisterService purchaseCounterpartyRegisterService,
                              IDocNumberGenerator docNumberGenerator,
                              IQueryRepository<PurchaseDoc> query,
                              IQueryRepository<VatRate> vatRateQuery,
                              IQueryRepository<Contract> contractQuery,
                              IQueryRepository<ProductTable> productTableQuery,
                              ICommandRepository<ProductTable> productTableCommand,
                              IQueryRepository<PurchaseDocTable> purchaseDocTableQuery,
                              IQueryRepository<ProductPrice> productPriceQuery,
                              ICommandRepository<ProductPrice> productPriceCommand,
                              IQueryRepository<PostingBatch> postingBatchQuery,
                              ICommandRepository<PostingBatch> postingBatchCommand,
                              IQueryRepository<AccountingRegisterEntry> accountingRegisterQuery,
                              ICommandRepository<AccountingRegisterEntry> accountingRegisterCommand,
                              IQueryRepository<RegisterBalance> inventoryRegisterQuery,
                              ICommandRepository<RegisterBalance> inventoryRegisterCommand,
                              IQueryRepository<CounterpartyRegisterBalance> counterpartyRegisterQuery,
                              IQueryRepository<SaleCondition> saleConditionQuery,
                              ICommandRepository<PurchaseDoc> command,
                              ICommandRepository<PurchaseDocProduct> productLineCommand,
                              ICommandRepository<PurchaseDocTable> tableLineCommand,
                              ILogger<PurchaseDocService> logger,
                              IUnitOfWork unitOfWork)
            : base(logger, unitOfWork)
    {
        _query = query;
        _command = command;
        _dispatcher = dispatcher;
        _purchaseCounterpartyRegisterService = purchaseCounterpartyRegisterService;
        _productLineCommand = productLineCommand;
        _tableLineCommand = tableLineCommand;
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _auditLogService = auditLogService;
        _vatRateQuery = vatRateQuery;
        _contractQuery = contractQuery;
        _productTableQuery = productTableQuery;
        _productTableCommand = productTableCommand;
        _purchaseDocTableQuery = purchaseDocTableQuery;
        _productPriceQuery = productPriceQuery;
        _productPriceCommand = productPriceCommand;
        _postingBatchQuery = postingBatchQuery;
        _postingBatchCommand = postingBatchCommand;
        _accountingRegisterQuery = accountingRegisterQuery;
        _accountingRegisterCommand = accountingRegisterCommand;
        _inventoryRegisterQuery = inventoryRegisterQuery;
        _inventoryRegisterCommand = inventoryRegisterCommand;
        _counterpartyRegisterQuery = counterpartyRegisterQuery;
        _saleConditionQuery = saleConditionQuery;
        _inventoryDispatcher = inventoryDispatcher;
        _docNumberGenerator = docNumberGenerator;
    }

    public Task<Result<PagedResponse<PurchaseDocListDto>>> GetAllAsync(PurchaseDocListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            var query = _queryBuilder.BuildPaged<PurchaseDoc, PurchaseDocListDto, PurchaseDocListFilter>(filter);
            var pagedList = await _query.GetPagedAsync(query, ct);
            return Result.Success(PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize));
        });

    public Task<Result<PurchaseDocDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            var query = _queryBuilder.For<PurchaseDoc>().Where(p => p.Id == id).As<PurchaseDocDto>().Build();
            var entity = await _query.GetAsync(query, ct);

            if (entity == null)
                return Result.Failure<PurchaseDocDto>(PurchaseDocErrors.NotFound(id, _userContext.LanguageId));

            return Result.Success(entity);
        });

    public Task<Result<long>> CreateAsync(PurchaseDocCreateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<long>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            if (dto.ContractId.HasValue)
            {
                var contractExists = await _contractQuery.AnyAsync(x => x.Id == dto.ContractId.Value);
                if (!contractExists)
                    return Result.Failure<long>(ContractErrors.NotFound(dto.ContractId.Value, _userContext.LanguageId));
            }

            var docNumber = await _docNumberGenerator.GenerateAsync(_userContext.OrganizationId.Value, "PUR", dto.DocDate, ct);

            var allLinesResult = await BuildAllLinesAsync(_userContext.OrganizationId.Value, dto.Lines, ct);
            if (!allLinesResult.IsSuccess)
                return Result.Failure<long>(allLinesResult.Error);

            var allLines = allLinesResult.Value;

            var doc = new PurchaseDoc
            {
                OrganizationId = _userContext.OrganizationId.Value,
                DocNumber = docNumber,
                DocDate = dto.DocDate,
                CurrencyId = dto.CurrencyId,
                ExchangeRate = dto.ExchangeRate == 0 ? 1m : dto.ExchangeRate,
                PurchaseDocProducts = allLines,
                TotalAmount = allLines.Sum(l => l.Amount),
                VatAmount = allLines.Sum(l => l.VatAmount),
                FinalAmount = allLines.Sum(l => l.TotalAmount),
                StatusId = DocumentStatusIdConst.DRAFT,
                Comment = dto.Comment,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.Now,
                WarehouseId = dto.WarehouseId,
                CounterpartyId = dto.CounterpartyId,
                ContractId = dto.ContractId,
            };

            await _command.CreateAsync(doc, ct);

            var docDto = await GetByIdInternalAsync(doc.Id, ct);
            if (docDto != null)
            {
                _auditLogService.SetNewValues(docDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.PurchaseDoc, doc.Id.ToString(), AuditLogOperationTypeConst.Create);
            }

            return Result.Success(doc.Id);
        }, ct);

    public Task<Result> UpdateAsync(long id, PurchaseDocUpdateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<long>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.For<PurchaseDoc>().Where(p => p.Id == id).Build();
            var doc = await _query.GetAsync(query, ct);

            if (doc == null)
                return Result.Failure(PurchaseDocErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(PurchaseDocErrors.CannotUpdateInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto != null)
                _auditLogService.SetOldValues(oldDocDto);

            var allLinesResult = await BuildAllLinesAsync(_userContext.OrganizationId.Value, dto.Lines, ct);
            if (!allLinesResult.IsSuccess)
                return Result.Failure(allLinesResult.Error);

            var newLines = allLinesResult.Value;
            var oldProductTableIds = await GetPurchaseProductTableIdsAsync(id, ct);

            // Eski qatorlarni o'chirib, yangilarini yozamiz
            await _tableLineCommand.DeleteAsync(l => l.Owner.OwnerId == id, ct);
            await _productLineCommand.DeleteAsync(l => l.OwnerId == id, ct);
            if (oldProductTableIds.Count > 0)
                await _productTableCommand.DeleteAsync(x => oldProductTableIds.Contains(x.Id), ct);

            foreach (var line in newLines)
                line.OwnerId = id;

            await _productLineCommand.CreateAsync(newLines, ct);

            doc.OrganizationId = _userContext.OrganizationId.Value;
            doc.DocDate = DateTime.SpecifyKind(dto.DocDate, DateTimeKind.Unspecified);
            doc.CounterpartyId = dto.CounterpartyId;
            doc.WarehouseId = dto.WarehouseId;
            doc.CurrencyId = dto.CurrencyId;
            doc.ExchangeRate = dto.ExchangeRate == 0 ? 1m : dto.ExchangeRate;
            doc.TotalAmount = newLines.Sum(l => l.Amount);
            doc.VatAmount = newLines.Sum(l => l.VatAmount);
            doc.FinalAmount = newLines.Sum(l => l.TotalAmount);
            doc.Comment = dto.Comment;
            doc.StateId = dto.StateId;

            await _command.UpdateAsync(doc, ct);

            var newDocDto = await GetByIdInternalAsync(id, ct);
            if (newDocDto != null)
            {
                _auditLogService.SetNewValues(newDocDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.PurchaseDoc, id.ToString(), AuditLogOperationTypeConst.Update, dto.Comment);
            }

            return Result.Success();
        }, ct);

    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(ConfirmAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var doc = await GetPurchaseDocForLifecycleAsync(id, ct);
            if (doc == null)
                return Result.Failure(PurchaseDocErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Failure(PurchaseDocErrors.AlreadyCancelled(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.POSTED)
                return Result.Success();

            if (doc.StatusId != DocumentStatusIdConst.DRAFT && doc.StatusId != DocumentStatusIdConst.PENDING)
                return Result.Failure(PurchaseDocErrors.CannotConfirmInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));

            var validation = ValidateForConfirm(doc);
            if (!validation.IsSuccess)
                return validation;

            if (await GetActivePostingBatchAsync(id, ct) != null || await HasBusinessEffectsAsync(id, ct))
                return Result.Failure(PurchaseDocErrors.BusinessEffectsAlreadyExist(id, _userContext.LanguageId));

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto != null)
                _auditLogService.SetOldValues(oldDocDto);

            var postingBatch = await CreatePostingBatchAsync(doc, "Purchase confirmed", ct);

            await UpdatePurchaseProductTablesAsync(doc, ProductTableStatusIdConst.IN_STOCK, StateIdConst.ACTIVE, ct);

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
            var doc = await GetPurchaseDocForLifecycleAsync(id, ct);
            if (doc == null)
                return Result.Failure(PurchaseDocErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Success();

            if (doc.StatusId != DocumentStatusIdConst.DRAFT &&
                doc.StatusId != DocumentStatusIdConst.PENDING &&
                doc.StatusId != DocumentStatusIdConst.POSTED)
                return Result.Failure(PurchaseDocErrors.CannotCancelInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto != null)
                _auditLogService.SetOldValues(oldDocDto);

            if (doc.StatusId == DocumentStatusIdConst.POSTED)
            {
                var inventoryValidation = ValidateInventoryCanBeCancelled(doc);
                if (!inventoryValidation.IsSuccess)
                    return inventoryValidation;

                var activePostingBatch = await GetActivePostingBatchAsync(id, ct);
                var hasBusinessEffects = await HasBusinessEffectsAsync(id, ct);
                if (activePostingBatch == null && !hasBusinessEffects)
                    return Result.Failure(PurchaseDocErrors.MissingPostingBatch(id, _userContext.LanguageId));

                var reversalBatch = await CreatePostingBatchAsync(doc, "Purchase cancelled", ct);

                await ReverseAccountingEntriesAsync(id, reversalBatch.Id, ct);
                await ReverseInventoryEntriesAsync(id, reversalBatch.Id, ct);

                var counterpartyReverse = await _purchaseCounterpartyRegisterService.ReverseAsync(doc, reversalBatch.Id, ct);
                if (!counterpartyReverse.IsSuccess)
                    return Result.Failure(counterpartyReverse.Error);

                if (activePostingBatch != null)
                {
                    activePostingBatch.Status = PostingBatchStatusConst.REVERSED;
                    activePostingBatch.ReversedAt = DateTime.Now;
                    activePostingBatch.ReversedByUserId = _userContext.Id;
                    await _postingBatchCommand.UpdateAsync(activePostingBatch, ct);
                }

                await UpdatePurchaseProductTablesAsync(doc, ProductTableStatusIdConst.RETURNED_TO_SUPPLIER, StateIdConst.PASSIVE, ct);
            }
            else
            {
                await UpdatePurchaseProductTablesAsync(doc, ProductTableStatusIdConst.BLOCKED, StateIdConst.PASSIVE, ct);
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

    public Task<Result> DeleteAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(DeleteAsync), async () =>
        {
            var query = _queryBuilder.For<PurchaseDoc>().Where(x => x.Id == id).Build();
            var doc = await _query.GetAsync(query, ct);

            if (doc == null)
                return Result.Failure(PurchaseDocErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(PurchaseDocErrors.CannotDeleteInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto != null)
                _auditLogService.SetOldValues(oldDocDto);

            var productTableIds = await GetPurchaseProductTableIdsAsync(id, ct);

            // Avval barcha qatorlarni o'chiramiz, keyin hujjatni
            await _tableLineCommand.DeleteAsync(l => l.Owner.OwnerId == id, ct);
            await _productLineCommand.DeleteAsync(l => l.OwnerId == id, ct);
            if (productTableIds.Count > 0)
                await _productTableCommand.DeleteAsync(x => productTableIds.Contains(x.Id), ct);

            doc.StateId = StateIdConst.PASSIVE;
            await _command.UpdateAsync(doc, ct);

            var newDocDto = await GetByIdInternalAsync(id, ct);
            if (newDocDto != null)
            {
                _auditLogService.SetNewValues(newDocDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.PurchaseDoc, id.ToString(), AuditLogOperationTypeConst.Delete);
            }

            return Result.Success();
        }, ct);

    private async Task<PurchaseDocDto?> GetByIdInternalAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<PurchaseDoc>().Where(p => p.Id == id).As<PurchaseDocDto>().Build();
        return await _query.GetAsync(query, ct);
    }

    private async Task<PurchaseDoc?> GetPurchaseDocForLifecycleAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<PurchaseDoc>().Where(p => p.Id == id).Build();
        query.AddIncludes(b => b.Include(d => d.PurchaseDocProducts).ThenInclude(l => l.Product));
        query.AddIncludes(b => b.Include(d => d.PurchaseDocProducts).ThenInclude(l => l.PurchaseDocTables).ThenInclude(t => t.ProductTable));

        return await _query.GetAsync(query, ct);
    }

    private async Task<List<int>> GetPurchaseProductTableIdsAsync(long purchaseDocId, CancellationToken ct)
    {
        var query = _queryBuilder.For<PurchaseDocTable>()
            .Where(x => x.Owner.OwnerId == purchaseDocId)
            .As(x => x.ProductTableId)
            .Build();

        return await _purchaseDocTableQuery.GetAllAsync(query, ct);
    }

    private Result ValidateForConfirm(PurchaseDoc doc)
    {
        if (doc.PurchaseDocProducts.Count == 0)
            return Result.Failure(PurchaseDocErrors.LinesRequired(doc.Id, _userContext.LanguageId));

        foreach (var line in doc.PurchaseDocProducts)
        {
            if (!line.Product.IsService && line.PurchaseDocTables.Count != (int)line.Quantity)
            {
                return Result.Failure(PurchaseDocTableErrors.ProductQuantityItemsMismatch(
                    line.ProductId,
                    line.Quantity,
                    line.PurchaseDocTables.Count,
                    _userContext.LanguageId));
            }

            var hasInvalidDraftItem = line.PurchaseDocTables.Any(x =>
                x.ProductTable.StatusId != ProductTableStatusIdConst.RESERVED ||
                x.ProductTable.StateId != StateIdConst.ACTIVE);

            if (hasInvalidDraftItem)
                return Result.Failure(PurchaseDocErrors.InvalidDraftInventoryState(doc.Id, _userContext.LanguageId));
        }

        return Result.Success();
    }

    private Result ValidateInventoryCanBeCancelled(PurchaseDoc doc)
    {
        var productTables = GetPurchaseProductTables(doc);
        var hasMovedItem = productTables.Any(x =>
            x.StatusId != ProductTableStatusIdConst.IN_STOCK ||
            x.StateId != StateIdConst.ACTIVE);

        return hasMovedItem
            ? Result.Failure(PurchaseDocErrors.CannotCancelMovedInventory(doc.Id, _userContext.LanguageId))
            : Result.Success();
    }

    private async Task<PostingBatch> CreatePostingBatchAsync(PurchaseDoc doc, string comment, CancellationToken ct)
    {
        var now = DateTime.Now;
        var batch = new PostingBatch
        {
            OrganizationId = doc.OrganizationId,
            DocumentTypeId = DocumentTypeIdConst.PURCHASE,
            DocumentId = doc.Id,
            Status = PostingBatchStatusConst.POSTED,
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

        var hasInventory = await _inventoryRegisterQuery.AnyAsync(x =>
            x.DocumentTypeId == DocumentTypeIdConst.PURCHASE &&
            x.DocumentId == purchaseDocId &&
            x.ReversalEntryId == null, ct);

        if (hasInventory)
            return true;

        return await _counterpartyRegisterQuery.AnyAsync(x =>
            x.DocumentTypeId == DocumentTypeIdConst.PURCHASE &&
            x.DocumentId == purchaseDocId &&
            x.ReversalEntryId == null, ct);
    }

    private async Task UpdatePurchaseProductTablesAsync(PurchaseDoc doc, short statusId, short stateId, CancellationToken ct)
    {
        var productTables = GetPurchaseProductTables(doc);
        if (productTables.Count == 0)
            return;

        foreach (var productTable in productTables)
        {
            productTable.StatusId = statusId;
            productTable.StateId = stateId;
        }

        await _productTableCommand.UpdateAsync(productTables, ct);
    }

    private async Task ReverseAccountingEntriesAsync(long purchaseDocId, long reversalBatchId, CancellationToken ct)
    {
        var query = _queryBuilder.For<AccountingRegisterEntry>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.PURCHASE &&
                        x.DocumentId == purchaseDocId &&
                        x.ReversalEntryId == null)
            .Build();
        query.AddIncludes(b => b.Include(x => x.RegisterEntrySubkontos));

        var entries = await _accountingRegisterQuery.GetAllAsync(query, ct);
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

        if (reversalEntries.Count > 0)
            await _accountingRegisterCommand.CreateAsync(reversalEntries, ct);
    }

    private async Task ReverseInventoryEntriesAsync(long purchaseDocId, long reversalBatchId, CancellationToken ct)
    {
        var query = _queryBuilder.For<RegisterBalance>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.PURCHASE &&
                        x.DocumentId == purchaseDocId &&
                        x.ReversalEntryId == null &&
                        x.OperationTypeId == OperationTypeIdConst.IN)
            .Build();

        var entries = await _inventoryRegisterQuery.GetAllAsync(query, ct);
        var now = DateTime.Now;

        var reversalEntries = entries.Select(entry => new RegisterBalance
        {
            OrganizationId = entry.OrganizationId,
            DocumentTypeId = entry.DocumentTypeId,
            DocumentId = entry.DocumentId,
            WarehouseId = entry.WarehouseId,
            ProductId = entry.ProductId,
            OperationTypeId = OperationTypeIdConst.OUT,
            Quantity = entry.Quantity,
            Amount = entry.Amount,
            DocDate = now,
            CreatedDate = now,
            PostingBatchId = reversalBatchId,
            SourceLineId = entry.SourceLineId,
            ReversalEntryId = entry.Id
        }).ToList();

        if (reversalEntries.Count > 0)
            await _inventoryRegisterCommand.CreateAsync(reversalEntries, ct);
    }

    private static List<ProductTable> GetPurchaseProductTables(PurchaseDoc doc) =>
        doc.PurchaseDocProducts
            .SelectMany(x => x.PurchaseDocTables)
            .Select(x => x.ProductTable)
            .GroupBy(x => x.Id)
            .Select(x => x.First())
            .ToList();

    private static string ReverseSubkontoSide(string side) =>
        side == SubkontoSideConst.DEBIT ? SubkontoSideConst.CREDIT :
        side == SubkontoSideConst.CREDIT ? SubkontoSideConst.DEBIT :
        side;

    private async Task<Result<List<PurchaseDocProduct>>> BuildAllLinesAsync(
        int organizationId,
        List<PurchaseDocLineDto> productLineDtos,
        CancellationToken ct)
    {
        var allLines = new List<PurchaseDocProduct>();

        if (productLineDtos.Count > 0)
        {
            var productResult = await BuildProductLinesAsync(organizationId, productLineDtos, ct);
            if (!productResult.IsSuccess)
                return Result.Failure<List<PurchaseDocProduct>>(productResult.Error);

            allLines.AddRange(productResult.Value);
        }

        return allLines;
    }

    private async Task<Result<List<PurchaseDocProduct>>> BuildProductLinesAsync(
        int organizationId, List<PurchaseDocLineDto> lineDtos, CancellationToken ct)
    {
        var lines = new List<PurchaseDocProduct>();
        var markingNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var dto in lineDtos)
        {
            if (dto.Quantity <= 0 || dto.Quantity != decimal.Truncate(dto.Quantity))
                return Result.Failure<List<PurchaseDocProduct>>(
                    PurchaseDocTableErrors.InvalidProductQuantity(dto.ProductId, dto.Quantity, _userContext.LanguageId));

            if (dto.UnitPrice < 0)
                return Result.Failure<List<PurchaseDocProduct>>(
                    PurchaseDocTableErrors.InvalidProductUnitPrice(dto.ProductId, dto.UnitPrice, _userContext.LanguageId));

            if (dto.Items.Count == 0)
                return Result.Failure<List<PurchaseDocProduct>>(
                    PurchaseDocTableErrors.ProductItemsRequired(dto.ProductId, _userContext.LanguageId));

            if (dto.Quantity != dto.Items.Count)
                return Result.Failure<List<PurchaseDocProduct>>(
                    PurchaseDocTableErrors.ProductQuantityItemsMismatch(dto.ProductId, dto.Quantity, dto.Items.Count, _userContext.LanguageId));

            foreach (var item in dto.Items)
            {
                if (string.IsNullOrWhiteSpace(item.MarkingNumber))
                    return Result.Failure<List<PurchaseDocProduct>>(
                        PurchaseDocTableErrors.MarkingNumberRequired(dto.ProductId, _userContext.LanguageId));

                if (!markingNumbers.Add(item.MarkingNumber.Trim()))
                    return Result.Failure<List<PurchaseDocProduct>>(
                        PurchaseDocTableErrors.DuplicateMarkingNumber(item.MarkingNumber, _userContext.LanguageId));
            }

            var vatRateId = dto.VatRateId;
            var amount = dto.UnitPrice * dto.Quantity;
            var vatAmount = 0m;

            if (vatRateId.HasValue)
            {
                var vatQuery = _queryBuilder.For<VatRate>().Where(v => v.Id == vatRateId.Value).Build();
                var vatRate = await _vatRateQuery.GetAsync(vatQuery, ct);

                if (vatRate == null)
                    return Result.Failure<List<PurchaseDocProduct>>(PurchaseDocTableErrors.VatRateNotFound(vatRateId.Value, _userContext.LanguageId));

                vatAmount = Math.Round(amount * vatRate.Rate / 100, 8);
            }

            var itemVatAmounts = SplitAmount(vatAmount, dto.Items.Count);

            lines.Add(new PurchaseDocProduct
            {
                ProductId = dto.ProductId,
                UnitId = dto.UnitId,
                Quantity = dto.Quantity,
                UnitPrice = dto.UnitPrice,
                Amount = amount,
                VatRateId = vatRateId,
                VatAmount = vatAmount,
                TotalAmount = amount + vatAmount,
                PurchaseDocTables = dto.Items.Select((item, index) => new PurchaseDocTable
                {
                    Amount = dto.UnitPrice,
                    VatRateId = vatRateId,
                    VatAmount = itemVatAmounts[index],
                    TotalAmount = dto.UnitPrice + itemVatAmounts[index],
                    ProductTable = new ProductTable
                    {
                        ProductId = dto.ProductId,
                        SerialNumber = item.SerialNumber,
                        MarkingNumber = item.MarkingNumber.Trim(),
                        CreatedDate = DateTime.Now,
                        OrganizationId = organizationId,
                        StateId = StateIdConst.ACTIVE,
                        StatusId = ProductTableStatusIdConst.RESERVED
                    }
                }).ToList()
            });
        }

        return lines;
    }

    private async Task UpdateProductCostPricesAsync(PurchaseDoc doc, CancellationToken ct)
    {
        var lines = doc.PurchaseDocProducts
            .Where(x => x.PurchaseDocTables.Count > 0)
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
            .Where(x => x.OrganizationId == organizationId &&
                        x.ProductId == productId &&
                        x.StatusId == ProductTableStatusIdConst.IN_STOCK &&
                        x.StateId == StateIdConst.ACTIVE &&
                        !newProductTableIds.Contains(x.Id))
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

    private static decimal CalculateUnitCost(decimal totalCost, int quantity) =>
        quantity > 0 ? Math.Round(totalCost / quantity, 2) : 0;

    private static List<decimal> SplitAmount(decimal amount, int count)
    {
        if (count <= 0)
            return new List<decimal>();

        var split = Math.Round(amount / count, 8);
        var result = Enumerable.Repeat(split, count).ToList();
        result[^1] += amount - result.Sum();
        return result;
    }

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
