using FluentValidation;

namespace Application.Features.Cmn.CurrencyRevaluations;

public sealed class CurrencyRevaluationListFilterValidator : AbstractValidator<CurrencyRevaluationListFilter>
{
    public CurrencyRevaluationListFilterValidator()
    {
        RuleFor(filter => filter.Search)
            .MaximumLength(100)
            .When(filter => !string.IsNullOrWhiteSpace(filter.Search));
        RuleFor(filter => filter.SortBy)
            .MaximumLength(128)
            .When(filter => filter.SortBy is not null);
        RuleFor(filter => filter.Page).GreaterThan(0);
        RuleFor(filter => filter.PageSize)
            .GreaterThan(0)
            .When(filter => filter.PageSize.HasValue);
    }
}
