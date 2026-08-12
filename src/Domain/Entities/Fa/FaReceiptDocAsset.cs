using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("fa_receipt_doc_asset")]
[Index(nameof(AssetAccountId), Name = "ix_fa_receipt_doc_asset_asset_account_id")]
[Index(nameof(FaAssetId), Name = "ix_fa_receipt_doc_asset_fa_asset_id")]
[Index(nameof(FaGroupId), Name = "ix_fa_receipt_doc_asset_fa_group_id")]
[Index(nameof(OkofId), Name = "ix_fa_receipt_doc_asset_okof_id")]
[Index(nameof(ReceiptDocLineId), Name = "ix_fa_receipt_doc_asset_receipt_doc_line_id")]
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

    [Column("initial_cost", TypeName = "numeric(24,8)")]
    public decimal InitialCost { get; set; }

    [Column("fa_group_id")]
    public int FaGroupId { get; set; }

    [Column("okof_id")]
    public short? OkofId { get; set; }

    [Column("asset_account_id")]
    public int AssetAccountId { get; set; }

    [ForeignKey(nameof(AssetAccountId))]
    [InverseProperty(nameof(ChartAccount.FaReceiptDocAssets))]
    public virtual ChartAccount AssetAccount { get; set; } = null!;

    [ForeignKey(nameof(FaAssetId))]
    [InverseProperty(nameof(FaAsset.FaReceiptDocAsset))]
    public virtual FaAsset? FaAsset { get; set; }

    [ForeignKey("FaGroupId")]
    public virtual FaGroup FaGroup { get; set; } = null!;

    [ForeignKey("OkofId")]
    public virtual FaOkof? Okof { get; set; }

    [ForeignKey(nameof(ReceiptDocLineId))]
    [InverseProperty(nameof(FaReceiptDocLine.Assets))]
    public virtual FaReceiptDocLine ReceiptDocLine { get; set; } = null!;
}
