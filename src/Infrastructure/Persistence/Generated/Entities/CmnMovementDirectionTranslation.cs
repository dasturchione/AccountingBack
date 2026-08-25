using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[PrimaryKey("MovementDirectionId", "LanguageId")]
[Table("cmn_movement_direction_translation")]
[Index("LanguageId", Name = "ix_cmn_movement_direction_translation_language_id")]
public partial class CmnMovementDirectionTranslation
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

    [ForeignKey("LanguageId")]
    [InverseProperty("CmnMovementDirectionTranslations")]
    public virtual CmnLanguage Language { get; set; } = null!;

    [ForeignKey("MovementDirectionId")]
    [InverseProperty("CmnMovementDirectionTranslations")]
    public virtual CmnMovementDirection MovementDirection { get; set; } = null!;
}
