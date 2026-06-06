namespace Domain.Entities;

public partial class ProductGroup
{
    public int Id { get; set; }

    public int OrganizationId { get; set; }

    public int? ParentId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public short StateId { get; set; }

    public DateTime CreatedDate { get; set; }
}
