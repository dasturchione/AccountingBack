using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.AuditLogs;
using Application.Features.DocumentNumbers;
using Application.Features.InventoryMovements;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.InventoryAdjustments;

public class InventoryAdjustmentService : BaseService, IInventoryAdjustmentService
{
    private static readonly HashSet<string> AllowedAdjustmentTypes =
    [
        "POSITIVE_ADJUSTMENT",
        "NEGATIVE_ADJUSTMENT",
        "WRITE_OFF",
        "DAMAGE",
        "LOSS",
        "FOUND_STOCK",
        "CORRECTION"
    ];

    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IAuditLogService _auditLogService;
    private readonly IInventoryAdjustmentLifecycleService _lifecycleService;
    private readonly IDocumentNumberService _documentNumberService;
    private readonly IQueryRepository<InventoryAdjustmentDoc> _query;
    private readonly ICommandRepository<InventoryAdjustmentDoc> _command;
    private readonly ICommandRepository<InventoryAdjustmentLine> _lineCommand;
    private readonly ICommandRepository<InventoryAdjustmentDocTable> _tableCommand;
    private readonly IQueryRepository<PostingBatch> _postingBatchQuery;
    private readonly IQueryRepository<WarehouseProductMovement> _warehouseMovementQuery;
    private readonly IQueryRepository<Organization> _organizationQuery;
    private readonly IQueryRepository<Warehouse> _warehouseQuery;
    private readonly IQueryRepository<Product> _productQuery;
    private readonly IQueryRepository<Unit> _unitQuery;
    private readonly IQueryRepository<ProductTable> _productTableQuery;

