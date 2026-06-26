using FluentValidation;

namespace Application.Features.Products;

public class ProductBaseDtoValidator : AbstractValidator<ProductBaseDto>
{
    public ProductBaseDtoValidator()
    {
        RuleFor(x => x.UnitId).GreaterThan((short)0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Barcode).MaximumLength(100).When(x => x.Barcode != null);
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description != null);
        RuleFor(x => x.Mxik).MaximumLength(17).When(x => x.Mxik != null);
    }
}
