using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_movement_direction")]
[Index(nameof(Code), Name = "ux_cmn_movement_direction_code", IsUnique = true)]
public partial class MovementDirection
{
    [Key]
    [Column("id")]
    public short Id { get; set; }

    [Column("code")]
    [StringLength(10)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    [InverseProperty(nameof(BankOperation.Direction))]
    public virtual ICollection<BankOperation> BankOperations { get; set; } = new List<BankOperation>();

    [InverseProperty(nameof(BankOperationClassificationRule.Direction))]
    public virtual ICollection<BankOperationClassificationRule> BankOperationClassificationRules { get; set; } = [];

    [InverseProperty(nameof(MovementDirectionTranslation.MovementDirection))]
    public virtual ICollection<MovementDirectionTranslation> MovementDirectionTranslations { get; set; } = new List<MovementDirectionTranslation>();

    [InverseProperty(nameof(InventoryAdjustmentDoc.Direction))]
    public virtual ICollection<InventoryAdjustmentDoc> InventoryAdjustmentDocs { get; set; } = new List<InventoryAdjustmentDoc>();

    [InverseProperty(nameof(WarehouseProductMovement.Direction))]
    public virtual ICollection<WarehouseProductMovement> WarehouseProductMovements { get; set; } = new List<WarehouseProductMovement>();

    [InverseProperty(nameof(MoneyRegisterBalance.Direction))]
    public virtual ICollection<MoneyRegisterBalance> MoneyRegisterBalances { get; set; } = new List<MoneyRegisterBalance>();
}
