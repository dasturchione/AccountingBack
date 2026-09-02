using FluentValidation;

namespace Application.Features.Positions;

public sealed class PositionListFilterValidator : AbstractValidator<PositionListFilter>
{
    public PositionListFilterValidator()
    {
        RuleFor(filter => filter.Search)
            .MaximumLength(100)
            .When(filter => !string.IsNullOrWhiteSpace(filter.Search));
        RuleFor(filter => filter.Page).GreaterThan(0);
        RuleFor(filter => filter.PageSize)
            .GreaterThan(0)
            .When(filter => filter.PageSize.HasValue);
    }
}
