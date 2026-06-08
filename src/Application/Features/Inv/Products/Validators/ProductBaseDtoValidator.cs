using FluentValidation;

namespace Application.Features.Products;

public class ProductBaseDtoValidator : AbstractValidator<ProductBaseDto>
{
    public ProductBaseDtoValidator()
    {
        RuleFor(x => x.OrganizationId).GreaterThan(0);
        RuleFor(x => x.UnitId).GreaterThan((short)0);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Barcode).MaximumLength(100).When(x => x.Barcode != null);
        RuleFor(x => x.Description).MaximumLength(2000).When(x => x.Description != null);
    }
}
