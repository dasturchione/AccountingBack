using Application.Abstractions;
using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;
using System.Linq.Expressions;

namespace Application.Features.RegulatedObligationSettings;

public sealed class RegulatedObligationSettingService : IRegulatedObligationSettingService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<RegulatedObligation> _obligationQuery;
    private readonly IQueryRepository<OrganizationRegulatedObligationSetting> _settingQuery;
    private readonly IQueryRepository<RegulatedObligationPeriodicity> _periodicityQuery;
    private readonly IQueryRepository<ChartAccount> _chartAccountQuery;
    private readonly ICommandRepository<OrganizationRegulatedObligationSetting> _settingCommand;

    public RegulatedObligationSettingService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IQueryRepository<RegulatedObligation> obligationQuery,
        IQueryRepository<OrganizationRegulatedObligationSetting> settingQuery,
        IQueryRepository<RegulatedObligationPeriodicity> periodicityQuery,
        IQueryRepository<ChartAccount> chartAccountQuery,
        ICommandRepository<OrganizationRegulatedObligationSetting> settingCommand)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _obligationQuery = obligationQuery;
        _settingQuery = settingQuery;
        _periodicityQuery = periodicityQuery;
        _chartAccountQuery = chartAccountQuery;
        _settingCommand = settingCommand;
    }

    public async Task<Result<List<RegulatedObligationSettingDto>>> GetAllAsync(
        RegulatedObligationSettingListFilter filter,
        CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is not int organizationId)
            return Result.Failure<List<RegulatedObligationSettingDto>>(
                RegulatedObligationSettingErrors.OrganizationRequired(_userContext.LanguageId));

        var languageId = _userContext.LanguageId ?? LanguageIdConst.UZ;
        var choosedDate = filter.ChoosedDate ?? DateOnly.FromDateTime(DateTime.Today);
        var categoryCode = NormalizeCode(filter.CategoryCode);
        var search = NormalizeSearch(filter.Search);

        var obligations = await _obligationQuery.GetAllAsync(
            _queryBuilder.For<RegulatedObligation>()
                .Where(x => x.StateId == StateIdConst.ACTIVE
                            && x.Category.StateId == StateIdConst.ACTIVE
                            && (categoryCode == null || x.Category.Code == categoryCode))
                .As(ToObligationRow(languageId))
                .Build(),
            ct);

        var settings = await _settingQuery.GetAllAsync(
            _queryBuilder.For<OrganizationRegulatedObligationSetting>()
                .Where(x => x.OrganizationId == organizationId)
                .As(ToSettingRow(languageId))
                .Build(),
            ct);

        var settingByObligation = settings
            .Where(x => RegulatedObligationSettingPeriodPolicy.IsEffectiveOn(
                x.EffectiveFrom,
                x.EffectiveTo,
                choosedDate))
            .GroupBy(x => x.RegulatedObligationId)
            .ToDictionary(x => x.Key, x => x.OrderByDescending(y => y.EffectiveFrom).ThenByDescending(y => y.Id).First());

        var result = obligations
            .Select(obligation => BuildDto(
                obligation,
                settingByObligation.GetValueOrDefault(obligation.Id),
                organizationId))
            .Where(x => !filter.IsConfigured.HasValue || (x.SettingId.HasValue == filter.IsConfigured.Value))
            .Where(x => search == null
                        || x.Code.Contains(search, StringComparison.OrdinalIgnoreCase)
                        || x.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                        || x.CategoryName.Contains(search, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Code)
            .ToList();

        return Result.Success(result);
    }

    public async Task<Result<RegulatedObligationSettingDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is not int organizationId)
            return Result.Failure<RegulatedObligationSettingDto>(
                RegulatedObligationSettingErrors.OrganizationRequired(_userContext.LanguageId));

        var languageId = _userContext.LanguageId ?? LanguageIdConst.UZ;
        var setting = await _settingQuery.GetAsync(
            _queryBuilder.For<OrganizationRegulatedObligationSetting>()
                .Where(x => x.Id == id && x.OrganizationId == organizationId)
                .As(ToSettingRow(languageId))
                .Build(),
            ct);

        if (setting is null)
            return Result.Failure<RegulatedObligationSettingDto>(
                RegulatedObligationSettingErrors.NotFound(id, _userContext.LanguageId));

        var obligation = await GetObligationAsync(setting.RegulatedObligationId, languageId, ct);
        if (obligation is null)
            return Result.Failure<RegulatedObligationSettingDto>(
                RegulatedObligationSettingErrors.ObligationNotFound(setting.RegulatedObligationId, _userContext.LanguageId));

        return Result.Success(BuildDto(obligation, setting, organizationId));
    }

    public async Task<Result<RegulatedObligationSettingDto>> GetByCodeAsync(
        string code,
        DateOnly? choosedDate = null,
        CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is not int organizationId)
            return Result.Failure<RegulatedObligationSettingDto>(
                RegulatedObligationSettingErrors.OrganizationRequired(_userContext.LanguageId));

        var normalizedCode = NormalizeCode(code) ?? string.Empty;
        var languageId = _userContext.LanguageId ?? LanguageIdConst.UZ;
        var obligation = await _obligationQuery.GetAsync(
            _queryBuilder.For<RegulatedObligation>()
                .Where(x => x.Code == normalizedCode && x.StateId == StateIdConst.ACTIVE)
                .As(ToObligationRow(languageId))
                .Build(),
            ct);

        if (obligation is null)
            return Result.Failure<RegulatedObligationSettingDto>(
                RegulatedObligationSettingErrors.ObligationNotFound(normalizedCode, _userContext.LanguageId));

        var effectiveDate = choosedDate ?? DateOnly.FromDateTime(DateTime.Today);
        var settings = await _settingQuery.GetAllAsync(
            _queryBuilder.For<OrganizationRegulatedObligationSetting>()
                .Where(x => x.OrganizationId == organizationId
                            && x.RegulatedObligationId == obligation.Id)
                .As(ToSettingRow(languageId))
                .Build(),
            ct);
        var setting = settings
            .Where(x => RegulatedObligationSettingPeriodPolicy.IsEffectiveOn(
                x.EffectiveFrom,
                x.EffectiveTo,
                effectiveDate))
            .OrderByDescending(x => x.EffectiveFrom)
            .ThenByDescending(x => x.Id)
            .FirstOrDefault();

        return Result.Success(BuildDto(obligation, setting, organizationId));
    }

    public async Task<Result<int>> CreateAsync(RegulatedObligationSettingCreateDto dto, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is not int organizationId)
            return Result.Failure<int>(RegulatedObligationSettingErrors.OrganizationRequired(_userContext.LanguageId));

        var validation = await ValidateAsync(dto, organizationId, null, ct);
        if (!validation.IsSuccess)
            return Result.Failure<int>(validation.Error);

        var entity = new OrganizationRegulatedObligationSetting
        {
            OrganizationId = organizationId,
            RegulatedObligationId = dto.RegulatedObligationId,
            PeriodicityId = dto.PeriodicityId,
            ClassifierCode = NormalizeOptional(dto.ClassifierCode),
            Rate = dto.Rate,
            ChartAccountId = dto.ChartAccountId,
            EffectiveFrom = dto.EffectiveFrom,
            EffectiveTo = dto.EffectiveTo,
            StateId = dto.StateId,
            CreatedDate = DateTime.Now
        };

        await _settingCommand.CreateAsync(entity, ct);
        return Result.Success(entity.Id);
    }

    public async Task<Result> UpdateAsync(
        int id,
        RegulatedObligationSettingUpdateDto dto,
        CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is not int organizationId)
            return Result.Failure(RegulatedObligationSettingErrors.OrganizationRequired(_userContext.LanguageId));

        var entity = await _settingQuery.GetAsync(
            _queryBuilder.For<OrganizationRegulatedObligationSetting>()
                .Where(x => x.Id == id && x.OrganizationId == organizationId)
                .Build(),
            ct);
        if (entity is null)
            return Result.Failure(RegulatedObligationSettingErrors.NotFound(id, _userContext.LanguageId));

        var validation = await ValidateAsync(dto, organizationId, id, ct);
        if (!validation.IsSuccess)
            return validation;

        entity.RegulatedObligationId = dto.RegulatedObligationId;
        entity.PeriodicityId = dto.PeriodicityId;
        entity.ClassifierCode = NormalizeOptional(dto.ClassifierCode);
        entity.Rate = dto.Rate;
        entity.ChartAccountId = dto.ChartAccountId;
        entity.EffectiveFrom = dto.EffectiveFrom;
        entity.EffectiveTo = dto.EffectiveTo;
        entity.StateId = dto.StateId;
        entity.UpdatedDate = DateTime.Now;

        await _settingCommand.UpdateAsync(entity, ct);
        return Result.Success();
    }

    private async Task<Result> ValidateAsync(
        RegulatedObligationSettingBaseDto dto,
        int organizationId,
        int? excludedId,
        CancellationToken ct)
    {
        if (dto.EffectiveTo.HasValue && dto.EffectiveTo.Value < dto.EffectiveFrom)
            return Result.Failure(RegulatedObligationSettingErrors.InvalidPeriod(_userContext.LanguageId));

        if (dto.Rate is < 0 or > 100)
            return Result.Failure(RegulatedObligationSettingErrors.InvalidRate(_userContext.LanguageId));

        if (!await _obligationQuery.AnyAsync(
                x => x.Id == dto.RegulatedObligationId && x.StateId == StateIdConst.ACTIVE,
                ct))
        {
            return Result.Failure(RegulatedObligationSettingErrors.ObligationNotFound(
                dto.RegulatedObligationId,
                _userContext.LanguageId));
        }

        if (!await _periodicityQuery.AnyAsync(
                x => x.Id == dto.PeriodicityId && x.StateId == StateIdConst.ACTIVE,
                ct))
        {
            return Result.Failure(RegulatedObligationSettingErrors.PeriodicityNotFound(
                dto.PeriodicityId,
                _userContext.LanguageId));
        }

        if (!await _chartAccountQuery.AnyAsync(
                x => x.Id == dto.ChartAccountId
                     && x.OrganizationId == organizationId
                     && x.StateId == StateIdConst.ACTIVE,
                ct))
        {
            return Result.Failure(RegulatedObligationSettingErrors.ChartAccountNotFound(
                dto.ChartAccountId,
                _userContext.LanguageId));
        }

        var existingPeriods = await _settingQuery.GetAllAsync(
            _queryBuilder.For<OrganizationRegulatedObligationSetting>()
                .Where(x => x.OrganizationId == organizationId
                            && x.RegulatedObligationId == dto.RegulatedObligationId
                            && (!excludedId.HasValue || x.Id != excludedId.Value))
                .As(x => new SettingPeriodRow
                {
                    EffectiveFrom = x.EffectiveFrom,
                    EffectiveTo = x.EffectiveTo
                })
                .Build(),
            ct);
        var overlaps = existingPeriods.Any(x => RegulatedObligationSettingPeriodPolicy.Overlaps(
            x.EffectiveFrom,
            x.EffectiveTo,
            dto.EffectiveFrom,
            dto.EffectiveTo));
        if (overlaps)
            return Result.Failure(RegulatedObligationSettingErrors.PeriodOverlap(_userContext.LanguageId));

        return Result.Success();
    }

    private async Task<ObligationRow?> GetObligationAsync(short id, short languageId, CancellationToken ct) =>
        await _obligationQuery.GetAsync(
            _queryBuilder.For<RegulatedObligation>()
                .Where(x => x.Id == id)
                .As(ToObligationRow(languageId))
                .Build(),
            ct);

    private static Expression<Func<RegulatedObligation, ObligationRow>> ToObligationRow(short languageId) =>
        x => new ObligationRow
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.Translations
                .Where(t => t.LanguageId == languageId)
                .Select(t => t.Name)
                .FirstOrDefault() ?? x.Name,
            CategoryId = x.CategoryId,
            CategoryCode = x.Category.Code,
            CategoryName = x.Category.Translations
                .Where(t => t.LanguageId == languageId)
                .Select(t => t.Name)
                .FirstOrDefault() ?? x.Category.Name
        };

    private static Expression<Func<OrganizationRegulatedObligationSetting, SettingRow>> ToSettingRow(short languageId) =>
        x => new SettingRow
        {
            Id = x.Id,
            RegulatedObligationId = x.RegulatedObligationId,
            PeriodicityId = x.PeriodicityId,
            PeriodicityCode = x.Periodicity.Code,
            PeriodicityName = x.Periodicity.Translations
                .Where(t => t.LanguageId == languageId)
                .Select(t => t.Name)
                .FirstOrDefault() ?? x.Periodicity.Name,
            ClassifierCode = x.ClassifierCode,
            Rate = x.Rate,
            ChartAccountId = x.ChartAccountId,
            ChartAccountNumber = x.ChartAccount.Number,
            ChartAccountName = x.ChartAccount.Name,
            EffectiveFrom = x.EffectiveFrom,
            EffectiveTo = x.EffectiveTo,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate,
            UpdatedDate = x.UpdatedDate
        };

    private static RegulatedObligationSettingDto BuildDto(
        ObligationRow obligation,
        SettingRow? setting,
        int organizationId) =>
        new()
        {
            RegulatedObligationId = obligation.Id,
            Code = obligation.Code,
            Name = obligation.Name,
            CategoryId = obligation.CategoryId,
            CategoryCode = obligation.CategoryCode,
            CategoryName = obligation.CategoryName,
            SettingId = setting?.Id,
            OrganizationId = organizationId,
            PeriodicityId = setting?.PeriodicityId,
            PeriodicityCode = setting?.PeriodicityCode,
            PeriodicityName = setting?.PeriodicityName,
            ClassifierCode = setting?.ClassifierCode,
            Rate = setting?.Rate,
            ChartAccountId = setting?.ChartAccountId,
            ChartAccountNumber = setting?.ChartAccountNumber,
            ChartAccountName = setting?.ChartAccountName,
            EffectiveFrom = setting?.EffectiveFrom,
            EffectiveTo = setting?.EffectiveTo,
            StateId = setting?.StateId,
            StateName = setting?.StateName,
            CreatedDate = setting?.CreatedDate,
            UpdatedDate = setting?.UpdatedDate
        };

    private static string? NormalizeCode(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();

    private static string? NormalizeSearch(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private sealed class ObligationRow
    {
        public short Id { get; init; }
        public string Code { get; init; } = null!;
        public string Name { get; init; } = null!;
        public short CategoryId { get; init; }
        public string CategoryCode { get; init; } = null!;
        public string CategoryName { get; init; } = null!;
    }

    private sealed class SettingRow
    {
        public int Id { get; init; }
        public short RegulatedObligationId { get; init; }
        public short PeriodicityId { get; init; }
        public string PeriodicityCode { get; init; } = null!;
        public string PeriodicityName { get; init; } = null!;
        public string? ClassifierCode { get; init; }
        public decimal? Rate { get; init; }
        public int ChartAccountId { get; init; }
        public string ChartAccountNumber { get; init; } = null!;
        public string ChartAccountName { get; init; } = null!;
        public DateOnly EffectiveFrom { get; init; }
        public DateOnly? EffectiveTo { get; init; }
        public short StateId { get; init; }
        public string StateName { get; init; } = null!;
        public DateTime CreatedDate { get; init; }
        public DateTime? UpdatedDate { get; init; }
    }

    private sealed class SettingPeriodRow
    {
        public DateOnly EffectiveFrom { get; init; }
        public DateOnly? EffectiveTo { get; init; }
    }
}
