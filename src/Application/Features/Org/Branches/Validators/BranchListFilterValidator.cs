using FluentValidation;

namespace Application.Features.Branches;

public sealed class BranchListFilterValidator : AbstractValidator<BranchListFilter>
{
    public BranchListFilterValidator()
    {
        RuleFor(filter => filter.RegionId)
            .GreaterThan(0)
            .When(filter => filter.RegionId.HasValue);
        RuleFor(filter => filter.Search)
            .MaximumLength(100)
            .When(filter => !string.IsNullOrWhiteSpace(filter.Search));
        RuleFor(filter => filter.Page).GreaterThan(0);
        RuleFor(filter => filter.PageSize)
            .GreaterThan(0)
            .When(filter => filter.PageSize.HasValue);
    }
}
