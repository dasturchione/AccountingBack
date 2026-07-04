using FluentValidation;

namespace Application.Features.Cmn.Currencies;

public sealed class CurrencyUpdateDtoValidator : AbstractValidator<CurrencyUpdateDto>
{
    public CurrencyUpdateDtoValidator()
    {
        Include(new CurrencyBaseDtoValidator());
        RuleFor(x => x.StateId).GreaterThan((short)0);
    }
}
