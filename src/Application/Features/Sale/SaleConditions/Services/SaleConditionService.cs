using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.SaleConditions;

public class SaleConditionService : ISaleConditionService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<SaleCondition> _query;
    private readonly ICommandRepository<SaleCondition> _command;

    public SaleConditionService(IUserContext userContext,
                                IQueryBuilder queryBuilder,
                                IQueryRepository<SaleCondition> query,
                                ICommandRepository<SaleCondition> command)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _query = query;
        _command = command;
    }

    public async Task<Result<PagedResponse<SaleConditionListDto>>> GetAllAsync(SaleConditionListFilter filter, CancellationToken ct = default)
    {
        var query = _queryBuilder.BuildPaged<SaleCondition, SaleConditionListDto, SaleConditionListFilter>(filter);
        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<SaleConditionDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is null)
            return Result.Failure<SaleConditionDto>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var orgId = _userContext.OrganizationId.Value;

        var query = _queryBuilder.For<SaleCondition>()
            .Where(x => x.Id == id && x.OrganizationId == orgId)
            .As<SaleConditionDto>()
            .Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity is null)
            return Result.Failure<SaleConditionDto>(SaleConditionErrors.NotFound(id, _userContext.LanguageId));

        return entity;
    }

    public async Task<Result<long>> CreateAsync(SaleConditionCreateDto dto, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is null)
            return Result.Failure<long>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var entity = new SaleCondition
        {
            OrganizationId = _userContext.OrganizationId.Value,
            CostingMethodId = dto.CostingMethodId,
            VatRateId = dto.VatRateId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Now
        };

        await _command.CreateAsync(entity, ct);
        return entity.Id;
    }

    public async Task<Result> DeleteAsync(long id, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is null)
            return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var orgId = _userContext.OrganizationId.Value;

        var query = _queryBuilder.For<SaleCondition>()
            .Where(x => x.Id == id && x.OrganizationId == orgId)
            .Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity is null)
            return Result.Failure(SaleConditionErrors.NotFound(id, _userContext.LanguageId));

        entity.StateId = StateIdConst.PASSIVE;
        await _command.UpdateAsync(entity, ct);

        return Result.Success();
    }

    public async Task<Result<SaleConditionDto>> GetNowAsync(CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is null)
            return Result.Failure<SaleConditionDto>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var orgId = _userContext.OrganizationId.Value;
        var now = DateTime.Now;

        var query = _queryBuilder.For<SaleCondition>()
            .Where(x => x.OrganizationId == orgId &&
                        x.StateId == StateIdConst.ACTIVE &&
                        x.StartDate <= now &&
                        (x.EndDate == null || x.EndDate >= now))
            .As<SaleConditionDto>()
            .OrderBy(x => x.StartDate)
            .Desc()
            .Build();

        var items = await _query.GetAllAsync(query, ct);
        var current = items.FirstOrDefault();

        if (current is null)
            return Result.Failure<SaleConditionDto>(SaleConditionErrors.CurrentNotFound(_userContext.LanguageId));

        return current;
    }
}
