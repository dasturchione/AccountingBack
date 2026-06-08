using FluentValidation;

namespace Application.Features.Products;

public class ProductUpdateDtoValidator : AbstractValidator<ProductUpdateDto>
{
    public ProductUpdateDtoValidator()
    {
        Include(new ProductBaseDtoValidator());
        RuleFor(x => x.StateId).GreaterThan((short)0);
    }
}
