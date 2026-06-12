using Domain.Entities;

namespace Application.Features.ProductGroups;

public class ProductGroupDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public int? ParentId { get; set; }
    public string? ParentName { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public List<ProductGroupTableDto> Products { get; set; } = new();
}

public class ProductGroupTableDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public short UnitId { get; set; }
    public string Code { get; set; } = null!;
    public string? Barcode { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsService { get; set; }
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
    public string OrganizationName { get; set; } = null!;
    public string UnitCode { get; set; } = null!;
    public string UnitName { get; set; } = null!;
    public string StateName { get; set; } = null!;
}