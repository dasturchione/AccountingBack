using Application.Features.Products;

namespace UnitTests;

public sealed class ProductValidatorTests
{
    [Fact]
    public void ListFilter_DefaultIsValidAndInvalidBoundsAreRejected()
    {
        var validator = new ProductListFilterValidator();

        Assert.True(validator.Validate(new ProductListFilter()).IsValid);
        Assert.False(validator.Validate(new ProductListFilter { ProductGroupId = 0 }).IsValid);
        Assert.False(validator.Validate(new ProductListFilter { Page = 0 }).IsValid);
        Assert.False(validator.Validate(new ProductListFilter { PageSize = 0 }).IsValid);
        Assert.False(validator.Validate(new ProductListFilter { Search = new string('x', 251) }).IsValid);
    }

    [Fact]
    public void BaseValidatorMatchesDomainStringAndReferenceBounds()
    {
        var validator = new ProductBaseDtoValidator();
        var dto = new ProductBaseDto
        {
            UnitId = 1,
            ProductGroupId = 1,
            Name = "Product",
            Code = new string('c', 100),
            Sku = new string('s', 100),
            Article = new string('a', 100),
            IsSold = true,
            DefaultVatRateId = 1
        };

        Assert.True(validator.Validate(dto).IsValid);
        dto.Code = new string('c', 101);
        dto.ProductGroupId = 0;
        dto.DefaultVatRateId = 0;
        Assert.False(validator.Validate(dto).IsValid);
    }
}
