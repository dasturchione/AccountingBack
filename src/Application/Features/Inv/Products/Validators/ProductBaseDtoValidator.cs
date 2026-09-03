using FluentValidation;

namespace Application.Features.Products;

public class ProductBaseDtoValidator : AbstractValidator<ProductBaseDto>
{
    public ProductBaseDtoValidator()
    {
        RuleFor(x => x.Code).MaximumLength(100).When(x => x.Code != null);
        RuleFor(x => x.Sku).MaximumLength(100).When(x => x.Sku != null);
        RuleFor(x => x.Article).MaximumLength(100).When(x => x.Article != null);
        RuleFor(x => x.ProductGroupId).GreaterThan(0).When(x => x.ProductGroupId.HasValue);
        RuleFor(x => x.UnitId).GreaterThan((short)0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Barcode).MaximumLength(100).When(x => x.Barcode != null);
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description != null);
        RuleFor(x => x.Mxik).Length(17).When(x => x.Mxik != null);
        RuleFor(x => x.DefaultVatRateId)
            .GreaterThan((short)0)
            .When(x => x.DefaultVatRateId.HasValue);
        RuleFor(x => x)
            .Must(x => x.IsSold || x.IsPurchased)
            .WithMessage("Product must be marked as sold or purchased.");
    }
}
