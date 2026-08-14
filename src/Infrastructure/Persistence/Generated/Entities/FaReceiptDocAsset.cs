using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("fa_receipt_doc_asset")]
[Index("AssetAccountId", Name = "ix_fa_receipt_doc_asset_asset_account_id")]
[Index("FaAssetId", Name = "ix_fa_receipt_doc_asset_fa_asset_id")]
[Index("FaGroupId", Name = "ix_fa_receipt_doc_asset_fa_group_id")]
[Index("OkofId", Name = "ix_fa_receipt_doc_asset_okof_id")]
[Index("ReceiptDocLineId", Name = "ix_fa_receipt_doc_asset_receipt_doc_line_id")]
public partial class FaReceiptDocAsset
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("receipt_doc_line_id")]
    public long ReceiptDocLineId { get; set; }

    [Column("fa_asset_id")]
    public long? FaAssetId { get; set; }

    [Column("inventory_number")]
    [StringLength(100)]
    public string InventoryNumber { get; set; } = null!;

    [Column("name")]
    [StringLength(500)]
    public string Name { get; set; } = null!;

    [Column("initial_cost")]
    [Precision(24, 8)]
    public decimal InitialCost { get; set; }

    [Column("fa_group_id")]
    public int FaGroupId { get; set; }

    [Column("okof_id")]
    public short? OkofId { get; set; }

    [Column("asset_account_id")]
    public int AssetAccountId { get; set; }

    [ForeignKey("AssetAccountId")]
    [InverseProperty("FaReceiptDocAssets")]
    public virtual AccChartAccount AssetAccount { get; set; } = null!;

    [ForeignKey("FaAssetId")]
    [InverseProperty("FaReceiptDocAsset")]
    public virtual FaAsset? FaAsset { get; set; }

    [ForeignKey("FaGroupId")]
    [InverseProperty("FaReceiptDocAssets")]
    public virtual CmnFaGroup FaGroup { get; set; } = null!;

    [ForeignKey("OkofId")]
    [InverseProperty("FaReceiptDocAssets")]
    public virtual CmnFaOkof? Okof { get; set; }

    [ForeignKey("ReceiptDocLineId")]
    [InverseProperty("FaReceiptDocAssets")]
    public virtual FaReceiptDocLine ReceiptDocLine { get; set; } = null!;
}
