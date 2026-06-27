using FluentValidation;

namespace Application.Features.ProductPrices;

public class ProductPriceUpdateDtoValidator : AbstractValidator<ProductPriceUpdateDto>
{
    public ProductPriceUpdateDtoValidator()
    {
        Include(new ProductPriceBaseDtoValidator());
        RuleFor(x => x.StateId).GreaterThan((short)0);
    }
}
