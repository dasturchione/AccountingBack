using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[PrimaryKey(nameof(CategoryId), nameof(LanguageId))]
[Table("cmn_bank_operation_category_translation")]
public class BankOperationCategoryTranslation
{
    [Key]
    [Column("category_id")]
    public short CategoryId { get; set; }

    [Key]
    [Column("language_id")]
    public short LanguageId { get; set; }

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [ForeignKey(nameof(CategoryId))]
    [InverseProperty(nameof(BankOperationCategory.Translations))]
    public virtual BankOperationCategory Category { get; set; } = null!;

    [ForeignKey(nameof(LanguageId))]
    [InverseProperty(nameof(Language.BankOperationCategoryTranslations))]
    public virtual Language Language { get; set; } = null!;
}
