using FluentValidation;

namespace Application.Features.Inv.ProductStocks;

public sealed class ProductTableStockFilterValidator : AbstractValidator<ProductTableStockFilter>
{
    public ProductTableStockFilterValidator()
    {
        RuleFor(x => x.WarehouseId)
            .GreaterThan(0)
            .When(x => x.WarehouseId.HasValue);
        RuleFor(x => x.ProductGroupId)
            .GreaterThan(0)
            .When(x => x.ProductGroupId.HasValue);
        RuleFor(x => x.ProductId)
            .GreaterThan(0)
            .When(x => x.ProductId.HasValue);
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .When(x => x.PageSize.HasValue);
    }
}
