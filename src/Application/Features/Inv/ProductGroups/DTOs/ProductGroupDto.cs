namespace Application.Features.ProductGroups;

public class ProductGroupDto
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public int? ParentId { get; set; }
    public bool IsAssignable { get; set; }
    public int SortOrder { get; set; }
    public string Name { get; set; } = null!;
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public List<ProductGroupTableDto> Products { get; set; } = new();
}
