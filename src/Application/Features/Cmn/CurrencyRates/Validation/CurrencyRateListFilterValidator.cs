using FluentValidation;

namespace Application.Features.Cmn.CurrencyRates;

public sealed class CurrencyRateListFilterValidator : AbstractValidator<CurrencyRateListFilter>
{
    public CurrencyRateListFilterValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).When(x => x.PageSize.HasValue);
        RuleFor(x => x.SortBy).MaximumLength(128).When(x => x.SortBy is not null);
    }
}
