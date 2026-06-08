using FluentValidation;

namespace Application.Features.CashBoxes;

public class CashBoxBaseDtoValidator : AbstractValidator<CashBoxBaseDto>
{
    public CashBoxBaseDtoValidator()
    {
        RuleFor(x => x.OrganizationId).GreaterThan(0);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.CurrencyId).GreaterThan((short)0);
    }
}
