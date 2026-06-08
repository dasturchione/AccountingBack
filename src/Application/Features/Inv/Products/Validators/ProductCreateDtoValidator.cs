using FluentValidation;

namespace Application.Features.Products;

public class ProductCreateDtoValidator : AbstractValidator<ProductCreateDto>
{
    public ProductCreateDtoValidator()
    {
        Include(new ProductBaseDtoValidator());
    }
}
