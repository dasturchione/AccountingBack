using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_inventory_adjustment_type")]
[Index("Code", Name = "idx_cmn_inventory_adjustment_type_code", IsUnique = true)]
public partial class InventoryAdjustmentType
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
    public virtual State State { get; set; } = null!;
}
