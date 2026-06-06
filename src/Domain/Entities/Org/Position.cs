namespace Domain.Entities;

public partial class Position
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
    public virtual Organization Organization { get; set; } = null!;
    public virtual State State { get; set; } = null!;
}
