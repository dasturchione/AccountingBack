using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[PrimaryKey("SubkontoTypeId", "LanguageId")]
[Table("acc_subkonto_type_translation")]
public partial class SubkontoTypeTranslation
{
    [Key]
    [Column("subkonto_type_id")]
    public short SubkontoTypeId { get; set; }

    [Key]
    [Column("language_id")]
    public short LanguageId { get; set; }

    [Column("name")]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    [ForeignKey("LanguageId")]
    [InverseProperty(nameof(Language.SubkontoTypeTranslations))]
    public virtual Language Language { get; set; } = null!;

    [ForeignKey("SubkontoTypeId")]
    [InverseProperty(nameof(SubkontoType.SubkontoTypeTranslations))]
    public virtual SubkontoType SubkontoType { get; set; } = null!;
}
