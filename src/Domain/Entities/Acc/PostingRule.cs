using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("acc_posting_rule")]
[Index("DocumentTypeId", Name = "idx_acc_posting_rule_document_type_id")]
[Index("OperationTypeId", Name = "idx_acc_posting_rule_operation_type_id")]
[Index("OrganizationId", "DocumentTypeId", "OperationTypeId", "Code", Name = "idx_acc_posting_rule_unique", IsUnique = true)]
public partial class PostingRule
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("organization_id")]
    public int? OrganizationId { get; set; }

    [Column("document_type_id")]
    public short DocumentTypeId { get; set; }

    [Column("operation_type_id")]
    public short? OperationTypeId { get; set; }

    [Column("code")]
    [StringLength(100)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [InverseProperty("Rule")]
    public virtual ICollection<PostingRuleLine> PostingRuleLines { get; set; } = new List<PostingRuleLine>();

    [ForeignKey("DocumentTypeId")]
    [InverseProperty("PostingRules")]
    public virtual DocumentType DocumentType { get; set; } = null!;

    [ForeignKey("OperationTypeId")]
    [InverseProperty("PostingRules")]
    public virtual OperationType? OperationType { get; set; }

    [ForeignKey("OrganizationId")]
    [InverseProperty("PostingRules")]
    public virtual Organization? Organization { get; set; }

    [ForeignKey("StateId")]
    [InverseProperty("PostingRules")]
    public virtual State State { get; set; } = null!;
}
