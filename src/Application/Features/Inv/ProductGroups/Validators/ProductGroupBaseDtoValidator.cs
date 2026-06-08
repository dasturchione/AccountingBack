using FluentValidation;

namespace Application.Features.ProductGroups;

public class ProductGroupBaseDtoValidator : AbstractValidator<ProductGroupBaseDto>
{
    public ProductGroupBaseDtoValidator()
    {
        RuleFor(x => x.OrganizationId).GreaterThan(0);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
    }
}
