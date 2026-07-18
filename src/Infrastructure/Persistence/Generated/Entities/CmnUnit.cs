using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("cmn_unit")]
[Index("Code", Name = "idx_cmn_unit_code", IsUnique = true)]
public partial class CmnUnit
{
    [Key]
    [Column("id")]
    public short Id { get; set; }

    [Column("code")]
    [StringLength(20)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column("state_id")]
    public short StateId { get; set; }

    [InverseProperty("Unit")]
    public virtual ICollection<InvInventoryAdjustmentLine> InvInventoryAdjustmentLines { get; set; } = new List<InvInventoryAdjustmentLine>();

    [InverseProperty("Unit")]
    public virtual ICollection<InvInventoryCountLine> InvInventoryCountLines { get; set; } = new List<InvInventoryCountLine>();

    [InverseProperty("Unit")]
    public virtual ICollection<InvProductPrice> InvProductPrices { get; set; } = new List<InvProductPrice>();

    [InverseProperty("Unit")]
    public virtual ICollection<InvProduct> InvProducts { get; set; } = new List<InvProduct>();

    [InverseProperty("Unit")]
    public virtual ICollection<InvTransferLine> InvTransferLines { get; set; } = new List<InvTransferLine>();

    [InverseProperty("Unit")]
    public virtual ICollection<InvWarehouseProduct> InvWarehouseProducts { get; set; } = new List<InvWarehouseProduct>();

    [InverseProperty("Unit")]
    public virtual ICollection<PurDocProduct> PurDocProducts { get; set; } = new List<PurDocProduct>();

    [InverseProperty("Unit")]
    public virtual ICollection<SaleDocProduct> SaleDocProducts { get; set; } = new List<SaleDocProduct>();

    [InverseProperty("Unit")]
    public virtual ICollection<SaleShipmentProduct> SaleShipmentProducts { get; set; } = new List<SaleShipmentProduct>();

    [ForeignKey("StateId")]
    [InverseProperty("CmnUnits")]
    public virtual CmnState State { get; set; } = null!;
}
