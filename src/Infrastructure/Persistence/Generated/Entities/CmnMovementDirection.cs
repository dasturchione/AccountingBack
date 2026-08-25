using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("cmn_movement_direction")]
[Index("Code", Name = "ux_cmn_movement_direction_code", IsUnique = true)]
public partial class CmnMovementDirection
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

    [InverseProperty("Direction")]
    public virtual ICollection<BankOperation> BankOperations { get; set; } = new List<BankOperation>();

    [InverseProperty("MovementDirection")]
    public virtual ICollection<CmnMovementDirectionTranslation> CmnMovementDirectionTranslations { get; set; } = new List<CmnMovementDirectionTranslation>();

    [InverseProperty("Direction")]
    public virtual ICollection<InvInventoryAdjustmentDoc> InvInventoryAdjustmentDocs { get; set; } = new List<InvInventoryAdjustmentDoc>();

    [InverseProperty("Direction")]
    public virtual ICollection<InvRegBalance> InvRegBalances { get; set; } = new List<InvRegBalance>();

    [InverseProperty("Direction")]
    public virtual ICollection<InvWarehouseProductMovement> InvWarehouseProductMovements { get; set; } = new List<InvWarehouseProductMovement>();

    [InverseProperty("Direction")]
    public virtual ICollection<MoneyRegBalance> MoneyRegBalances { get; set; } = new List<MoneyRegBalance>();
}
