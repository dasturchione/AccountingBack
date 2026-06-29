using FluentValidation;

namespace Application.Features.Inv.ProductPrices;

public class ProductPriceCreateDtoValidator : AbstractValidator<ProductPriceCreateDto>
{
    public ProductPriceCreateDtoValidator()
    {
        Include(new ProductPriceBaseDtoValidator());
    }
}
