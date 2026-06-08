using FluentValidation;

namespace Application.Features.ProductGroups;

public class ProductGroupCreateDtoValidator : AbstractValidator<ProductGroupCreateDto>
{
    public ProductGroupCreateDtoValidator()
    {
        Include(new ProductGroupBaseDtoValidator());
    }
}
