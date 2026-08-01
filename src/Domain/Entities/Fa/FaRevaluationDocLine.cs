using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("fa_revaluation_doc_line")]
public partial class FaRevaluationDocLine
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("revaluation_doc_id")]
    public long RevaluationDocId { get; set; }

    [Column("fa_asset_id")]
    public long FaAssetId { get; set; }

    [Column("old_value", TypeName = "numeric(18,2)")]
    public decimal OldValue { get; set; }

    [Column("new_value", TypeName = "numeric(18,2)")]
    public decimal NewValue { get; set; }

    [Column("revaluation_amount", TypeName = "numeric(18,2)")]
    public decimal RevaluationAmount { get; set; }

    [Column("note")]
    [StringLength(500)]
    public string? Note { get; set; }

    [Column("asset_account_id")]
    public int? AssetAccountId { get; set; }

    [Column("accumulated_depreciation_account_id")]
    public int? AccumulatedDepreciationAccountId { get; set; }

    [ForeignKey(nameof(AssetAccountId))]
    [InverseProperty(nameof(ChartAccount.FaRevaluationDocLineAssetAccounts))]
    public virtual ChartAccount? AssetAccount { get; set; }

    [ForeignKey(nameof(AccumulatedDepreciationAccountId))]
    [InverseProperty(nameof(ChartAccount.FaRevaluationDocLineAccumulatedDepreciationAccounts))]
    public virtual ChartAccount? AccumulatedDepreciationAccount { get; set; }

    [ForeignKey("FaAssetId")]
    public virtual FaAsset FaAsset { get; set; } = null!;

    [ForeignKey("RevaluationDocId")]
    [InverseProperty("Lines")]
    public virtual FaRevaluationDoc RevaluationDoc { get; set; } = null!;
}
