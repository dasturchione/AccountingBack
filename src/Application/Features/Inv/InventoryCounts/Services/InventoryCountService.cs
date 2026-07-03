using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Extensions;
using Application.Common.Pagination;
using Application.Features.AuditLogs;
using Application.Features.InventoryAdjustments;
using Application.Features.InventoryRegisterBalances;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.InventoryCounts;

public class InventoryCountService : BaseService, IInventoryCountService
{
    private readonly IUserContext _userContext;
    private readonly IInventoryReadDbContext _inventoryReadDbContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IAuditLogService _auditLogService;
    private readonly IInventoryCountLifecycleService _lifecycleService;
    private readonly IDocNumberGenerator _docNumberGenerator;
    private readonly IQueryRepository<InventoryCountDoc> _query;
    private readonly ICommandRepository<InventoryCountDoc> _command;
    private readonly ICommandRepository<InventoryCountLine> _lineCommand;
    private readonly ICommandRepository<InventoryCountDocTable> _tableCommand;
    private readonly IQueryRepository<PostingBatch> _postingBatchQuery;
    private readonly IQueryRepository<RegisterBalance> _inventoryRegisterQuery;
    private readonly IQueryRepository<Organization> _organizationQuery;
    private readonly IQueryRepository<Warehouse> _warehouseQuery;
    private readonly IQueryRepository<Product> _productQuery;
    private readonly IQueryRepository<Unit> _unitQuery;
    private readonly IQueryRepository<ProductTable> _productTableQuery;
    private readonly IQueryRepository<InventoryAdjustmentDoc> _inventoryAdjustmentQuery;

    public InventoryCountService(
        IUserContext userContext,
        IInventoryReadDbContext inventoryReadDbContext,
        IQueryBuilder queryBuilder,
        IAuditLogService auditLogService,
        IInventoryCountLifecycleService lifecycleService,
        IDocNumberGenerator docNumberGenerator,
        IQueryRepository<InventoryCountDoc> query,
        ICommandRepository<InventoryCountDoc> command,
        ICommandRepository<InventoryCountLine> lineCommand,
        ICommandRepository<InventoryCountDocTable> tableCommand,
        IQueryRepository<PostingBatch> postingBatchQuery,
        IQueryRepository<RegisterBalance> inventoryRegisterQuery,
        IQueryRepository<Organization> organizationQuery,
        IQueryRepository<Warehouse> warehouseQuery,
        IQueryRepository<Product> productQuery,
        IQueryRepository<Unit> unitQuery,
        IQueryRepository<ProductTable> productTableQuery,
        IQueryRepository<InventoryAdjustmentDoc> inventoryAdjustmentQuery,
        ILogger<InventoryCountService> logger,
        IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _inventoryReadDbContext = inventoryReadDbContext;
        _queryBuilder = queryBuilder;
        _auditLogService = auditLogService;
        _lifecycleService = lifecycleService;
        _docNumberGenerator = docNumberGenerator;
        _query = query;
        _command = command;
        _lineCommand = lineCommand;
        _tableCommand = tableCommand;
        _postingBatchQuery = postingBatchQuery;
        _inventoryRegisterQuery = inventoryRegisterQuery;
        _organizationQuery = organizationQuery;
        _warehouseQuery = warehouseQuery;
        _productQuery = productQuery;
        _unitQuery = unitQuery;
        _productTableQuery = productTableQuery;
        _inventoryAdjustmentQuery = inventoryAdjustmentQuery;
    }

