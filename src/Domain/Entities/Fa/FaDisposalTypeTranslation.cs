using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[PrimaryKey("DisposalTypeId", "LanguageId")]
[Table("fa_disposal_type_translation")]
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
    [InverseProperty(nameof(FaDisposalType.FaDisposalTypeTranslations))]
    public virtual FaDisposalType DisposalType { get; set; } = null!;

    [ForeignKey("LanguageId")]
    [InverseProperty(nameof(Language.FaDisposalTypeTranslations))]
    public virtual Language Language { get; set; } = null!;
}
