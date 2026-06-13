using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("acc_posting_rule")]
[Index("DocumentTypeId", Name = "idx_acc_posting_rule_document_type_id")]
[Index("OperationTypeId", Name = "idx_acc_posting_rule_operation_type_id")]
[Index("OrganizationId", "DocumentTypeId", "OperationTypeId", "Code", Name = "idx_acc_posting_rule_unique", IsUnique = true)]
public partial class AccPostingRule
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
    public virtual ICollection<AccPostingRuleLine> AccPostingRuleLines { get; set; } = new List<AccPostingRuleLine>();

    [ForeignKey("DocumentTypeId")]
    [InverseProperty("AccPostingRules")]
    public virtual CmnDocumentType DocumentType { get; set; } = null!;

    [ForeignKey("OperationTypeId")]
    [InverseProperty("AccPostingRules")]
    public virtual CmnOperationType? OperationType { get; set; }

    [ForeignKey("OrganizationId")]
    [InverseProperty("AccPostingRules")]
    public virtual OrgOrganization? Organization { get; set; }

    [ForeignKey("StateId")]
    [InverseProperty("AccPostingRules")]
    public virtual CmnState State { get; set; } = null!;
}
