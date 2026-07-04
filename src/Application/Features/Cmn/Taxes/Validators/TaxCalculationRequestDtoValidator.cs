using FluentValidation;

namespace Application.Features.Cmn.Taxes;

public sealed class TaxCalculationRequestDtoValidator : AbstractValidator<TaxCalculationRequestDto>
{
    public TaxCalculationRequestDtoValidator()
    {
        RuleFor(x => x.TaxTypeId).GreaterThan((short)0);
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CalculationMode).IsInEnum();
        RuleFor(x => x.OrganizationId).GreaterThan(0).When(x => x.OrganizationId.HasValue);
        RuleFor(x => x.RoundingPrecision).GreaterThan((short)0).When(x => x.RoundingPrecision.HasValue);
    }
}
