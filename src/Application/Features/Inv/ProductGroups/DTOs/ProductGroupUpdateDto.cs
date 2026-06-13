namespace Application.Features.ProductGroups;

public class ProductGroupUpdateDto : ProductGroupBaseDto 
{ 
    public short StateId { get; set; }
    public List<ProductInGroupUpdateDto> Products { get; set; } = new();
}

public class ProductInGroupUpdateDto : ProductInGroupBaseDto
{
    public int? Id { get; set; }

    public short? StateId { get; set; }
}