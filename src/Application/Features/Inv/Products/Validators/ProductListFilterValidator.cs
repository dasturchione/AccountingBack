using FluentValidation;

namespace Application.Features.Products;

public sealed class ProductListFilterValidator : AbstractValidator<ProductListFilter>
{
    public ProductListFilterValidator()
    {
        RuleFor(filter => filter.ProductGroupId)
            .GreaterThan(0)
            .When(filter => filter.ProductGroupId.HasValue);
        RuleFor(filter => filter.Search)
            .MaximumLength(250)
            .When(filter => !string.IsNullOrWhiteSpace(filter.Search));
        RuleFor(filter => filter.Page).GreaterThan(0);
        RuleFor(filter => filter.PageSize)
            .GreaterThan(0)
            .When(filter => filter.PageSize.HasValue);
    }
}
