using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.Acc.AccountingPeriods;
using Application.Features.AuditLogs;
using Application.Features.InventoryCounts;
using Application.Features.InventoryMovements;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.InventoryAdjustments;

public class InventoryAdjustmentLifecycleService : BaseService, IInventoryAdjustmentLifecycleService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IDocumentPostingLock _postingLock;
    private readonly IAccountingPeriodValidator _periodValidator;
    private readonly IActiveInventoryCountGuardService _activeInventoryCountGuardService;
    private readonly IAuditLogService _auditLogService;
    private readonly IInventoryDispatcher _inventoryDispatcher;
    private readonly IQueryRepository<InventoryAdjustmentDoc> _query;
    private readonly ICommandRepository<InventoryAdjustmentDoc> _command;
    private readonly ICommandRepository<InventoryAdjustmentDocTable> _tableCommand;
    private readonly ICommandRepository<ProductTable> _productTableCommand;
    private readonly IQueryRepository<PostingBatch> _postingBatchQuery;
    private readonly ICommandRepository<PostingBatch> _postingBatchCommand;
    private readonly IQueryRepository<WarehouseProductMovement> _warehouseMovementQuery;

    public InventoryAdjustmentLifecycleService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IDocumentPostingLock postingLock,
        IAccountingPeriodValidator periodValidator,
        IActiveInventoryCountGuardService activeInventoryCountGuardService,
        IAuditLogService auditLogService,
        IInventoryDispatcher inventoryDispatcher,
        IQueryRepository<InventoryAdjustmentDoc> query,
        ICommandRepository<InventoryAdjustmentDoc> command,
        ICommandRepository<InventoryAdjustmentDocTable> tableCommand,
        ICommandRepository<ProductTable> productTableCommand,
        IQueryRepository<PostingBatch> postingBatchQuery,
        ICommandRepository<PostingBatch> postingBatchCommand,
        IQueryRepository<WarehouseProductMovement> warehouseMovementQuery,
        ILogger<InventoryAdjustmentLifecycleService> logger,
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
        _query = query;
        _command = command;
        _tableCommand = tableCommand;
        _productTableCommand = productTableCommand;
        _postingBatchQuery = postingBatchQuery;
        _postingBatchCommand = postingBatchCommand;
        _warehouseMovementQuery = warehouseMovementQuery;
    }

    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(ConfirmAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            await _postingLock.AcquireAsync(DocumentTypeIdConst.INVENTORYADJUSTMENT, id, ct);

            var doc = await GetForLifecycleAsync(id, ct);
            if (doc == null)
                return Result.Failure(InventoryAdjustmentErrors.NotFound(id, _userContext.LanguageId));

            if (doc.OrganizationId != _userContext.OrganizationId.Value)
                return Result.Failure(InventoryAdjustmentErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Failure(InventoryAdjustmentErrors.AlreadyCancelled(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.POSTED)
            {
                return await GetActivePostingBatchAsync(id, ct) is not null
                    ? Result.Success()
                    : Result.Failure(InventoryAdjustmentErrors.MissingPostingBatch(id, _userContext.LanguageId));
            }

            if (doc.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(InventoryAdjustmentErrors.CannotConfirmInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));

            var periodValidation = await _periodValidator.EnsureOpenAsync(doc.OrganizationId, doc.DocDate, ct);
            if (!periodValidation.IsSuccess)
                return periodValidation;

            var countGuard = await _activeInventoryCountGuardService.EnsureWarehouseIsNotBlockedAsync(doc.OrganizationId, doc.WarehouseId, "InventoryAdjustmentConfirm", ct: ct);
            if (!countGuard.IsSuccess)
                return countGuard;

            var validation = ValidateForConfirm(doc);
            if (!validation.IsSuccess)
                return validation;

            await ReloadAdjustmentProductTablesAsync(doc, ct);
            validation = ValidateForConfirm(doc);
            if (!validation.IsSuccess)
                return validation;

            if (await GetActivePostingBatchAsync(id, ct) != null || await HasBusinessEffectsAsync(id, ct))
                return Result.Failure(InventoryAdjustmentErrors.BusinessEffectsAlreadyExist(id, _userContext.LanguageId));

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto != null)
                _auditLogService.SetOldValues(oldDocDto);

            var touchedProductTables = PrepareProductTablesForConfirm(doc);
            var existingProductTables = touchedProductTables.Where(x => x.Id != 0).GroupBy(x => x.Id).Select(x => x.First()).ToList();
            if (existingProductTables.Count != 0)
                await _productTableCommand.UpdateAsync(existingProductTables, ct);

            var createdProductTables = touchedProductTables.Where(x => x.Id == 0).ToList();
            if (createdProductTables.Count != 0)
            {
                await _productTableCommand.CreateAsync(createdProductTables, ct);
                foreach (var item in doc.InventoryAdjustmentLines.SelectMany(x => x.InventoryAdjustmentDocTables).Where(x => x.WasCreated && x.ProductTable != null))
                    item.ProductTableId = item.ProductTable!.Id;
            }

            await _tableCommand.UpdateAsync(doc.InventoryAdjustmentLines.SelectMany(x => x.InventoryAdjustmentDocTables).ToList(), ct);

            var postingBatch = await CreatePostingBatchAsync(doc, PostingBatchStatusConst.POSTED, "Inventory adjustment confirmed", ct);

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
                await _auditLogService.CreateAsync(AuditLogTableConst.InventoryAdjustmentDoc, id.ToString(), AuditLogOperationTypeConst.Update, "Confirmed");
            }

            return Result.Success();
        }, ct);

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CancelAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            await _postingLock.AcquireAsync(DocumentTypeIdConst.INVENTORYADJUSTMENT, id, ct);

            var doc = await GetForLifecycleAsync(id, ct);
            if (doc == null)
                return Result.Failure(InventoryAdjustmentErrors.NotFound(id, _userContext.LanguageId));

            if (doc.OrganizationId != _userContext.OrganizationId.Value)
                return Result.Failure(InventoryAdjustmentErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Success();

            if (doc.StatusId != DocumentStatusIdConst.DRAFT &&
                doc.StatusId != DocumentStatusIdConst.POSTED)
                return Result.Failure(InventoryAdjustmentErrors.CannotCancelInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));

            var periodValidation = await _periodValidator.EnsureOpenAsync(doc.OrganizationId, doc.DocDate, ct);
            if (!periodValidation.IsSuccess)
                return periodValidation;

            var reversalPeriodValidation = await _periodValidator.EnsureOpenAsync(doc.OrganizationId, DateTime.Now, ct);
            if (!reversalPeriodValidation.IsSuccess)
                return reversalPeriodValidation;

            var countGuard = await _activeInventoryCountGuardService.EnsureWarehouseIsNotBlockedAsync(doc.OrganizationId, doc.WarehouseId, "InventoryAdjustmentCancel", ct: ct);
            if (!countGuard.IsSuccess)
                return countGuard;

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
                    return Result.Failure(InventoryAdjustmentErrors.MissingPostingBatch(id, _userContext.LanguageId));

                var reversalBatch = await CreatePostingBatchAsync(doc, PostingBatchStatusConst.REVERSAL, "Inventory adjustment cancelled", ct);

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
                await _auditLogService.CreateAsync(AuditLogTableConst.InventoryAdjustmentDoc, id.ToString(), AuditLogOperationTypeConst.Update, "Cancelled");
            }

            return Result.Success();
        }, ct);

    private async Task<InventoryAdjustmentDto?> GetByIdInternalAsync(long id, CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return null;

        var query = _queryBuilder.For<InventoryAdjustmentDoc>()
            .Where(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value)
            .As<InventoryAdjustmentDto>()
            .Build();
        return await _query.GetAsync(query, ct);
    }

    private async Task<InventoryAdjustmentDoc?> GetForLifecycleAsync(long id, CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return null;

        var query = _queryBuilder.For<InventoryAdjustmentDoc>()
            .Where(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value)
            .Build();
        query.AddIncludes(b => b.Include(x => x.Warehouse));
        query.AddIncludes(b => b.Include(x => x.InventoryAdjustmentLines).ThenInclude(x => x.Product));
        query.AddIncludes(b => b.Include(x => x.InventoryAdjustmentLines).ThenInclude(x => x.InventoryAdjustmentDocTables).ThenInclude(x => x.ProductTable!).ThenInclude(x => x.Product));
        query.AddIncludes(b => b.Include(x => x.InventoryAdjustmentLines).ThenInclude(x => x.InventoryAdjustmentDocTables).ThenInclude(x => x.ProductTable!).ThenInclude(x => x.WarehouseProductTable));
        return await _query.GetAsync(query, ct);
    }

    private Result ValidateForConfirm(InventoryAdjustmentDoc doc)
    {
        if (!InventoryAdjustmentDirectionPolicy.IsCompatible(doc.AdjustmentType, doc.DirectionId))
            return Result.Failure(InventoryAdjustmentErrors.InvalidDirection(doc.AdjustmentType, doc.DirectionId, _userContext.LanguageId));

        if (doc.Warehouse.StateId != StateIdConst.ACTIVE)
            return Result.Failure(InventoryAdjustmentErrors.WarehouseInactive(doc.WarehouseId, _userContext.LanguageId));

        if (doc.InventoryAdjustmentLines.Count == 0)
            return Result.Failure(InventoryAdjustmentErrors.LinesRequired(doc.Id, _userContext.LanguageId));

        var seenProductTableIds = new HashSet<int>();

        foreach (var line in doc.InventoryAdjustmentLines)
        {
            if (line.Product.IsService)
                return Result.Failure(InventoryAdjustmentErrors.ProductServiceNotAllowed(line.ProductId, _userContext.LanguageId));

            if (line.Quantity <= 0)
                return Result.Failure(InventoryAdjustmentErrors.InvalidQuantity(line.ProductId, line.Quantity, _userContext.LanguageId));

            if (!line.Product.IsPieceTracked)
            {
                if (line.InventoryAdjustmentDocTables.Count > 0)
                    return Result.Failure(InventoryAdjustmentErrors.QuantityItemsMismatch(line.ProductId, 0m, line.InventoryAdjustmentDocTables.Count, _userContext.LanguageId));

                continue;
            }

            if (line.Quantity != decimal.Truncate(line.Quantity) || line.InventoryAdjustmentDocTables.Count != (int)line.Quantity)
                return Result.Failure(InventoryAdjustmentErrors.QuantityItemsMismatch(line.ProductId, line.Quantity, line.InventoryAdjustmentDocTables.Count, _userContext.LanguageId));

            foreach (var item in line.InventoryAdjustmentDocTables)
            {
                if (!item.ProductTableId.HasValue)
                {
                    if (doc.DirectionId == MovementDirectionIdConst.OUT)
                        return Result.Failure(InventoryAdjustmentErrors.ProductTableNotFound(0, _userContext.LanguageId));

                    continue;
                }

                if (!seenProductTableIds.Add(item.ProductTableId.Value))
                    return Result.Failure(InventoryAdjustmentErrors.DuplicateProductTable(item.ProductTableId.Value, _userContext.LanguageId));

                if (item.ProductTable == null || item.ProductTable.Product.OrganizationId != doc.OrganizationId)
                    return Result.Failure(InventoryAdjustmentErrors.ProductTableNotFound(item.ProductTableId.Value, _userContext.LanguageId));

                if (item.ProductTable.ProductId != line.ProductId)
                    return Result.Failure(InventoryAdjustmentErrors.ProductTableProductMismatch(item.ProductTableId.Value, line.ProductId, _userContext.LanguageId));

                if (item.ProductTable.Product.StateId != StateIdConst.ACTIVE)
                    return Result.Failure(InventoryAdjustmentErrors.ProductTableInactive(item.ProductTableId.Value, _userContext.LanguageId));

                if (item.ProductTable.WarehouseProductTable?.WarehouseId != doc.WarehouseId)
                    return Result.Failure(InventoryAdjustmentErrors.ProductTableWarehouseMismatch(item.ProductTableId.Value, doc.WarehouseId, _userContext.LanguageId));

                if (doc.DirectionId == MovementDirectionIdConst.OUT && item.ProductTable.WarehouseProductTable?.StatusId != ProductTableStatusIdConst.IN_STOCK)
                    return Result.Failure(InventoryAdjustmentErrors.ProductTableUnavailable(
                        item.ProductTableId.Value,
                        item.ProductTable.WarehouseProductTable?.StatusId ?? ProductTableStatusIdConst.SOLD,
                        _userContext.LanguageId));
            }
        }

        return Result.Success();
    }

    private Result ValidateForCancel(InventoryAdjustmentDoc doc)
    {
        var requiresExistingProductTable = doc.DirectionId == MovementDirectionIdConst.OUT;

        foreach (var item in doc.InventoryAdjustmentLines.SelectMany(x => x.InventoryAdjustmentDocTables))
        {
            if (requiresExistingProductTable && item.ProductTable == null)
                return Result.Failure(InventoryAdjustmentErrors.ProductTableNotFound(item.ProductTableId ?? 0, _userContext.LanguageId));

            if (item.WasCreated && item.ProductTable == null)
                return Result.Failure(InventoryAdjustmentErrors.ProductTableNotFound(item.ProductTableId ?? 0, _userContext.LanguageId));
        }

        return Result.Success();
    }

    private List<ProductTable> PrepareProductTablesForConfirm(InventoryAdjustmentDoc doc)
    {
        var createdProductTables = new List<ProductTable>();

        foreach (var line in doc.InventoryAdjustmentLines)
        {
            foreach (var item in line.InventoryAdjustmentDocTables.Where(x => !x.ProductTableId.HasValue))
            {
                var productTable = new ProductTable
                {
                    ProductId = line.ProductId,
                    CreatedDate = DateTime.Now
                };

                item.WasCreated = true;
                item.ProductTable = productTable;
                createdProductTables.Add(productTable);
            }
        }

        return createdProductTables;
    }
    private async Task ReloadAdjustmentProductTablesAsync(InventoryAdjustmentDoc doc, CancellationToken ct)
    {
        foreach (var productTable in doc.InventoryAdjustmentLines
                     .SelectMany(x => x.InventoryAdjustmentDocTables)
                     .Where(x => x.ProductTable != null && x.ProductTableId.HasValue)
                     .Select(x => x.ProductTable!)
                     .GroupBy(x => x.Id)
                     .Select(x => x.First()))
        {
            await _productTableCommand.ReloadAsync(productTable, ct);
        }
    }

    private async Task<PostingBatch> CreatePostingBatchAsync(InventoryAdjustmentDoc doc, string status, string comment, CancellationToken ct)
    {
        var now = DateTime.Now;
        var batch = new PostingBatch
        {
            OrganizationId = doc.OrganizationId,
            DocumentTypeId = DocumentTypeIdConst.INVENTORYADJUSTMENT,
            DocumentId = doc.Id,
            Status = status,
            PostedByUserId = _userContext.Id,
            PostedAt = now,
            Comment = comment
        };

        await _postingBatchCommand.CreateAsync(batch, ct);
        return batch;
    }

    private async Task<PostingBatch?> GetActivePostingBatchAsync(long docId, CancellationToken ct)
    {
        var query = _queryBuilder.For<PostingBatch>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.INVENTORYADJUSTMENT &&
                        x.DocumentId == docId &&
                        x.Status == PostingBatchStatusConst.POSTED)
            .Build();

        return await _postingBatchQuery.GetAsync(query, ct);
    }

    private async Task<bool> HasBusinessEffectsAsync(long docId, CancellationToken ct) =>
        await _warehouseMovementQuery.AnyAsync(x =>
            x.DocumentTypeId == DocumentTypeIdConst.INVENTORYADJUSTMENT &&
            x.DocumentId == docId, ct);

    private Task<Result> ReverseInventoryEntriesAsync(InventoryAdjustmentDoc doc, long reversalBatchId, CancellationToken ct) =>
        _inventoryDispatcher.ReverseAsync(doc, ct);

}
