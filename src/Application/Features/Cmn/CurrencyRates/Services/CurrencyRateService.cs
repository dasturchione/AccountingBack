using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Filters;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Cmn.CurrencyRates;

public sealed class CurrencyRateService : ICurrencyRateService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<CurrencyRate> _query;
    private readonly ICommandRepository<CurrencyRate> _command;
    private readonly IQueryRepository<Currency> _currencyQuery;
    private readonly ICriteriaBuilder<CurrencyRateListDto, CurrencyRateListFilter> _listCriteriaBuilder;

    public CurrencyRateService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IQueryRepository<CurrencyRate> query,
        ICommandRepository<CurrencyRate> command,
        IQueryRepository<Currency> currencyQuery,
        ICriteriaBuilder<CurrencyRateListDto, CurrencyRateListFilter> listCriteriaBuilder)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _query = query;
        _command = command;
        _currencyQuery = currencyQuery;
        _listCriteriaBuilder = listCriteriaBuilder;
    }

    public async Task<Result<PagedResponse<CurrencyRateListDto>>> GetAllAsync(CurrencyRateListFilter filter, CancellationToken ct = default)
    {
        var page = filter.Page > 0 ? filter.Page : 1;
        var pageSize = filter.PageSize is > 0 ? filter.PageSize.Value : 50;
        var query = _queryBuilder.For<CurrencyRate>()
            .As<CurrencyRateListDto>()
            .Where(_listCriteriaBuilder.Build(filter))
            .OrderBy(BuildOrder(filter))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .BuildPaged();
        var pagedList = await _query.GetPagedAsync(query, ct);
        return Result.Success(PagedResponseFactory.Create(pagedList, page, pageSize));
    }

    public async Task<Result<CurrencyRateDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<CurrencyRate>()
            .Where(x => x.Id == id)
            .As<CurrencyRateDto>()
            .Build();

        var entity = await _query.GetAsync(query, ct);
        return entity is null ? Result.Failure<CurrencyRateDto>(CurrencyRateErrors.NotFound(id, _userContext.LanguageId ?? 0)) : Result.Success(entity);
    }

    public async Task<Result<CurrencyRateDto>> GetLatestAsync(short baseCurrencyId, short targetCurrencyId, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<CurrencyRate>()
            .Where(x => x.BaseCurrencyId == baseCurrencyId && x.TargetCurrencyId == targetCurrencyId && x.StateId == StateIdConst.ACTIVE && x.IsActive)
            .As<CurrencyRateDto>()
            .OrderBy(items => items
                .OrderByDescending(x => x.EffectiveDate)
                .ThenByDescending(x => x.Id))
            .Build();

        var entity = await _query.GetAsync(query, ct);
        return entity is not null
            ? Result.Success(entity)
            : Result.Failure<CurrencyRateDto>(CurrencyRateErrors.LatestNotFound(baseCurrencyId, targetCurrencyId, _userContext.LanguageId ?? 0));
    }

    public async Task<Result<PagedResponse<CurrencyRateListDto>>> GetHistoryAsync(short baseCurrencyId, short targetCurrencyId, CurrencyRateListFilter filter, CancellationToken ct = default)
    {
        filter.BaseCurrencyId = baseCurrencyId;
        filter.TargetCurrencyId = targetCurrencyId;
        return await GetAllAsync(filter, ct);
    }

    public async Task<Result<long>> CreateAsync(CurrencyRateCreateDto dto, CancellationToken ct = default)
    {
        var validation = await ValidateRateAsync(dto.BaseCurrencyId, dto.TargetCurrencyId, dto.EffectiveDate, dto.IsActive, null, ct);
        if (!validation.IsSuccess)
            return Result.Failure<long>(validation.Error);

        var entity = new CurrencyRate
        {
            BaseCurrencyId = dto.BaseCurrencyId,
            TargetCurrencyId = dto.TargetCurrencyId,
            EffectiveDate = dto.EffectiveDate,
            BuyRate = dto.BuyRate,
            SellRate = dto.SellRate,
            OfficialRate = dto.OfficialRate,
            RateSource = dto.RateSource,
            IsActive = dto.IsActive,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Now
        };

        await _command.CreateAsync(entity, ct);
        return entity.Id;
    }

    public async Task<Result> UpdateAsync(long id, CurrencyRateUpdateDto dto, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<CurrencyRate>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity is null)
            return Result.Failure(CurrencyRateErrors.NotFound(id, _userContext.LanguageId ?? 0));

        var validation = await ValidateRateAsync(dto.BaseCurrencyId, dto.TargetCurrencyId, dto.EffectiveDate, dto.IsActive, id, ct);
        if (!validation.IsSuccess)
            return validation;

        entity.BaseCurrencyId = dto.BaseCurrencyId;
        entity.TargetCurrencyId = dto.TargetCurrencyId;
        entity.EffectiveDate = dto.EffectiveDate;
        entity.BuyRate = dto.BuyRate;
        entity.SellRate = dto.SellRate;
        entity.OfficialRate = dto.OfficialRate;
        entity.RateSource = dto.RateSource;
        entity.IsActive = dto.IsActive;
        entity.StateId = dto.StateId;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    private async Task<Result> ValidateRateAsync(short baseCurrencyId, short targetCurrencyId, DateTime effectiveDate, bool isActive, long? currentId, CancellationToken ct)
    {
        if (baseCurrencyId == targetCurrencyId)
            return Result.Failure(CurrencyRateErrors.SameCurrency(_userContext.LanguageId ?? 0));

        if (effectiveDate.Date > DateTime.Today)
            return Result.Failure(CurrencyRateErrors.FutureEffectiveDate(_userContext.LanguageId ?? 0));

        if (baseCurrencyId != CurrencyIdConst.UZS && !await IsActiveCurrencyAsync(baseCurrencyId, ct))
            return Result.Failure(CurrencyRateErrors.InactiveCurrency(baseCurrencyId, _userContext.LanguageId ?? 0));

        if (!await IsActiveCurrencyAsync(targetCurrencyId, ct))
            return Result.Failure(CurrencyRateErrors.InactiveCurrency(targetCurrencyId, _userContext.LanguageId ?? 0));

        if (!await CurrencyExistsAsync(baseCurrencyId, ct))
            return Result.Failure(CurrencyRateErrors.MissingCurrency(baseCurrencyId, _userContext.LanguageId ?? 0));

        if (!await CurrencyExistsAsync(targetCurrencyId, ct))
            return Result.Failure(CurrencyRateErrors.MissingCurrency(targetCurrencyId, _userContext.LanguageId ?? 0));

        var duplicateQuery = _queryBuilder.For<CurrencyRate>()
            .Where(x => x.BaseCurrencyId == baseCurrencyId &&
                        x.TargetCurrencyId == targetCurrencyId &&
                        x.EffectiveDate == effectiveDate &&
                        x.StateId == StateIdConst.ACTIVE &&
                        x.IsActive)
            .Build();

        if (await _query.AnyAsync(x => x.BaseCurrencyId == baseCurrencyId &&
                                      x.TargetCurrencyId == targetCurrencyId &&
                                      x.EffectiveDate == effectiveDate &&
                                      x.StateId == StateIdConst.ACTIVE &&
                                      x.IsActive &&
                                      (!currentId.HasValue || x.Id != currentId.Value), ct))
            return Result.Failure(CurrencyRateErrors.DuplicateEffectiveDate(_userContext.LanguageId ?? 0));

        var overlapQuery = _queryBuilder.For<CurrencyRate>()
            .Where(x => x.BaseCurrencyId == baseCurrencyId &&
                        x.TargetCurrencyId == targetCurrencyId &&
                        x.StateId == StateIdConst.ACTIVE &&
                        x.IsActive &&
                        x.EffectiveDate <= effectiveDate &&
                        (!currentId.HasValue || x.Id != currentId.Value))
            .Build();

        var existingRates = await _query.GetAllAsync(overlapQuery, ct);
        if (existingRates.Any(x => x.EffectiveDate.Date == effectiveDate.Date))
            return Result.Failure(CurrencyRateErrors.DuplicateEffectiveDate(_userContext.LanguageId ?? 0));

        if (isActive && await HasOtherActiveRateAsync(baseCurrencyId, targetCurrencyId, effectiveDate, currentId, ct))
            return Result.Failure(CurrencyRateErrors.DuplicateEffectiveDate(_userContext.LanguageId ?? 0));

        return Result.Success();
    }

    private async Task<bool> HasOtherActiveRateAsync(short baseCurrencyId, short targetCurrencyId, DateTime effectiveDate, long? currentId, CancellationToken ct)
        => await _query.AnyAsync(x => x.BaseCurrencyId == baseCurrencyId &&
                                      x.TargetCurrencyId == targetCurrencyId &&
                                      x.EffectiveDate == effectiveDate &&
                                      x.StateId == StateIdConst.ACTIVE &&
                                      x.IsActive &&
                                      (!currentId.HasValue || x.Id != currentId.Value), ct);

    private async Task<bool> CurrencyExistsAsync(short currencyId, CancellationToken ct)
    {
        return await _currencyQuery.AnyAsync(x => x.Id == currencyId, ct);
    }

    private async Task<bool> IsActiveCurrencyAsync(short currencyId, CancellationToken ct)
    {
        return await _currencyQuery.AnyAsync(x => x.Id == currencyId && x.StateId == StateIdConst.ACTIVE, ct);
    }

    public async Task<Result> DeleteAsync(long id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<CurrencyRate>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity is null)
            return Result.Failure(CurrencyRateErrors.NotFound(id, _userContext.LanguageId ?? 0));

        entity.StateId = StateIdConst.PASSIVE;
        entity.IsActive = false;
        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    private static Func<IQueryable<CurrencyRateListDto>, IOrderedQueryable<CurrencyRateListDto>> BuildOrder(
        CurrencyRateListFilter filter)
    {
        return (filter.SortBy?.Trim().ToLowerInvariant(), filter.SortDirection) switch
        {
            ("basecurrencycode", SortDirection.Asc) => query => query.OrderBy(x => x.BaseCurrencyCode).ThenByDescending(x => x.EffectiveDate).ThenByDescending(x => x.Id),
            ("basecurrencycode", SortDirection.Desc) => query => query.OrderByDescending(x => x.BaseCurrencyCode).ThenByDescending(x => x.EffectiveDate).ThenByDescending(x => x.Id),
            ("targetcurrencycode", SortDirection.Asc) => query => query.OrderBy(x => x.TargetCurrencyCode).ThenByDescending(x => x.EffectiveDate).ThenByDescending(x => x.Id),
            ("targetcurrencycode", SortDirection.Desc) => query => query.OrderByDescending(x => x.TargetCurrencyCode).ThenByDescending(x => x.EffectiveDate).ThenByDescending(x => x.Id),
            ("effectivedate", SortDirection.Asc) => query => query.OrderBy(x => x.EffectiveDate).ThenByDescending(x => x.Id),
            ("effectivedate", SortDirection.Desc) => query => query.OrderByDescending(x => x.EffectiveDate).ThenByDescending(x => x.Id),
            ("buyrate", SortDirection.Asc) => query => query.OrderBy(x => x.BuyRate).ThenByDescending(x => x.EffectiveDate).ThenByDescending(x => x.Id),
            ("buyrate", SortDirection.Desc) => query => query.OrderByDescending(x => x.BuyRate).ThenByDescending(x => x.EffectiveDate).ThenByDescending(x => x.Id),
            ("sellrate", SortDirection.Asc) => query => query.OrderBy(x => x.SellRate).ThenByDescending(x => x.EffectiveDate).ThenByDescending(x => x.Id),
            ("sellrate", SortDirection.Desc) => query => query.OrderByDescending(x => x.SellRate).ThenByDescending(x => x.EffectiveDate).ThenByDescending(x => x.Id),
            ("officialrate", SortDirection.Asc) => query => query.OrderBy(x => x.OfficialRate).ThenByDescending(x => x.EffectiveDate).ThenByDescending(x => x.Id),
            ("officialrate", SortDirection.Desc) => query => query.OrderByDescending(x => x.OfficialRate).ThenByDescending(x => x.EffectiveDate).ThenByDescending(x => x.Id),
            _ when filter.SortDirection == SortDirection.Asc => query => query.OrderBy(x => x.EffectiveDate).ThenByDescending(x => x.Id),
            _ => query => query.OrderByDescending(x => x.EffectiveDate).ThenByDescending(x => x.Id)
        };
    }
}
