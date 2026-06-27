using FluentValidation;

namespace Application.Features.ProductPrices;

public class ProductPriceCreateDtoValidator : AbstractValidator<ProductPriceCreateDto>
{
    public ProductPriceCreateDtoValidator()
    {
        Include(new ProductPriceBaseDtoValidator());
    }
}
