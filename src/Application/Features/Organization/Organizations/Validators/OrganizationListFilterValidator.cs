using FluentValidation;

namespace Application.Features.Organizations;

public sealed class OrganizationListFilterValidator : AbstractValidator<OrganizationListFilter>
{
    public OrganizationListFilterValidator()
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
