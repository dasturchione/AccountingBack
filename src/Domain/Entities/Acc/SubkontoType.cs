namespace Domain.Entities;

public partial class SubkontoType
{
    public short Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string SourceTable { get; set; } = null!;
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }

    public virtual State State { get; set; } = null!;
}
