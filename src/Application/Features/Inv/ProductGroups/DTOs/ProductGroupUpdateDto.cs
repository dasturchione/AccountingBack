namespace Application.Features.ProductGroups;

public class ProductGroupUpdateDto : ProductGroupBaseDto 
{ 
    public short StateId { get; set; }
    public List<ProductInGroupUpdateDto> Products { get; set; } = new();
}
