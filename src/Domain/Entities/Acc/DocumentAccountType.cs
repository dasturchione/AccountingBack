using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("acc_document_account_type")]
public partial class DocumentAccountType
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

    [InverseProperty(nameof(DocumentAccountTypeRole.DocumentAccountType))]
    public virtual ICollection<DocumentAccountTypeRole> DocumentAccountTypeRoles { get; set; } = new List<DocumentAccountTypeRole>();

    [InverseProperty(nameof(DocumentAccountTypeTranslation.DocumentAccountType))]
    public virtual ICollection<DocumentAccountTypeTranslation> DocumentAccountTypeTranslations { get; set; } = new List<DocumentAccountTypeTranslation>();

    [ForeignKey("StateId")]
    [InverseProperty(nameof(State.DocumentAccountTypes))]
    public virtual State State { get; set; } = null!;
}
