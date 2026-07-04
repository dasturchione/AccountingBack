using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[PrimaryKey("PostingAliasId", "LanguageId")]
[Table("acc_posting_alias_translation")]
public partial class AccPostingAliasTranslation
{
    [Key]
    [Column("posting_alias_id")]
    public short PostingAliasId { get; set; }

    [Key]
    [Column("language_id")]
    public short LanguageId { get; set; }

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [ForeignKey("LanguageId")]
    [InverseProperty("AccPostingAliasTranslations")]
    public virtual CmnLanguage Language { get; set; } = null!;

    [ForeignKey("PostingAliasId")]
    [InverseProperty("AccPostingAliasTranslations")]
    public virtual AccPostingAlias PostingAlias { get; set; } = null!;
}
