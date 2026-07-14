using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("acc_document_account_setting")]
public partial class DocumentAccountSetting
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("document_account_type_role_id")]
    public int DocumentAccountTypeRoleId { get; set; }

    [Column("chart_account_id")]
    public int ChartAccountId { get; set; }

    [Column("is_default")]
    public bool IsDefault { get; set; }

    [Column("can_change")]
    public bool CanChange { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedDate { get; set; }

    [ForeignKey("ChartAccountId")]
    [InverseProperty(nameof(ChartAccount.DocumentAccountSettings))]
    public virtual ChartAccount ChartAccount { get; set; } = null!;

    [ForeignKey("DocumentAccountTypeRoleId")]
    [InverseProperty(nameof(DocumentAccountTypeRole.DocumentAccountSettings))]
    public virtual DocumentAccountTypeRole DocumentAccountTypeRole { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    [InverseProperty(nameof(Organization.DocumentAccountSettings))]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey("StateId")]
    [InverseProperty(nameof(State.DocumentAccountSettings))]
    public virtual State State { get; set; } = null!;
}
