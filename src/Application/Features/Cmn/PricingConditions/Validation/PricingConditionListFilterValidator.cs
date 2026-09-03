using FluentValidation;

namespace Application.Features.PricingConditions;

public sealed class PricingConditionListFilterValidator : AbstractValidator<PricingConditionListFilter>
{
    public PricingConditionListFilterValidator()
    {
        RuleFor(filter => filter.PricingMethodId)
            .GreaterThan((short)0)
            .When(filter => filter.PricingMethodId.HasValue);
        RuleFor(filter => filter.RoundingMethodId)
            .GreaterThan((short)0)
            .When(filter => filter.RoundingMethodId.HasValue);
        RuleFor(filter => filter.StateId)
            .GreaterThan((short)0)
            .When(filter => filter.StateId.HasValue);
        RuleFor(filter => filter.Search)
            .MaximumLength(100)
            .When(filter => !string.IsNullOrWhiteSpace(filter.Search));
        RuleFor(filter => filter.Page).GreaterThan(0);
        RuleFor(filter => filter.PageSize)
            .GreaterThan(0)
            .When(filter => filter.PageSize.HasValue);
    }
}
