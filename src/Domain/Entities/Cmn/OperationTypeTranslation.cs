using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[PrimaryKey("OperationTypeId", "LanguageId")]
[Table("cmn_operation_type_translation")]
public partial class OperationTypeTranslation
{
    [Key]
    [Column("operation_type_id")]
    public short OperationTypeId { get; set; }

    [Key]
    [Column("language_id")]
    public short LanguageId { get; set; }

    [Column("name")]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    [ForeignKey("LanguageId")]
    [InverseProperty(nameof(Entities.Language.OperationTypeTranslations))]
    public virtual Language Language { get; set; } = null!;

    [ForeignKey("OperationTypeId")]
    [InverseProperty(nameof(Entities.OperationType.OperationTypeTranslations))]
    public virtual OperationType OperationType { get; set; } = null!;
}
