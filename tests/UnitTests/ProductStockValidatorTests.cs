using Application.Features.Inv.ProductStocks;

namespace UnitTests;

public sealed class ProductStockValidatorTests
{
    [Fact]
    public void GroupFilterRejectsInvalidWarehouseAndPaging()
    {
        var validator = new ProductGroupStockFilterValidator();

        Assert.True(validator.Validate(new ProductGroupStockFilter()).IsValid);
        Assert.False(validator.Validate(new ProductGroupStockFilter { WarehouseId = 0 }).IsValid);
        Assert.False(validator.Validate(new ProductGroupStockFilter { Page = 0 }).IsValid);
        Assert.False(validator.Validate(new ProductGroupStockFilter { PageSize = 0 }).IsValid);
    }

    [Fact]
    public void ProductFilterRejectsInvalidReferencesSearchAndPaging()
    {
        var validator = new ProductStockFilterValidator();

        Assert.True(validator.Validate(new ProductStockFilter()).IsValid);
        Assert.False(validator.Validate(new ProductStockFilter { WarehouseId = 0 }).IsValid);
        Assert.False(validator.Validate(new ProductStockFilter { ProductGroupId = 0 }).IsValid);
        Assert.False(validator.Validate(new ProductStockFilter { Search = new string('x', 251) }).IsValid);
        Assert.False(validator.Validate(new ProductStockFilter { Page = 0 }).IsValid);
        Assert.False(validator.Validate(new ProductStockFilter { PageSize = 0 }).IsValid);
    }

    [Fact]
    public void ProductTableFilterRejectsInvalidReferencesAndPaging()
    {
        var validator = new ProductTableStockFilterValidator();

        Assert.True(validator.Validate(new ProductTableStockFilter()).IsValid);
        Assert.False(validator.Validate(new ProductTableStockFilter { WarehouseId = 0 }).IsValid);
        Assert.False(validator.Validate(new ProductTableStockFilter { ProductGroupId = 0 }).IsValid);
        Assert.False(validator.Validate(new ProductTableStockFilter { ProductId = 0 }).IsValid);
        Assert.False(validator.Validate(new ProductTableStockFilter { Page = 0 }).IsValid);
        Assert.False(validator.Validate(new ProductTableStockFilter { PageSize = 0 }).IsValid);
    }
}
