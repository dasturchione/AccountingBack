using FluentValidation;

namespace Application.Features.ProductGroups;

public class ProductGroupCreateDtoValidator : AbstractValidator<ProductGroupCreateDto>
{
    public ProductGroupCreateDtoValidator()
    {
        Include(new ProductGroupBaseDtoValidator());
        RuleForEach(x => x.Products)
            .SetValidator(new ProductInGroupBaseDtoValidator());
    }
}
