using Application.Features.ProductGroups;

namespace UnitTests;

public sealed class ProductGroupPublicContractTests
{
    [Fact]
    public void SplitDtoPropertiesRemainStable()
    {
        AssertProperties<ProductInGroupBaseDto>(
            ("Article", typeof(string)), ("Barcode", typeof(string)), ("Code", typeof(string)),
            ("DefaultVatRateId", typeof(short?)), ("Description", typeof(string)),
            ("IsPieceTracked", typeof(bool)), ("IsPurchased", typeof(bool)),
            ("IsService", typeof(bool)), ("IsSold", typeof(bool)), ("MinStock", typeof(decimal?)),
            ("Mxik", typeof(string)), ("Name", typeof(string)), ("Sku", typeof(string)),
            ("UnitId", typeof(short)));
        AssertProperties<ProductInGroupUpdateDto>(
            ("Article", typeof(string)), ("Barcode", typeof(string)), ("Code", typeof(string)),
            ("DefaultVatRateId", typeof(short?)), ("Description", typeof(string)), ("Id", typeof(int?)),
            ("IsPieceTracked", typeof(bool)), ("IsPurchased", typeof(bool)),
            ("IsService", typeof(bool)), ("IsSold", typeof(bool)), ("MinStock", typeof(decimal?)),
            ("Mxik", typeof(string)), ("Name", typeof(string)), ("Sku", typeof(string)),
            ("StateId", typeof(short?)), ("UnitId", typeof(short)));
        AssertProperties<ProductGroupTableDto>(
            ("Article", typeof(string)), ("Barcode", typeof(string)), ("Code", typeof(string)),
            ("CreatedDate", typeof(DateTime)), ("DefaultVatRateId", typeof(short?)),
            ("Description", typeof(string)), ("Id", typeof(int)), ("IsPieceTracked", typeof(bool)),
            ("IsPurchased", typeof(bool)), ("IsService", typeof(bool)), ("IsSold", typeof(bool)),
            ("MinStock", typeof(decimal?)), ("Mxik", typeof(string)), ("Name", typeof(string)),
            ("OrganizationId", typeof(int)), ("OrganizationName", typeof(string)), ("Sku", typeof(string)),
            ("StateId", typeof(short)), ("StateName", typeof(string)), ("UnitCode", typeof(string)),
            ("UnitId", typeof(short)), ("UnitName", typeof(string)));
    }

    private static void AssertProperties<T>(params (string Name, Type Type)[] expected)
    {
        var actual = typeof(T).GetProperties()
            .Select(property => (property.Name, property.PropertyType))
            .OrderBy(property => property.Name)
            .ToArray();

        Assert.Equal(expected.OrderBy(property => property.Name), actual);
    }
}
