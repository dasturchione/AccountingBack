using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_bank_operation_classification_rule_set")]
public class BankOperationClassificationRuleSet
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("bank_id")]
    public int BankId { get; set; }

    [Column("code")]
    [StringLength(100)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("version")]
    public short Version { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey(nameof(BankId))]
    [InverseProperty(nameof(Bank.BankOperationClassificationRuleSets))]
    public virtual Bank Bank { get; set; } = null!;

    [ForeignKey(nameof(StateId))]
    [InverseProperty(nameof(State.BankOperationClassificationRuleSets))]
    public virtual State State { get; set; } = null!;

    [InverseProperty(nameof(BankOperationClassificationRule.RuleSet))]
    public virtual ICollection<BankOperationClassificationRule> Rules { get; set; } = [];
}
