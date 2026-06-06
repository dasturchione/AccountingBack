using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Extensions;
using SharedKernel.Results;

namespace Application.Features.CounterpartyContacts;

public class CounterpartyContactService : ICounterpartyContactService
{
    private readonly IUserContext _userContext;
    private readonly IQueryRepository<CounterpartyContact> _query;
    private readonly ICommandRepository<CounterpartyContact> _command;
    private readonly IQueryBuilder<CounterpartyContact> _queryBuilder;

    public CounterpartyContactService(IUserContext userContext, IQueryRepository<CounterpartyContact> query,
        ICommandRepository<CounterpartyContact> command, IQueryBuilder<CounterpartyContact> queryBuilder)
    {
        _userContext = userContext; _query = query; _command = command; _queryBuilder = queryBuilder;
    }

    public async Task<Result<int>> CreateAsync(CounterpartyContactCreateDto dto, CancellationToken ct = default)
    {
        var entity = new CounterpartyContact
        {
            OrganizationId = dto.OrganizationId,
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
        var entity = await _query.GetAsync(_queryBuilder.ById(id), ct);
        if (entity == null) return Result.Failure(CounterpartyContactErrors.NotFound(id, _userContext.LanguageId));
        entity.StateId = StateIdConst.PASSIVE;
        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<CounterpartyContactListDto>>> GetAllAsync(CounterpartyContactListFilter filter, CancellationToken ct = default)
    {
        var pagedList = await _query.GetPagedAsync(_queryBuilder.BuildPaged<CounterpartyContactListDto, CounterpartyContactListFilter>(filter), ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<CounterpartyContactDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _query.GetAsync(_queryBuilder.ById<CounterpartyContact, CounterpartyContactDto>(id), ct);
        if (entity == null) return Result.Failure<CounterpartyContactDto>(CounterpartyContactErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result> UpdateAsync(int id, CounterpartyContactUpdateDto dto, CancellationToken ct = default)
    {
        var entity = await _query.GetAsync(_queryBuilder.ById(id), ct);
        if (entity == null) return Result.Failure(CounterpartyContactErrors.NotFound(id, _userContext.LanguageId));

        entity.OrganizationId = dto.OrganizationId;
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
}
