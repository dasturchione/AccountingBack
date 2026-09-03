using FluentValidation;

namespace Application.Features.ProductGroups;

public sealed class ProductGroupListFilterValidator : AbstractValidator<ProductGroupListFilter>
{
    public ProductGroupListFilterValidator()
    {
        RuleFor(filter => filter.Search)
            .MaximumLength(250)
            .When(filter => !string.IsNullOrWhiteSpace(filter.Search));
        RuleFor(filter => filter.Page).GreaterThan(0);
        RuleFor(filter => filter.PageSize)
            .GreaterThan(0)
            .When(filter => filter.PageSize.HasValue);
    }
}
