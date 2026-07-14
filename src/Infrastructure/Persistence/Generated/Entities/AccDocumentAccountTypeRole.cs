using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("acc_document_account_type_role")]
[Index("DocumentAccountTypeId", "DocumentAccountRoleId", Name = "acc_document_account_type_rol_document_account_type_id_docu_key", IsUnique = true)]
public partial class AccDocumentAccountTypeRole
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

    [InverseProperty("DocumentAccountTypeRole")]
    public virtual ICollection<AccDocumentAccountSetting> AccDocumentAccountSettings { get; set; } = new List<AccDocumentAccountSetting>();

    [ForeignKey("DocumentAccountRoleId")]
    [InverseProperty("AccDocumentAccountTypeRoles")]
    public virtual AccDocumentAccountRole DocumentAccountRole { get; set; } = null!;

    [ForeignKey("DocumentAccountTypeId")]
    [InverseProperty("AccDocumentAccountTypeRoles")]
    public virtual AccDocumentAccountType DocumentAccountType { get; set; } = null!;
}
