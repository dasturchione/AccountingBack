using FluentValidation;

namespace Application.Features.Cmn.CurrencyRates;

public class CurrencyRateBaseDtoValidator : AbstractValidator<CurrencyRateBaseDto>
{
    public CurrencyRateBaseDtoValidator()
    {
        RuleFor(x => x.BaseCurrencyId).GreaterThan((short)0);
        RuleFor(x => x.TargetCurrencyId).GreaterThan((short)0);
        RuleFor(x => x.EffectiveDate).NotEmpty();
        RuleFor(x => x.BuyRate).GreaterThan(0);
        RuleFor(x => x.SellRate).GreaterThan(0);
        RuleFor(x => x.OfficialRate).GreaterThan(0);
        RuleFor(x => x.RateSource).MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.RateSource));
    }
}
