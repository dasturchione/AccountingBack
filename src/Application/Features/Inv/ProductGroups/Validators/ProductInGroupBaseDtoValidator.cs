using FluentValidation;

namespace Application.Features.ProductGroups;

public class ProductInGroupBaseDtoValidator : AbstractValidator<ProductInGroupBaseDto>
{
    public ProductInGroupBaseDtoValidator()
    {
        RuleFor(product => product.Code).MaximumLength(100).When(product => product.Code != null);
        RuleFor(product => product.Sku).MaximumLength(100).When(product => product.Sku != null);
        RuleFor(product => product.Article).MaximumLength(100).When(product => product.Article != null);
        RuleFor(product => product.Mxik).Length(17).When(product => product.Mxik != null);
        RuleFor(product => product.UnitId).GreaterThan((short)0);
        RuleFor(product => product.Barcode).MaximumLength(100).When(product => product.Barcode != null);
        RuleFor(product => product.Name).NotEmpty().MaximumLength(250);
        RuleFor(product => product.Description).MaximumLength(1000).When(product => product.Description != null);
        RuleFor(product => product.DefaultVatRateId)
            .GreaterThan((short)0)
            .When(product => product.DefaultVatRateId.HasValue);
        RuleFor(product => product)
            .Must(product => product.IsSold || product.IsPurchased)
            .WithMessage("Product must be marked as sold or purchased.");
    }
}
