using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.AuditLogs;
using Application.Features.InventoryCounts;
using Application.Features.InventoryMovements;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Filters;
using SharedKernel.Query;
using SharedKernel.Query.Specifications;
using SharedKernel.Results;
using System.Linq.Expressions;

namespace Application.Features.Inv.OpeningInventories;

public partial class OpeningInventoryService : BaseService, IOpeningInventoryService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<OpeningInventory> _query;
    private readonly ICommandRepository<OpeningInventory> _command;
    private readonly ICommandRepository<OpeningInventoryProduct> _lineCommand;
    private readonly ICommandRepository<OpeningInventoryTable> _itemCommand;
    private readonly IQueryRepository<OpeningInventoryTable> _itemQuery;
    private readonly ICommandRepository<ProductTable> _productTableCommand;
    private readonly IQueryRepository<Product> _productQuery;
    private readonly IQueryRepository<Unit> _unitQuery;
    private readonly IQueryRepository<ChartAccount> _chartAccountQuery;
    private readonly IQueryRepository<Warehouse> _warehouseQuery;
    private readonly IQueryRepository<CounterpartyCard> _counterpartyQuery;
    private readonly IQueryRepository<Contract> _contractQuery;
    private readonly IQueryRepository<Organization> _organizationQuery;
    private readonly IQueryRepository<OrganizationConfig> _organizationConfigQuery;
    private readonly IQueryRepository<Currency> _currencyQuery;
    private readonly IQueryRepository<OpeningBalance> _openingBalanceQuery;
    private readonly IQueryRepository<OpeningBalanceAccount> _openingBalanceAccountQuery;
    private readonly ICommandRepository<OpeningBalanceAccount> _openingBalanceAccountCommand;
    private readonly IQueryRepository<OpeningBalanceAccountDetail> _openingBalanceDetailQuery;
    private readonly ICommandRepository<OpeningBalanceAccountDetail> _openingBalanceDetailCommand;
    private readonly ICommandRepository<OpeningBalanceAccountDetailSubkonto> _openingBalanceSubkontoCommand;
    private readonly IQueryRepository<ChartAccountSubkonto> _chartAccountSubkontoQuery;
    private readonly IQueryRepository<WarehouseProductMovement> _movementQuery;
    private readonly ICommandRepository<WarehouseProductMovement> _movementCommand;
    private readonly IQueryRepository<WarehouseProductBatch> _batchQuery;
    private readonly ICommandRepository<WarehouseProductBatch> _batchCommand;
    private readonly IQueryRepository<WarehouseProductBatchAllocation> _allocationQuery;
    private readonly ICommandRepository<WarehouseProductBatchAllocation> _allocationCommand;
    private readonly ICommandRepository<WarehouseProductBatchTable> _batchTableCommand;
    private readonly IQueryRepository<SaleDocProductBatch> _saleBatchQuery;
    private readonly IQueryRepository<SaleShipmentProductBatch> _shipmentBatchQuery;
    private readonly IQueryRepository<SaleDocTable> _saleItemQuery;
    private readonly IQueryRepository<SaleShipmentTable> _shipmentItemQuery;
    private readonly IQueryRepository<WarehouseTransferDocTable> _transferItemQuery;
    private readonly IQueryRepository<InventoryAdjustmentDocTable> _adjustmentItemQuery;
    private readonly IQueryRepository<InventoryCountDocTable> _countItemQuery;
    private readonly IQueryRepository<FaAsset> _assetQuery;
    private readonly IInventoryDispatcher _inventoryDispatcher;
    private readonly IActiveInventoryCountGuardService _inventoryCountGuard;
    private readonly IDocumentPostingLock _documentLock;
    private readonly IDocNumberGenerator _docNumberGenerator;
    private readonly IAuditLogService _auditLogService;

    public OpeningInventoryService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IQueryRepository<OpeningInventory> query,
        ICommandRepository<OpeningInventory> command,
        ICommandRepository<OpeningInventoryProduct> lineCommand,
        ICommandRepository<OpeningInventoryTable> itemCommand,
        IQueryRepository<OpeningInventoryTable> itemQuery,
        ICommandRepository<ProductTable> productTableCommand,
        IQueryRepository<Product> productQuery,
        IQueryRepository<Unit> unitQuery,
        IQueryRepository<ChartAccount> chartAccountQuery,
        IQueryRepository<Warehouse> warehouseQuery,
        IQueryRepository<CounterpartyCard> counterpartyQuery,
        IQueryRepository<Contract> contractQuery,
        IQueryRepository<Organization> organizationQuery,
        IQueryRepository<OrganizationConfig> organizationConfigQuery,
        IQueryRepository<Currency> currencyQuery,
        IQueryRepository<OpeningBalance> openingBalanceQuery,
        IQueryRepository<OpeningBalanceAccount> openingBalanceAccountQuery,
        ICommandRepository<OpeningBalanceAccount> openingBalanceAccountCommand,
        IQueryRepository<OpeningBalanceAccountDetail> openingBalanceDetailQuery,
        ICommandRepository<OpeningBalanceAccountDetail> openingBalanceDetailCommand,
        ICommandRepository<OpeningBalanceAccountDetailSubkonto> openingBalanceSubkontoCommand,
        IQueryRepository<ChartAccountSubkonto> chartAccountSubkontoQuery,
        IQueryRepository<WarehouseProductMovement> movementQuery,
        ICommandRepository<WarehouseProductMovement> movementCommand,
        IQueryRepository<WarehouseProductBatch> batchQuery,
        ICommandRepository<WarehouseProductBatch> batchCommand,
        IQueryRepository<WarehouseProductBatchAllocation> allocationQuery,
        ICommandRepository<WarehouseProductBatchAllocation> allocationCommand,
        ICommandRepository<WarehouseProductBatchTable> batchTableCommand,
        IQueryRepository<SaleDocProductBatch> saleBatchQuery,
        IQueryRepository<SaleShipmentProductBatch> shipmentBatchQuery,
        IQueryRepository<SaleDocTable> saleItemQuery,
        IQueryRepository<SaleShipmentTable> shipmentItemQuery,
        IQueryRepository<WarehouseTransferDocTable> transferItemQuery,
        IQueryRepository<InventoryAdjustmentDocTable> adjustmentItemQuery,
        IQueryRepository<InventoryCountDocTable> countItemQuery,
        IQueryRepository<FaAsset> assetQuery,
        IInventoryDispatcher inventoryDispatcher,
        IActiveInventoryCountGuardService inventoryCountGuard,
        IDocumentPostingLock documentLock,
        IDocNumberGenerator docNumberGenerator,
        IAuditLogService auditLogService,
        ILogger<OpeningInventoryService> logger,
        IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _query = query;
        _command = command;
        _lineCommand = lineCommand;
        _itemCommand = itemCommand;
        _itemQuery = itemQuery;
        _productTableCommand = productTableCommand;
        _productQuery = productQuery;
        _unitQuery = unitQuery;
        _chartAccountQuery = chartAccountQuery;
        _warehouseQuery = warehouseQuery;
        _counterpartyQuery = counterpartyQuery;
        _contractQuery = contractQuery;
        _organizationQuery = organizationQuery;
        _organizationConfigQuery = organizationConfigQuery;
        _currencyQuery = currencyQuery;
        _openingBalanceQuery = openingBalanceQuery;
        _openingBalanceAccountQuery = openingBalanceAccountQuery;
        _openingBalanceAccountCommand = openingBalanceAccountCommand;
        _openingBalanceDetailQuery = openingBalanceDetailQuery;
        _openingBalanceDetailCommand = openingBalanceDetailCommand;
        _openingBalanceSubkontoCommand = openingBalanceSubkontoCommand;
        _chartAccountSubkontoQuery = chartAccountSubkontoQuery;
        _movementQuery = movementQuery;
        _movementCommand = movementCommand;
        _batchQuery = batchQuery;
        _batchCommand = batchCommand;
        _allocationQuery = allocationQuery;
        _allocationCommand = allocationCommand;
        _batchTableCommand = batchTableCommand;
        _saleBatchQuery = saleBatchQuery;
        _shipmentBatchQuery = shipmentBatchQuery;
        _saleItemQuery = saleItemQuery;
        _shipmentItemQuery = shipmentItemQuery;
        _transferItemQuery = transferItemQuery;
        _adjustmentItemQuery = adjustmentItemQuery;
        _countItemQuery = countItemQuery;
        _assetQuery = assetQuery;
        _inventoryDispatcher = inventoryDispatcher;
        _inventoryCountGuard = inventoryCountGuard;
        _documentLock = documentLock;
        _docNumberGenerator = docNumberGenerator;
        _auditLogService = auditLogService;
    }

    public Task<Result<PagedResponse<OpeningInventoryListDto>>> GetAllAsync(
        OpeningInventoryListFilter filter,
        CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<PagedResponse<OpeningInventoryListDto>>(
                    CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var page = Math.Max(filter.Page, 1);
            var pageSize = Math.Clamp(filter.PageSize.GetValueOrDefault(50), 1, 200);
            var criteria = new OpeningInventoryByListFilterCriteriaBuilder(_userContext).Build(filter);
            var resultCriteria = new OpeningInventoryListDtoByListFilterCriteriaBuilder().Build(filter);
            var projection = new OpeningInventoryListDtoProjection().Build();

            var specification = new PagedQuerySpecification<OpeningInventory, OpeningInventoryListDto>
            {
                Criteria = criteria,
                ResultCriteria = resultCriteria,
                Selector = projection,
                OrderBy = BuildListOrder(filter),
                Take = pageSize,
                Skip = (page - 1) * pageSize
            };

            var paged = await _query.GetPagedAsync(specification, ct);
            return Result.Success(PagedResponseFactory.Create(paged, page, pageSize));
        });

    public Task<Result<OpeningInventoryDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<OpeningInventoryDto>(
                    CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var dto = await GetByIdInternalAsync(id, ct);
            return dto is null
                ? Result.Failure<OpeningInventoryDto>(OpeningInventoryErrors.NotFound(id))
                : Result.Success(dto);
        });

    public Task<Result<long>> CreateAsync(OpeningInventoryCreateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<long>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var organizationId = _userContext.OrganizationId.Value;
            var validation = await ValidateHeaderAndOpeningBalanceAsync(organizationId, dto, ct);
            if (!validation.IsSuccess)
                return Result.Failure<long>(validation.Error);

            var linesResult = await BuildLinesAsync(organizationId, dto.Lines, dto.TotalAmount, ct);
            if (!linesResult.IsSuccess)
                return Result.Failure<long>(linesResult.Error);

            var guard = await _inventoryCountGuard.EnsureWarehouseIsNotBlockedAsync(
                organizationId, dto.WarehouseId, "OpeningInventoryCreate", ct: ct);
            if (!guard.IsSuccess)
                return Result.Failure<long>(guard.Error);

            var document = new OpeningInventory
            {
                OrganizationId = organizationId,
                DocNumber = await _docNumberGenerator.GenerateAsync(organizationId, "OPN", dto.DocDate, ct),
                DocDate = DateTime.SpecifyKind(dto.DocDate, DateTimeKind.Unspecified),
                CounterpartyId = dto.CounterpartyId,
                ContractId = dto.ContractId,
                WarehouseId = dto.WarehouseId,
                TotalAmount = linesResult.Value.Sum(x => x.Amount),
                StatusId = DocumentStatusIdConst.DRAFT,
                Comment = dto.Comment,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.Now,
                OpeningInventoryProducts = linesResult.Value
            };

            await _command.CreateAsync(document, ct);
            var effectsDocument = await GetForEffectsAsync(document.Id, ct);
            if (effectsDocument is null)
                return Result.Failure<long>(OpeningInventoryErrors.NotFound(document.Id));

            var effects = await ApplyEffectsAsync(effectsDocument, ct);
            if (!effects.IsSuccess)
                return Result.Failure<long>(effects.Error);

            effectsDocument.StatusId = DocumentStatusIdConst.POSTED;
            effectsDocument.PostedAt = DateTime.Now;
            effectsDocument.PostedByUserId = _userContext.Id;
            await _command.UpdateAsync(effectsDocument, ct);

            var createdDto = await GetByIdInternalAsync(document.Id, ct);
            if (createdDto is not null)
            {
                _auditLogService.SetNewValues(createdDto);
                await _auditLogService.CreateAsync(
                    AuditLogTableConst.OpeningInventory,
                    document.Id.ToString(),
                    AuditLogOperationTypeConst.Create);
            }

            return Result.Success(document.Id);
        }, ct);

    public Task<Result> UpdateAsync(long id, OpeningInventoryUpdateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            await _documentLock.AcquireAsync(DocumentTypeIdConst.OPENINGINVENTORY, id, ct);
            var document = await GetForEffectsAsync(id, ct);
            if (document is null)
                return Result.Failure(OpeningInventoryErrors.NotFound(id));

            var organizationId = _userContext.OrganizationId.Value;
            var validation = await ValidateHeaderAndOpeningBalanceAsync(organizationId, dto, ct);
            if (!validation.IsSuccess)
                return validation;

            var linesResult = await BuildLinesAsync(organizationId, dto.Lines, dto.TotalAmount, ct);
            if (!linesResult.IsSuccess)
                return Result.Failure(linesResult.Error);

            foreach (var warehouseId in new[] { document.WarehouseId, dto.WarehouseId }.Distinct())
            {
                var guard = await _inventoryCountGuard.EnsureWarehouseIsNotBlockedAsync(
                    organizationId, warehouseId, "OpeningInventoryUpdate", ct: ct);
                if (!guard.IsSuccess)
                    return guard;
            }

            var removable = await EnsureEffectsAreRemovableAsync(document, ct);
            if (!removable.IsSuccess)
                return removable;

            var oldDto = await GetByIdInternalAsync(id, ct);
            if (oldDto is not null)
                _auditLogService.SetOldValues(oldDto);

            var oldProductTableIds = document.OpeningInventoryProducts
                .SelectMany(x => x.OpeningInventoryTables)
                .Select(x => x.ProductTableId)
                .Distinct()
                .ToList();
            var oldLineIds = document.OpeningInventoryProducts.Select(x => x.Id).ToList();

            var removeEffects = await RemoveEffectsAsync(document, ct);
            if (!removeEffects.IsSuccess)
                return removeEffects;

            document.DocDate = DateTime.SpecifyKind(dto.DocDate, DateTimeKind.Unspecified);
            document.CounterpartyId = dto.CounterpartyId;
            document.ContractId = dto.ContractId;
            document.WarehouseId = dto.WarehouseId;
            document.TotalAmount = linesResult.Value.Sum(x => x.Amount);
            document.Comment = dto.Comment;
            document.StatusId = DocumentStatusIdConst.POSTED;
            document.PostedAt = DateTime.Now;
            document.PostedByUserId = _userContext.Id;
            document.OpeningInventoryProducts = [];
            await _command.UpdateAsync(document, ct);

            if (oldLineIds.Count > 0)
                await _itemCommand.DeleteAsync(x => oldLineIds.Contains(x.OwnerId), ct);
            await _lineCommand.DeleteAsync(x => x.OwnerId == id, ct);
            if (oldProductTableIds.Count > 0)
                await _productTableCommand.DeleteAsync(x => oldProductTableIds.Contains(x.Id), ct);

            foreach (var line in linesResult.Value)
                line.OwnerId = id;
            await _lineCommand.CreateAsync(linesResult.Value, ct);

            var newEffectsDocument = CreateEffectsDocument(document, linesResult.Value);
            var applyEffects = await ApplyEffectsAsync(newEffectsDocument, ct);
            if (!applyEffects.IsSuccess)
                return applyEffects;

            var newDto = await GetByIdInternalAsync(id, ct);
            if (newDto is not null)
            {
                _auditLogService.SetNewValues(newDto);
                await _auditLogService.CreateAsync(
                    AuditLogTableConst.OpeningInventory,
                    id.ToString(),
                    AuditLogOperationTypeConst.Update,
                    dto.Comment);
            }

            return Result.Success();
        }, ct);

    public Task<Result> DeleteAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(DeleteAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            await _documentLock.AcquireAsync(DocumentTypeIdConst.OPENINGINVENTORY, id, ct);
            var document = await GetForEffectsAsync(id, ct);
            if (document is null)
                return Result.Failure(OpeningInventoryErrors.NotFound(id));

            var guard = await _inventoryCountGuard.EnsureWarehouseIsNotBlockedAsync(
                document.OrganizationId, document.WarehouseId, "OpeningInventoryDelete", ct: ct);
            if (!guard.IsSuccess)
                return guard;

            var removable = await EnsureEffectsAreRemovableAsync(document, ct);
            if (!removable.IsSuccess)
                return removable;

            var oldDto = await GetByIdInternalAsync(id, ct);
            if (oldDto is not null)
                _auditLogService.SetOldValues(oldDto);

            var productTableIds = document.OpeningInventoryProducts
                .SelectMany(x => x.OpeningInventoryTables)
                .Select(x => x.ProductTableId)
                .Distinct()
                .ToList();
            var lineIds = document.OpeningInventoryProducts.Select(x => x.Id).ToList();

            var removeEffects = await RemoveEffectsAsync(document, ct);
            if (!removeEffects.IsSuccess)
                return removeEffects;

            if (lineIds.Count > 0)
                await _itemCommand.DeleteAsync(x => lineIds.Contains(x.OwnerId), ct);
            await _lineCommand.DeleteAsync(x => x.OwnerId == id, ct);
            if (productTableIds.Count > 0)
                await _productTableCommand.DeleteAsync(x => productTableIds.Contains(x.Id), ct);
            document.OpeningInventoryProducts = [];
            await _command.DeleteAsync(document, ct);

            await _auditLogService.CreateAsync(
                AuditLogTableConst.OpeningInventory,
                id.ToString(),
                AuditLogOperationTypeConst.Delete);

            return Result.Success();
        }, ct);

    private async Task<Result> ValidateHeaderAndOpeningBalanceAsync(
        int organizationId,
        OpeningInventoryBaseDto dto,
        CancellationToken ct)
    {
        if (!await _organizationQuery.AnyAsync(
                x => x.Id == organizationId && x.StateId == StateIdConst.ACTIVE, ct))
            return Result.Failure(
                OpeningInventoryErrors.ReferenceNotFound("Organization", organizationId));

        if (!await _counterpartyQuery.AnyAsync(x =>
                x.Id == dto.CounterpartyId &&
                x.OrganizationId == organizationId &&
                x.StateId == StateIdConst.ACTIVE, ct))
            return Result.Failure(OpeningInventoryErrors.ReferenceNotFound("Counterparty", dto.CounterpartyId));

        if (!await _warehouseQuery.AnyAsync(x =>
                x.Id == dto.WarehouseId &&
                x.OrganizationId == organizationId &&
                x.StateId == StateIdConst.ACTIVE, ct))
            return Result.Failure(OpeningInventoryErrors.ReferenceNotFound("Warehouse", dto.WarehouseId));

        if (dto.ContractId.HasValue &&
            !await _contractQuery.AnyAsync(x =>
                x.Id == dto.ContractId.Value &&
                x.OrganizationId == organizationId &&
                x.CounterpartyId == dto.CounterpartyId &&
                x.StateId == StateIdConst.ACTIVE, ct))
            return Result.Failure(OpeningInventoryErrors.ReferenceNotFound("Contract", dto.ContractId.Value));

        if (!await _openingBalanceQuery.AnyAsync(x =>
                x.OrganizationId == organizationId && x.StateId == StateIdConst.ACTIVE, ct))
            return Result.Failure(OpeningInventoryErrors.OpeningBalanceNotFound(organizationId));

        var config = await _organizationConfigQuery.GetAsync(
            _queryBuilder.For<OrganizationConfig>()
                .Where(x => x.OrganizationId == organizationId)
                .Build(),
            ct);
        if (config?.BaseCurrencyId is null ||
            !await _currencyQuery.AnyAsync(
                x => x.Id == config.BaseCurrencyId.Value && x.StateId == StateIdConst.ACTIVE, ct))
            return Result.Failure(OpeningInventoryErrors.BaseCurrencyNotConfigured(organizationId));

        return Result.Success();
    }

    private async Task<Result<List<OpeningInventoryProduct>>> BuildLinesAsync(
        int organizationId,
        List<OpeningInventoryProductCreateDto> dtos,
        decimal suppliedTotal,
        CancellationToken ct)
    {
        if (dtos.Count == 0)
            return Result.Failure<List<OpeningInventoryProduct>>(
                OpeningInventoryErrors.LinesRequired());

        var duplicateProduct = dtos.GroupBy(x => x.ProductId).FirstOrDefault(x => x.Count() > 1);
        if (duplicateProduct is not null)
            return Result.Failure<List<OpeningInventoryProduct>>(
                OpeningInventoryErrors.DuplicateProduct(duplicateProduct.Key));

        var productIds = dtos.Select(x => x.ProductId).Distinct().ToList();
        var unitIds = dtos.Select(x => x.UnitId).Distinct().ToList();
        var accountIds = dtos.Select(x => x.DebitAccountId).Distinct().ToList();

        var products = await _productQuery.GetAllAsync(
            _queryBuilder.For<Product>()
                .Where(x => productIds.Contains(x.Id) &&
                            x.OrganizationId == organizationId &&
                            x.StateId == StateIdConst.ACTIVE)
                .Build(),
            ct);
        var productById = products.ToDictionary(x => x.Id);

        var foundUnitIds = (await _unitQuery.GetAllAsync(
                _queryBuilder.For<Unit>()
                    .Where(x => unitIds.Contains(x.Id) && x.StateId == StateIdConst.ACTIVE)
                    .As(x => x.Id)
                    .Build(),
                ct))
            .ToHashSet();

        var foundAccountIds = (await _chartAccountQuery.GetAllAsync(
                _queryBuilder.For<ChartAccount>()
                    .Where(x => accountIds.Contains(x.Id) &&
                                x.OrganizationId == organizationId &&
                                x.StateId == StateIdConst.ACTIVE &&
                                !x.IsGroup)
                    .As(x => x.Id)
                    .Build(),
                ct))
            .ToHashSet();

        var markingNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var lines = new List<OpeningInventoryProduct>(dtos.Count);

        foreach (var dto in dtos)
        {
            if (!productById.TryGetValue(dto.ProductId, out var product))
                return Result.Failure<List<OpeningInventoryProduct>>(
                    OpeningInventoryErrors.ReferenceNotFound("Product", dto.ProductId));
            if (!foundUnitIds.Contains(dto.UnitId))
                return Result.Failure<List<OpeningInventoryProduct>>(
                    OpeningInventoryErrors.ReferenceNotFound("Unit", dto.UnitId));
            if (!foundAccountIds.Contains(dto.DebitAccountId))
                return Result.Failure<List<OpeningInventoryProduct>>(
                    OpeningInventoryErrors.ChartAccountNotFound(dto.DebitAccountId));
            if (dto.Quantity <= 0m)
                return Result.Failure<List<OpeningInventoryProduct>>(
                    OpeningInventoryErrors.InvalidQuantity(dto.ProductId, dto.Quantity));
            if (dto.UnitPrice < 0m)
                return Result.Failure<List<OpeningInventoryProduct>>(
                    OpeningInventoryErrors.InvalidUnitPrice(dto.ProductId, dto.UnitPrice));

            var calculatedAmount = dto.Quantity * dto.UnitPrice;
            if (dto.Amount != calculatedAmount)
                return Result.Failure<List<OpeningInventoryProduct>>(
                    OpeningInventoryErrors.InvalidAmount(dto.ProductId, calculatedAmount, dto.Amount));

            if (product.IsService || !product.IsPieceTracked)
            {
                if (dto.Items.Count > 0)
                    return Result.Failure<List<OpeningInventoryProduct>>(
                        OpeningInventoryErrors.ItemsNotAllowed(dto.ProductId));
            }
            else
            {
                if (dto.Quantity != decimal.Truncate(dto.Quantity) ||
                    dto.Quantity != dto.Items.Count)
                    return Result.Failure<List<OpeningInventoryProduct>>(
                        OpeningInventoryErrors.ItemsQuantityMismatch(
                            dto.ProductId, dto.Quantity, dto.Items.Count));

                if (dto.Items.Count == 0)
                    return Result.Failure<List<OpeningInventoryProduct>>(
                        OpeningInventoryErrors.ItemsRequired(dto.ProductId));

                foreach (var item in dto.Items)
                {
                    if (string.IsNullOrWhiteSpace(item.MarkingNumber))
                        return Result.Failure<List<OpeningInventoryProduct>>(
                            OpeningInventoryErrors.MarkingNumberRequired(dto.ProductId));
                    var markingNumber = item.MarkingNumber.Trim();
                    if (!markingNumbers.Add(markingNumber))
                        return Result.Failure<List<OpeningInventoryProduct>>(
                            OpeningInventoryErrors.DuplicateMarkingNumber(markingNumber));
                }
            }

            lines.Add(new OpeningInventoryProduct
            {
                ProductId = dto.ProductId,
                Product = product,
                Quantity = dto.Quantity,
                UnitId = dto.UnitId,
                UnitPrice = dto.UnitPrice,
                Amount = calculatedAmount,
                DebitAccountId = dto.DebitAccountId,
                OpeningInventoryTables = dto.Items.Select(item => new OpeningInventoryTable
                {
                    Amount = dto.UnitPrice,
                    ProductTable = new ProductTable
                    {
                        ProductId = dto.ProductId,
                        MarkingNumber = item.MarkingNumber.Trim(),
                        SerialNumber = string.IsNullOrWhiteSpace(item.SerialNumber)
                            ? null
                            : item.SerialNumber.Trim(),
                        CreatedDate = DateTime.Now
                    }
                }).ToList()
            });
        }

        var calculatedTotal = lines.Sum(x => x.Amount);
        return calculatedTotal != suppliedTotal
            ? Result.Failure<List<OpeningInventoryProduct>>(
                OpeningInventoryErrors.InvalidTotal(calculatedTotal, suppliedTotal))
            : Result.Success(lines);
    }

    private async Task<OpeningInventory?> GetForEffectsAsync(long id, CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return null;

        var specification = _queryBuilder.For<OpeningInventory>()
            .Where(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value)
            .Build();
        specification.AddIncludes(b => b
            .Include(x => x.OpeningInventoryProducts)
            .ThenInclude(x => x.Product));
        specification.AddIncludes(b => b
            .Include(x => x.OpeningInventoryProducts)
            .ThenInclude(x => x.OpeningInventoryTables)
            .ThenInclude(x => x.ProductTable)
            .ThenInclude(x => x.WarehouseProductTable));
        return await _query.GetAsync(specification, ct);
    }

    private async Task<OpeningInventoryDto?> GetByIdInternalAsync(long id, CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return null;

        return await _query.GetAsync(
            _queryBuilder.For<OpeningInventory>()
                .Where(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value)
                .As<OpeningInventoryDto>()
                .Build(),
            ct);
    }

    private static OpeningInventory CreateEffectsDocument(
        OpeningInventory header,
        List<OpeningInventoryProduct> lines) =>
        new()
        {
            Id = header.Id,
            OrganizationId = header.OrganizationId,
            DocNumber = header.DocNumber,
            DocDate = header.DocDate,
            CounterpartyId = header.CounterpartyId,
            ContractId = header.ContractId,
            WarehouseId = header.WarehouseId,
            TotalAmount = header.TotalAmount,
            StatusId = header.StatusId,
            StateId = header.StateId,
            CreatedDate = header.CreatedDate,
            OpeningInventoryProducts = lines
        };

    private static Func<IQueryable<OpeningInventoryListDto>, IOrderedQueryable<OpeningInventoryListDto>>
        BuildListOrder(OpeningInventoryListFilter filter)
    {
        var descending = filter.SortDirection == SortDirection.Desc;
        Expression<Func<OpeningInventoryListDto, object>> key =
            filter.SortBy?.Trim().ToLowerInvariant() switch
            {
                "docnumber" => x => x.DocNumber,
                "counterparty" or "counterpartyname" => x => x.CounterpartyName,
                "warehouse" or "warehousename" => x => x.WarehouseName,
                "totalamount" => x => x.TotalAmount,
                "createddate" => x => x.CreatedDate,
                _ => x => x.DocDate
            };

        return descending
            ? query => query.OrderByDescending(key).ThenByDescending(x => x.Id)
            : query => query.OrderBy(key).ThenBy(x => x.Id);
    }
}