    public InventoryAdjustmentService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IAuditLogService auditLogService,
        IInventoryAdjustmentLifecycleService lifecycleService,
        IDocumentNumberService documentNumberService,
        IQueryRepository<InventoryAdjustmentDoc> query,
        ICommandRepository<InventoryAdjustmentDoc> command,
        ICommandRepository<InventoryAdjustmentLine> lineCommand,
        ICommandRepository<InventoryAdjustmentDocTable> tableCommand,
        IQueryRepository<PostingBatch> postingBatchQuery,
        IQueryRepository<WarehouseProductMovement> warehouseMovementQuery,
        IQueryRepository<Organization> organizationQuery,
        IQueryRepository<Warehouse> warehouseQuery,
        IQueryRepository<Product> productQuery,
        IQueryRepository<Unit> unitQuery,
        IQueryRepository<ProductTable> productTableQuery,
        ILogger<InventoryAdjustmentService> logger,
        IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _auditLogService = auditLogService;
        _lifecycleService = lifecycleService;
        _documentNumberService = documentNumberService;
        _query = query;
        _command = command;
        _lineCommand = lineCommand;
        _tableCommand = tableCommand;
        _postingBatchQuery = postingBatchQuery;
        _warehouseMovementQuery = warehouseMovementQuery;
        _organizationQuery = organizationQuery;
        _warehouseQuery = warehouseQuery;
        _productQuery = productQuery;
        _unitQuery = unitQuery;
        _productTableQuery = productTableQuery;
    }

    public Task<Result<PagedResponse<InventoryAdjustmentListDto>>> GetAllAsync(InventoryAdjustmentListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            var query = _queryBuilder.BuildPaged<InventoryAdjustmentDoc, InventoryAdjustmentListDto, InventoryAdjustmentListFilter>(filter);
            var pagedList = await _query.GetPagedAsync(query, ct);
            return Result.Success(PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize));
        });

    public Task<Result<InventoryAdjustmentDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<InventoryAdjustmentDto>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.For<InventoryAdjustmentDoc>()
                .Where(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value)
                .As<InventoryAdjustmentDto>()
                .Build();
            var entity = await _query.GetAsync(query, ct);

            if (entity == null)
                return Result.Failure<InventoryAdjustmentDto>(InventoryAdjustmentErrors.NotFound(id, _userContext.LanguageId));

            return Result.Success(entity);
        });

    public Task<Result<long>> CreateAsync(InventoryAdjustmentCreateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<long>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var organizationId = _userContext.OrganizationId.Value;
            var validationResult = await ValidateDraftAsync(organizationId, dto, ct);
            if (!validationResult.IsSuccess)
                return Result.Failure<long>(validationResult.Error);

            var documentNumberResult = await _documentNumberService.GetNextAsync(
                organizationId,
                DocumentTypeIdConst.INVENTORYADJUSTMENT,
                dto.DocDate,
                ct);
            if (!documentNumberResult.IsSuccess)
                return Result.Failure<long>(documentNumberResult.Error);

            var lines = BuildLines(dto);

            var doc = new InventoryAdjustmentDoc
            {
                OrganizationId = organizationId,
                DocNumber = documentNumberResult.Value.DocumentNumber,
                DocDate = DateTime.SpecifyKind(dto.DocDate, DateTimeKind.Unspecified),
                WarehouseId = dto.WarehouseId,
                AdjustmentType = dto.AdjustmentType.Trim().ToUpperInvariant(),
                StatusId = DocumentStatusIdConst.DRAFT,
                Comment = dto.Comment,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.Now,
                InventoryAdjustmentLines = lines
            };

            await _command.CreateAsync(doc, ct);

            var docDto = await GetByIdInternalAsync(doc.Id, ct);
            if (docDto != null)
            {
                _auditLogService.SetNewValues(docDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.InventoryAdjustmentDoc, doc.Id.ToString(), AuditLogOperationTypeConst.Create);
            }

            return Result.Success(doc.Id);
        }, ct);

    public Task<Result> UpdateAsync(long id, InventoryAdjustmentUpdateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.For<InventoryAdjustmentDoc>()
                .Where(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value)
                .Build();
            var doc = await _query.GetAsync(query, ct);
            if (doc == null)
                return Result.Failure(InventoryAdjustmentErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(InventoryAdjustmentErrors.CannotUpdateInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));

            var validationResult = await ValidateDraftAsync(_userContext.OrganizationId.Value, dto, ct);
            if (!validationResult.IsSuccess)
                return Result.Failure(validationResult.Error);

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto != null)
                _auditLogService.SetOldValues(oldDocDto);

            var lines = BuildLines(dto);

            await _tableCommand.DeleteAsync(x => x.Owner.OwnerId == id, ct);
            await _lineCommand.DeleteAsync(x => x.OwnerId == id, ct);

            foreach (var line in lines)
                line.OwnerId = id;

            await _lineCommand.CreateAsync(lines, ct);

            doc.DocDate = DateTime.SpecifyKind(dto.DocDate, DateTimeKind.Unspecified);
            doc.WarehouseId = dto.WarehouseId;
            doc.AdjustmentType = dto.AdjustmentType.Trim().ToUpperInvariant();
            doc.Comment = dto.Comment;
            doc.StateId = dto.StateId;

            await _command.UpdateAsync(doc, ct);

            var newDocDto = await GetByIdInternalAsync(id, ct);
            if (newDocDto != null)
            {
                _auditLogService.SetNewValues(newDocDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.InventoryAdjustmentDoc, id.ToString(), AuditLogOperationTypeConst.Update, dto.Comment);
            }

            return Result.Success();
        }, ct);

    public Task<Result> DeleteAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(DeleteAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.For<InventoryAdjustmentDoc>()
                .Where(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value)
                .Build();
            var doc = await _query.GetAsync(query, ct);
            if (doc == null)
                return Result.Failure(InventoryAdjustmentErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(InventoryAdjustmentErrors.CannotDeleteInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));

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
                await _auditLogService.CreateAsync(AuditLogTableConst.InventoryAdjustmentDoc, id.ToString(), AuditLogOperationTypeConst.Delete);
            }

            return Result.Success();
        }, ct);

    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) =>
        _lifecycleService.ConfirmAsync(id, ct);

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        _lifecycleService.CancelAsync(id, ct);

    public Task<Result<List<InventoryAdjustmentPostingBatchDto>>> GetPostingBatchesAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetPostingBatchesAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<List<InventoryAdjustmentPostingBatchDto>>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            if (!await _query.AnyAsync(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value, ct))
                return Result.Failure<List<InventoryAdjustmentPostingBatchDto>>(InventoryAdjustmentErrors.NotFound(id, _userContext.LanguageId));

            var query = _queryBuilder.For<PostingBatch>()
                .Where(x => x.DocumentTypeId == DocumentTypeIdConst.INVENTORYADJUSTMENT && x.DocumentId == id)
                .As(x => new InventoryAdjustmentPostingBatchDto
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

    public Task<Result<List<InventoryMovementListDto>>> GetInventoryMovementsAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetInventoryMovementsAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<List<InventoryMovementListDto>>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            if (!await _query.AnyAsync(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value, ct))
                return Result.Failure<List<InventoryMovementListDto>>(InventoryAdjustmentErrors.NotFound(id, _userContext.LanguageId));

            var query = _queryBuilder.For<WarehouseProductMovement>()
                .Where(x => x.DocumentTypeId == DocumentTypeIdConst.INVENTORYADJUSTMENT && x.DocumentId == id)
                .As<InventoryMovementListDto>()
                .Build();

            return Result.Success(await _warehouseMovementQuery.GetAllAsync(query, ct));
        });

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

    private async Task<Result> ValidateDraftAsync(int organizationId, InventoryAdjustmentBaseDto dto, CancellationToken ct)
    {
        var organizationExists = await _organizationQuery.AnyAsync(x => x.Id == organizationId, ct);
        if (!organizationExists)
            return Result.Failure(InventoryAdjustmentErrors.OrganizationNotFound(organizationId, _userContext.LanguageId));

        var normalizedType = dto.AdjustmentType.Trim().ToUpperInvariant();
        if (!AllowedAdjustmentTypes.Contains(normalizedType))
            return Result.Failure(InventoryAdjustmentErrors.InvalidAdjustmentType(dto.AdjustmentType, _userContext.LanguageId));

        var warehouseQuery = _queryBuilder.For<Warehouse>().Where(x => x.Id == dto.WarehouseId).Build();
        var warehouse = await _warehouseQuery.GetAsync(warehouseQuery, ct);
        if (warehouse == null)
            return Result.Failure(InventoryAdjustmentErrors.WarehouseNotFound(dto.WarehouseId, _userContext.LanguageId));

        if (warehouse.OrganizationId != organizationId)
            return Result.Failure(InventoryAdjustmentErrors.WarehouseOrganizationMismatch(dto.WarehouseId, organizationId, _userContext.LanguageId));

        if (warehouse.StateId != StateIdConst.ACTIVE)
            return Result.Failure(InventoryAdjustmentErrors.WarehouseInactive(dto.WarehouseId, _userContext.LanguageId));

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
            productTablesQuery.AddIncludes(x => x.Include(p => p.Product));
            productTablesQuery.AddIncludes(x => x.Include(p => p.WarehouseProductTable));
            productTableById = (await _productTableQuery.GetAllAsync(productTablesQuery, ct)).ToDictionary(x => x.Id);
        }

        var seenLines = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var seenProductTableIds = new HashSet<int>();

        foreach (var line in dto.Lines)
        {
            var lineKey = $"{line.ProductId}:{line.UnitId}";
            if (!seenLines.Add(lineKey))
                return Result.Failure(InventoryAdjustmentErrors.DuplicateLine(line.ProductId, line.UnitId, _userContext.LanguageId));

            if (!productById.TryGetValue(line.ProductId, out var product) || product.OrganizationId != organizationId)
                return Result.Failure(InventoryAdjustmentErrors.ProductNotFound(line.ProductId, _userContext.LanguageId));

            if (product.IsService)
                return Result.Failure(InventoryAdjustmentErrors.ProductServiceNotAllowed(line.ProductId, _userContext.LanguageId));

            if (!unitIdsSet.Contains(line.UnitId))
                return Result.Failure(InventoryAdjustmentErrors.UnitNotFound(line.UnitId, _userContext.LanguageId));

            if (line.Quantity <= 0)
                return Result.Failure(InventoryAdjustmentErrors.InvalidQuantity(line.ProductId, line.Quantity, _userContext.LanguageId));

            if (!product.IsPieceTracked)
            {
                if (line.Items.Count > 0)
                    return Result.Failure(InventoryAdjustmentErrors.QuantityItemsMismatch(line.ProductId, 0m, line.Items.Count, _userContext.LanguageId));

                continue;
            }

            if (line.Items.Count == 0)
                return Result.Failure(InventoryAdjustmentErrors.ItemsRequired(line.ProductId, _userContext.LanguageId));

            if (line.Quantity != decimal.Truncate(line.Quantity) || line.Quantity != line.Items.Count)
                return Result.Failure(InventoryAdjustmentErrors.QuantityItemsMismatch(line.ProductId, line.Quantity, line.Items.Count, _userContext.LanguageId));

            foreach (var item in line.Items)
            {
                if (item.ProductTableId.HasValue && !seenProductTableIds.Add(item.ProductTableId.Value))
                    return Result.Failure(InventoryAdjustmentErrors.DuplicateProductTable(item.ProductTableId.Value, _userContext.LanguageId));

                if (!item.ProductTableId.HasValue)
                {
                    if (!IsPositiveFlow(normalizedType))
                        return Result.Failure(InventoryAdjustmentErrors.ProductTableNotFound(0, _userContext.LanguageId));

                    continue;
                }

                if (!productTableById.TryGetValue(item.ProductTableId.Value, out var productTable) || productTable.Product.OrganizationId != organizationId)
                    return Result.Failure(InventoryAdjustmentErrors.ProductTableNotFound(item.ProductTableId.Value, _userContext.LanguageId));

                if (productTable.ProductId != line.ProductId)
                    return Result.Failure(InventoryAdjustmentErrors.ProductTableProductMismatch(item.ProductTableId.Value, line.ProductId, _userContext.LanguageId));

                if (productTable.WarehouseProductTable?.WarehouseId != dto.WarehouseId)
                    return Result.Failure(InventoryAdjustmentErrors.ProductTableWarehouseMismatch(item.ProductTableId.Value, dto.WarehouseId, _userContext.LanguageId));
            }
        }

        return Result.Success();
    }

    private static List<InventoryAdjustmentLine> BuildLines(InventoryAdjustmentBaseDto dto) =>
        dto.Lines.Select(line => new InventoryAdjustmentLine
        {
            ProductId = line.ProductId,
            UnitId = line.UnitId,
            Quantity = line.Quantity,
            Comment = line.Comment,
            InventoryAdjustmentDocTables = line.Items.Select(item => new InventoryAdjustmentDocTable
            {
                ProductTableId = item.ProductTableId,
                CostPrice = item.CostPrice
            }).ToList()
        }).ToList();

    private static bool IsPositiveFlow(string adjustmentType) =>
        adjustmentType is "POSITIVE_ADJUSTMENT" or "FOUND_STOCK" or "CORRECTION";
}
