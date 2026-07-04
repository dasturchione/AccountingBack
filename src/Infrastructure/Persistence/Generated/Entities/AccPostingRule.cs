using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("acc_posting_rule")]
[Index("Code", Name = "acc_posting_rule_code_key", IsUnique = true)]
public partial class AccPostingRule
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

    [InverseProperty("Template")]
    public virtual ICollection<AccPostingRuleLine> AccPostingRuleLines { get; set; } = new List<AccPostingRuleLine>();
}
