using FluentValidation;

namespace Application.Features.ProductGroups;

public class ProductGroupBaseDtoValidator : AbstractValidator<ProductGroupBaseDto>
{
    public ProductGroupBaseDtoValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ParentId).GreaterThan(0).When(x => x.ParentId.HasValue);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
    }
}