    public Task<Result<PagedResponse<InventoryCountListDto>>> GetAllAsync(InventoryCountListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<PagedResponse<InventoryCountListDto>>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.BuildPaged<InventoryCountDoc, InventoryCountListDto, InventoryCountListFilter>(filter);
            var pagedList = await _query.GetPagedAsync(query, ct);
            return Result.Success(PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize));
        });

    public Task<Result<InventoryCountDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<InventoryCountDto>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.For<InventoryCountDoc>()
                .Where(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value)
                .As<InventoryCountDto>()
                .Build();
            var entity = await _query.GetAsync(query, ct);

            if (entity == null)
                return Result.Failure<InventoryCountDto>(InventoryCountErrors.NotFound(id, _userContext.LanguageId));

            return Result.Success(entity);
        });

    public Task<Result<long>> CreateAsync(InventoryCountCreateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<long>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var organizationId = _userContext.OrganizationId.Value;
            var validationResult = await ValidateDraftAsync(organizationId, dto, null, ct);
            if (!validationResult.IsSuccess)
                return Result.Failure<long>(validationResult.Error);

            var docNumber = await _docNumberGenerator.GenerateAsync(organizationId, "ICT", dto.DocDate, ct);
            var lines = await BuildLinesAsync(dto, organizationId, ct);

            var doc = new InventoryCountDoc
            {
                OrganizationId = organizationId,
                DocNumber = docNumber,
                DocDate = DateTime.SpecifyKind(dto.DocDate, DateTimeKind.Unspecified),
                WarehouseId = dto.WarehouseId,
                StatusId = DocumentStatusIdConst.DRAFT,
                Comment = dto.Comment,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.Now,
                InventoryCountLines = lines
            };

            await _command.CreateAsync(doc, ct);

            var docDto = await GetByIdInternalAsync(doc.Id, ct);
            if (docDto != null)
            {
                _auditLogService.SetNewValues(docDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.InventoryCountDoc, doc.Id.ToString(), AuditLogOperationTypeConst.Create);
            }

            return Result.Success(doc.Id);
        }, ct);

    public Task<Result> UpdateAsync(long id, InventoryCountUpdateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.For<InventoryCountDoc>().Where(x => x.Id == id).Build();
            var doc = await _query.GetAsync(query, ct);
            if (doc == null)
                return Result.Failure(InventoryCountErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(InventoryCountErrors.CannotUpdateInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));

            var validationResult = await ValidateDraftAsync(_userContext.OrganizationId.Value, dto, id, ct);
            if (!validationResult.IsSuccess)
                return Result.Failure(validationResult.Error);

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto != null)
                _auditLogService.SetOldValues(oldDocDto);

            var lines = await BuildLinesAsync(dto, _userContext.OrganizationId.Value, ct);

            await _tableCommand.DeleteAsync(x => x.Owner.OwnerId == id, ct);
            await _lineCommand.DeleteAsync(x => x.OwnerId == id, ct);

            foreach (var line in lines)
                line.OwnerId = id;

            await _lineCommand.CreateAsync(lines, ct);

            doc.DocDate = DateTime.SpecifyKind(dto.DocDate, DateTimeKind.Unspecified);
            doc.WarehouseId = dto.WarehouseId;
            doc.Comment = dto.Comment;
            doc.StateId = dto.StateId;

            if (dto.IsCountCompleted)
            {
                doc.StatusId = DocumentStatusIdConst.PENDING;
                doc.CountCompletedAt ??= DateTime.Now;
                doc.CountCompletedByUserId ??= _userContext.Id;
            }

            await _command.UpdateAsync(doc, ct);

            var newDocDto = await GetByIdInternalAsync(id, ct);
            if (newDocDto != null)
            {
                _auditLogService.SetNewValues(newDocDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.InventoryCountDoc, id.ToString(), AuditLogOperationTypeConst.Update, dto.Comment);
            }

            return Result.Success();
        }, ct);

    public Task<Result> DeleteAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(DeleteAsync), async () =>
        {
            var query = _queryBuilder.For<InventoryCountDoc>().Where(x => x.Id == id).Build();
            var doc = await _query.GetAsync(query, ct);
            if (doc == null)
                return Result.Failure(InventoryCountErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(InventoryCountErrors.CannotDeleteInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto != null)
                _auditLogService.SetOldValues(oldDocDto);

            await _tableCommand.DeleteAsync(x => x.Owner.OwnerId == id, ct);
            await _lineCommand.DeleteAsync(x => x.OwnerId == id, ct);

            doc.StateId = StateIdConst.PASSIVE;
            await _command.UpdateAsync(doc, ct);

            var newDocDto = await GetByIdInternalAsync(id, ct);
            if (newDocDto != null)
            {
                _auditLogService.SetNewValues(newDocDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.InventoryCountDoc, id.ToString(), AuditLogOperationTypeConst.Delete);
            }

            return Result.Success();
        }, ct);

    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) => _lifecycleService.ConfirmAsync(id, ct);
    public Task<Result> CancelAsync(long id, CancellationToken ct = default) => _lifecycleService.CancelAsync(id, ct);

    public Task<Result<List<InventoryCountPostingBatchDto>>> GetPostingBatchesAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetPostingBatchesAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<List<InventoryCountPostingBatchDto>>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            if (!await _query.AnyAsync(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value, ct))
                return Result.Failure<List<InventoryCountPostingBatchDto>>(InventoryCountErrors.NotFound(id, _userContext.LanguageId));

            var query = _queryBuilder.For<PostingBatch>()
                .Where(x => x.DocumentTypeId == DocumentTypeIdConst.INVENTORYCOUNT && x.DocumentId == id)
                .As(x => new InventoryCountPostingBatchDto
                {
                    Id = x.Id,
                    OrganizationId = x.OrganizationId,
                    DocumentTypeId = x.DocumentTypeId,
                    DocumentId = x.DocumentId,
                    Status = x.Status,
                    PostedByUserId = x.PostedByUserId,
                    PostedAt = x.PostedAt,
                    ReversedByUserId = x.ReversedByUserId,
                    ReversedAt = x.ReversedAt,
                    Comment = x.Comment
                })
                .Build();

            return Result.Success(await _postingBatchQuery.GetAllAsync(query, ct));
        });

    public Task<Result<List<InventoryRegisterBalanceListDto>>> GetInventoryMovementsAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetInventoryMovementsAsync), async () =>
        {
            var doc = await GetForDetailsAsync(id, ct);
            if (doc == null)
                return Result.Failure<List<InventoryRegisterBalanceListDto>>(InventoryCountErrors.NotFound(id, _userContext.LanguageId));

            var adjustmentIds = new List<long>();
            if (doc.PositiveAdjustmentDocId.HasValue)
                adjustmentIds.Add(doc.PositiveAdjustmentDocId.Value);
            if (doc.NegativeAdjustmentDocId.HasValue)
                adjustmentIds.Add(doc.NegativeAdjustmentDocId.Value);

            if (adjustmentIds.Count == 0)
                return Result.Success(new List<InventoryRegisterBalanceListDto>());

            var query = _queryBuilder.For<RegisterBalance>()
                .Where(x => x.DocumentTypeId == DocumentTypeIdConst.INVENTORYADJUSTMENT && adjustmentIds.Contains(x.DocumentId))
                .As<InventoryRegisterBalanceListDto>()
                .Build();

            return Result.Success(await _inventoryRegisterQuery.GetAllAsync(query, ct));
        });

    public Task<Result<List<InventoryCountDifferenceDto>>> GetDifferencesAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetDifferencesAsync), async () =>
        {
            var doc = await GetForDetailsAsync(id, ct);
            if (doc == null)
                return Result.Failure<List<InventoryCountDifferenceDto>>(InventoryCountErrors.NotFound(id, _userContext.LanguageId));

            return Result.Success(doc.StatusId == DocumentStatusIdConst.POSTED || doc.StatusId == DocumentStatusIdConst.CANCELLED
                ? await BuildPostedDifferencesAsync(doc, ct)
                : await BuildDraftDifferencesAsync(doc, ct));
        });

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

    private async Task<InventoryCountDoc?> GetForDetailsAsync(long id, CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return null;

        var query = _queryBuilder.For<InventoryCountDoc>()
            .Where(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value)
            .Build();
        query.AddIncludes(b => b.Include(x => x.Warehouse));
        query.AddIncludes(b => b.Include(x => x.InventoryCountLines).ThenInclude(x => x.Product));
        query.AddIncludes(b => b.Include(x => x.InventoryCountLines).ThenInclude(x => x.Unit));
        query.AddIncludes(b => b.Include(x => x.InventoryCountLines).ThenInclude(x => x.InventoryCountDocTables).ThenInclude(x => x.ProductTable));
        return await _query.GetAsync(query, ct);
    }

    private async Task<Result> ValidateDraftAsync(int organizationId, InventoryCountBaseDto dto, long? currentId, CancellationToken ct)
    {
        var organizationExists = await _organizationQuery.AnyAsync(x => x.Id == organizationId, ct);
        if (!organizationExists)
            return Result.Failure(InventoryCountErrors.OrganizationNotFound(organizationId, _userContext.LanguageId));

        var warehouseQuery = _queryBuilder.For<Warehouse>().Where(x => x.Id == dto.WarehouseId).Build();
        var warehouse = await _warehouseQuery.GetAsync(warehouseQuery, ct);
        if (warehouse == null)
            return Result.Failure(InventoryCountErrors.WarehouseNotFound(dto.WarehouseId, _userContext.LanguageId));

        if (warehouse.OrganizationId != organizationId)
            return Result.Failure(InventoryCountErrors.WarehouseOrganizationMismatch(dto.WarehouseId, organizationId, _userContext.LanguageId));

        if (warehouse.StateId != StateIdConst.ACTIVE)
            return Result.Failure(InventoryCountErrors.WarehouseInactive(dto.WarehouseId, _userContext.LanguageId));

        if (dto.Lines.Count == 0)
            return Result.Failure(InventoryCountErrors.LinesRequired(_userContext.LanguageId));

        var productIds = dto.Lines.Select(x => x.ProductId).Distinct().ToList();
        var unitIds = dto.Lines.Select(x => x.UnitId).Distinct().ToList();
        var productTableIds = dto.Lines
            .SelectMany(x => x.Items)
            .Where(x => x.ProductTableId.HasValue)
            .Select(x => x.ProductTableId!.Value)
            .Distinct()
            .ToList();

        var productsQuery = _queryBuilder.For<Product>()
            .Where(x => productIds.Contains(x.Id))
            .Build();
        var productById = (await _productQuery.GetAllAsync(productsQuery, ct)).ToDictionary(x => x.Id);

        var unitsQuery = _queryBuilder.For<Unit>()
            .Where(x => unitIds.Contains(x.Id))
            .Build();
        var unitIdsSet = (await _unitQuery.GetAllAsync(unitsQuery, ct)).Select(x => x.Id).ToHashSet();

        var productTableById = new Dictionary<int, ProductTable>();
        if (productTableIds.Count > 0)
        {
            var productTablesQuery = _queryBuilder.For<ProductTable>()
                .Where(x => productTableIds.Contains(x.Id))
                .Build();
            productTableById = (await _productTableQuery.GetAllAsync(productTablesQuery, ct)).ToDictionary(x => x.Id);
        }

        var activeCountExists = await _query.AnyAsync(x =>
            x.OrganizationId == organizationId &&
            x.WarehouseId == dto.WarehouseId &&
            x.StateId == StateIdConst.ACTIVE &&
            x.StatusId != DocumentStatusIdConst.CANCELLED &&
            x.StatusId != DocumentStatusIdConst.POSTED &&
            (!currentId.HasValue || x.Id != currentId.Value), ct);

        if (activeCountExists)
            return Result.Failure(InventoryCountErrors.SimultaneousCountExists(dto.WarehouseId, _userContext.LanguageId));

        var seenLines = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var seenProductTableIds = new HashSet<int>();
        var seenBarcodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var seenSerials = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var seenMarkings = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in dto.Lines)
        {
            var lineKey = $"{line.ProductId}:{line.UnitId}";
            if (!seenLines.Add(lineKey))
                return Result.Failure(InventoryCountErrors.DuplicateLine(line.ProductId, line.UnitId, _userContext.LanguageId));

            if (!productById.TryGetValue(line.ProductId, out var product) || product.OrganizationId != organizationId)
                return Result.Failure(InventoryCountErrors.ProductNotFound(line.ProductId, _userContext.LanguageId));

            if (product.IsService)
                return Result.Failure(InventoryCountErrors.ProductServiceNotAllowed(line.ProductId, _userContext.LanguageId));

            if (!unitIdsSet.Contains(line.UnitId))
                return Result.Failure(InventoryCountErrors.UnitNotFound(line.UnitId, _userContext.LanguageId));

            if (line.CountedQuantity <= 0)
                return Result.Failure(InventoryCountErrors.InvalidQuantity(line.ProductId, line.CountedQuantity, _userContext.LanguageId));

            if (line.CountedQuantity != decimal.Truncate(line.CountedQuantity))
                return Result.Failure(InventoryCountErrors.InvalidQuantity(line.ProductId, line.CountedQuantity, _userContext.LanguageId));

            if (line.CountedQuantity < line.Items.Count)
                return Result.Failure(InventoryCountErrors.InvalidItemCount(line.ProductId, line.CountedQuantity, line.Items.Count, _userContext.LanguageId));

            foreach (var item in line.Items)
            {
                if (item.ProductTableId.HasValue)
                {
                    if (!seenProductTableIds.Add(item.ProductTableId.Value))
                        return Result.Failure(InventoryCountErrors.DuplicateProductTable(item.ProductTableId.Value, _userContext.LanguageId));

                    if (!productTableById.TryGetValue(item.ProductTableId.Value, out var productTable) || productTable.OrganizationId != organizationId)
                        return Result.Failure(InventoryCountErrors.ProductTableNotFound(item.ProductTableId.Value, _userContext.LanguageId));

                    if (productTable.ProductId != line.ProductId)
                        return Result.Failure(InventoryCountErrors.ProductTableProductMismatch(item.ProductTableId.Value, line.ProductId, _userContext.LanguageId));

                    if (productTable.StateId != StateIdConst.ACTIVE)
                        return Result.Failure(InventoryCountErrors.ProductTableInactive(item.ProductTableId.Value, _userContext.LanguageId));

                    if (productTable.StatusId != ProductTableStatusIdConst.IN_STOCK)
                        return Result.Failure(InventoryCountErrors.ProductTableUnavailable(item.ProductTableId.Value, productTable.StatusId, _userContext.LanguageId));

                    if (productTable.CurrentWarehouseId != dto.WarehouseId)
                        return Result.Failure(InventoryCountErrors.ProductTableWarehouseMismatch(item.ProductTableId.Value, dto.WarehouseId, _userContext.LanguageId));
                }

                if (!string.IsNullOrWhiteSpace(item.Barcode) && !seenBarcodes.Add(item.Barcode.Trim()))
                    return Result.Failure(InventoryCountErrors.DuplicateBarcode(item.Barcode.Trim(), _userContext.LanguageId));

                if (!string.IsNullOrWhiteSpace(item.SerialNumber) && !seenSerials.Add(item.SerialNumber.Trim()))
                    return Result.Failure(InventoryCountErrors.DuplicateSerial(item.SerialNumber.Trim(), _userContext.LanguageId));

                if (!string.IsNullOrWhiteSpace(item.MarkingNumber) && !seenMarkings.Add(item.MarkingNumber.Trim()))
                    return Result.Failure(InventoryCountErrors.DuplicateMarking(item.MarkingNumber.Trim(), _userContext.LanguageId));
            }
        }

        return Result.Success();
    }

    private async Task<List<InventoryCountLine>> BuildLinesAsync(InventoryCountBaseDto dto, int organizationId, CancellationToken ct)
    {
        var lines = new List<InventoryCountLine>();

        foreach (var line in dto.Lines)
        {
            var entity = new InventoryCountLine
            {
                ProductId = line.ProductId,
                UnitId = line.UnitId,
                CountedQuantity = line.CountedQuantity,
                DefaultCostPrice = line.DefaultCostPrice,
                Comment = line.Comment
            };

            foreach (var item in line.Items)
            {
                var productTable = item.ProductTableId.HasValue
                    ? await _productTableQuery.GetAsync(_queryBuilder.For<ProductTable>().Where(x => x.Id == item.ProductTableId.Value).Build(), ct)
                    : null;

                entity.InventoryCountDocTables.Add(new InventoryCountDocTable
                {
                    ProductTableId = item.ProductTableId,
                    Barcode = !string.IsNullOrWhiteSpace(item.Barcode) ? item.Barcode.Trim() : productTable?.Product?.Barcode,
                    SerialNumber = !string.IsNullOrWhiteSpace(item.SerialNumber) ? item.SerialNumber.Trim() : productTable?.SerialNumber,
                    MarkingNumber = !string.IsNullOrWhiteSpace(item.MarkingNumber) ? item.MarkingNumber.Trim() : productTable?.MarkingNumber,
                    CostPrice = item.CostPrice
                });
            }

            lines.Add(entity);
        }

        return lines;
    }

    private async Task<List<InventoryCountDifferenceDto>> BuildDraftDifferencesAsync(InventoryCountDoc doc, CancellationToken ct)
    {
        var expectedProductTables = await GetExpectedProductTablesAsync(doc.OrganizationId, doc.WarehouseId, ct);
        var expectedByProduct = expectedProductTables.GroupBy(x => new { x.ProductId, x.ProductName, x.UnitId, x.UnitName })
            .ToDictionary(
                x => (x.Key.ProductId, x.Key.UnitId),
                x => x.ToList());

        var result = new Dictionary<(int ProductId, short UnitId), InventoryCountDifferenceDto>();

        foreach (var line in doc.InventoryCountLines)
        {
            var key = (line.ProductId, line.UnitId);
            expectedByProduct.TryGetValue(key, out var expectedRows);
            expectedRows ??= new List<InventoryCountExpectedRow>();

            var existingCountedIds = line.InventoryCountDocTables
                .Where(x => x.ProductTableId.HasValue)
                .Select(x => x.ProductTableId!.Value)
                .Distinct()
                .ToHashSet();

            var missingRows = expectedRows.Where(x => !existingCountedIds.Contains(x.Id)).Select(x => x.Id).ToList();
            var foundItems = line.InventoryCountDocTables
                .Where(x => !x.ProductTableId.HasValue)
                .Select(x => new InventoryCountFoundItemDto
                {
                    ProductTableId = x.ProductTableId,
                    Barcode = x.Barcode,
                    SerialNumber = x.SerialNumber,
                    MarkingNumber = x.MarkingNumber,
                    CostPrice = x.CostPrice
                })
                .ToList();

            var anonymousFoundCount = (int)(line.CountedQuantity - line.InventoryCountDocTables.Count);
            for (var i = 0; i < anonymousFoundCount; i++)
            {
                foundItems.Add(new InventoryCountFoundItemDto
                {
                    CostPrice = line.DefaultCostPrice
                });
            }

            result[key] = new InventoryCountDifferenceDto
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

        foreach (var entry in expectedByProduct)
        {
            if (result.ContainsKey(entry.Key))
                continue;

            result[entry.Key] = new InventoryCountDifferenceDto
            {
                ProductId = entry.Key.ProductId,
                ProductName = entry.Value[0].ProductName,
                UnitId = entry.Key.UnitId,
                UnitName = entry.Value[0].UnitName,
                ExpectedQuantity = entry.Value.Count,
                CountedQuantity = 0,
                CorrectQuantity = 0,
                MissingQuantity = entry.Value.Count,
                FoundQuantity = 0,
                MissingProductTableIds = entry.Value.Select(x => x.Id).ToList()
            };
        }

        return result.Values.OrderBy(x => x.ProductName).ToList();
    }

    private async Task<List<InventoryCountDifferenceDto>> BuildPostedDifferencesAsync(InventoryCountDoc doc, CancellationToken ct)
    {
        var result = doc.InventoryCountLines.ToDictionary(
            x => (x.ProductId, x.UnitId),
            x => new InventoryCountDifferenceDto
            {
                ProductId = x.ProductId,
                ProductName = x.Product.Name,
                UnitId = x.UnitId,
                UnitName = x.Unit.Name,
                CountedQuantity = x.CountedQuantity,
                FoundItems = x.InventoryCountDocTables
                    .Where(t => !t.ProductTableId.HasValue)
                    .Select(t => new InventoryCountFoundItemDto
                    {
                        Barcode = t.Barcode,
                        SerialNumber = t.SerialNumber,
                        MarkingNumber = t.MarkingNumber,
                        CostPrice = t.CostPrice
                    }).ToList()
            });

        if (doc.PositiveAdjustmentDocId.HasValue)
        {
            var positive = await GetInventoryAdjustmentAsync(doc.PositiveAdjustmentDocId.Value, ct);
            if (positive != null)
            {
                foreach (var line in positive.InventoryAdjustmentLines)
                {
                    var key = (line.ProductId, line.UnitId);
                    if (!result.TryGetValue(key, out var dto))
                    {
                        dto = new InventoryCountDifferenceDto
                        {
                            ProductId = line.ProductId,
                            ProductName = line.Product.Name,
                            UnitId = line.UnitId,
                            UnitName = line.Unit.Name
                        };
                        result[key] = dto;
                    }

                    dto.FoundQuantity += line.Quantity;
                }
            }
        }

        if (doc.NegativeAdjustmentDocId.HasValue)
        {
            var negative = await GetInventoryAdjustmentAsync(doc.NegativeAdjustmentDocId.Value, ct);
            if (negative != null)
            {
                foreach (var line in negative.InventoryAdjustmentLines)
                {
                    var key = (line.ProductId, line.UnitId);
                    if (!result.TryGetValue(key, out var dto))
                    {
                        dto = new InventoryCountDifferenceDto
                        {
                            ProductId = line.ProductId,
                            ProductName = line.Product.Name,
                            UnitId = line.UnitId,
                            UnitName = line.Unit.Name
                        };
                        result[key] = dto;
                    }

                    dto.MissingQuantity += line.Quantity;
                    dto.MissingProductTableIds.AddRange(line.InventoryAdjustmentDocTables.Where(x => x.ProductTableId.HasValue).Select(x => x.ProductTableId!.Value));
                }
            }
        }

        foreach (var dto in result.Values)
        {
            dto.CorrectQuantity = dto.CountedQuantity - dto.FoundQuantity;
            dto.ExpectedQuantity = dto.CorrectQuantity + dto.MissingQuantity;
        }

        return result.Values.OrderBy(x => x.ProductName).ToList();
    }

    private async Task<List<InventoryCountExpectedRow>> GetExpectedProductTablesAsync(int organizationId, int warehouseId, CancellationToken ct)
    {
        return await _inventoryReadDbContext.ProductTables
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId &&
                        x.CurrentWarehouseId == warehouseId &&
                        x.StateId == StateIdConst.ACTIVE &&
                        x.StatusId == ProductTableStatusIdConst.IN_STOCK)
            .Select(x => new InventoryCountExpectedRow
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductName = x.Product.Name,
                UnitId = x.Product.UnitId,
                UnitName = x.Product.Unit.Name
            })
            .ToListAsyncSafe(ct);
    }

    private async Task<InventoryAdjustmentDoc?> GetInventoryAdjustmentAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<InventoryAdjustmentDoc>().Where(x => x.Id == id).Build();
        query.AddIncludes(b => b.Include(x => x.InventoryAdjustmentLines).ThenInclude(x => x.Product));
        query.AddIncludes(b => b.Include(x => x.InventoryAdjustmentLines).ThenInclude(x => x.Unit));
        query.AddIncludes(b => b.Include(x => x.InventoryAdjustmentLines).ThenInclude(x => x.InventoryAdjustmentDocTables));
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
}
