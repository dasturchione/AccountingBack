using Application.Features.ProductGroups;

namespace UnitTests;

public sealed class ProductGroupValidatorTests
{
    [Fact]
    public void ListFilter_DefaultIsValidAndInvalidBoundsAreRejected()
    {
        var validator = new ProductGroupListFilterValidator();

        Assert.True(validator.Validate(new ProductGroupListFilter()).IsValid);
        Assert.False(validator.Validate(new ProductGroupListFilter { Page = 0 }).IsValid);
        Assert.False(validator.Validate(new ProductGroupListFilter { PageSize = 0 }).IsValid);
        Assert.False(validator.Validate(new ProductGroupListFilter { Search = new string('x', 251) }).IsValid);
    }

    [Fact]
    public void CreateValidatorValidatesNestedProductSchemaAndBusinessShape()
    {
        var validator = new ProductGroupCreateDtoValidator();
        var dto = new ProductGroupCreateDto
        {
            Code = "GROUP",
            Name = "Group",
            Products =
            [
                new ProductInGroupBaseDto
                {
                    UnitId = 1,
                    Name = "Product",
                    IsSold = true,
                    Code = new string('c', 100),
                    Sku = new string('s', 100),
                    Article = new string('a', 100),
                    Barcode = new string('b', 100),
                    Description = new string('d', 1000)
                }
            ]
        };

        Assert.True(validator.Validate(dto).IsValid);
        dto.Products[0].UnitId = 0;
        dto.Products[0].Name = string.Empty;
        dto.Products[0].Code = new string('c', 101);
        dto.Products[0].IsSold = false;
        dto.Products[0].IsPurchased = false;

        Assert.False(validator.Validate(dto).IsValid);
    }
}
