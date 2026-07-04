using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Cmn.Currencies;

public sealed class CurrencyService : ICurrencyService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<Currency> _query;
    private readonly ICommandRepository<Currency> _command;

    public CurrencyService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IQueryRepository<Currency> query,
        ICommandRepository<Currency> command)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _query = query;
        _command = command;
    }

    public async Task<Result<PagedResponse<CurrencyListDto>>> GetAllAsync(CurrencyListFilter filter, CancellationToken ct = default)
    {
        var query = _queryBuilder.BuildPaged<Currency, CurrencyListDto, CurrencyListFilter>(filter);
        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<CurrencyDto>> GetByIdAsync(short id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<Currency>()
            .Where(x => x.Id == id)
            .As<CurrencyDto>()
            .Build();

        var entity = await _query.GetAsync(query, ct);
        return entity == null
            ? Result.Failure<CurrencyDto>(CurrencyErrors.NotFound(id, _userContext.LanguageId))
            : Result.Success(entity);
    }

    public async Task<Result<short>> CreateAsync(CurrencyCreateDto dto, CancellationToken ct = default)
    {
        if (await _query.AnyAsync(x => x.Code == dto.Code, ct))
            return Result.Failure<short>(CurrencyErrors.CodeConflict(dto.Code, _userContext.LanguageId));

        var entity = new Currency
        {
            Code = dto.Code,
            Name = dto.Name,
            Symbol = dto.Symbol,
            StateId = StateIdConst.ACTIVE,
        };

        await _command.CreateAsync(entity, ct);
        return Result.Success(entity.Id);
    }

    public async Task<Result> UpdateAsync(short id, CurrencyUpdateDto dto, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<Currency>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity == null)
            return Result.Failure(CurrencyErrors.NotFound(id, _userContext.LanguageId));

        if (entity.Code != dto.Code && await _query.AnyAsync(x => x.Code == dto.Code, ct))
            return Result.Failure(CurrencyErrors.CodeConflict(dto.Code, _userContext.LanguageId));

        entity.Code = dto.Code;
        entity.Name = dto.Name;
        entity.Symbol = dto.Symbol;
        entity.StateId = dto.StateId;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(short id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<Currency>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity == null)
            return Result.Failure(CurrencyErrors.NotFound(id, _userContext.LanguageId));

        entity.StateId = StateIdConst.PASSIVE;
        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }
}
