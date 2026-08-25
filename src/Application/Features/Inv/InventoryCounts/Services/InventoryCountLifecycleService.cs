using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.Acc.AccountingPeriods;
using Application.Features.AuditLogs;
using Application.Features.DocumentNumbers;
using Application.Features.InventoryAdjustments;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.InventoryCounts;

public class InventoryCountLifecycleService : BaseService, IInventoryCountLifecycleService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IDocumentPostingLock _postingLock;
    private readonly IAccountingPeriodValidator _periodValidator;
    private readonly IAuditLogService _auditLogService;
    private readonly IDocumentNumberService _documentNumberService;
    private readonly IInventoryAdjustmentLifecycleService _inventoryAdjustmentLifecycleService;
    private readonly IQueryRepository<InventoryCountDoc> _query;
    private readonly ICommandRepository<InventoryCountDoc> _command;
    private readonly ICommandRepository<PostingBatch> _postingBatchCommand;
    private readonly IQueryRepository<PostingBatch> _postingBatchQuery;
    private readonly ICommandRepository<InventoryAdjustmentDoc> _inventoryAdjustmentCommand;
    private readonly IQueryRepository<InventoryAdjustmentDoc> _inventoryAdjustmentQuery;
    private readonly ICommandRepository<ProductTable> _productTableCommand;
    private readonly IQueryRepository<ProductTable> _productTableQuery;
    private readonly IQueryRepository<WarehouseProductBatchTable> _warehouseProductBatchTableQuery;

    public InventoryCountLifecycleService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IDocumentPostingLock postingLock,
        IAccountingPeriodValidator periodValidator,
        IAuditLogService auditLogService,
        IDocumentNumberService documentNumberService,
        IInventoryAdjustmentLifecycleService inventoryAdjustmentLifecycleService,
        IQueryRepository<InventoryCountDoc> query,
        ICommandRepository<InventoryCountDoc> command,
        ICommandRepository<PostingBatch> postingBatchCommand,
        IQueryRepository<PostingBatch> postingBatchQuery,
        ICommandRepository<InventoryAdjustmentDoc> inventoryAdjustmentCommand,
        IQueryRepository<InventoryAdjustmentDoc> inventoryAdjustmentQuery,
        ICommandRepository<ProductTable> productTableCommand,
        IQueryRepository<ProductTable> productTableQuery,
        IQueryRepository<WarehouseProductBatchTable> warehouseProductBatchTableQuery,
        ILogger<InventoryCountLifecycleService> logger,
        IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _postingLock = postingLock;
        _periodValidator = periodValidator;
        _auditLogService = auditLogService;
        _documentNumberService = documentNumberService;
        _inventoryAdjustmentLifecycleService = inventoryAdjustmentLifecycleService;
        _query = query;
        _command = command;
        _postingBatchCommand = postingBatchCommand;
        _postingBatchQuery = postingBatchQuery;
        _inventoryAdjustmentCommand = inventoryAdjustmentCommand;
        _inventoryAdjustmentQuery = inventoryAdjustmentQuery;
        _productTableCommand = productTableCommand;
        _productTableQuery = productTableQuery;
        _warehouseProductBatchTableQuery = warehouseProductBatchTableQuery;
    }

    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(ConfirmAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            await _postingLock.AcquireAsync(DocumentTypeIdConst.INVENTORYCOUNT, id, ct);

            var doc = await GetForLifecycleAsync(id, ct);
            if (doc == null)
                return Result.Failure(InventoryCountErrors.NotFound(id, _userContext.LanguageId));

            if (doc.OrganizationId != _userContext.OrganizationId.Value)
                return Result.Failure(InventoryCountErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Failure(InventoryCountErrors.AlreadyCancelled(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.POSTED)
            {
                return await GetActivePostingBatchAsync(id, ct) is not null
                    ? Result.Success()
                    : Result.Failure(InventoryCountErrors.MissingPostingBatch(id, _userContext.LanguageId));
            }

            if (doc.StatusId != DocumentStatusIdConst.PENDING)
                return Result.Failure(InventoryCountErrors.CannotConfirmInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));

            if (!doc.CountCompletedAt.HasValue)
                return Result.Failure(InventoryCountErrors.CountNotCompleted(id, _userContext.LanguageId));

            var periodValidation = await _periodValidator.EnsureOpenAsync(doc.OrganizationId, doc.DocDate, ct);
            if (!periodValidation.IsSuccess)
                return periodValidation;

            if (doc.Warehouse.StateId != StateIdConst.ACTIVE)
                return Result.Failure(InventoryCountErrors.WarehouseInactive(doc.WarehouseId, _userContext.LanguageId));

            if (await GetActivePostingBatchAsync(id, ct) != null || await HasBusinessEffectsAsync(doc, ct))
                return Result.Failure(InventoryCountErrors.BusinessEffectsAlreadyExist(id, _userContext.LanguageId));

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto != null)
                _auditLogService.SetOldValues(oldDocDto);

            var differences = await BuildDifferencesAsync(doc, ct);
            var postingBatch = await CreatePostingBatchAsync(doc, PostingBatchStatusConst.POSTED, "Inventory count confirmed", ct);

            var negativeDifferenceRows = differences.Where(x => x.MissingQuantity > 0).ToList();
            if (negativeDifferenceRows.Count > 0)
            {
                var negativeAdjustmentResult = await CreateNegativeAdjustmentAsync(doc, negativeDifferenceRows, ct);
                if (!negativeAdjustmentResult.IsSuccess)
                    return Result.Failure(negativeAdjustmentResult.Error);

                var negativeAdjustment = negativeAdjustmentResult.Value;
                doc.NegativeAdjustmentDocId = negativeAdjustment.Id;

                var negativeConfirm = await _inventoryAdjustmentLifecycleService.ConfirmAsync(negativeAdjustment.Id, ct);
                if (!negativeConfirm.IsSuccess)
                    return negativeConfirm;
            }

            var positiveDifferenceRows = differences.Where(x => x.FoundQuantity > 0).ToList();
            if (positiveDifferenceRows.Count > 0)
            {
                var positiveAdjustmentResult = await CreatePositiveAdjustmentAsync(doc, positiveDifferenceRows, ct);
                if (!positiveAdjustmentResult.IsSuccess)
                    return Result.Failure(positiveAdjustmentResult.Error);

                var positiveAdjustment = positiveAdjustmentResult.Value;
                doc.PositiveAdjustmentDocId = positiveAdjustment.Id;

                var positiveConfirm = await _inventoryAdjustmentLifecycleService.ConfirmAsync(positiveAdjustment.Id, ct);
                if (!positiveConfirm.IsSuccess)
                    return positiveConfirm;

                await ApplyFoundStockMetadataAsync(positiveAdjustment.Id, positiveDifferenceRows.SelectMany(x => x.FoundItems).ToList(), ct);
            }

            doc.StatusId = DocumentStatusIdConst.POSTED;
            doc.PostedAt ??= DateTime.Now;
            doc.PostedByUserId ??= _userContext.Id;
            await _command.UpdateAsync(doc, ct);

            var newDocDto = await GetByIdInternalAsync(id, ct);
            if (newDocDto != null)
            {
                _auditLogService.SetNewValues(newDocDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.InventoryCountDoc, id.ToString(), AuditLogOperationTypeConst.Update, "Confirmed");
            }

            return Result.Success();
        }, ct);

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CancelAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            await _postingLock.AcquireAsync(DocumentTypeIdConst.INVENTORYCOUNT, id, ct);

            var doc = await GetForLifecycleAsync(id, ct);
            if (doc == null)
                return Result.Failure(InventoryCountErrors.NotFound(id, _userContext.LanguageId));

            if (doc.OrganizationId != _userContext.OrganizationId.Value)
                return Result.Failure(InventoryCountErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Success();

            if (doc.StatusId != DocumentStatusIdConst.DRAFT &&
                doc.StatusId != DocumentStatusIdConst.PENDING &&
                doc.StatusId != DocumentStatusIdConst.POSTED)
                return Result.Failure(InventoryCountErrors.CannotCancelInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));

            var periodValidation = await _periodValidator.EnsureOpenAsync(doc.OrganizationId, doc.DocDate, ct);
            if (!periodValidation.IsSuccess)
                return periodValidation;

            var reversalPeriodValidation = await _periodValidator.EnsureOpenAsync(doc.OrganizationId, DateTime.Now, ct);
            if (!reversalPeriodValidation.IsSuccess)
                return reversalPeriodValidation;

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto != null)
                _auditLogService.SetOldValues(oldDocDto);

            if (doc.StatusId == DocumentStatusIdConst.POSTED)
            {
                var activePostingBatch = await GetActivePostingBatchAsync(id, ct);
                if (activePostingBatch == null)
                    return Result.Failure(InventoryCountErrors.MissingPostingBatch(id, _userContext.LanguageId));

                if (doc.PositiveAdjustmentDocId.HasValue)
                {
                    var cancelPositive = await _inventoryAdjustmentLifecycleService.CancelAsync(doc.PositiveAdjustmentDocId.Value, ct);
                    if (!cancelPositive.IsSuccess)
                        return cancelPositive;
                }

                if (doc.NegativeAdjustmentDocId.HasValue)
                {
                    var cancelNegative = await _inventoryAdjustmentLifecycleService.CancelAsync(doc.NegativeAdjustmentDocId.Value, ct);
                    if (!cancelNegative.IsSuccess)
                        return cancelNegative;
                }

                await CreatePostingBatchAsync(doc, PostingBatchStatusConst.REVERSAL, "Inventory count cancelled", ct);
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
                await _auditLogService.CreateAsync(AuditLogTableConst.InventoryCountDoc, id.ToString(), AuditLogOperationTypeConst.Update, "Cancelled");
            }

            return Result.Success();
        }, ct);

    private async Task<InventoryCountDoc?> GetForLifecycleAsync(long id, CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return null;

        var query = _queryBuilder.For<InventoryCountDoc>()
            .Where(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value)
            .Build();
        query.AddIncludes(b => b.Include(x => x.Warehouse));
        query.AddIncludes(b => b.Include(x => x.InventoryCountLines).ThenInclude(x => x.Product).ThenInclude(x => x.Unit));
        query.AddIncludes(b => b.Include(x => x.InventoryCountLines).ThenInclude(x => x.InventoryCountDocTables).ThenInclude(x => x.ProductTable!).ThenInclude(x => x.Product));
        query.AddIncludes(b => b.Include(x => x.InventoryCountLines).ThenInclude(x => x.InventoryCountDocTables).ThenInclude(x => x.ProductTable!).ThenInclude(x => x.WarehouseProductTable));
        return await _query.GetAsync(query, ct);
    }

    private async Task<InventoryCountDto?> GetByIdInternalAsync(long id, CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return null;

        var query = _queryBuilder.For<InventoryCountDoc>()
            .Where(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value)
            .As<InventoryCountDto>()
            .Build();
        return await _query.GetAsync(query, ct);
    }

    private async Task<List<InventoryCountDifferenceDto>> BuildDifferencesAsync(InventoryCountDoc doc, CancellationToken ct)
    {
        var expectedQuery = _queryBuilder.For<ProductTable>()
            .Where(x => x.Product.OrganizationId == doc.OrganizationId &&
                        x.WarehouseProductTable != null &&
                        x.WarehouseProductTable.WarehouseId == doc.WarehouseId &&
                        x.Product.StateId == StateIdConst.ACTIVE &&
                        x.WarehouseProductTable.StatusId == ProductTableStatusIdConst.IN_STOCK)
            .As(x => new InventoryCountExpectedRow
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductName = x.Product.Name,
                UnitId = x.Product.UnitId,
                UnitName = x.Product.Unit.Name
            })
            .Build();

        var expectedProductTables = await _productTableQuery.GetAllAsync(expectedQuery, ct);

        var expectedByProduct = expectedProductTables.GroupBy(x => new { x.ProductId, x.UnitId })
            .ToDictionary(x => (x.Key.ProductId, x.Key.UnitId), x => x.ToList());

        var differences = new Dictionary<(int ProductId, short UnitId), InventoryCountDifferenceDto>();

        foreach (var line in doc.InventoryCountLines)
        {
            var key = (line.ProductId, line.UnitId);
            expectedByProduct.TryGetValue(key, out var expectedRows);
            expectedRows ??= new List<InventoryCountExpectedRow>();

            var existingCountedIds = line.InventoryCountDocTables.Where(x => x.ProductTableId.HasValue).Select(x => x.ProductTableId!.Value).Distinct().ToHashSet();
            var missingRows = expectedRows.Where(x => !existingCountedIds.Contains(x.Id)).Select(x => x.Id).ToList();

            var foundItems = line.InventoryCountDocTables
                .Where(x => !x.ProductTableId.HasValue)
                .Select(x => new InventoryCountFoundItemDto
                {
                    Barcode = x.Barcode,
                    SerialNumber = x.SerialNumber,
                    MarkingNumber = x.MarkingNumber,
                    CostPrice = x.CostPrice
                }).ToList();

            var anonymousFoundCount = (int)(line.CountedQuantity - line.InventoryCountDocTables.Count);
            for (var i = 0; i < anonymousFoundCount; i++)
                foundItems.Add(new InventoryCountFoundItemDto { CostPrice = line.DefaultCostPrice });

            differences[key] = new InventoryCountDifferenceDto
            {
                ProductId = line.ProductId,
                ProductName = line.Product.Name,
                UnitId = line.UnitId,
                UnitName = line.Unit.Name,
                ExpectedQuantity = expectedRows.Count,
                CountedQuantity = line.CountedQuantity,
                CorrectQuantity = existingCountedIds.Count,
                MissingQuantity = missingRows.Count,
                FoundQuantity = foundItems.Count,
                MissingProductTableIds = missingRows,
                FoundItems = foundItems
            };
        }

        foreach (var expectedEntry in expectedByProduct)
        {
            if (differences.ContainsKey(expectedEntry.Key))
                continue;

            differences[expectedEntry.Key] = new InventoryCountDifferenceDto
            {
                ProductId = expectedEntry.Key.ProductId,
                ProductName = expectedEntry.Value[0].ProductName,
                UnitId = expectedEntry.Key.UnitId,
                UnitName = expectedEntry.Value[0].UnitName,
                ExpectedQuantity = expectedEntry.Value.Count,
                MissingQuantity = expectedEntry.Value.Count,
                MissingProductTableIds = expectedEntry.Value.Select(x => x.Id).ToList()
            };
        }

        return differences.Values.OrderBy(x => x.ProductName).ToList();
    }

    private async Task<Result<InventoryAdjustmentDoc>> CreateNegativeAdjustmentAsync(
        InventoryCountDoc doc,
        List<InventoryCountDifferenceDto> differences,
        CancellationToken ct)
    {
        var documentNumberResult = await _documentNumberService.GetNextAsync(
            doc.OrganizationId,
            DocumentTypeIdConst.INVENTORYADJUSTMENT,
            doc.DocDate,
            ct);
        if (!documentNumberResult.IsSuccess)
            return Result.Failure<InventoryAdjustmentDoc>(documentNumberResult.Error);

        var costMap = await ResolveMissingCostMapAsync(differences.SelectMany(x => x.MissingProductTableIds).Distinct().ToList(), ct);
        var adjustmentDoc = new InventoryAdjustmentDoc
        {
            OrganizationId = doc.OrganizationId,
            DocNumber = documentNumberResult.Value.DocumentNumber,
            DocDate = doc.DocDate,
            WarehouseId = doc.WarehouseId,
            AdjustmentType = "NEGATIVE_ADJUSTMENT",
            DirectionId = MovementDirectionIdConst.OUT,
            StatusId = DocumentStatusIdConst.DRAFT,
            Comment = $"Generated from inventory count {doc.DocNumber}",
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Now,
            InventoryAdjustmentLines = differences.Select(d => new InventoryAdjustmentLine
            {
                ProductId = d.ProductId,
                UnitId = d.UnitId,
                Quantity = d.MissingQuantity,
                Comment = $"Generated from inventory count {doc.DocNumber}",
                InventoryAdjustmentDocTables = d.MissingProductTableIds.Select(productTableId => new InventoryAdjustmentDocTable
                {
                    ProductTableId = productTableId,
                    CostPrice = costMap.GetValueOrDefault(productTableId, 0m)
                }).ToList()
            }).ToList()
        };

        await _inventoryAdjustmentCommand.CreateAsync(adjustmentDoc, ct);
        return Result.Success(adjustmentDoc);
    }

    private async Task<Result<InventoryAdjustmentDoc>> CreatePositiveAdjustmentAsync(
        InventoryCountDoc doc,
        List<InventoryCountDifferenceDto> differences,
        CancellationToken ct)
    {
        var documentNumberResult = await _documentNumberService.GetNextAsync(
            doc.OrganizationId,
            DocumentTypeIdConst.INVENTORYADJUSTMENT,
            doc.DocDate,
            ct);
        if (!documentNumberResult.IsSuccess)
            return Result.Failure<InventoryAdjustmentDoc>(documentNumberResult.Error);

        var lineMap = doc.InventoryCountLines.ToDictionary(x => (x.ProductId, x.UnitId));
        var adjustmentDoc = new InventoryAdjustmentDoc
        {
            OrganizationId = doc.OrganizationId,
            DocNumber = documentNumberResult.Value.DocumentNumber,
            DocDate = doc.DocDate,
            WarehouseId = doc.WarehouseId,
            AdjustmentType = "POSITIVE_ADJUSTMENT",
            DirectionId = MovementDirectionIdConst.IN,
            StatusId = DocumentStatusIdConst.DRAFT,
            Comment = $"Generated from inventory count {doc.DocNumber}",
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Now,
            InventoryAdjustmentLines = new List<InventoryAdjustmentLine>()
        };

        foreach (var difference in differences)
        {
            var sourceLine = lineMap[(difference.ProductId, difference.UnitId)];
            var line = new InventoryAdjustmentLine
            {
                ProductId = difference.ProductId,
                UnitId = difference.UnitId,
                Quantity = difference.FoundQuantity,
                Comment = $"Generated from inventory count {doc.DocNumber}"
            };

            foreach (var foundItem in difference.FoundItems)
            {
                line.InventoryAdjustmentDocTables.Add(new InventoryAdjustmentDocTable
                {
                    ProductTableId = null,
                    CostPrice = foundItem.CostPrice > 0 ? foundItem.CostPrice : sourceLine.DefaultCostPrice
                });
            }

            adjustmentDoc.InventoryAdjustmentLines.Add(line);
        }

        await _inventoryAdjustmentCommand.CreateAsync(adjustmentDoc, ct);
        return Result.Success(adjustmentDoc);
    }

    private async Task ApplyFoundStockMetadataAsync(long positiveAdjustmentDocId, List<InventoryCountFoundItemDto> foundItems, CancellationToken ct)
    {
        if (foundItems.Count == 0)
            return;

        var adjustment = await GetInventoryAdjustmentAsync(positiveAdjustmentDocId, ct);
        if (adjustment == null)
            return;

        var createdTables = adjustment.InventoryAdjustmentLines
            .SelectMany(x => x.InventoryAdjustmentDocTables)
            .Where(x => x.WasCreated && x.ProductTable != null)
            .Select(x => x.ProductTable!)
            .ToList();

        for (var i = 0; i < createdTables.Count && i < foundItems.Count; i++)
        {
            createdTables[i].SerialNumber = foundItems[i].SerialNumber;
            createdTables[i].MarkingNumber = foundItems[i].MarkingNumber;
        }

        if (createdTables.Count > 0)
            await _productTableCommand.UpdateAsync(createdTables, ct);
    }

    private async Task<Dictionary<int, decimal>> ResolveMissingCostMapAsync(List<int> productTableIds, CancellationToken ct)
    {
        if (productTableIds.Count == 0)
            return new Dictionary<int, decimal>();

        var query = _queryBuilder.For<WarehouseProductBatchTable>()
            .Where(x => productTableIds.Contains(x.ProductTableId))
            .As(x => new InventoryCountBatchCostRow(
                x.ProductTableId,
                x.BatchId,
                x.Batch.ReceivedDate,
                x.Batch.UnitCost))
            .Build();
        var batchLinks = await _warehouseProductBatchTableQuery.GetAllAsync(query, ct);

        return batchLinks
            .GroupBy(x => x.ProductTableId)
            .ToDictionary(
                x => x.Key,
                x => x.OrderByDescending(e => e.ReceivedDate)
                    .ThenByDescending(e => e.BatchId)
                    .First()
                    .UnitCost ?? 0m);
    }

    private async Task<InventoryAdjustmentDoc?> GetInventoryAdjustmentAsync(long id, CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return null;

        var query = _queryBuilder.For<InventoryAdjustmentDoc>()
            .Where(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value)
            .Build();
        query.AddIncludes(b => b.Include(x => x.InventoryAdjustmentLines).ThenInclude(x => x.InventoryAdjustmentDocTables).ThenInclude(x => x.ProductTable));
        return await _inventoryAdjustmentQuery.GetAsync(query, ct);
    }

    private sealed class InventoryCountExpectedRow
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public short UnitId { get; set; }
        public string UnitName { get; set; } = null!;
    }

    private sealed record InventoryCountBatchCostRow(
        int ProductTableId,
        long BatchId,
        DateTime ReceivedDate,
        decimal? UnitCost);

    private async Task<PostingBatch> CreatePostingBatchAsync(InventoryCountDoc doc, string status, string comment, CancellationToken ct)
    {
        var now = DateTime.Now;
        var batch = new PostingBatch
        {
            OrganizationId = doc.OrganizationId,
            DocumentTypeId = DocumentTypeIdConst.INVENTORYCOUNT,
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
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.INVENTORYCOUNT &&
                        x.DocumentId == docId &&
                        x.Status == PostingBatchStatusConst.POSTED)
            .Build();
        return await _postingBatchQuery.GetAsync(query, ct);
    }

    private async Task<bool> HasBusinessEffectsAsync(InventoryCountDoc doc, CancellationToken ct)
    {
        if (doc.PositiveAdjustmentDocId.HasValue && await _inventoryAdjustmentQuery.AnyAsync(x => x.Id == doc.PositiveAdjustmentDocId.Value, ct))
            return true;

        if (doc.NegativeAdjustmentDocId.HasValue && await _inventoryAdjustmentQuery.AnyAsync(x => x.Id == doc.NegativeAdjustmentDocId.Value, ct))
            return true;

        return false;
    }
}
