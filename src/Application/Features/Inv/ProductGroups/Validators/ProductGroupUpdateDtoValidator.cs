using FluentValidation;

namespace Application.Features.ProductGroups;

public class ProductGroupUpdateDtoValidator : AbstractValidator<ProductGroupUpdateDto>
{
    public ProductGroupUpdateDtoValidator()
    {
        Include(new ProductGroupBaseDtoValidator());
        RuleFor(x => x.StateId).GreaterThan((short)0);
        RuleForEach(x => x.Products)
            .SetValidator(new ProductInGroupUpdateDtoValidator());
    }
}
