using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[PrimaryKey(nameof(MovementDirectionId), nameof(LanguageId))]
[Table("cmn_movement_direction_translation")]
[Index(nameof(LanguageId), Name = "ix_cmn_movement_direction_translation_language_id")]
public partial class MovementDirectionTranslation
{
    [Key]
    [Column("movement_direction_id")]
    public short MovementDirectionId { get; set; }

    [Key]
    [Column("language_id")]
    public short LanguageId { get; set; }

    [Column("name")]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    [ForeignKey(nameof(LanguageId))]
    [InverseProperty(nameof(Entities.Language.MovementDirectionTranslations))]
    public virtual Language Language { get; set; } = null!;

    [ForeignKey(nameof(MovementDirectionId))]
    [InverseProperty(nameof(Entities.MovementDirection.MovementDirectionTranslations))]
    public virtual MovementDirection MovementDirection { get; set; } = null!;
}
