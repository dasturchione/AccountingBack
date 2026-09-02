using Application.Features.Products;

namespace UnitTests;

public sealed class ProductPublicContractTests
{
    [Fact]
    public void ProductsCreateDtoPropertiesRemainStable()
    {
        var properties = typeof(ProductsCreateDto).GetProperties();

        var property = Assert.Single(properties);
        Assert.Equal("Products", property.Name);
        Assert.Equal(typeof(List<ProductCreateDto>), property.PropertyType);
    }
}
