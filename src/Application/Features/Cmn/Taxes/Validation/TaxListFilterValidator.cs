using FluentValidation;

namespace Application.Features.Cmn.Taxes;

public sealed class TaxListFilterValidator : AbstractValidator<TaxListFilter>
{
    public TaxListFilterValidator()
    {
        RuleFor(x => x.StateId).GreaterThan((short)0).When(x => x.StateId.HasValue);
        RuleFor(x => x.Search).MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.Search));
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).When(x => x.PageSize.HasValue);
    }
}
