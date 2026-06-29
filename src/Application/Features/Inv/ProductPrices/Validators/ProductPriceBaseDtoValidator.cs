using FluentValidation;

namespace Application.Features.Inv.ProductPrices;

public class ProductPriceBaseDtoValidator : AbstractValidator<ProductPriceBaseDto>
{
    public ProductPriceBaseDtoValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.CurrencyId).GreaterThan((short)0);
        RuleFor(x => x.PriceTypeId).GreaterThan((short)0);
        RuleFor(x => x.UnitId).GreaterThan((short)0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x)
            .Must(x => !x.EndDate.HasValue || x.EndDate.Value >= x.StartDate)
            .WithMessage("EndDate must be greater than or equal to StartDate.");
    }
}
