using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[PrimaryKey("DisposalTypeId", "LanguageId")]
[Table("fa_disposal_type_translation")]
[Index("LanguageId", Name = "ix_fa_disposal_type_translation_language_id")]
public partial class FaDisposalTypeTranslation
{
    [Key]
    [Column("disposal_type_id")]
    public short DisposalTypeId { get; set; }

    [Key]
    [Column("language_id")]
    public short LanguageId { get; set; }

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [ForeignKey("DisposalTypeId")]
    [InverseProperty("FaDisposalTypeTranslations")]
    public virtual FaDisposalType DisposalType { get; set; } = null!;

    [ForeignKey("LanguageId")]
    [InverseProperty("FaDisposalTypeTranslations")]
    public virtual CmnLanguage Language { get; set; } = null!;
}
