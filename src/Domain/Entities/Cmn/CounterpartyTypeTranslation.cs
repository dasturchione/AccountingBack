using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[PrimaryKey("CounterpartyTypeId", "LanguageId")]
[Table("cmn_counterparty_type_translation")]
public partial class CounterpartyTypeTranslation
{
    [Key]
    [Column("counterparty_type_id")]
    public short CounterpartyTypeId { get; set; }

    [Key]
    [Column("language_id")]
    public short LanguageId { get; set; }

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [ForeignKey("CounterpartyTypeId")]
    [InverseProperty(nameof(Entities.CounterpartyType.CounterpartyTypeTranslations))]
    public virtual CounterpartyType CounterpartyType { get; set; } = null!;

    [ForeignKey("LanguageId")]
    [InverseProperty(nameof(Entities.Language.CounterpartyTypeTranslations))]
    public virtual Language Language { get; set; } = null!;
}
