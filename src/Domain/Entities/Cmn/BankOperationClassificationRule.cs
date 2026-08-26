using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_bank_operation_classification_rule")]
public class BankOperationClassificationRule
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("rule_set_id")]
    public int RuleSetId { get; set; }

    [Column("category_id")]
    public short CategoryId { get; set; }

    [Column("code")]
    [StringLength(100)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("priority")]
    public short Priority { get; set; }

    [Column("direction_id")]
    public short? DirectionId { get; set; }

    [Column("is_fallback")]
    public bool IsFallback { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [ForeignKey(nameof(RuleSetId))]
    [InverseProperty(nameof(BankOperationClassificationRuleSet.Rules))]
    public virtual BankOperationClassificationRuleSet RuleSet { get; set; } = null!;

    [ForeignKey(nameof(CategoryId))]
    [InverseProperty(nameof(BankOperationCategory.ClassificationRules))]
    public virtual BankOperationCategory Category { get; set; } = null!;

    [ForeignKey(nameof(DirectionId))]
    [InverseProperty(nameof(MovementDirection.BankOperationClassificationRules))]
    public virtual MovementDirection? Direction { get; set; }

    [ForeignKey(nameof(StateId))]
    [InverseProperty(nameof(State.BankOperationClassificationRules))]
    public virtual State State { get; set; } = null!;

    [InverseProperty(nameof(BankOperationClassificationCondition.Rule))]
    public virtual ICollection<BankOperationClassificationCondition> Conditions { get; set; } = [];

    [InverseProperty(nameof(BankOperation.ClassificationRule))]
    public virtual ICollection<BankOperation> BankOperations { get; set; } = [];
}
