using FluentValidation;

namespace Application.Features.Inv.ProductStocks;

public sealed class ProductGroupStockFilterValidator : AbstractValidator<ProductGroupStockFilter>
{
    public ProductGroupStockFilterValidator()
    {
        RuleFor(x => x.WarehouseId)
            .GreaterThan(0)
            .When(x => x.WarehouseId.HasValue);
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .When(x => x.PageSize.HasValue);
    }
}
