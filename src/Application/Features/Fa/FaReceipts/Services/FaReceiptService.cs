using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.AuditLogs;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.FaReceipts;

public class FaReceiptService : BaseService, IFaReceiptService
{
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IFaReceiptLifecycleService _lifecycleService;
    private readonly IAuditLogService _auditLogService;
    private readonly IQueryRepository<FaReceiptDoc> _query;
    private readonly IQueryRepository<CounterpartyCard> _counterpartyQuery;
    private readonly IQueryRepository<Warehouse> _warehouseQuery;
    private readonly IQueryRepository<Currency> _currencyQuery;
    private readonly IQueryRepository<Product> _productQuery;
    private readonly IQueryRepository<VatRate> _vatRateQuery;
    private readonly IQueryRepository<FaGroup> _faGroupQuery;
    private readonly IQueryRepository<FaOkof> _okofQuery;
    private readonly IQueryRepository<FaDepreciationMethod> _depreciationMethodQuery;
    private readonly IQueryRepository<FaReceiptType> _faReceiptTypeQuery;
    private readonly IQueryRepository<Department> _departmentQuery;
    private readonly IQueryRepository<User> _userQuery;
    private readonly IQueryRepository<FaAsset> _faAssetQuery;
    private readonly IFaReceiptCommandRepository _command;
    private readonly IDocNumberGenerator _docNumberGenerator;

