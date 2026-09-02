using FluentValidation;

namespace Application.Features.Inv.ProductStocks;

public sealed class ProductStockFilterValidator : AbstractValidator<ProductStockFilter>
{
    public ProductStockFilterValidator()
    {
        RuleFor(x => x.WarehouseId)
            .GreaterThan(0)
            .When(x => x.WarehouseId.HasValue);
        RuleFor(x => x.ProductGroupId)
            .GreaterThan(0)
            .When(x => x.ProductGroupId.HasValue);
        RuleFor(x => x.Search)
            .MaximumLength(250)
            .When(x => x.Search != null);
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .When(x => x.PageSize.HasValue);
    }
}
