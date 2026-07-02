using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("acc_posting_alias")]
[Index("Code", Name = "acc_posting_alias_code_key", IsUnique = true)]
public partial class AccPostingAlias
{
    [Key]
    [Column("id")]
    public short Id { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [InverseProperty("Alias")]
    public virtual ICollection<AccPaymentPurpose> AccPaymentPurposes { get; set; } = new List<AccPaymentPurpose>();

    [InverseProperty("PostingAlias")]
    public virtual ICollection<AccPostingAliasTranslation> AccPostingAliasTranslations { get; set; } = new List<AccPostingAliasTranslation>();

    [InverseProperty("CreditAlias")]
    public virtual ICollection<AccPostingRuleLine> AccPostingRuleLineCreditAliases { get; set; } = new List<AccPostingRuleLine>();

    [InverseProperty("DebitAlias")]
    public virtual ICollection<AccPostingRuleLine> AccPostingRuleLineDebitAliases { get; set; } = new List<AccPostingRuleLine>();
}
