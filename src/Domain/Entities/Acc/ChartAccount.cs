namespace Domain.Entities;

public partial class ChartAccount
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int? ParentId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool IsGroup { get; set; }
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }

    public virtual Organization Organization { get; set; } = null!;
    public virtual ChartAccount? Parent { get; set; }
    public virtual State State { get; set; } = null!;
}
