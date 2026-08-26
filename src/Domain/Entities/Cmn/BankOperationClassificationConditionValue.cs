using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_bank_operation_classification_condition_value")]
public class BankOperationClassificationConditionValue
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("condition_id")]
    public int ConditionId { get; set; }

    [Column("value_order")]
    public short ValueOrder { get; set; }

    [Column("compare_value")]
    [StringLength(500)]
    public string CompareValue { get; set; } = null!;

    [ForeignKey(nameof(ConditionId))]
    [InverseProperty(nameof(BankOperationClassificationCondition.Values))]
    public virtual BankOperationClassificationCondition Condition { get; set; } = null!;
}
