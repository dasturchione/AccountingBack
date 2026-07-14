using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("acc_document_account_role")]
[Index("Code", Name = "acc_document_account_role_code_key", IsUnique = true)]
public partial class AccDocumentAccountRole
{
    [Key]
    [Column("id")]
    public short Id { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("description")]
    [StringLength(500)]
    public string? Description { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [InverseProperty("DocumentAccountRole")]
    public virtual ICollection<AccDocumentAccountRoleTranslation> AccDocumentAccountRoleTranslations { get; set; } = new List<AccDocumentAccountRoleTranslation>();

    [InverseProperty("DocumentAccountRole")]
    public virtual ICollection<AccDocumentAccountTypeRole> AccDocumentAccountTypeRoles { get; set; } = new List<AccDocumentAccountTypeRole>();

    [ForeignKey("StateId")]
    [InverseProperty("AccDocumentAccountRoles")]
    public virtual CmnState State { get; set; } = null!;
}
