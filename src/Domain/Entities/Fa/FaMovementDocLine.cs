using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("fa_movement_doc_line")]
[Index("MovementDocId", Name = "idx_fa_movement_doc_line_movement_doc_id")]
[Index("FaAssetId", Name = "idx_fa_movement_doc_line_fa_asset_id")]
public partial class FaMovementDocLine
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("movement_doc_id")]
    public long MovementDocId { get; set; }

    [Column("fa_asset_id")]
    public long FaAssetId { get; set; }

    [Column("note")]
    [StringLength(500)]
    public string? Note { get; set; }

    [ForeignKey("FaAssetId")]
    public virtual FaAsset FaAsset { get; set; } = null!;

    [ForeignKey("MovementDocId")]
    [InverseProperty("Lines")]
    public virtual FaMovementDoc MovementDoc { get; set; } = null!;
}
