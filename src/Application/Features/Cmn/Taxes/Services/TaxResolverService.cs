using Application.Abstractions;
using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Cmn.Taxes;

public sealed class TaxResolverService : ITaxResolverService
{
    private readonly IUserContext _userContext;
    private readonly IQueryRepository<OrganizationRegulatedObligationSetting> _organizationSettingQuery;
    private readonly IQueryRepository<RegulatedObligation> _regulatedObligationQuery;
    private readonly IQueryRepository<VatRate> _vatRateQuery;
    private readonly IQueryBuilder _queryBuilder;

    public TaxResolverService(
        IUserContext userContext,
        IQueryRepository<OrganizationRegulatedObligationSetting> organizationSettingQuery,
        IQueryRepository<RegulatedObligation> regulatedObligationQuery,
        IQueryRepository<VatRate> vatRateQuery,
        IQueryBuilder queryBuilder)
    {
        _userContext = userContext;
        _organizationSettingQuery = organizationSettingQuery;
        _regulatedObligationQuery = regulatedObligationQuery;
        _vatRateQuery = vatRateQuery;
        _queryBuilder = queryBuilder;
    }

    public async Task<Result<TaxResolutionResultDto>> ResolveAsync(int organizationId, short taxTypeId, DateOnly? effectiveDate = null, CancellationToken ct = default)
    {
        if (organizationId <= 0)
            return Result.Failure<TaxResolutionResultDto>(TaxBusinessErrors.OrganizationRequired(_userContext.LanguageId));

        if (_userContext.UserKind != CurrentUserKind.SuperAdmin
            && _userContext.OrganizationId.HasValue
            && _userContext.OrganizationId.Value != organizationId)
        {
            return Result.Failure<TaxResolutionResultDto>(CommonErrors.Forbidden(_userContext.LanguageId));
        }

        var date = effectiveDate ?? DateOnly.FromDateTime(DateTime.Now);

        var obligation = await _regulatedObligationQuery.GetAsync(_queryBuilder.For<RegulatedObligation>()
            .Where(x => x.Id == taxTypeId && x.StateId == StateIdConst.ACTIVE)
            .Build(), ct);

        if (obligation is null)
            return Result.Failure<TaxResolutionResultDto>(TaxBusinessErrors.InactiveTaxType(taxTypeId, _userContext.LanguageId));

        var settings = await _organizationSettingQuery.GetAllAsync(_queryBuilder.For<OrganizationRegulatedObligationSetting>()
            .Where(x => x.OrganizationId == organizationId
                        && x.RegulatedObligationId == taxTypeId
                        && x.EffectiveFrom <= date
                        && (x.EffectiveTo == null || x.EffectiveTo >= date))
            .OrderBy(q => q.OrderByDescending(x => x.EffectiveFrom))
            .Build(), ct);

        if (settings.Count == 0)
            return Result.Failure<TaxResolutionResultDto>(TaxBusinessErrors.MissingTaxConfiguration(organizationId, taxTypeId, _userContext.LanguageId));

        if (settings.Count > 1)
            return Result.Failure<TaxResolutionResultDto>(TaxBusinessErrors.DuplicateTaxConfiguration(organizationId, taxTypeId, _userContext.LanguageId));

        var setting = settings[0];

        if (setting.StateId != StateIdConst.ACTIVE)
            return Result.Failure<TaxResolutionResultDto>(TaxBusinessErrors.DisabledTax(organizationId, taxTypeId, _userContext.LanguageId));

        var vatRate = await _vatRateQuery.GetAsync(_queryBuilder.For<VatRate>()
            .Where(x => x.StateId == StateIdConst.ACTIVE
                        && x.EffectiveFrom.HasValue
                        && x.EffectiveFrom.Value <= date
                        && (x.EffectiveTo == null || x.EffectiveTo >= date))
            .OrderBy(q => q.OrderByDescending(x => x.EffectiveFrom))
            .Build(), ct);

        var resolvedRate = setting.Rate ?? vatRate?.Rate;
        if (!resolvedRate.HasValue)
            return Result.Failure<TaxResolutionResultDto>(TaxBusinessErrors.MissingOrganizationConfiguration(_userContext.LanguageId));

        return Result.Success(new TaxResolutionResultDto
        {
            OrganizationId = organizationId,
            TaxTypeId = obligation.Id,
            TaxTypeCode = obligation.Code,
            TaxTypeName = obligation.Name,
            VatRateId = vatRate?.Id ?? 0,
            VatRateCode = vatRate?.Code ?? obligation.Code,
            VatRateName = vatRate?.Name ?? obligation.Name,
            Rate = resolvedRate.Value,
            EffectiveDate = setting.EffectiveFrom,
            EffectiveTo = setting.EffectiveTo,
            StateId = setting.StateId
        });
    }
}
