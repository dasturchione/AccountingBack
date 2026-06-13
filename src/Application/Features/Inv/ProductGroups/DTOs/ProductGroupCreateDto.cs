namespace Application.Features.ProductGroups;

public class ProductGroupCreateDto : ProductGroupBaseDto
{
    public List<ProductInGroupBaseDto> Products { get; set; } = [];
}
