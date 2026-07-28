using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[PrimaryKey("AccountTypeId", "LanguageId")]
[Table("acc_account_type_translation")]
public partial class AccountTypeTranslation
{
    [Key]
    [Column("account_type_id")]
    public short AccountTypeId { get; set; }

    [Key]
    [Column("language_id")]
    public short LanguageId { get; set; }

    [Column("name")]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    [ForeignKey("AccountTypeId")]
    [InverseProperty(nameof(Entities.AccountType.AccountTypeTranslations))]
    public virtual AccountType AccountType { get; set; } = null!;

    [ForeignKey("LanguageId")]
    [InverseProperty(nameof(Entities.Language.AccountTypeTranslations))]
    public virtual Language Language { get; set; } = null!;
}
