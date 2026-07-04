using FluentValidation;

namespace Application.Features.Cmn.CurrencyRates;

public sealed class CurrencyRateUpdateDtoValidator : AbstractValidator<CurrencyRateUpdateDto>
{
    public CurrencyRateUpdateDtoValidator()
    {
        Include(new CurrencyRateBaseDtoValidator());
        RuleFor(x => x.StateId).GreaterThan((short)0);
    }
}
