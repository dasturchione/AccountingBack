using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("acc_document_account_type_role")]
public partial class DocumentAccountTypeRole
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("document_account_type_id")]
    public short DocumentAccountTypeId { get; set; }

    [Column("document_account_role_id")]
    public short DocumentAccountRoleId { get; set; }

    [Column("account_side")]
    [StringLength(10)]
    public string AccountSide { get; set; } = null!;

    [Column("is_required")]
    public bool IsRequired { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; }

    [InverseProperty(nameof(DocumentAccountSetting.DocumentAccountTypeRole))]
    public virtual ICollection<DocumentAccountSetting> DocumentAccountSettings { get; set; } = new List<DocumentAccountSetting>();

    [ForeignKey("DocumentAccountRoleId")]
    [InverseProperty(nameof(DocumentAccountRole.DocumentAccountTypeRoles))]
    public virtual DocumentAccountRole DocumentAccountRole { get; set; } = null!;

    [ForeignKey("DocumentAccountTypeId")]
    [InverseProperty(nameof(DocumentAccountType.DocumentAccountTypeRoles))]
    public virtual DocumentAccountType DocumentAccountType { get; set; } = null!;
}
