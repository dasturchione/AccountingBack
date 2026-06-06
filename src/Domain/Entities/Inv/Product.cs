namespace Domain.Entities;

public partial class Product
{
    public int Id { get; set; }

    public int OrganizationId { get; set; }

    public int? ProductGroupId { get; set; }

    public short UnitId { get; set; }

    public string Code { get; set; } = null!;

    public string? Barcode { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsService { get; set; }

    public short StateId { get; set; }

    public DateTime CreatedDate { get; set; }
}
