using FluentValidation;

namespace Application.Features.OrganizationSetup;

public sealed class OrganizationSetupAccountingPolicyDtoValidator
    : AbstractValidator<OrganizationSetupAccountingPolicyDto>
{
    public OrganizationSetupAccountingPolicyDtoValidator()
    {
        RuleFor(dto => dto.InventoryValuationMethod).NotEmpty().MaximumLength(20);
        RuleFor(dto => dto.AccountingPolicyId)
            .GreaterThan((short)0)
            .When(dto => dto.AccountingPolicyId.HasValue);
        RuleFor(dto => dto.BaseCurrencyId)
            .GreaterThan((short)0)
            .When(dto => dto.BaseCurrencyId.HasValue);
        RuleFor(dto => dto.FiscalYearStartMonth).InclusiveBetween((short)1, (short)12);
    }
}
