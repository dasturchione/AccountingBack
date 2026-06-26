namespace Application.Features.Products;

public class ProductCreateDto : ProductBaseDto
{
}

public class ProductsCreateDto
{
    public List<ProductCreateDto> Products { get; set; } = new();
}