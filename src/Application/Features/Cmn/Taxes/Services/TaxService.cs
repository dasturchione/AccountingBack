using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Cmn.Taxes;

public sealed class TaxService : ITaxService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<VatRate> _query;
    private readonly ICommandRepository<VatRate> _command;

    public TaxService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IQueryRepository<VatRate> query,
        ICommandRepository<VatRate> command)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _query = query;
        _command = command;
    }

    public async Task<Result<PagedResponse<TaxListDto>>> GetPagedAsync(TaxListFilter filter, CancellationToken ct = default)
    {
        var query = _queryBuilder.BuildPaged<VatRate, TaxListDto, TaxListFilter>(filter);
        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<TaxDto>> GetByIdAsync(short id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<VatRate>()
            .Where(x => x.Id == id)
            .As<TaxDto>()
            .Build();

        var entity = await _query.GetAsync(query, ct);
        return entity is null
            ? Result.Failure<TaxDto>(TaxErrors.NotFound(id, _userContext.LanguageId))
            : Result.Success(entity);
    }

    public async Task<Result<short>> CreateAsync(TaxCreateDto dto, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is null)
            return Result.Failure<short>(TaxErrors.OrganizationRequired(_userContext.LanguageId));

        var validation = await ValidateAsync(dto.Code, dto.Name, dto.Rate, dto.EffectiveFrom, dto.EffectiveTo, null, ct);
        if (!validation.IsSuccess)
            return Result.Failure<short>(validation.Error);

        var entity = new VatRate
        {
            Code = dto.Code,
            Name = dto.Name,
            Rate = dto.Rate,
            EffectiveFrom = dto.EffectiveFrom,
            EffectiveTo = dto.EffectiveTo,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Now
        };

        await _command.CreateAsync(entity, ct);
        return Result.Success(entity.Id);
    }

    public async Task<Result> UpdateAsync(short id, TaxUpdateDto dto, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is null)
            return Result.Failure(TaxErrors.OrganizationRequired(_userContext.LanguageId));

        var query = _queryBuilder.For<VatRate>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity is null)
            return Result.Failure(TaxErrors.NotFound(id, _userContext.LanguageId));

        var validation = await ValidateAsync(dto.Code, dto.Name, dto.Rate, dto.EffectiveFrom, dto.EffectiveTo, id, ct);
        if (!validation.IsSuccess)
            return validation;

        if (dto.StateId != StateIdConst.ACTIVE && dto.StateId != StateIdConst.PASSIVE)
            return Result.Failure(TaxErrors.InvalidState(_userContext.LanguageId));

        entity.Code = dto.Code;
        entity.Name = dto.Name;
        entity.Rate = dto.Rate;
        entity.EffectiveFrom = dto.EffectiveFrom;
        entity.EffectiveTo = dto.EffectiveTo;
        entity.StateId = dto.StateId;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(short id, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is null)
            return Result.Failure(TaxErrors.OrganizationRequired(_userContext.LanguageId));

        var query = _queryBuilder.For<VatRate>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity is null)
            return Result.Failure(TaxErrors.NotFound(id, _userContext.LanguageId));

        entity.StateId = StateIdConst.PASSIVE;
        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    private async Task<Result> ValidateAsync(string code, string name, decimal rate, DateOnly? effectiveFrom, DateOnly? effectiveTo, short? currentId, CancellationToken ct)
    {
        if (rate <= 0 || rate > 100)
            return Result.Failure(TaxErrors.InvalidRate(_userContext.LanguageId));

        if (effectiveFrom is null)
            return Result.Failure(TaxErrors.InvalidRate(_userContext.LanguageId));

        if (effectiveTo.HasValue && effectiveTo.Value < effectiveFrom.Value)
            return Result.Failure(TaxErrors.InvalidRate(_userContext.LanguageId));

        if (await _query.AnyAsync(x => x.Code == code && (!currentId.HasValue || x.Id != currentId.Value), ct))
            return Result.Failure(TaxErrors.CodeConflict(code, _userContext.LanguageId));

        if (await _query.AnyAsync(x => x.Name == name && (!currentId.HasValue || x.Id != currentId.Value), ct))
            return Result.Failure(TaxErrors.NameConflict(name, _userContext.LanguageId));

        return Result.Success();
    }
}
