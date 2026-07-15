using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.Acc.AccountingPeriods;
using Application.Features.AuditLogs;
using Application.Features.InventoryCounts;
using Application.Features.InventoryRegisterBalances;
using Application.Features.Inv.WarehouseProducts;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.WarehouseTransfers;

public class WarehouseTransferLifecycleService : BaseService, IWarehouseTransferLifecycleService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IDocumentPostingLock _postingLock;
    private readonly IAccountingPeriodValidator _periodValidator;
    private readonly IActiveInventoryCountGuardService _activeInventoryCountGuardService;
    private readonly IAuditLogService _auditLogService;
    private readonly IInventoryDispatcher _inventoryDispatcher;
    private readonly IWarehouseProductBalanceService _warehouseProductBalanceService;
    private readonly IQueryRepository<WarehouseTransferDoc> _query;
    private readonly ICommandRepository<WarehouseTransferDoc> _command;
    private readonly ICommandRepository<ProductTable> _productTableCommand;
    private readonly IQueryRepository<PostingBatch> _postingBatchQuery;
    private readonly ICommandRepository<PostingBatch> _postingBatchCommand;
    private readonly IQueryRepository<RegisterBalance> _inventoryRegisterQuery;
    private readonly ICommandRepository<RegisterBalance> _inventoryRegisterCommand;

    public WarehouseTransferLifecycleService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IDocumentPostingLock postingLock,
        IAccountingPeriodValidator periodValidator,
        IActiveInventoryCountGuardService activeInventoryCountGuardService,
        IAuditLogService auditLogService,
        IInventoryDispatcher inventoryDispatcher,
        IWarehouseProductBalanceService warehouseProductBalanceService,
        IQueryRepository<WarehouseTransferDoc> query,
        ICommandRepository<WarehouseTransferDoc> command,
        ICommandRepository<ProductTable> productTableCommand,
        IQueryRepository<PostingBatch> postingBatchQuery,
        ICommandRepository<PostingBatch> postingBatchCommand,
        IQueryRepository<RegisterBalance> inventoryRegisterQuery,
        ICommandRepository<RegisterBalance> inventoryRegisterCommand,
        ILogger<WarehouseTransferLifecycleService> logger,
        IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _postingLock = postingLock;
        _periodValidator = periodValidator;
        _activeInventoryCountGuardService = activeInventoryCountGuardService;
        _auditLogService = auditLogService;
        _inventoryDispatcher = inventoryDispatcher;
        _warehouseProductBalanceService = warehouseProductBalanceService;
        _query = query;
        _command = command;
        _productTableCommand = productTableCommand;
        _postingBatchQuery = postingBatchQuery;
        _postingBatchCommand = postingBatchCommand;
        _inventoryRegisterQuery = inventoryRegisterQuery;
        _inventoryRegisterCommand = inventoryRegisterCommand;
    }

    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(ConfirmAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            await _postingLock.AcquireAsync(DocumentTypeIdConst.WAREHOUSETRANSFER, id, ct);

            var doc = await GetForLifecycleAsync(id, ct);
            if (doc == null)
                return Result.Failure(WarehouseTransferErrors.NotFound(id, _userContext.LanguageId));

            if (doc.OrganizationId != _userContext.OrganizationId.Value)
                return Result.Failure(WarehouseTransferErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Failure(WarehouseTransferErrors.AlreadyCancelled(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.POSTED)
            {
                return await GetActivePostingBatchAsync(id, ct) is not null
                    ? Result.Success()
                    : Result.Failure(WarehouseTransferErrors.MissingPostingBatch(id, _userContext.LanguageId));
            }

            if (doc.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(WarehouseTransferErrors.CannotConfirmInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));

            var periodValidation = await _periodValidator.EnsureOpenAsync(doc.OrganizationId, doc.DocDate, ct);
            if (!periodValidation.IsSuccess)
                return periodValidation;

            var sourceCountGuard = await _activeInventoryCountGuardService.EnsureWarehouseIsNotBlockedAsync(doc.OrganizationId, doc.SourceWarehouseId, "WarehouseTransferConfirm", ct: ct);
            if (!sourceCountGuard.IsSuccess)
                return sourceCountGuard;

            var destinationCountGuard = await _activeInventoryCountGuardService.EnsureWarehouseIsNotBlockedAsync(doc.OrganizationId, doc.DestinationWarehouseId, "WarehouseTransferConfirm", ct: ct);
            if (!destinationCountGuard.IsSuccess)
                return destinationCountGuard;

            var validation = ValidateForConfirm(doc);
            if (!validation.IsSuccess)
                return validation;

            await ReloadTransferProductTablesAsync(doc, ct);
            validation = ValidateForConfirm(doc);
            if (!validation.IsSuccess)
                return validation;

            if (await GetActivePostingBatchAsync(id, ct) != null || await HasBusinessEffectsAsync(id, ct))
                return Result.Failure(WarehouseTransferErrors.BusinessEffectsAlreadyExist(id, _userContext.LanguageId));

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto != null)
                _auditLogService.SetOldValues(oldDocDto);

            var postingBatch = await CreatePostingBatchAsync(doc, PostingBatchStatusConst.POSTED, "Warehouse transfer confirmed", ct);


            var inventoryDispatch = await _inventoryDispatcher.ProcessAsync(doc, ct, postingBatch.Id);
            if (!inventoryDispatch.IsSuccess)
                return Result.Failure(inventoryDispatch.Error);

            doc.StatusId = DocumentStatusIdConst.POSTED;
            doc.PostedAt ??= DateTime.Now;
            doc.PostedByUserId ??= _userContext.Id;
            await _command.UpdateAsync(doc, ct);

            var newDocDto = await GetByIdInternalAsync(id, ct);
            if (newDocDto != null)
            {
                _auditLogService.SetNewValues(newDocDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.WarehouseTransferDoc, id.ToString(), AuditLogOperationTypeConst.Update, "Confirmed");
            }

            return Result.Success();
        }, ct);

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CancelAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            await _postingLock.AcquireAsync(DocumentTypeIdConst.WAREHOUSETRANSFER, id, ct);

            var doc = await GetForLifecycleAsync(id, ct);
            if (doc == null)
                return Result.Failure(WarehouseTransferErrors.NotFound(id, _userContext.LanguageId));

            if (doc.OrganizationId != _userContext.OrganizationId.Value)
                return Result.Failure(WarehouseTransferErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Success();

            if (doc.StatusId != DocumentStatusIdConst.DRAFT &&
                doc.StatusId != DocumentStatusIdConst.POSTED)
                return Result.Failure(WarehouseTransferErrors.CannotCancelInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));

            var periodValidation = await _periodValidator.EnsureOpenAsync(doc.OrganizationId, doc.DocDate, ct);
            if (!periodValidation.IsSuccess)
                return periodValidation;

            var reversalPeriodValidation = await _periodValidator.EnsureOpenAsync(doc.OrganizationId, DateTime.Now, ct);
            if (!reversalPeriodValidation.IsSuccess)
                return reversalPeriodValidation;

            var sourceCountGuard = await _activeInventoryCountGuardService.EnsureWarehouseIsNotBlockedAsync(doc.OrganizationId, doc.SourceWarehouseId, "WarehouseTransferCancel", ct: ct);
            if (!sourceCountGuard.IsSuccess)
                return sourceCountGuard;

            var destinationCountGuard = await _activeInventoryCountGuardService.EnsureWarehouseIsNotBlockedAsync(doc.OrganizationId, doc.DestinationWarehouseId, "WarehouseTransferCancel", ct: ct);
            if (!destinationCountGuard.IsSuccess)
                return destinationCountGuard;

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto != null)
                _auditLogService.SetOldValues(oldDocDto);

            if (doc.StatusId == DocumentStatusIdConst.POSTED)
            {
                var validation = ValidateForCancel(doc);
                if (!validation.IsSuccess)
                    return validation;

                var activePostingBatch = await GetActivePostingBatchAsync(id, ct);
                if (activePostingBatch == null)
                    return Result.Failure(WarehouseTransferErrors.MissingPostingBatch(id, _userContext.LanguageId));

                var reversalBatch = await CreatePostingBatchAsync(doc, PostingBatchStatusConst.REVERSAL, "Warehouse transfer cancelled", ct);

                var inventoryReverse = await ReverseInventoryEntriesAsync(doc, reversalBatch.Id, ct);
                if (!inventoryReverse.IsSuccess)
                    return inventoryReverse;

                activePostingBatch.Status = PostingBatchStatusConst.REVERSED;
                activePostingBatch.ReversedAt = DateTime.Now;
                activePostingBatch.ReversedByUserId = _userContext.Id;
                await _postingBatchCommand.UpdateAsync(activePostingBatch, ct);

            }

            doc.StatusId = DocumentStatusIdConst.CANCELLED;
            doc.CancelledAt ??= DateTime.Now;
            doc.CancelledByUserId ??= _userContext.Id;
            await _command.UpdateAsync(doc, ct);

            var newDocDto = await GetByIdInternalAsync(id, ct);
            if (newDocDto != null)
            {
                _auditLogService.SetNewValues(newDocDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.WarehouseTransferDoc, id.ToString(), AuditLogOperationTypeConst.Update, "Cancelled");
            }

            return Result.Success();
        }, ct);

    private async Task<WarehouseTransferDto?> GetByIdInternalAsync(long id, CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return null;

        var query = _queryBuilder.For<WarehouseTransferDoc>()
            .Where(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value)
            .As<WarehouseTransferDto>()
            .Build();
        return await _query.GetAsync(query, ct);
    }

    private async Task<WarehouseTransferDoc?> GetForLifecycleAsync(long id, CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return null;

        var query = _queryBuilder.For<WarehouseTransferDoc>()
            .Where(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value)
            .Build();
        query.AddIncludes(b => b.Include(x => x.SourceWarehouse));
        query.AddIncludes(b => b.Include(x => x.DestinationWarehouse));
        query.AddIncludes(b => b.Include(x => x.WarehouseTransferLines).ThenInclude(x => x.Product));
        query.AddIncludes(b => b.Include(x => x.WarehouseTransferLines).ThenInclude(x => x.WarehouseTransferDocTables).ThenInclude(x => x.ProductTable).ThenInclude(x => x.WarehouseProductTable));
        return await _query.GetAsync(query, ct);
    }

    private Result ValidateForConfirm(WarehouseTransferDoc doc)
    {
        if (doc.SourceWarehouseId == doc.DestinationWarehouseId)
            return Result.Failure(WarehouseTransferErrors.WarehousesMustDiffer(_userContext.LanguageId));

        if (doc.SourceWarehouse.StateId != StateIdConst.ACTIVE)
            return Result.Failure(WarehouseTransferErrors.WarehouseInactive(doc.SourceWarehouseId, _userContext.LanguageId));

        if (doc.DestinationWarehouse.StateId != StateIdConst.ACTIVE)
            return Result.Failure(WarehouseTransferErrors.WarehouseInactive(doc.DestinationWarehouseId, _userContext.LanguageId));

        if (doc.WarehouseTransferLines.Count == 0)
            return Result.Failure(WarehouseTransferErrors.LinesRequired(doc.Id, _userContext.LanguageId));

        var seenProductTableIds = new HashSet<int>();

        foreach (var line in doc.WarehouseTransferLines)
        {
            if (line.Product.IsService)
                return Result.Failure(WarehouseTransferErrors.ProductServiceNotAllowed(line.ProductId, _userContext.LanguageId));

            if (line.Quantity <= 0)
                return Result.Failure(WarehouseTransferErrors.InvalidQuantity(line.ProductId, line.Quantity, _userContext.LanguageId));

            if (!line.Product.IsPieceTracked)
            {
                if (line.WarehouseTransferDocTables.Count > 0)
                    return Result.Failure(WarehouseTransferErrors.QuantityItemsMismatch(line.ProductId, 0m, line.WarehouseTransferDocTables.Count, _userContext.LanguageId));

                continue;
            }

            if (line.Quantity != decimal.Truncate(line.Quantity) || line.WarehouseTransferDocTables.Count != (int)line.Quantity)
                return Result.Failure(WarehouseTransferErrors.QuantityItemsMismatch(line.ProductId, line.Quantity, line.WarehouseTransferDocTables.Count, _userContext.LanguageId));

            foreach (var item in line.WarehouseTransferDocTables)
            {
                if (!seenProductTableIds.Add(item.ProductTableId))
                    return Result.Failure(WarehouseTransferErrors.DuplicateProductTable(item.ProductTableId, _userContext.LanguageId));

                if (item.ProductTable.Product.OrganizationId != doc.OrganizationId)
                    return Result.Failure(WarehouseTransferErrors.ProductTableNotFound(item.ProductTableId, _userContext.LanguageId));

                if (item.ProductTable.ProductId != line.ProductId)
                    return Result.Failure(WarehouseTransferErrors.ProductTableProductMismatch(item.ProductTableId, line.ProductId, _userContext.LanguageId));

                if (item.ProductTable.WarehouseProductTable == null)
                    return Result.Failure(WarehouseTransferErrors.ProductTableInactive(item.ProductTableId, _userContext.LanguageId));

                if (item.ProductTable.WarehouseProductTable.StatusId != ProductTableStatusIdConst.IN_STOCK)
                    return Result.Failure(WarehouseTransferErrors.ProductTableUnavailable(item.ProductTableId, item.ProductTable.WarehouseProductTable.StatusId, _userContext.LanguageId));

                if (item.ProductTable.WarehouseProductTable.WarehouseId != doc.SourceWarehouseId)
                    return Result.Failure(WarehouseTransferErrors.ProductTableWarehouseMismatch(item.ProductTableId, doc.SourceWarehouseId, _userContext.LanguageId));            }
        }

        return Result.Success();
    }

    private Result ValidateForCancel(WarehouseTransferDoc doc)
    {
        foreach (var productTable in GetTransferProductTables(doc))
        {
            if (productTable.WarehouseProductTable == null)
                return Result.Failure(WarehouseTransferErrors.ProductTableInactive(productTable.Id, _userContext.LanguageId));

            if (productTable.WarehouseProductTable.StatusId != ProductTableStatusIdConst.IN_STOCK)
                return Result.Failure(WarehouseTransferErrors.ProductTableUnavailable(productTable.Id, productTable.WarehouseProductTable.StatusId, _userContext.LanguageId));

            if (productTable.WarehouseProductTable.WarehouseId != doc.DestinationWarehouseId)
                return Result.Failure(WarehouseTransferErrors.ProductTableWarehouseMismatch(productTable.Id, doc.DestinationWarehouseId, _userContext.LanguageId));
        }
        return Result.Success();
    }

    private async Task<PostingBatch> CreatePostingBatchAsync(WarehouseTransferDoc doc, string status, string comment, CancellationToken ct)
    {
        var now = DateTime.Now;
        var batch = new PostingBatch
        {
            OrganizationId = doc.OrganizationId,
            DocumentTypeId = DocumentTypeIdConst.WAREHOUSETRANSFER,
            DocumentId = doc.Id,
            Status = status,
            PostedByUserId = _userContext.Id,
            PostedAt = now,
            Comment = comment
        };

        await _postingBatchCommand.CreateAsync(batch, ct);
        return batch;
    }

    private async Task<PostingBatch?> GetActivePostingBatchAsync(long transferDocId, CancellationToken ct)
    {
        var query = _queryBuilder.For<PostingBatch>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.WAREHOUSETRANSFER &&
                        x.DocumentId == transferDocId &&
                        x.Status == PostingBatchStatusConst.POSTED)
            .Build();

        return await _postingBatchQuery.GetAsync(query, ct);
    }

    private async Task<bool> HasBusinessEffectsAsync(long transferDocId, CancellationToken ct) =>
        await _inventoryRegisterQuery.AnyAsync(x =>
            x.DocumentTypeId == DocumentTypeIdConst.WAREHOUSETRANSFER &&
            x.DocumentId == transferDocId &&
            x.ReversalEntryId == null, ct);

    private async Task ReloadTransferProductTablesAsync(WarehouseTransferDoc doc, CancellationToken ct)
    {
        foreach (var productTable in GetTransferProductTables(doc))
            await _productTableCommand.ReloadAsync(productTable, ct);
    }

    private async Task<Result> ReverseInventoryEntriesAsync(WarehouseTransferDoc doc, long reversalBatchId, CancellationToken ct)
    {
        var expectedRows = doc.WarehouseTransferLines
            .Where(x => !x.Product.IsService)
            .Sum(x => x.Product.IsPieceTracked ? x.WarehouseTransferDocTables.Count * 2 : 2);

        var query = _queryBuilder.For<RegisterBalance>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.WAREHOUSETRANSFER &&
                        x.DocumentId == doc.Id &&
                        x.ReversalEntryId == null)
            .Build();

        var entries = await _inventoryRegisterQuery.GetAllAsync(query, ct);
        if (entries.Count != expectedRows)
            return Result.Failure(WarehouseTransferErrors.MissingInventoryRegisterEntries(doc.Id, _userContext.LanguageId));

        var now = DateTime.Now;
        var reversals = entries.Select(entry => new RegisterBalance
        {
            OrganizationId = entry.OrganizationId,
            DocumentTypeId = entry.DocumentTypeId,
            DocumentId = entry.DocumentId,
            WarehouseId = entry.WarehouseId,
            ProductId = entry.ProductId,
            ProductTableId = entry.ProductTableId,
            OperationTypeId = entry.OperationTypeId == OperationTypeIdConst.OUT ? OperationTypeIdConst.IN : OperationTypeIdConst.OUT,
            Quantity = entry.Quantity,
            Amount = entry.Amount,
            DocDate = now,
            CreatedDate = now,
            PostingBatchId = reversalBatchId,
            SourceLineId = entry.SourceLineId,
            ReversalEntryId = entry.Id
        }).ToList();

        await _inventoryRegisterCommand.CreateAsync(reversals, ct);
        var warehouseProductUpdate = await _warehouseProductBalanceService.ApplyInventoryEntriesAsync(reversals, ct);
        if (!warehouseProductUpdate.IsSuccess)
            return Result.Failure(warehouseProductUpdate.Error);

        return Result.Success();
    }

    private static List<ProductTable> GetTransferProductTables(WarehouseTransferDoc doc) =>
        doc.WarehouseTransferLines
            .SelectMany(x => x.WarehouseTransferDocTables)
            .Select(x => x.ProductTable)
            .GroupBy(x => x.Id)
            .Select(x => x.First())
            .ToList();
}
