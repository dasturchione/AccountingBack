using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[PrimaryKey("SubkontoTypeId", "LanguageId")]
[Table("acc_subkonto_type_translation")]
[Index("LanguageId", Name = "idx_acc_subkonto_type_translation_language_id")]
public partial class AccSubkontoTypeTranslation
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
    [InverseProperty("AccSubkontoTypeTranslations")]
    public virtual CmnLanguage Language { get; set; } = null!;

    [ForeignKey("SubkontoTypeId")]
    [InverseProperty("AccSubkontoTypeTranslations")]
    public virtual AccSubkontoType SubkontoType { get; set; } = null!;
}
