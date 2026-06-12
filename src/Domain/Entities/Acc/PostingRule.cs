namespace Domain.Entities;

public partial class PostingRule
{
    public int Id { get; set; }
    public int? OrganizationId { get; set; }
    public short DocumentTypeId { get; set; }
    public short? OperationTypeId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
    public virtual ICollection<PostingRuleLine> PostingRuleLines { get; set; } = new List<PostingRuleLine>();
    public virtual DocumentType DocumentType { get; set; } = null!;
    public virtual OperationType? OperationType { get; set; }
    public virtual Organization? Organization { get; set; }
    public virtual State State { get; set; } = null!;
}
