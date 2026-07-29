using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[PrimaryKey("CostingMethodId", "LanguageId")]
[Table("cmn_costing_method_translation")]
public partial class CostingMethodTranslation
{
    [Key]
    [Column("costing_method_id")]
    public short CostingMethodId { get; set; }

    [Key]
    [Column("language_id")]
    public short LanguageId { get; set; }

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [ForeignKey("CostingMethodId")]
    [InverseProperty(nameof(CostingMethod.CostingMethodTranslations))]
    public virtual CostingMethod CostingMethod { get; set; } = null!;

    [ForeignKey("LanguageId")]
    [InverseProperty(nameof(Language.CostingMethodTranslations))]
    public virtual Language Language { get; set; } = null!;
}
