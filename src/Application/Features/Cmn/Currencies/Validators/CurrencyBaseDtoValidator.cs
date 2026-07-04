using FluentValidation;

namespace Application.Features.Cmn.Currencies;

public class CurrencyBaseDtoValidator : AbstractValidator<CurrencyBaseDto>
{
    public CurrencyBaseDtoValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Symbol).MaximumLength(10).When(x => !string.IsNullOrWhiteSpace(x.Symbol));
    }
}
