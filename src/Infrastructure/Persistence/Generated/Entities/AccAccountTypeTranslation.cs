using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[PrimaryKey("AccountTypeId", "LanguageId")]
[Table("acc_account_type_translation")]
[Index("LanguageId", Name = "ix_acc_account_type_translation_language_id")]
public partial class AccAccountTypeTranslation
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
    [InverseProperty("AccAccountTypeTranslations")]
    public virtual AccAccountType AccountType { get; set; } = null!;

    [ForeignKey("LanguageId")]
    [InverseProperty("AccAccountTypeTranslations")]
    public virtual CmnLanguage Language { get; set; } = null!;
}
