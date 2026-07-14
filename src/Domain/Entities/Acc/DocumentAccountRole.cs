using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Table("acc_document_account_role")]
public partial class DocumentAccountRole
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
    public virtual ICollection<DocumentAccountRoleTranslation> DocumentAccountRoleTranslations { get; set; } = new List<DocumentAccountRoleTranslation>();

    [InverseProperty("DocumentAccountRole")]
    public virtual ICollection<DocumentAccountTypeRole> DocumentAccountTypeRoles { get; set; } = new List<DocumentAccountTypeRole>();

    [ForeignKey("StateId")]
    [InverseProperty(nameof(State.DocumentAccountRoles))]
    public virtual State State { get; set; } = null!;
}
