namespace Application.Features.ProductGroups;

public class ProductInGroupUpdateDto : ProductInGroupBaseDto
{
    public int? Id { get; set; }
    public short? StateId { get; set; }
}
