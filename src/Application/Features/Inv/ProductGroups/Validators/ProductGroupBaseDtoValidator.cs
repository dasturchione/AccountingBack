using FluentValidation;

namespace Application.Features.ProductGroups;

public class ProductGroupBaseDtoValidator : AbstractValidator<ProductGroupBaseDto>
{
    public ProductGroupBaseDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
    }
}
