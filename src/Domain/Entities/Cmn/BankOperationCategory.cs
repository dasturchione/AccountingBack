using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_bank_operation_category")]
public class BankOperationCategory
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

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey(nameof(StateId))]
    [InverseProperty(nameof(State.BankOperationCategories))]
    public virtual State State { get; set; } = null!;

    [InverseProperty(nameof(BankOperationCategoryTranslation.Category))]
    public virtual ICollection<BankOperationCategoryTranslation> Translations { get; set; } = [];

    [InverseProperty(nameof(BankOperationClassificationRule.Category))]
    public virtual ICollection<BankOperationClassificationRule> ClassificationRules { get; set; } = [];

    [InverseProperty(nameof(BankOperation.ClassificationCategory))]
    public virtual ICollection<BankOperation> BankOperations { get; set; } = [];
}
