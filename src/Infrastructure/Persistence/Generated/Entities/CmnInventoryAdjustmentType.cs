using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("cmn_inventory_adjustment_type")]
[Index("Code", Name = "idx_cmn_inventory_adjustment_type_code", IsUnique = true)]
public partial class CmnInventoryAdjustmentType
{
    [Key]
    [Column("id")]
    public short Id { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column("state_id")]
    public short StateId { get; set; }

    [ForeignKey("StateId")]
    [InverseProperty("CmnInventoryAdjustmentTypes")]
    public virtual CmnState State { get; set; } = null!;
}
