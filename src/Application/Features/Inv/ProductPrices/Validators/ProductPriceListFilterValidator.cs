using FluentValidation;

namespace Application.Features.Inv.ProductPrices;

public sealed class ProductPriceListFilterValidator : AbstractValidator<ProductPriceListFilter>
{
    public ProductPriceListFilterValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0)
            .When(x => x.ProductId.HasValue);
        RuleFor(x => x.PriceTypeId)
            .GreaterThan((short)0)
            .When(x => x.PriceTypeId.HasValue);
        RuleFor(x => x.UnitId)
            .GreaterThan((short)0)
            .When(x => x.UnitId.HasValue);
        RuleFor(x => x.Search)
            .MaximumLength(250)
            .When(x => x.Search != null);
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .When(x => x.PageSize.HasValue);
    }
}
