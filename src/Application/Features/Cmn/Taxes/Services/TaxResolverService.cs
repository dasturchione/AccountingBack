using Application.Abstractions;
using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Specifications;
using SharedKernel.Results;

namespace Application.Features.Cmn.Taxes;

public sealed class TaxResolverService : ITaxResolverService
{
    private readonly IUserContext _userContext;
    private readonly IQueryRepository<OrganizationTaxSetting> _organizationTaxSettingQuery;
    private readonly IQueryRepository<TaxType> _taxTypeQuery;
    private readonly IQueryRepository<VatRate> _vatRateQuery;

    public TaxResolverService(
        IUserContext userContext,
        IQueryRepository<OrganizationTaxSetting> organizationTaxSettingQuery,
        IQueryRepository<TaxType> taxTypeQuery,
        IQueryRepository<VatRate> vatRateQuery)
    {
        _userContext = userContext;
        _organizationTaxSettingQuery = organizationTaxSettingQuery;
        _taxTypeQuery = taxTypeQuery;
        _vatRateQuery = vatRateQuery;
    }

    public async Task<Result<TaxResolutionResultDto>> ResolveAsync(int organizationId, short taxTypeId, DateOnly? effectiveDate = null, CancellationToken ct = default)
    {
        if (organizationId <= 0)
            return Result.Failure<TaxResolutionResultDto>(TaxBusinessErrors.OrganizationRequired(_userContext.LanguageId));

        if (!_userContext.HasGlobalAccess
            && _userContext.OrganizationId.HasValue
            && _userContext.OrganizationId.Value != organizationId)
        {
            return Result.Failure<TaxResolutionResultDto>(CommonErrors.Forbidden(_userContext.LanguageId));
        }

        var date = effectiveDate ?? DateOnly.FromDateTime(DateTime.Now);

        var taxType = await _taxTypeQuery.GetAsync(
            new SharedKernel.Query.Specifications.QuerySpecification<TaxType>
            {
                Criteria = x => x.Id == taxTypeId && x.StateId == StateIdConst.ACTIVE
            }, ct);

        if (taxType is null)
            return Result.Failure<TaxResolutionResultDto>(TaxBusinessErrors.InactiveTaxType(taxTypeId, _userContext.LanguageId));

        var settings = await _organizationTaxSettingQuery.GetAllAsync(new QuerySpecification<OrganizationTaxSetting>
        {
            Criteria = x => x.OrganizationId == organizationId
                         && x.TaxTypeId == taxTypeId
                         && x.EffectiveFrom <= date
                         && (x.EffectiveTo == null || x.EffectiveTo >= date),
            OrderBy = q => q.OrderByDescending(x => x.EffectiveFrom)
        }, ct);

        if (settings.Count == 0)
            return Result.Failure<TaxResolutionResultDto>(TaxBusinessErrors.MissingTaxConfiguration(organizationId, taxTypeId, _userContext.LanguageId));

        if (settings.Count > 1)
            return Result.Failure<TaxResolutionResultDto>(TaxBusinessErrors.DuplicateTaxConfiguration(organizationId, taxTypeId, _userContext.LanguageId));

        var setting = settings[0];

        if (setting.StateId != StateIdConst.ACTIVE)
            return Result.Failure<TaxResolutionResultDto>(TaxBusinessErrors.DisabledTax(organizationId, taxTypeId, _userContext.LanguageId));

        if (taxTypeId == TaxTypeIdConst.VAT && !setting.IsVatPayer)
            return Result.Failure<TaxResolutionResultDto>(TaxBusinessErrors.DisabledTax(organizationId, taxTypeId, _userContext.LanguageId));

        var vatRate = await _vatRateQuery.GetAsync(
            new QuerySpecification<VatRate>
            {
                Criteria = x => x.StateId == StateIdConst.ACTIVE
                             && x.EffectiveFrom.HasValue
                             && x.EffectiveFrom.Value <= date
                             && (x.EffectiveTo == null || x.EffectiveTo >= date),
                OrderBy = q => q.OrderByDescending(x => x.EffectiveFrom)
            }, ct);

        if (vatRate is null)
            return Result.Failure<TaxResolutionResultDto>(TaxBusinessErrors.MissingOrganizationConfiguration(_userContext.LanguageId));

        return Result.Success(new TaxResolutionResultDto
        {
            OrganizationId = organizationId,
            TaxTypeId = taxType.Id,
            TaxTypeCode = taxType.Code,
            TaxTypeName = taxType.Name,
            VatRateId = vatRate.Id,
            VatRateCode = vatRate.Code,
            VatRateName = vatRate.Name,
            Rate = vatRate.Rate,
            EffectiveDate = setting.EffectiveFrom,
            EffectiveTo = setting.EffectiveTo,
            StateId = setting.StateId
        });
    }
}
