using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Contracts;

public class ContractService : IContractService
{
    private readonly IUserContext _userContext;
    private readonly IQueryRepository<Contract> _query;
    private readonly ICommandRepository<Contract> _command;
    private readonly IQueryBuilder _queryBuilder;

    public ContractService(IUserContext userContext,
                           IQueryBuilder queryBuilder,
                           IQueryRepository<Contract> query,
                           ICommandRepository<Contract> command)
    {
        _query = query;
        _command = command;
        _userContext = userContext;
        _queryBuilder = queryBuilder;
    }

    public async Task<Result<long>> CreateAsync(ContractCreateDto dto, CancellationToken ct = default)
    {
        var entity = new Contract
        {
            OrganizationId = dto.OrganizationId,
            CounterpartyId = dto.CounterpartyId,
            ContractType = dto.ContractType,
            ContractNumber = string.Empty,
            ContractDate = dto.ContractDate,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Comment = dto.Comment,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Now
        };
        await _command.CreateAsync(entity, ct);
        return entity.Id;
    }

    public async Task<Result> DeleteAsync(long id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<Contract>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity == null)
            return Result.Failure(ContractErrors.NotFound(id, _userContext.LanguageId));

        entity.StateId = StateIdConst.PASSIVE;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<ContractListDto>>> GetAllAsync(ContractListFilter filter, CancellationToken ct = default)
    {
        var query = _queryBuilder.BuildPaged<Contract, ContractListDto, ContractListFilter>(filter);
        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<ContractDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<Contract>().Where(x => x.Id == id).As<ContractDto>().Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null)
            return Result.Failure<ContractDto>(ContractErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result> UpdateAsync(long id, ContractUpdateDto dto, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<Contract>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null)
            return Result.Failure(ContractErrors.NotFound(id, _userContext.LanguageId));

        entity.OrganizationId = dto.OrganizationId;
        entity.CounterpartyId = dto.CounterpartyId;
        entity.ContractType = dto.ContractType;
        entity.ContractDate = dto.ContractDate;
        entity.StartDate = dto.StartDate;
        entity.EndDate = dto.EndDate;
        entity.Comment = dto.Comment;
        entity.StateId = dto.StateId;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }
}
