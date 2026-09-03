using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.AuditLogs;
using Application.Features.DocumentNumbers;
using Application.Features.InventoryMovements;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.WarehouseTransfers;

public class WarehouseTransferService : BaseService, IWarehouseTransferService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IAuditLogService _auditLogService;
    private readonly IWarehouseTransferLifecycleService _lifecycleService;
    private readonly IDocumentNumberService _documentNumberService;
    private readonly IQueryRepository<WarehouseTransferDoc> _query;
    private readonly ICommandRepository<WarehouseTransferDoc> _command;
    private readonly ICommandRepository<WarehouseTransferLine> _lineCommand;
    private readonly ICommandRepository<WarehouseTransferDocTable> _tableCommand;
    private readonly IQueryRepository<PostingBatch> _postingBatchQuery;
    private readonly IQueryRepository<WarehouseProductMovement> _warehouseMovementQuery;
    private readonly IQueryRepository<Organization> _organizationQuery;
    private readonly IQueryRepository<Warehouse> _warehouseQuery;
    private readonly IQueryRepository<Product> _productQuery;
    private readonly IQueryRepository<Unit> _unitQuery;
    private readonly IQueryRepository<ProductTable> _productTableQuery;

    public WarehouseTransferService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IAuditLogService auditLogService,
        IWarehouseTransferLifecycleService lifecycleService,
        IDocumentNumberService documentNumberService,
        IQueryRepository<WarehouseTransferDoc> query,
        ICommandRepository<WarehouseTransferDoc> command,
        ICommandRepository<WarehouseTransferLine> lineCommand,
        ICommandRepository<WarehouseTransferDocTable> tableCommand,
        IQueryRepository<PostingBatch> postingBatchQuery,
        IQueryRepository<WarehouseProductMovement> warehouseMovementQuery,
        IQueryRepository<Organization> organizationQuery,
        IQueryRepository<Warehouse> warehouseQuery,
        IQueryRepository<Product> productQuery,
        IQueryRepository<Unit> unitQuery,
        IQueryRepository<ProductTable> productTableQuery,
        ILogger<WarehouseTransferService> logger,
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

    public Task<Result<PagedResponse<WarehouseTransferListDto>>> GetAllAsync(WarehouseTransferListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            var query = _queryBuilder.BuildPaged<WarehouseTransferDoc, WarehouseTransferListDto, WarehouseTransferListFilter>(filter);
            var pagedList = await _query.GetPagedAsync(query, ct);
            return Result.Success(PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize));
        });

    public Task<Result<WarehouseTransferDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<WarehouseTransferDto>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.For<WarehouseTransferDoc>()
                .Where(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value)
                .As<WarehouseTransferDto>()
                .Build();
            var entity = await _query.GetAsync(query, ct);

            if (entity == null)
                return Result.Failure<WarehouseTransferDto>(WarehouseTransferErrors.NotFound(id, _userContext.LanguageId));

            return Result.Success(entity);
        });

    public Task<Result<long>> CreateAsync(WarehouseTransferCreateDto dto, CancellationToken ct = default) =>
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
                DocumentTypeIdConst.WAREHOUSETRANSFER,
                dto.DocDate,
                ct);
            if (!documentNumberResult.IsSuccess)
                return Result.Failure<long>(documentNumberResult.Error);

            var lines = await BuildLinesAsync(dto, ct);

            var doc = new WarehouseTransferDoc
            {
                OrganizationId = organizationId,
                DocNumber = documentNumberResult.Value.DocumentNumber,
                DocDate = DateTime.SpecifyKind(dto.DocDate, DateTimeKind.Unspecified),
                SourceWarehouseId = dto.SourceWarehouseId,
                DestinationWarehouseId = dto.DestinationWarehouseId,
                StatusId = DocumentStatusIdConst.DRAFT,
                Comment = dto.Comment,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.Now,
                WarehouseTransferLines = lines
            };

            await _command.CreateAsync(doc, ct);

            var docDto = await GetByIdInternalAsync(doc.Id, ct);
            if (docDto != null)
            {
                _auditLogService.SetNewValues(docDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.WarehouseTransferDoc, doc.Id.ToString(), AuditLogOperationTypeConst.Create);
            }

            return Result.Success(doc.Id);
        }, ct);

    public Task<Result> UpdateAsync(long id, WarehouseTransferUpdateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.For<WarehouseTransferDoc>()
                .Where(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value)
                .Build();
            var doc = await _query.GetAsync(query, ct);
            if (doc == null)
                return Result.Failure(WarehouseTransferErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(WarehouseTransferErrors.CannotUpdateInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));

            var validationResult = await ValidateDraftAsync(_userContext.OrganizationId.Value, dto, ct);
            if (!validationResult.IsSuccess)
                return Result.Failure(validationResult.Error);

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto != null)
                _auditLogService.SetOldValues(oldDocDto);

            var lines = await BuildLinesAsync(dto, ct);

            await _tableCommand.DeleteAsync(x => x.Owner.OwnerId == id, ct);
            await _lineCommand.DeleteAsync(x => x.OwnerId == id, ct);

            foreach (var line in lines)
                line.OwnerId = id;

            await _lineCommand.CreateAsync(lines, ct);

            doc.DocDate = DateTime.SpecifyKind(dto.DocDate, DateTimeKind.Unspecified);
            doc.SourceWarehouseId = dto.SourceWarehouseId;
            doc.DestinationWarehouseId = dto.DestinationWarehouseId;
            doc.Comment = dto.Comment;
            doc.StateId = dto.StateId;

            await _command.UpdateAsync(doc, ct);

            var newDocDto = await GetByIdInternalAsync(id, ct);
            if (newDocDto != null)
            {
                _auditLogService.SetNewValues(newDocDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.WarehouseTransferDoc, id.ToString(), AuditLogOperationTypeConst.Update, dto.Comment);
            }

            return Result.Success();
        }, ct);

    public Task<Result> DeleteAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(DeleteAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.For<WarehouseTransferDoc>()
                .Where(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value)
                .Build();
            var doc = await _query.GetAsync(query, ct);
            if (doc == null)
                return Result.Failure(WarehouseTransferErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(WarehouseTransferErrors.CannotDeleteInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));

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
                await _auditLogService.CreateAsync(AuditLogTableConst.WarehouseTransferDoc, id.ToString(), AuditLogOperationTypeConst.Delete);
            }

            return Result.Success();
        }, ct);

    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) =>
        _lifecycleService.ConfirmAsync(id, ct);

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        _lifecycleService.CancelAsync(id, ct);

    public Task<Result<List<WarehouseTransferPostingBatchDto>>> GetPostingBatchesAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetPostingBatchesAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<List<WarehouseTransferPostingBatchDto>>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var docExists = await _query.AnyAsync(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value, ct);
            if (!docExists)
                return Result.Failure<List<WarehouseTransferPostingBatchDto>>(WarehouseTransferErrors.NotFound(id, _userContext.LanguageId));

            var query = _queryBuilder.For<PostingBatch>()
                .Where(x => x.DocumentTypeId == DocumentTypeIdConst.WAREHOUSETRANSFER && x.DocumentId == id)
                .As(x => new WarehouseTransferPostingBatchDto
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

            var docExists = await _query.AnyAsync(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value, ct);
            if (!docExists)
                return Result.Failure<List<InventoryMovementListDto>>(WarehouseTransferErrors.NotFound(id, _userContext.LanguageId));

            var query = _queryBuilder.For<WarehouseProductMovement>()
                .Where(x => x.OrganizationId == _userContext.OrganizationId.Value &&
                            x.DocumentTypeId == DocumentTypeIdConst.WAREHOUSETRANSFER &&
                            x.DocumentId == id)
                .As<InventoryMovementListDto>()
                .Build();

            return Result.Success(await _warehouseMovementQuery.GetAllAsync(query, ct));
        });

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

    private async Task<Result> ValidateDraftAsync(int organizationId, WarehouseTransferBaseDto dto, CancellationToken ct)
    {
        var organizationExists = await _organizationQuery.AnyAsync(x => x.Id == organizationId, ct);
        if (!organizationExists)
            return Result.Failure(WarehouseTransferErrors.OrganizationNotFound(organizationId, _userContext.LanguageId));

        if (dto.SourceWarehouseId == dto.DestinationWarehouseId)
            return Result.Failure(WarehouseTransferErrors.WarehousesMustDiffer(_userContext.LanguageId));

        var sourceWarehouseQuery = _queryBuilder.For<Warehouse>().Where(x => x.Id == dto.SourceWarehouseId).Build();
        var sourceWarehouse = await _warehouseQuery.GetAsync(sourceWarehouseQuery, ct);
        if (sourceWarehouse == null)
            return Result.Failure(WarehouseTransferErrors.SourceWarehouseNotFound(dto.SourceWarehouseId, _userContext.LanguageId));

        if (sourceWarehouse.OrganizationId != organizationId)
            return Result.Failure(WarehouseTransferErrors.WarehouseOrganizationMismatch(dto.SourceWarehouseId, organizationId, _userContext.LanguageId));

        if (sourceWarehouse.StateId != StateIdConst.ACTIVE)
            return Result.Failure(WarehouseTransferErrors.WarehouseInactive(dto.SourceWarehouseId, _userContext.LanguageId));

        var destinationWarehouseQuery = _queryBuilder.For<Warehouse>().Where(x => x.Id == dto.DestinationWarehouseId).Build();
        var destinationWarehouse = await _warehouseQuery.GetAsync(destinationWarehouseQuery, ct);
        if (destinationWarehouse == null)
            return Result.Failure(WarehouseTransferErrors.DestinationWarehouseNotFound(dto.DestinationWarehouseId, _userContext.LanguageId));

        if (destinationWarehouse.OrganizationId != organizationId)
            return Result.Failure(WarehouseTransferErrors.WarehouseOrganizationMismatch(dto.DestinationWarehouseId, organizationId, _userContext.LanguageId));

        if (destinationWarehouse.StateId != StateIdConst.ACTIVE)
            return Result.Failure(WarehouseTransferErrors.WarehouseInactive(dto.DestinationWarehouseId, _userContext.LanguageId));

        var productIds = dto.Lines.Select(x => x.ProductId).Distinct().ToList();
        var unitIds = dto.Lines.Select(x => x.UnitId).Distinct().ToList();
        var productTableIds = dto.Lines
            .SelectMany(x => x.Items)
            .Select(x => x.ProductTableId)
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

        var productTablesQuery = _queryBuilder.For<ProductTable>()
            .Where(x => productTableIds.Contains(x.Id))
            .Build();
        var productTableById = (await _productTableQuery.GetAllAsync(productTablesQuery, ct)).ToDictionary(x => x.Id);

        var seenProductTableIds = new HashSet<int>();

        foreach (var line in dto.Lines)
        {
            if (!productById.TryGetValue(line.ProductId, out var product) || product.OrganizationId != organizationId)
                return Result.Failure(WarehouseTransferErrors.ProductNotFound(line.ProductId, _userContext.LanguageId));

            if (product.IsService)
                return Result.Failure(WarehouseTransferErrors.ProductServiceNotAllowed(line.ProductId, _userContext.LanguageId));

            if (!unitIdsSet.Contains(line.UnitId))
                return Result.Failure(WarehouseTransferErrors.UnitNotFound(line.UnitId, _userContext.LanguageId));

            if (line.Quantity <= 0)
                return Result.Failure(WarehouseTransferErrors.InvalidQuantity(line.ProductId, line.Quantity, _userContext.LanguageId));

            if (!product.IsPieceTracked)
            {
                if (line.Items.Count > 0)
                    return Result.Failure(WarehouseTransferErrors.QuantityItemsMismatch(line.ProductId, 0m, line.Items.Count, _userContext.LanguageId));

                continue;
            }

            if (line.Items.Count == 0)
                return Result.Failure(WarehouseTransferErrors.ItemsRequired(line.ProductId, _userContext.LanguageId));

            if (line.Quantity != decimal.Truncate(line.Quantity) || line.Quantity != line.Items.Count)
                return Result.Failure(WarehouseTransferErrors.QuantityItemsMismatch(line.ProductId, line.Quantity, line.Items.Count, _userContext.LanguageId));

            foreach (var item in line.Items)
            {
                if (!seenProductTableIds.Add(item.ProductTableId))
                    return Result.Failure(WarehouseTransferErrors.DuplicateProductTable(item.ProductTableId, _userContext.LanguageId));

                if (!productTableById.TryGetValue(item.ProductTableId, out var productTable) || productTable.Product.OrganizationId != organizationId)
                    return Result.Failure(WarehouseTransferErrors.ProductTableNotFound(item.ProductTableId, _userContext.LanguageId));

                if (productTable.ProductId != line.ProductId)
                    return Result.Failure(WarehouseTransferErrors.ProductTableProductMismatch(item.ProductTableId, line.ProductId, _userContext.LanguageId));

                if (productTable.WarehouseProductTable == null || productTable.WarehouseProductTable.WarehouseId != dto.SourceWarehouseId)
                    return Result.Failure(WarehouseTransferErrors.ProductTableWarehouseMismatch(item.ProductTableId, dto.SourceWarehouseId, _userContext.LanguageId));
            }
        }

        return Result.Success();
    }

    private Task<List<WarehouseTransferLine>> BuildLinesAsync(WarehouseTransferBaseDto dto, CancellationToken ct)
    {
        var lines = dto.Lines.Select(line => new WarehouseTransferLine
        {
            ProductId = line.ProductId,
            UnitId = line.UnitId,
            Quantity = line.Quantity,
            Comment = line.Comment,
            WarehouseTransferDocTables = line.Items.Select(item => new WarehouseTransferDocTable
            {
                ProductTableId = item.ProductTableId,
                SourceWarehouseId = dto.SourceWarehouseId,
                DestinationWarehouseId = dto.DestinationWarehouseId,
                CostPrice = item.CostPrice
            }).ToList()
        }).ToList();

        return Task.FromResult(lines);
    }
}
