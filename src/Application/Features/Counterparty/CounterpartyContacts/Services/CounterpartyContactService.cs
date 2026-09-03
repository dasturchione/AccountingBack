using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.CounterpartyContacts;

public class CounterpartyContactService : ICounterpartyContactService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<CounterpartyContact> _query;
    private readonly ICommandRepository<CounterpartyContact> _command;
    private readonly IQueryRepository<CounterpartyCard> _counterpartyQuery;

    public CounterpartyContactService(IUserContext userContext,
                                      IQueryBuilder queryBuilder, 
                                      IQueryRepository<CounterpartyContact> query,
                                      ICommandRepository<CounterpartyContact> command,
                                      IQueryRepository<CounterpartyCard> counterpartyQuery)
    {
        _query = query;
        _command = command;
        _counterpartyQuery = counterpartyQuery;
        _userContext = userContext; 
        _queryBuilder = queryBuilder;
    }

    public async Task<Result<int>> CreateAsync(CounterpartyContactCreateDto dto, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is not int organizationId || organizationId <= 0)
            return Result.Failure<int>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        if (!await CounterpartyBelongsToOrganizationAsync(dto.CounterpartyId, organizationId, ct))
            return Result.Failure<int>(
                CounterpartyContactErrors.CounterpartyNotFound(dto.CounterpartyId, _userContext.LanguageId));

        var entity = new CounterpartyContact
        {
            OrganizationId = organizationId,
            CounterpartyId = dto.CounterpartyId,
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email,
            Position = dto.Position,
            Comment = dto.Comment,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Now
        };
        await _command.CreateAsync(entity, ct);
        return entity.Id;
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is not int organizationId || organizationId <= 0)
            return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var query = _queryBuilder.For<CounterpartyContact>()
            .Where(x => x.Id == id && x.OrganizationId == organizationId)
            .Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity == null) 
            return Result.Failure(CounterpartyContactErrors.NotFound(id, _userContext.LanguageId));

        entity.StateId = StateIdConst.PASSIVE;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<CounterpartyContactListDto>>> GetAllAsync(CounterpartyContactListFilter filter, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is not int organizationId || organizationId <= 0)
            return Result.Failure<PagedResponse<CounterpartyContactListDto>>(
                CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        filter.OrganizationId = organizationId;
        var query = _queryBuilder.BuildPaged<CounterpartyContact, CounterpartyContactListDto, CounterpartyContactListFilter>(filter);
        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<CounterpartyContactDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is not int organizationId || organizationId <= 0)
            return Result.Failure<CounterpartyContactDto>(
                CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var query = _queryBuilder.For<CounterpartyContact>()
            .Where(x => x.Id == id && x.OrganizationId == organizationId)
            .As<CounterpartyContactDto>()
            .Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure<CounterpartyContactDto>(CounterpartyContactErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result> UpdateAsync(int id, CounterpartyContactUpdateDto dto, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is not int organizationId || organizationId <= 0)
            return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var query = _queryBuilder.For<CounterpartyContact>()
            .Where(x => x.Id == id && x.OrganizationId == organizationId)
            .Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity == null) 
            return Result.Failure(CounterpartyContactErrors.NotFound(id, _userContext.LanguageId));

        if (!await CounterpartyBelongsToOrganizationAsync(dto.CounterpartyId, organizationId, ct))
            return Result.Failure(
                CounterpartyContactErrors.CounterpartyNotFound(dto.CounterpartyId, _userContext.LanguageId));

        entity.CounterpartyId = dto.CounterpartyId;
        entity.FullName = dto.FullName;
        entity.PhoneNumber = dto.PhoneNumber;
        entity.Email = dto.Email;
        entity.Position = dto.Position;
        entity.Comment = dto.Comment;
        entity.StateId = dto.StateId;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    private Task<bool> CounterpartyBelongsToOrganizationAsync(
        int counterpartyId,
        int organizationId,
        CancellationToken ct) =>
        _counterpartyQuery.AnyAsync(
            counterparty => counterparty.Id == counterpartyId &&
                            counterparty.OrganizationId == organizationId,
            ct);
}
