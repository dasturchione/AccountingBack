using FluentValidation;

namespace Application.Features.OrganizationSetup;

public sealed class OrganizationSetupTaxSettingsDtoValidator
    : AbstractValidator<OrganizationSetupTaxSettingsDto>
{
    public OrganizationSetupTaxSettingsDtoValidator()
    {
        RuleFor(dto => dto.TaxTypeId).GreaterThan((short)0);
        RuleFor(dto => dto.VatRegistrationNumber)
            .MaximumLength(100)
            .When(dto => dto.VatRegistrationNumber is not null);
        RuleFor(dto => dto.EffectiveFrom).NotEmpty();
        RuleFor(dto => dto.EffectiveTo)
            .GreaterThanOrEqualTo(dto => dto.EffectiveFrom)
            .When(dto => dto.EffectiveTo.HasValue);
        RuleFor(dto => dto.StateId).GreaterThan((short)0);
    }
}
