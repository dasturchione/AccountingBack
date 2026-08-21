using Application.Features.SaleDocs.EdoSalePreflight;
using Domain.Entities;
using SharedKernel.Constants;

public sealed class EdoSaleProductEligibilityTests
{
    [Fact]
    public void PurchaseOnlyProductIsNotSaleEligible()
    {
        var result = Resolve(new Product { Id = 462, IsPurchased = true, IsSold = false }, 10);

        Assert.Equal("SALE_PRODUCT_NOT_ENABLED", result.SafeErrorCode);
        Assert.Null(result.Product);
    }

    [Fact]
    public void SoldProductWithoutStockIsBlocked()
    {
        var result = Resolve(new Product { Id = 69, IsSold = true }, 10);

        Assert.Equal("SALE_PRODUCT_STOCK_MAPPING_REQUIRED", result.SafeErrorCode);
        Assert.Null(result.Product);
    }

    [Fact]
    public void SoldProductWithSufficientStockIsEligible()
    {
        var product = new Product { Id = 69, IsSold = true };
        var result = Resolve(product, 10, new Dictionary<int, decimal> { [69] = 13 });

        Assert.Equal("READY", result.Status);
        Assert.Equal(69, result.Product!.Id);
    }

    [Fact]
    public void AmbiguousProductsAreNeverAutoSelected()
    {
        var result = Resolve(
            new Product { Id = 69, IsSold = true },
            1,
            new Dictionary<int, decimal> { [69] = 5 },
            new Product { Id = 442, IsSold = true });

        Assert.Equal("PRODUCT_MAPPING_AMBIGUOUS", result.SafeErrorCode);
        Assert.Null(result.Product);
    }

    [Fact]
    public void OtherOrganizationProductIsIgnored()
    {
        var product = new Product { Id = 69, OrganizationId = 3, IsSold = true };
        var result = Resolve(product, 1, new Dictionary<int, decimal> { [69] = 5 });

        Assert.Equal("PRODUCT_MAPPING_REQUIRED", result.SafeErrorCode);
    }

    private static EdoSaleProductEligibilityResult Resolve(
        Product first,
        decimal quantity,
        IReadOnlyDictionary<int, decimal>? stock = null,
        params Product[] additional)
    {
        first.OrganizationId = first.OrganizationId == 0 ? 2 : first.OrganizationId;
        first.StateId = StateIdConst.ACTIVE;
        first.Mxik = "08415001007001106";
        foreach (var product in additional)
        {
            product.OrganizationId = 2;
            product.StateId = StateIdConst.ACTIVE;
            product.Mxik = first.Mxik;
        }

        return EdoSaleProductEligibility.Resolve(
            2,
            first.Mxik,
            quantity,
            [first, .. additional],
            stock ?? new Dictionary<int, decimal>(),
            true);
    }
}
