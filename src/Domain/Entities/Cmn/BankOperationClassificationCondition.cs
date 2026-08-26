using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_bank_operation_classification_condition")]
public class BankOperationClassificationCondition
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("rule_id")]
    public int RuleId { get; set; }

    [Column("condition_order")]
    public short ConditionOrder { get; set; }

    [Column("field_code")]
    [StringLength(50)]
    public string FieldCode { get; set; } = null!;

    [Column("operator_code")]
    [StringLength(30)]
    public string OperatorCode { get; set; } = null!;

    [Column("value_source_code")]
    [StringLength(30)]
    public string ValueSourceCode { get; set; } = null!;

    [Column("compare_value")]
    [StringLength(1000)]
    public string? CompareValue { get; set; }

    [Column("normalization_code")]
    [StringLength(30)]
    public string NormalizationCode { get; set; } = null!;

    [ForeignKey(nameof(RuleId))]
    [InverseProperty(nameof(BankOperationClassificationRule.Conditions))]
    public virtual BankOperationClassificationRule Rule { get; set; } = null!;

    [InverseProperty(nameof(BankOperationClassificationConditionValue.Condition))]
    public virtual ICollection<BankOperationClassificationConditionValue> Values { get; set; } = [];
}
