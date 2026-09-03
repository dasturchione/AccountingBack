using Application.Features.Inv.ProductPrices;

namespace UnitTests;

public sealed class ProductPricePublicContractTests
{
    [Fact]
    public void SplitPriceDetailDtoPropertiesRemainStable()
    {
        AssertProperties<ProductCostPriceDto>(
            ("CostPrice", typeof(decimal)),
            ("Purchases", typeof(List<ProductCostPriceTableDto>)));
        AssertProperties<ProductCostPriceTableDto>(
            ("PurchaseId", typeof(long)),
            ("DocNumber", typeof(string)),
            ("Date", typeof(DateTime)),
            ("Quantity", typeof(int)),
            ("UnitPrice", typeof(decimal)),
            ("ProductTableIds", typeof(List<int>)));
        AssertProperties<ProductSalePriceDto>(
            ("SalePrice", typeof(decimal)),
            ("SalePrices", typeof(List<ProductSalePriceTableDto>)));
        AssertProperties<ProductSalePriceTableDto>(
            ("PurchaseId", typeof(long)),
            ("PurchaseDate", typeof(DateTime)),
            ("UnitPrice", typeof(decimal)),
            ("Quantity", typeof(decimal)),
            ("ProductTableIds", typeof(List<int>)));
    }

    private static void AssertProperties<T>(params (string Name, Type Type)[] expected)
    {
        var actual = typeof(T).GetProperties()
            .Select(property => (property.Name, property.PropertyType))
            .OrderBy(property => property.Name)
            .ToArray();

        Assert.Equal(expected.OrderBy(property => property.Name).ToArray(), actual);
    }
}
