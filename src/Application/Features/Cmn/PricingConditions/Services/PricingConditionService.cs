using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.PricingConditions;

public class PricingConditionService : IPricingConditionService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<PricingCondition> _query;
    private readonly ICommandRepository<PricingCondition> _command;

    public PricingConditionService(IUserContext userContext,
                                   IQueryBuilder queryBuilder,
                                   IQueryRepository<PricingCondition> query,
                                   ICommandRepository<PricingCondition> command)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _query = query;
        _command = command;
    }

    public async Task<Result<PagedResponse<PricingConditionListDto>>> GetAllAsync(PricingConditionListFilter filter, CancellationToken ct = default)
    {
        var query = _queryBuilder.BuildPaged<PricingCondition, PricingConditionListDto, PricingConditionListFilter>(filter);
        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<PricingConditionDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is null)
            return Result.Failure<PricingConditionDto>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var orgId = _userContext.OrganizationId.Value;

        var query = _queryBuilder.For<PricingCondition>()
            .Where(x => x.Id == id && x.OrganizationId == orgId)
            .As<PricingConditionDto>()
            .Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity is null)
            return Result.Failure<PricingConditionDto>(PricingConditionErrors.NotFound(id, _userContext.LanguageId));

        return entity;
    }

    public async Task<Result<long>> CreateAsync(PricingConditionCreateDto dto, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is null)
            return Result.Failure<long>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var entity = new PricingCondition
        {
            OrganizationId = _userContext.OrganizationId.Value,
            PricingMethodId = dto.PricingMethodId,
            PricingValue = dto.PricingValue,
            RoundingMethodId = dto.RoundingMethodId,
            RoundingPrecision = dto.RoundingPrecision,
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

        var query = _queryBuilder.For<PricingCondition>()
            .Where(x => x.Id == id && x.OrganizationId == orgId)
            .Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity is null)
            return Result.Failure(PricingConditionErrors.NotFound(id, _userContext.LanguageId));

        entity.StateId = StateIdConst.PASSIVE;
        await _command.UpdateAsync(entity, ct);

        return Result.Success();
    }

    public async Task<Result<PricingConditionDto>> GetNowAsync(CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is null)
            return Result.Failure<PricingConditionDto>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var orgId = _userContext.OrganizationId.Value;
        var now = DateTime.Now;

        var query = _queryBuilder.For<PricingCondition>()
            .Where(x => x.OrganizationId == orgId &&
                        x.StateId == StateIdConst.ACTIVE &&
                        x.StartDate <= now &&
                        (x.EndDate == null || x.EndDate >= now))
            .As<PricingConditionDto>()
            .OrderBy(items => items
                .OrderByDescending(x => x.StartDate)
                .ThenByDescending(x => x.Id))
            .Build();

        var current = await _query.GetAsync(query, ct);

        if (current is null)
            return Result.Failure<PricingConditionDto>(PricingConditionErrors.CurrentNotFound(_userContext.LanguageId));

        return current;
    }
}
