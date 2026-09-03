using Application.Features.Inv.ProductPrices;

namespace UnitTests;

public sealed class ProductPriceValidatorTests
{
    [Fact]
    public void ListFilter_DefaultIsValidAndInvalidBoundsAreRejected()
    {
        var validator = new ProductPriceListFilterValidator();

        Assert.True(validator.Validate(new ProductPriceListFilter()).IsValid);
        Assert.False(validator.Validate(new ProductPriceListFilter { ProductId = 0 }).IsValid);
        Assert.False(validator.Validate(new ProductPriceListFilter { PriceTypeId = 0 }).IsValid);
        Assert.False(validator.Validate(new ProductPriceListFilter { UnitId = 0 }).IsValid);
        Assert.False(validator.Validate(new ProductPriceListFilter { Page = 0 }).IsValid);
        Assert.False(validator.Validate(new ProductPriceListFilter { PageSize = 0 }).IsValid);
        Assert.False(validator.Validate(new ProductPriceListFilter { Search = new string('x', 251) }).IsValid);
    }

    [Fact]
    public void BaseValidatorRejectsInvalidReferencesPriceAndDateRange()
    {
        var validator = new ProductPriceBaseDtoValidator();
        var startDate = new DateTime(2026, 1, 1);
        var dto = new ProductPriceBaseDto
        {
            ProductId = 1,
            CurrencyId = 1,
            PriceTypeId = 1,
            UnitId = 1,
            Price = 0,
            StartDate = startDate,
            EndDate = startDate
        };

        Assert.True(validator.Validate(dto).IsValid);

        dto.ProductId = 0;
        dto.CurrencyId = 0;
        dto.PriceTypeId = 0;
        dto.UnitId = 0;
        dto.Price = -1;
        dto.EndDate = startDate.AddTicks(-1);

        Assert.False(validator.Validate(dto).IsValid);
    }
}
