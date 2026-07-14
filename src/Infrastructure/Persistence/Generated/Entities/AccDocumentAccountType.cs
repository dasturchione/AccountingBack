using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("acc_document_account_type")]
[Index("Code", Name = "acc_document_account_type_code_key", IsUnique = true)]
public partial class AccDocumentAccountType
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

    [InverseProperty("DocumentAccountType")]
    public virtual ICollection<AccDocumentAccountTypeRole> AccDocumentAccountTypeRoles { get; set; } = new List<AccDocumentAccountTypeRole>();

    [InverseProperty("DocumentAccountType")]
    public virtual ICollection<AccDocumentAccountTypeTranslation> AccDocumentAccountTypeTranslations { get; set; } = new List<AccDocumentAccountTypeTranslation>();

    [ForeignKey("StateId")]
    [InverseProperty("AccDocumentAccountTypes")]
    public virtual CmnState State { get; set; } = null!;
}
