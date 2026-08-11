using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Specifications;
using SharedKernel.Results;

namespace Application.Features.FaAssets;

public class FaAssetService : BaseService, IFaAssetService
{
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<FaAsset> _query;
    private readonly IQueryRepository<FaGroup> _faGroupQuery;
    private readonly IQueryRepository<FaOkof> _okofQuery;
    private readonly IQueryRepository<FaDepreciationMethod> _depreciationMethodQuery;
    private readonly IQueryRepository<FaAssetStatus> _statusQuery;
    private readonly IQueryRepository<Department> _departmentQuery;
    private readonly IQueryRepository<User> _userQuery;
    private readonly IQueryRepository<ProductTable> _productTableQuery;
    private readonly IFaAssetCommandRepository _command;

    public FaAssetService(
        IUserContext userContext,
        IUnitOfWork unitOfWork,
        IQueryBuilder queryBuilder,
        IQueryRepository<FaAsset> query,
        IQueryRepository<FaGroup> faGroupQuery,
        IQueryRepository<FaOkof> okofQuery,
        IQueryRepository<FaDepreciationMethod> depreciationMethodQuery,
        IQueryRepository<FaAssetStatus> statusQuery,
        IQueryRepository<Department> departmentQuery,
        IQueryRepository<User> userQuery,
        IQueryRepository<ProductTable> productTableQuery,
        IFaAssetCommandRepository command,
        ILogger<FaAssetService> logger)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _unitOfWork = unitOfWork;
        _queryBuilder = queryBuilder;
        _query = query;
        _faGroupQuery = faGroupQuery;
        _okofQuery = okofQuery;
        _depreciationMethodQuery = depreciationMethodQuery;
        _statusQuery = statusQuery;
        _departmentQuery = departmentQuery;
        _userQuery = userQuery;
        _productTableQuery = productTableQuery;
        _command = command;
    }

    public Task<Result<long>> CreateAsync(FaAssetCreateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<long>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var orgId = _userContext.OrganizationId.Value;
            var statusId = dto.ProcessingMode == FaAssetProcessingMode.Immediate
                ? FaAssetStatusIdConst.ACTIVE
                : FaAssetStatusIdConst.NOT_COMMISSIONED;
            var validationError = await ValidateAsync(dto, orgId, statusId, null, ct);
            if (validationError is not null)
                return Result.Failure<long>(validationError);

            var entity = new FaAsset
            {
                OrganizationId = orgId,
                StateId = StateIdConst.ACTIVE,
                InventoryNumber = dto.InventoryNumber,
                Name = dto.Name,
                FaGroupId = dto.FaGroupId,
                OkofId = dto.OkofId,
                DepreciationMethodId = dto.DepreciationMethodId,
                UsefulLifeMonths = dto.UsefulLifeMonths,
                InitialCost = dto.InitialCost,
                SalvageValue = dto.SalvageValue,
                CommissioningDate = dto.CommissioningDate,
                DeprStartDate = dto.DeprStartDate,
                PlannedUnitsTotal = dto.PlannedUnitsTotal,
                SourceProductTableId = dto.SourceProductTableId,
                DepartmentId = dto.DepartmentId,
                ResponsibleUserId = dto.ResponsibleUserId,
                AssetAccountId = dto.AssetAccountId,
                AccumulatedDepreciationAccountId = dto.AccumulatedDepreciationAccountId,
                DepreciationExpenseAccountId = dto.DepreciationExpenseAccountId,
                StatusId = statusId,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            await _command.CreateAsync(entity, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success(entity.Id);
        }, ct);

    public Task<Result> UpdateAsync(long id, FaAssetUpdateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var orgId = _userContext.OrganizationId.Value;
            var query = _queryBuilder.For<FaAsset>().Where(x => x.Id == id).Build();
            var entity = await _query.GetAsync(query, ct);

            if (entity is null)
                return Result.Failure(FaAssetErrors.NotFound(id, _userContext.LanguageId));

            var validationError = await ValidateAsync(dto, orgId, entity.StatusId, entity.Id, ct);
            if (validationError is not null)
                return Result.Failure(validationError);

            entity.InventoryNumber = dto.InventoryNumber;
            entity.Name = dto.Name;
            entity.FaGroupId = dto.FaGroupId;
            entity.OkofId = dto.OkofId;
            entity.DepreciationMethodId = dto.DepreciationMethodId;
            entity.UsefulLifeMonths = dto.UsefulLifeMonths;
            entity.InitialCost = dto.InitialCost;
            entity.SalvageValue = dto.SalvageValue;
            entity.CommissioningDate = dto.CommissioningDate;
            entity.DeprStartDate = dto.DeprStartDate;
            entity.PlannedUnitsTotal = dto.PlannedUnitsTotal;
            entity.SourceProductTableId = dto.SourceProductTableId;
            entity.DepartmentId = dto.DepartmentId;
            entity.ResponsibleUserId = dto.ResponsibleUserId;
            entity.AssetAccountId = dto.AssetAccountId;
            entity.AccumulatedDepreciationAccountId = dto.AccumulatedDepreciationAccountId;
            entity.DepreciationExpenseAccountId = dto.DepreciationExpenseAccountId;
            entity.StateId = dto.StateId;
            entity.UpdatedDate = DateTime.Now;

            await _command.UpdateAsync(entity, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }, ct);

    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(ConfirmAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var entity = await GetForCurrentOrganizationAsync(id, _userContext.OrganizationId.Value, ct);
            if (entity is null)
                return Result.Failure(FaAssetErrors.NotFound(id, _userContext.LanguageId));

            if (entity.StateId != StateIdConst.ACTIVE || entity.StatusId != FaAssetStatusIdConst.NOT_COMMISSIONED)
            {
                return entity.StateId == StateIdConst.ACTIVE && entity.StatusId == FaAssetStatusIdConst.ACTIVE
                    ? Result.Success()
                    : Result.Failure(FaAssetErrors.CannotConfirmInCurrentStatus(id, entity.StatusId, _userContext.LanguageId));
            }

            var validationError = await ValidateAsync(
                ToBaseDto(entity),
                entity.OrganizationId,
                FaAssetStatusIdConst.ACTIVE,
                entity.Id,
                ct);
            if (validationError is not null)
                return Result.Failure(validationError);

            entity.StatusId = FaAssetStatusIdConst.ACTIVE;
            entity.UpdatedDate = DateTime.Now;
            await _command.UpdateAsync(entity, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }, ct);

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CancelAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var entity = await GetForCurrentOrganizationAsync(id, _userContext.OrganizationId.Value, ct);
            if (entity is null)
                return Result.Failure(FaAssetErrors.NotFound(id, _userContext.LanguageId));

            if (entity.StateId != StateIdConst.ACTIVE || entity.StatusId != FaAssetStatusIdConst.ACTIVE)
            {
                return entity.StateId == StateIdConst.ACTIVE && entity.StatusId == FaAssetStatusIdConst.NOT_COMMISSIONED
                    ? Result.Success()
                    : Result.Failure(FaAssetErrors.CannotCancelInCurrentStatus(id, entity.StatusId, _userContext.LanguageId));
            }

            entity.StatusId = FaAssetStatusIdConst.NOT_COMMISSIONED;
            entity.UpdatedDate = DateTime.Now;
            await _command.UpdateAsync(entity, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }, ct);

    public Task<Result> DeleteAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(DeleteAsync), async () =>
        {
            var query = _queryBuilder.For<FaAsset>().Where(x => x.Id == id).Build();
            var entity = await _query.GetAsync(query, ct);

            if (entity is null)
                return Result.Failure(FaAssetErrors.NotFound(id, _userContext.LanguageId));

            entity.StateId = StateIdConst.PASSIVE;
            entity.UpdatedDate = DateTime.Now;

            await _command.UpdateAsync(entity, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }, ct);

    public Task<Result<PagedResponse<FaAssetListDto>>> GetAllAsync(FaAssetListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            var query = _queryBuilder.BuildPaged<FaAsset, FaAssetListDto, FaAssetListFilter>(filter);
            var pagedList = await _query.GetPagedAsync(query, ct);
            return Result.Success(PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize));
        });

    public Task<Result<FaAssetDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            var query = _queryBuilder.For<FaAsset>().Where(x => x.Id == id).As<FaAssetDto>().Build();
            var entity = await _query.GetAsync(query, ct);

            return entity is null
                ? Result.Failure<FaAssetDto>(FaAssetErrors.NotFound(id, _userContext.LanguageId))
                : Result.Success(entity);
        });

    private async Task<Error?> ValidateAsync(
        FaAssetBaseDto dto,
        int organizationId,
        short statusId,
        long? currentId,
        CancellationToken ct)
    {
        if (await _query.AnyAsync(x => x.OrganizationId == organizationId &&
                                       x.InventoryNumber == dto.InventoryNumber &&
                                       (!currentId.HasValue || x.Id != currentId.Value), ct))
        {
            return FaAssetErrors.InventoryNumberConflict(dto.InventoryNumber, _userContext.LanguageId);
        }

        if (!await _faGroupQuery.AnyAsync(x => x.Id == dto.FaGroupId && x.StateId == StateIdConst.ACTIVE, ct))
            return FaAssetErrors.FaGroupNotFound(dto.FaGroupId, _userContext.LanguageId);

        if (dto.OkofId.HasValue &&
            !await _okofQuery.AnyAsync(x => x.Id == dto.OkofId.Value && x.StateId == StateIdConst.ACTIVE, ct))
        {
            return FaAssetErrors.OkofNotFound(dto.OkofId.Value, _userContext.LanguageId);
        }

        var depreciationMethod = await _depreciationMethodQuery.GetAsync(new QuerySpecification<FaDepreciationMethod>
        {
            Criteria = x => x.Id == dto.DepreciationMethodId && x.StateId == StateIdConst.ACTIVE
        }, ct);

        if (depreciationMethod is null)
            return FaAssetErrors.DepreciationMethodNotFound(dto.DepreciationMethodId, _userContext.LanguageId);

        if (!await _statusQuery.AnyAsync(x => x.Id == statusId && x.StateId == StateIdConst.ACTIVE, ct))
            return FaAssetErrors.StatusNotFound(statusId, _userContext.LanguageId);

        if (dto.DepartmentId.HasValue &&
            !await _departmentQuery.AnyAsync(x => x.Id == dto.DepartmentId.Value &&
                                                  x.OrganizationId == organizationId &&
                                                  x.StateId == StateIdConst.ACTIVE, ct))
        {
            return FaAssetErrors.DepartmentNotFound(dto.DepartmentId.Value, _userContext.LanguageId);
        }

        if (dto.ResponsibleUserId.HasValue &&
            !await _userQuery.AnyAsync(x => x.Id == dto.ResponsibleUserId.Value &&
                                            x.UserOrganizations.Any(a => a.OrganizationId == organizationId) &&
                                            x.StateId == StateIdConst.ACTIVE, ct))
        {
            return FaAssetErrors.ResponsibleUserNotFound(dto.ResponsibleUserId.Value, _userContext.LanguageId);
        }

        if (dto.SourceProductTableId.HasValue &&
            !await _productTableQuery.AnyAsync(x => x.Id == dto.SourceProductTableId.Value &&
                                                    x.Product.OrganizationId == organizationId &&
                                                    x.Product.StateId == StateIdConst.ACTIVE, ct))
        {
            return FaAssetErrors.SourceProductTableNotFound(dto.SourceProductTableId.Value, _userContext.LanguageId);
        }

        if (depreciationMethod.Code == FaDepreciationMethodCodeConst.UNITS_OF_PRODUCTION &&
            !dto.PlannedUnitsTotal.HasValue)
        {
            return FaAssetErrors.PlannedUnitsRequired(_userContext.LanguageId);
        }

        if (statusId == FaAssetStatusIdConst.ACTIVE && dto.CommissioningDate is null)
            return FaAssetErrors.CommissioningDateRequiredForActive(_userContext.LanguageId);

        if (statusId == FaAssetStatusIdConst.ACTIVE && dto.DeprStartDate is null)
            return FaAssetErrors.DepreciationStartDateRequiredForActive(_userContext.LanguageId);

        return null;
    }

    private async Task<FaAsset?> GetForCurrentOrganizationAsync(long id, int organizationId, CancellationToken ct)
    {
        var query = _queryBuilder.For<FaAsset>()
            .Where(x => x.Id == id && x.OrganizationId == organizationId)
            .Build();
        return await _query.GetAsync(query, ct);
    }

    private static FaAssetBaseDto ToBaseDto(FaAsset entity) => new()
    {
        InventoryNumber = entity.InventoryNumber,
        Name = entity.Name,
        FaGroupId = entity.FaGroupId,
        OkofId = entity.OkofId,
        DepreciationMethodId = entity.DepreciationMethodId,
        UsefulLifeMonths = entity.UsefulLifeMonths,
        InitialCost = entity.InitialCost,
        SalvageValue = entity.SalvageValue,
        CommissioningDate = entity.CommissioningDate,
        DeprStartDate = entity.DeprStartDate,
        PlannedUnitsTotal = entity.PlannedUnitsTotal,
        SourceProductTableId = entity.SourceProductTableId,
        DepartmentId = entity.DepartmentId,
        ResponsibleUserId = entity.ResponsibleUserId
    };
}
