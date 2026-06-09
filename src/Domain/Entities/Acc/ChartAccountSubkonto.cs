namespace Domain.Entities;

public partial class ChartAccountSubkonto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int AccountId { get; set; }
    public short SubkontoTypeId { get; set; }
    public int SortOrder { get; set; }
    public bool IsRequired { get; set; }
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }

    public virtual Organization Organization { get; set; } = null!;
    public virtual ChartAccount Account { get; set; } = null!;
    public virtual SubkontoType SubkontoType { get; set; } = null!;
    public virtual State State { get; set; } = null!;
}