    public FaReceiptService(
        IUserContext userContext,
        IUnitOfWork unitOfWork,
        IQueryBuilder queryBuilder,
        IFaReceiptLifecycleService lifecycleService,
        IAuditLogService auditLogService,
        IQueryRepository<FaReceiptDoc> query,
        IQueryRepository<CounterpartyCard> counterpartyQuery,
        IQueryRepository<Warehouse> warehouseQuery,
        IQueryRepository<Currency> currencyQuery,
        IQueryRepository<Product> productQuery,
        IQueryRepository<VatRate> vatRateQuery,
        IQueryRepository<FaGroup> faGroupQuery,
        IQueryRepository<FaOkof> okofQuery,
        IQueryRepository<FaDepreciationMethod> depreciationMethodQuery,
        IQueryRepository<FaReceiptType> faReceiptTypeQuery,
        IQueryRepository<Department> departmentQuery,
        IQueryRepository<User> userQuery,
        IQueryRepository<FaAsset> faAssetQuery,
        IFaReceiptCommandRepository command,
        IDocNumberGenerator docNumberGenerator,
        ILogger<FaReceiptService> logger)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _unitOfWork = unitOfWork;
        _queryBuilder = queryBuilder;
        _lifecycleService = lifecycleService;
        _auditLogService = auditLogService;
        _query = query;
        _counterpartyQuery = counterpartyQuery;
        _warehouseQuery = warehouseQuery;
        _currencyQuery = currencyQuery;
        _productQuery = productQuery;
        _vatRateQuery = vatRateQuery;
        _faGroupQuery = faGroupQuery;
        _okofQuery = okofQuery;
        _depreciationMethodQuery = depreciationMethodQuery;
        _faReceiptTypeQuery = faReceiptTypeQuery;
        _departmentQuery = departmentQuery;
        _userQuery = userQuery;
        _faAssetQuery = faAssetQuery;
        _command = command;
        _docNumberGenerator = docNumberGenerator;
    }

    public Task<Result<PagedResponse<FaReceiptListDto>>> GetAllAsync(FaReceiptListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            var query = _queryBuilder.BuildPaged<FaReceiptDoc, FaReceiptListDto, FaReceiptListFilter>(filter);
            var pagedList = await _query.GetPagedAsync(query, ct);
            return Result.Success(PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize));
        });

    public Task<Result<FaReceiptDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            var entity = await GetByIdInternalAsync(id, ct);
            return entity is null
                ? Result.Failure<FaReceiptDto>(FaReceiptErrors.NotFound(id, _userContext.LanguageId))
                : Result.Success(entity);
        });

    public Task<Result<long>> CreateAsync(FaReceiptCreateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<long>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var organizationId = _userContext.OrganizationId.Value;
            var headerValidation = await ValidateHeaderReferencesAsync(dto, organizationId, ct);
            if (!headerValidation.IsSuccess)
                return Result.Failure<long>(headerValidation.Error);

            var linesResult = await BuildLinesAsync(organizationId, dto.Lines, ct);
            if (!linesResult.IsSuccess)
                return Result.Failure<long>(linesResult.Error);

            var now = DateTime.Now;
            var lines = linesResult.Value;
            var doc = new FaReceiptDoc
            {
                OrganizationId = organizationId,
                StateId = StateIdConst.ACTIVE,
                DocNumber = await _docNumberGenerator.GenerateAsync(organizationId, "FA", dto.DocDate, ct),
                DocDate = NormalizeDateTime(dto.DocDate),
                CounterpartyId = dto.CounterpartyId,
                WarehouseId = dto.WarehouseId,
                CurrencyId = dto.CurrencyId,
                TotalAmount = lines.Sum(x => x.Amount),
                VatAmount = lines.Sum(x => x.VatAmount),
                FinalAmount = lines.Sum(x => x.TotalAmount),
                StatusId = DocumentStatusIdConst.DRAFT,
                ReceiptTypeId = dto.ReceiptTypeId,
                SupplierAccountId = dto.SupplierAccountId,
                CreatedDate = now,
                UpdatedDate = now,
                Lines = lines
            };

            await _command.CreateAsync(doc, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var docDto = await GetByIdInternalAsync(doc.Id, ct);
            if (docDto is not null)
            {
                _auditLogService.SetNewValues(docDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.FaReceiptDoc, doc.Id.ToString(), AuditLogOperationTypeConst.Create);
            }

            return Result.Success(doc.Id);
        }, ct);

    public Task<Result> UpdateAsync(long id, FaReceiptUpdateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var organizationId = _userContext.OrganizationId.Value;
            var doc = await GetAggregateAsync(id, ct);
            if (doc is null)
                return Result.Failure(FaReceiptErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(FaReceiptErrors.CannotUpdateInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto is not null)
                _auditLogService.SetOldValues(oldDocDto);

            var headerValidation = await ValidateHeaderReferencesAsync(dto, organizationId, ct);
            if (!headerValidation.IsSuccess)
                return Result.Failure(headerValidation.Error);

            var linesResult = await BuildLinesAsync(organizationId, dto.Lines, ct);
            if (!linesResult.IsSuccess)
                return Result.Failure(linesResult.Error);

            if (doc.Lines.Count > 0)
            {
                await _command.DeleteLinesAsync(doc.Lines.ToList(), ct);
                doc.Lines.Clear();
            }

            foreach (var line in linesResult.Value)
                doc.Lines.Add(line);

            doc.DocDate = NormalizeDateTime(dto.DocDate);
            doc.CounterpartyId = dto.CounterpartyId;
            doc.WarehouseId = dto.WarehouseId;
            doc.CurrencyId = dto.CurrencyId;
            doc.TotalAmount = doc.Lines.Sum(x => x.Amount);
            doc.VatAmount = doc.Lines.Sum(x => x.VatAmount);
            doc.FinalAmount = doc.Lines.Sum(x => x.TotalAmount);
            doc.ReceiptTypeId = dto.ReceiptTypeId;
            doc.SupplierAccountId = dto.SupplierAccountId;
            doc.UpdatedDate = DateTime.Now;

            await _command.UpdateAsync(doc, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var newDocDto = await GetByIdInternalAsync(id, ct);
            if (newDocDto is not null)
            {
                _auditLogService.SetNewValues(newDocDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.FaReceiptDoc, id.ToString(), AuditLogOperationTypeConst.Update);
            }

            return Result.Success();
        }, ct);

    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) =>
        _lifecycleService.ConfirmAsync(id, ct);

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        _lifecycleService.CancelAsync(id, ct);

    public Task<Result> DeleteAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(DeleteAsync), async () =>
        {
            var doc = await GetAggregateAsync(id, ct);
            if (doc is null)
                return Result.Failure(FaReceiptErrors.NotFound(id, _userContext.LanguageId));

            if (doc.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(FaReceiptErrors.CannotDeleteInCurrentStatus(id, doc.StatusId, _userContext.LanguageId));

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto is not null)
                _auditLogService.SetOldValues(oldDocDto);

            if (doc.Lines.Count > 0)
            {
                await _command.DeleteLinesAsync(doc.Lines.ToList(), ct);
                doc.Lines.Clear();
            }

            doc.StateId = StateIdConst.PASSIVE;
            doc.UpdatedDate = DateTime.Now;

            await _command.UpdateAsync(doc, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var newDocDto = await GetByIdInternalAsync(id, ct);
            if (newDocDto is not null)
            {
                _auditLogService.SetNewValues(newDocDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.FaReceiptDoc, id.ToString(), AuditLogOperationTypeConst.Delete);
            }

            return Result.Success();
        }, ct);

    private async Task<Result> ValidateHeaderReferencesAsync(FaReceiptBaseDto dto, int organizationId, CancellationToken ct)
    {
        if (dto.ReceiptTypeId is not (FaReceiptTypeIdConst.PURCHASE or FaReceiptTypeIdConst.CONSTRUCTION or FaReceiptTypeIdConst.OTHER) ||
            !await _faReceiptTypeQuery.AnyAsync(x => x.Id == dto.ReceiptTypeId, ct))
        {
            return Result.Failure(FaReceiptErrors.InvalidReceiptType(dto.ReceiptTypeId, _userContext.LanguageId));
        }

        if (dto.CounterpartyId.HasValue &&
            !await _counterpartyQuery.AnyAsync(x => x.Id == dto.CounterpartyId.Value &&
                                                    x.OrganizationId == organizationId &&
                                                    x.StateId == StateIdConst.ACTIVE, ct))
        {
            return Result.Failure(FaReceiptErrors.CounterpartyNotFound(dto.CounterpartyId.Value, _userContext.LanguageId));
        }

        if (dto.WarehouseId.HasValue &&
            !await _warehouseQuery.AnyAsync(x => x.Id == dto.WarehouseId.Value &&
                                                 x.OrganizationId == organizationId &&
                                                 x.StateId == StateIdConst.ACTIVE, ct))
        {
            return Result.Failure(FaReceiptErrors.WarehouseNotFound(dto.WarehouseId.Value, _userContext.LanguageId));
        }

        if (!await _currencyQuery.AnyAsync(x => x.Id == dto.CurrencyId && x.StateId == StateIdConst.ACTIVE, ct))
            return Result.Failure(FaReceiptErrors.CurrencyNotFound(dto.CurrencyId, _userContext.LanguageId));

        return Result.Success();
    }

    private async Task<Result<List<FaReceiptDocLine>>> BuildLinesAsync(int organizationId, List<FaReceiptLineWriteDto> lineDtos, CancellationToken ct)
    {
        if (lineDtos.Count == 0)
            return Result.Failure<List<FaReceiptDocLine>>(FaReceiptErrors.LinesRequired(_userContext.LanguageId));

        var sourceProductIds = lineDtos.Where(x => x.SourceProductId.HasValue).Select(x => x.SourceProductId!.Value).Distinct().ToList();
        var vatRateIds = lineDtos.Where(x => x.VatRateId.HasValue).Select(x => x.VatRateId!.Value).Distinct().ToList();
        var faGroupIds = lineDtos.SelectMany(x => x.Assets).Select(x => x.FaGroupId).Distinct().ToList();
        var depreciationMethodIds = lineDtos.SelectMany(x => x.Assets).Select(x => x.DepreciationMethodId).Distinct().ToList();
        var okofIds = lineDtos.SelectMany(x => x.Assets).Where(x => x.OkofId.HasValue).Select(x => x.OkofId!.Value).Distinct().ToList();
        var departmentIds = lineDtos.SelectMany(x => x.Assets).Where(x => x.DepartmentId.HasValue).Select(x => x.DepartmentId!.Value).Distinct().ToList();
        var userIds = lineDtos.SelectMany(x => x.Assets).Where(x => x.ResponsibleUserId.HasValue).Select(x => x.ResponsibleUserId!.Value).Distinct().ToList();
        var inventoryNumbers = lineDtos.SelectMany(x => x.Assets).Select(x => x.InventoryNumber.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        var productById = await LoadProductsAsync(sourceProductIds, organizationId, ct);
        var vatRateById = await LoadVatRatesAsync(vatRateIds, ct);
        var faGroupById = await LoadFaGroupsAsync(faGroupIds, organizationId, ct);
        var depreciationMethodById = await LoadDepreciationMethodsAsync(depreciationMethodIds, ct);
        var okofById = await LoadOkofsAsync(okofIds, ct);
        var departmentById = await LoadDepartmentsAsync(departmentIds, organizationId, ct);
        var userById = await LoadUsersAsync(userIds, organizationId, ct);

        var existingInventoryNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (inventoryNumbers.Count > 0)
        {
            var query = _queryBuilder.For<FaAsset>()
                .Where(x => x.OrganizationId == organizationId && inventoryNumbers.Contains(x.InventoryNumber))
                .As(x => x.InventoryNumber)
                .Build();
            var values = await _faAssetQuery.GetAllAsync(query, ct);
            existingInventoryNumbers = values.ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        var lines = new List<FaReceiptDocLine>();
        var documentInventoryNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var lineDto in lineDtos)
        {
            if (lineDto.SourceProductId.HasValue && !productById.ContainsKey(lineDto.SourceProductId.Value))
                return Result.Failure<List<FaReceiptDocLine>>(FaReceiptErrors.ProductNotFound(lineDto.SourceProductId.Value, _userContext.LanguageId));

            if (lineDto.VatRateId.HasValue && !vatRateById.ContainsKey(lineDto.VatRateId.Value))
                return Result.Failure<List<FaReceiptDocLine>>(FaReceiptErrors.VatRateNotFound(lineDto.VatRateId.Value, _userContext.LanguageId));

            if (lineDto.Assets.Count == 0)
                return Result.Failure<List<FaReceiptDocLine>>(FaReceiptErrors.AssetLinesRequired(lineDto.Name, _userContext.LanguageId));

            if (lineDto.Quantity != decimal.Truncate(lineDto.Quantity) || lineDto.Quantity != lineDto.Assets.Count)
            {
                return Result.Failure<List<FaReceiptDocLine>>(
                    FaReceiptErrors.LineQuantityMismatch(lineDto.Name, lineDto.Quantity, lineDto.Assets.Count, _userContext.LanguageId));
            }

            var amount = Math.Round(lineDto.Price * lineDto.Quantity, 8);
            var vatAmount = lineDto.VatRateId.HasValue
                ? Math.Round(amount * vatRateById[lineDto.VatRateId.Value].Rate / 100, 8)
                : 0m;

            var assets = new List<FaReceiptDocAsset>();
            foreach (var assetDto in lineDto.Assets)
            {
                var inventoryNumber = assetDto.InventoryNumber.Trim();
                if (!documentInventoryNumbers.Add(inventoryNumber))
                    return Result.Failure<List<FaReceiptDocLine>>(FaReceiptErrors.DuplicateInventoryNumber(inventoryNumber, _userContext.LanguageId));

                if (existingInventoryNumbers.Contains(inventoryNumber))
                    return Result.Failure<List<FaReceiptDocLine>>(FaReceiptErrors.InventoryNumberConflict(inventoryNumber, _userContext.LanguageId));

                if (!faGroupById.ContainsKey(assetDto.FaGroupId))
                    return Result.Failure<List<FaReceiptDocLine>>(FaReceiptErrors.FaGroupNotFound(assetDto.FaGroupId, _userContext.LanguageId));

                if (!depreciationMethodById.TryGetValue(assetDto.DepreciationMethodId, out var depreciationMethod))
                {
                    return Result.Failure<List<FaReceiptDocLine>>(
                        FaReceiptErrors.DepreciationMethodNotFound(assetDto.DepreciationMethodId, _userContext.LanguageId));
                }

                if (assetDto.OkofId.HasValue && !okofById.ContainsKey(assetDto.OkofId.Value))
                    return Result.Failure<List<FaReceiptDocLine>>(FaReceiptErrors.OkofNotFound(assetDto.OkofId.Value, _userContext.LanguageId));

                if (assetDto.DepartmentId.HasValue && !departmentById.ContainsKey(assetDto.DepartmentId.Value))
                {
                    return Result.Failure<List<FaReceiptDocLine>>(
                        FaReceiptErrors.DepartmentNotFound(assetDto.DepartmentId.Value, _userContext.LanguageId));
                }

                if (assetDto.ResponsibleUserId.HasValue && !userById.ContainsKey(assetDto.ResponsibleUserId.Value))
                {
                    return Result.Failure<List<FaReceiptDocLine>>(
                        FaReceiptErrors.ResponsibleUserNotFound(assetDto.ResponsibleUserId.Value, _userContext.LanguageId));
                }

                if (depreciationMethod.Code == FaDepreciationMethodCodeConst.UNITS_OF_PRODUCTION &&
                    !assetDto.PlannedUnitsTotal.HasValue)
                {
                    return Result.Failure<List<FaReceiptDocLine>>(
                        FaReceiptErrors.PlannedUnitsRequired(inventoryNumber, _userContext.LanguageId));
                }

                assets.Add(new FaReceiptDocAsset
                {
                    InventoryNumber = inventoryNumber,
                    Name = assetDto.Name.Trim(),
                    InitialCost = assetDto.InitialCost,
                    SalvageValue = assetDto.SalvageValue,
                    UsefulLifeMonths = assetDto.UsefulLifeMonths,
                    DepreciationMethodId = assetDto.DepreciationMethodId,
                    FaGroupId = assetDto.FaGroupId,
                    OkofId = assetDto.OkofId,
                    CommissioningDate = NormalizeDateTime(assetDto.CommissioningDate),
                    DeprStartDate = NormalizeDateTime(assetDto.DeprStartDate),
                    PlannedUnitsTotal = assetDto.PlannedUnitsTotal,
                    DepartmentId = assetDto.DepartmentId,
                    ResponsibleUserId = assetDto.ResponsibleUserId,
                    AssetAccountId = assetDto.AssetAccountId,
                    AccumulatedDepreciationAccountId = assetDto.AccumulatedDepreciationAccountId,
                    DepreciationExpenseAccountId = assetDto.DepreciationExpenseAccountId
                });
            }

            var assetInitialCostTotal = assets.Sum(x => x.InitialCost);
            if (Math.Abs(assetInitialCostTotal - amount) > 0.01m)
            {
                return Result.Failure<List<FaReceiptDocLine>>(
                    FaReceiptErrors.LineAmountMismatch(lineDto.Name, amount, assetInitialCostTotal, _userContext.LanguageId));
            }

            lines.Add(new FaReceiptDocLine
            {
                SourceProductId = lineDto.SourceProductId,
                Name = lineDto.Name.Trim(),
                Quantity = lineDto.Quantity,
                Price = lineDto.Price,
                Amount = amount,
                VatRateId = lineDto.VatRateId,
                VatAmount = vatAmount,
                TotalAmount = amount + vatAmount,
                CapitalInvestmentAccountId = lineDto.CapitalInvestmentAccountId,
                VatAccountId = lineDto.VatAccountId,
                Assets = assets
            });
        }

        return Result.Success(lines);
    }

    private async Task<FaReceiptDoc?> GetAggregateAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<FaReceiptDoc>().Where(x => x.Id == id).Build();
        query.AddIncludes(x => x.Include(d => d.Lines).ThenInclude(l => l.Assets));
        return await _query.GetAsync(query, ct);
    }

    private async Task<FaReceiptDto?> GetByIdInternalAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<FaReceiptDoc>().Where(x => x.Id == id).As<FaReceiptDto>().Build();
        return await _query.GetAsync(query, ct);
    }

    private async Task<Dictionary<int, Product>> LoadProductsAsync(IReadOnlyCollection<int> ids, int organizationId, CancellationToken ct)
    {
        if (ids.Count == 0)
            return new Dictionary<int, Product>();

        var query = _queryBuilder.For<Product>()
            .Where(x => ids.Contains(x.Id) && x.OrganizationId == organizationId && x.StateId == StateIdConst.ACTIVE)
            .Build();
        var entities = await _productQuery.GetAllAsync(query, ct);
        return entities.ToDictionary(x => x.Id);
    }

    private async Task<Dictionary<short, VatRate>> LoadVatRatesAsync(IReadOnlyCollection<short> ids, CancellationToken ct)
    {
        if (ids.Count == 0)
            return new Dictionary<short, VatRate>();

        var query = _queryBuilder.For<VatRate>()
            .Where(x => ids.Contains(x.Id) && x.StateId == StateIdConst.ACTIVE)
            .Build();
        var entities = await _vatRateQuery.GetAllAsync(query, ct);
        return entities.ToDictionary(x => x.Id);
    }

    private async Task<Dictionary<int, FaGroup>> LoadFaGroupsAsync(IReadOnlyCollection<int> ids, int organizationId, CancellationToken ct)
    {
        if (ids.Count == 0)
            return new Dictionary<int, FaGroup>();

        var query = _queryBuilder.For<FaGroup>()
            .Where(x => ids.Contains(x.Id) && x.OrganizationId == organizationId && x.StateId == StateIdConst.ACTIVE)
            .Build();
        var entities = await _faGroupQuery.GetAllAsync(query, ct);
        return entities.ToDictionary(x => x.Id);
    }

    private async Task<Dictionary<short, FaDepreciationMethod>> LoadDepreciationMethodsAsync(IReadOnlyCollection<short> ids, CancellationToken ct)
    {
        if (ids.Count == 0)
            return new Dictionary<short, FaDepreciationMethod>();

        var query = _queryBuilder.For<FaDepreciationMethod>()
            .Where(x => ids.Contains(x.Id) && x.StateId == StateIdConst.ACTIVE)
            .Build();
        var entities = await _depreciationMethodQuery.GetAllAsync(query, ct);
        return entities.ToDictionary(x => x.Id);
    }

    private async Task<Dictionary<short, FaOkof>> LoadOkofsAsync(IReadOnlyCollection<short> ids, CancellationToken ct)
    {
        if (ids.Count == 0)
            return new Dictionary<short, FaOkof>();

        var query = _queryBuilder.For<FaOkof>()
            .Where(x => ids.Contains(x.Id) && x.StateId == StateIdConst.ACTIVE)
            .Build();
        var entities = await _okofQuery.GetAllAsync(query, ct);
        return entities.ToDictionary(x => x.Id);
    }

    private async Task<Dictionary<int, Department>> LoadDepartmentsAsync(IReadOnlyCollection<int> ids, int organizationId, CancellationToken ct)
    {
        if (ids.Count == 0)
            return new Dictionary<int, Department>();

        var query = _queryBuilder.For<Department>()
            .Where(x => ids.Contains(x.Id) && x.OrganizationId == organizationId && x.StateId == StateIdConst.ACTIVE)
            .Build();
        var entities = await _departmentQuery.GetAllAsync(query, ct);
        return entities.ToDictionary(x => x.Id);
    }

    private async Task<Dictionary<int, User>> LoadUsersAsync(IReadOnlyCollection<int> ids, int organizationId, CancellationToken ct)
    {
        if (ids.Count == 0)
            return new Dictionary<int, User>();

        var query = _queryBuilder.For<User>()
            .Where(x => ids.Contains(x.Id) &&
                        x.StateId == StateIdConst.ACTIVE &&
                        x.UserOrganizations.Any(membership =>
                            membership.OrganizationId == organizationId &&
                            membership.StateId == StateIdConst.ACTIVE))
            .Build();
        var entities = await _userQuery.GetAllAsync(query, ct);
        return entities.ToDictionary(x => x.Id);
    }

    private static DateTime NormalizeDateTime(DateTime value) =>
        DateTime.SpecifyKind(value, DateTimeKind.Unspecified);

    private static DateTime? NormalizeDateTime(DateTime? value) =>
        value.HasValue ? NormalizeDateTime(value.Value) : null;

    private static string NormalizeReceiptType(string value) =>
        value.Trim().ToUpperInvariant();
}
