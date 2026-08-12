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
    private readonly IFaAssetCommandRepository _command;

    public FaAssetService(
        IUserContext userContext,
        IUnitOfWork unitOfWork,
        IQueryBuilder queryBuilder,
        IQueryRepository<FaAsset> query,
        IQueryRepository<FaGroup> faGroupQuery,
        IQueryRepository<FaOkof> okofQuery,
        IQueryRepository<FaDepreciationMethod> depreciationMethodQuery,
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
        _command = command;
    }

    public Task<Result> UpdateAsync(long id, FaAssetUpdateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var organizationId = _userContext.OrganizationId.Value;
            var query = _queryBuilder.For<FaAsset>()
                .Where(x => x.Id == id && x.OrganizationId == organizationId)
                .Build();
            var entity = await _query.GetAsync(query, ct);

            if (entity is null)
                return Result.Failure(FaAssetErrors.NotFound(id, _userContext.LanguageId));

            var validationError = await ValidateAsync(dto, organizationId, entity.Id, ct);
            if (validationError is not null)
                return Result.Failure(validationError);

            entity.InventoryNumber = dto.InventoryNumber.Trim();
            entity.Name = dto.Name.Trim();
            entity.FaGroupId = dto.FaGroupId;
            entity.OkofId = dto.OkofId;
            entity.DepreciationMethodId = dto.DepreciationMethodId;
            entity.UsefulLifeMonths = dto.UsefulLifeMonths;
            entity.PlannedUnitsTotal = dto.PlannedUnitsTotal;
            entity.AssetAccountId = dto.AssetAccountId;
            entity.AccumulatedDepreciationAccountId = dto.AccumulatedDepreciationAccountId;
            entity.DepreciationExpenseAccountId = dto.DepreciationExpenseAccountId;
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
        FaAssetUpdateDto dto,
        int organizationId,
        long currentId,
        CancellationToken ct)
    {
        var inventoryNumber = dto.InventoryNumber.Trim();
        if (await _query.AnyAsync(x => x.OrganizationId == organizationId &&
                                       x.InventoryNumber == inventoryNumber &&
                                       x.Id != currentId, ct))
        {
            return FaAssetErrors.InventoryNumberConflict(inventoryNumber, _userContext.LanguageId);
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

        if (depreciationMethod.Code == FaDepreciationMethodCodeConst.UNITS_OF_PRODUCTION &&
            !dto.PlannedUnitsTotal.HasValue)
        {
            return FaAssetErrors.PlannedUnitsRequired(_userContext.LanguageId);
        }

        return null;
    }
}