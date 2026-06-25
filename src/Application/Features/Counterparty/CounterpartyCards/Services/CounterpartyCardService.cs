using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.CounterpartyCards;

public class CounterpartyCardService : ICounterpartyCardService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<CounterpartyCard> _query;
    private readonly ICommandRepository<CounterpartyCard> _command;

    public CounterpartyCardService(IUserContext userContext,
                                   IQueryBuilder queryBuilder, 
                                   IQueryRepository<CounterpartyCard> query,
                                   ICommandRepository<CounterpartyCard> command)
    {
        _query = query;
        _command = command;
        _userContext = userContext; 
        _queryBuilder = queryBuilder;
    }

    public async Task<Result<CounterpartyCardCreateResultDto>> CreateAsync(CounterpartyCardCreateDto dto, CancellationToken ct = default)
    {
        var orgId = _userContext.OrganizationId!.Value;

        if (await _query.AnyAsync(x => x.ShortName == dto.ShortName, ct))
            return Result.Failure<CounterpartyCardCreateResultDto>(CounterpartyCardErrors.ShortNameConflict(dto.ShortName, _userContext.LanguageId));

        var entity = BuildCreateEntity(dto, orgId);
        await _command.CreateAsync(entity, ct);
        return ToCreateResult(entity);
    }

    public async Task<Result<List<CounterpartyCardCreateResultDto>>> CreateManyAsync(CounterpartyCardCreateManyDto dto, CancellationToken ct = default)
    {
        var orgId = _userContext.OrganizationId!.Value;
        var duplicateShortName = dto.Counterparties
            .GroupBy(x => x.ShortName)
            .FirstOrDefault(x => x.Count() > 1)
            ?.Key;

        if (duplicateShortName != null)
            return Result.Failure<List<CounterpartyCardCreateResultDto>>(CounterpartyCardErrors.ShortNameConflict(duplicateShortName, _userContext.LanguageId));

        foreach (var counterparty in dto.Counterparties)
        {
            if (await _query.AnyAsync(x => x.ShortName == counterparty.ShortName, ct))
                return Result.Failure<List<CounterpartyCardCreateResultDto>>(CounterpartyCardErrors.ShortNameConflict(counterparty.ShortName, _userContext.LanguageId));
        }

        var entities = dto.Counterparties.Select(counterparty => BuildCreateEntity(counterparty, orgId)).ToList();

        await _command.CreateAsync(entities, ct);

        return entities.Select(ToCreateResult).ToList();
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<CounterpartyCard>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity == null) 
            return Result.Failure(CounterpartyCardErrors.NotFound(id, _userContext.LanguageId));

        entity.StateId = StateIdConst.PASSIVE;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<CounterpartyCardListDto>>> GetAllAsync(CounterpartyCardListFilter filter, CancellationToken ct = default)
    {
        var query = _queryBuilder.BuildPaged<CounterpartyCard, CounterpartyCardListDto, CounterpartyCardListFilter>(filter);
        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<CounterpartyCardDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<CounterpartyCard>().Where(x => x.Id == id).As<CounterpartyCardDto>().Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure<CounterpartyCardDto>(CounterpartyCardErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result> UpdateAsync(int id, CounterpartyCardUpdateDto dto, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<CounterpartyCard>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) return Result.Failure(CounterpartyCardErrors.NotFound(id, _userContext.LanguageId));

        if (entity.ShortName != dto.ShortName && await _query.AnyAsync(x => x.ShortName == dto.ShortName, ct))
            return Result.Failure(CounterpartyCardErrors.ShortNameConflict(dto.ShortName, _userContext.LanguageId));
        entity.CounterpartyTypeId = dto.CounterpartyTypeId;
        entity.ShortName = dto.ShortName;
        entity.FullName = dto.FullName;
        entity.Inn = dto.Inn;
        entity.PhoneNumber = dto.PhoneNumber;
        entity.Email = dto.Email;
        entity.RegionId = dto.RegionId;
        entity.DistrictId = dto.DistrictId;
        entity.Address = dto.Address;
        entity.StateId = dto.StateId;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    private static CounterpartyCard BuildCreateEntity(CounterpartyCardCreateDto dto, int orgId) =>
        new()
        {
            OrganizationId = orgId,
            CounterpartyTypeId = dto.CounterpartyTypeId,
            ShortName = dto.ShortName,
            FullName = dto.FullName,
            Inn = dto.Inn,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email,
            RegionId = dto.RegionId,
            DistrictId = dto.DistrictId,
            Address = dto.Address,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Now
        };

    private static CounterpartyCardCreateResultDto ToCreateResult(CounterpartyCard entity) =>
        new()
        {
            Id = entity.Id,
            Inn = entity.Inn,
            ShortName = entity.ShortName
        };
}
