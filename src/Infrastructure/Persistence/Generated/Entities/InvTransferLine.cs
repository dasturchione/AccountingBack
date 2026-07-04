using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("inv_transfer_line")]
[Index("ProductId", Name = "idx_inv_transfer_line_product_id")]
[Index("UnitId", Name = "idx_inv_transfer_line_unit_id")]
[Index("OwnerId", Name = "ix_inv_transfer_line_owner_id")]
public partial class InvTransferLine
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("owner_id")]
    public long OwnerId { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("unit_id")]
    public short UnitId { get; set; }

    [Column("quantity")]
    [Precision(24, 8)]
    public decimal Quantity { get; set; }

    [Column("comment")]
    [StringLength(1000)]
    public string? Comment { get; set; }

    [InverseProperty("Owner")]
    public virtual ICollection<InvTransferDocTable> InvTransferDocTables { get; set; } = new List<InvTransferDocTable>();

    [ForeignKey("OwnerId")]
    [InverseProperty("InvTransferLines")]
    public virtual InvTransferDoc Owner { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("InvTransferLines")]
    public virtual InvProduct Product { get; set; } = null!;

    [ForeignKey("UnitId")]
    [InverseProperty("InvTransferLines")]
    public virtual CmnUnit Unit { get; set; } = null!;
}
