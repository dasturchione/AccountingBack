using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Extensions;
using SharedKernel.Results;

namespace Application.Features.CounterpartyCards;

public class CounterpartyCardService : ICounterpartyCardService
{
    private readonly IUserContext _userContext;
    private readonly IQueryRepository<CounterpartyCard> _query;
    private readonly ICommandRepository<CounterpartyCard> _command;
    private readonly IQueryBuilder<CounterpartyCard> _queryBuilder;

    public CounterpartyCardService(IUserContext userContext, IQueryRepository<CounterpartyCard> query,
        ICommandRepository<CounterpartyCard> command, IQueryBuilder<CounterpartyCard> queryBuilder)
    {
        _userContext = userContext; _query = query; _command = command; _queryBuilder = queryBuilder;
    }

    public async Task<Result<int>> CreateAsync(CounterpartyCardCreateDto dto, CancellationToken ct = default)
    {
        if (await _query.AnyAsync(x => x.OrganizationId == dto.OrganizationId && x.ShortName == dto.ShortName, ct))
            return Result.Failure<int>(CounterpartyCardErrors.ShortNameConflict(dto.ShortName, _userContext.LanguageId));

        var entity = new CounterpartyCard
        {
            OrganizationId = dto.OrganizationId,
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
        await _command.CreateAsync(entity, ct);
        return entity.Id;
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _query.GetAsync(_queryBuilder.ById(id), ct);
        if (entity == null) return Result.Failure(CounterpartyCardErrors.NotFound(id, _userContext.LanguageId));
        entity.StateId = StateIdConst.PASSIVE;
        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<CounterpartyCardListDto>>> GetAllAsync(CounterpartyCardListFilter filter, CancellationToken ct = default)
    {
        var pagedList = await _query.GetPagedAsync(_queryBuilder.BuildPaged<CounterpartyCardListDto, CounterpartyCardListFilter>(filter), ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<CounterpartyCardDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _query.GetAsync(_queryBuilder.ById<CounterpartyCard, CounterpartyCardDto>(id), ct);
        if (entity == null) return Result.Failure<CounterpartyCardDto>(CounterpartyCardErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result> UpdateAsync(int id, CounterpartyCardUpdateDto dto, CancellationToken ct = default)
    {
        var entity = await _query.GetAsync(_queryBuilder.ById(id), ct);
        if (entity == null) return Result.Failure(CounterpartyCardErrors.NotFound(id, _userContext.LanguageId));

        if (entity.ShortName != dto.ShortName && await _query.AnyAsync(x => x.OrganizationId == dto.OrganizationId && x.ShortName == dto.ShortName, ct))
            return Result.Failure(CounterpartyCardErrors.ShortNameConflict(dto.ShortName, _userContext.LanguageId));

        entity.OrganizationId = dto.OrganizationId;
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
}
