using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Banks;

public class BankService : IBankService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<Bank> _query;
    private readonly ICommandRepository<Bank> _command;

    public BankService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IQueryRepository<Bank> query,
        ICommandRepository<Bank> command)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _query = query;
        _command = command;
    }

    public async Task<Result<int>> CreateAsync(BankCreateDto dto, CancellationToken ct = default)
    {
        if (await _query.AnyAsync(x => x.Code == dto.Code, ct))
            return Result.Failure<int>(BankErrors.CodeConflict(dto.Code, _userContext.LanguageId));

        var entity = new Bank
        {
            Code = dto.Code,
            Name = dto.Name,
            Mfo = dto.Mfo,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Now
        };

        await _command.CreateAsync(entity, ct);
        return entity.Id;
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<Bank>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity == null)
            return Result.Failure(BankErrors.NotFound(id, _userContext.LanguageId));

        entity.StateId = StateIdConst.PASSIVE;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<BankListDto>>> GetAllAsync(BankListFilter filter, CancellationToken ct = default)
    {
        var query = _queryBuilder.BuildPaged<Bank, BankListDto, BankListFilter>(filter);
        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<BankDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<Bank>().Where(x => x.Id == id).As<BankDto>().Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity == null)
            return Result.Failure<BankDto>(BankErrors.NotFound(id, _userContext.LanguageId));

        return entity;
    }

    public async Task<Result> UpdateAsync(int id, BankUpdateDto dto, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<Bank>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity == null)
            return Result.Failure(BankErrors.NotFound(id, _userContext.LanguageId));

        if (entity.Code != dto.Code && await _query.AnyAsync(x => x.Code == dto.Code, ct))
            return Result.Failure(BankErrors.CodeConflict(dto.Code, _userContext.LanguageId));

        entity.Code = dto.Code;
        entity.Name = dto.Name;
        entity.Mfo = dto.Mfo;
        entity.StateId = dto.StateId;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }
}
