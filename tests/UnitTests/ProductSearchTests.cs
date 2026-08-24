using Application.Features.Products;

public sealed class ProductSearchTests
{
    [Fact]
    public void MxikSearchIsTrimmedAndCaseInsensitive()
    {
        var product = new ProductListDto
        {
            Name = "Product 158",
            Mxik = "08418002002001007"
        };

        var matches = Build(" 08418002002001007 ").Compile()(product);

        Assert.True(matches);
    }

    [Fact]
    public void MxikSearchReturnsProductWithExpectedCode()
    {
        var products = new[]
        {
            new ProductListDto { Id = 158, Name = "Product 158", Mxik = "08418002002001007" },
            new ProductListDto { Id = 159, Name = "Other product", Mxik = "08418002002001008" }
        };

        var matches = products.Where(Build("08418002002001007").Compile()).ToArray();

        var product = Assert.Single(matches);
        Assert.Equal(158, product.Id);
    }

    [Theory]
    [InlineData("Product")]
    [InlineData("CODE-1")]
    [InlineData("SKU-1")]
    [InlineData("ARTICLE-1")]
    [InlineData("BARCODE-1")]
    public void ExistingSearchFieldsRemainSupported(string search)
    {
        var product = new ProductListDto
        {
            Name = "Product name",
            Code = "CODE-1",
            Sku = "SKU-1",
            Article = "ARTICLE-1",
            Barcode = "BARCODE-1"
        };

        Assert.True(Build($"  {search.ToLower()} ").Compile()(product));
    }

    private static System.Linq.Expressions.Expression<Func<ProductListDto, bool>> Build(string search)
        => new ProductListDtoByListFilterCriteriaBuilder().Build(new ProductListFilter { Search = search });
}
