using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("fa_revaluation_doc_line")]
[Index("RevaluationDocId", Name = "idx_fa_revaluation_doc_line_doc_id")]
[Index("FaAssetId", Name = "idx_fa_revaluation_doc_line_fa_asset_id")]
[Index("AccumulatedDepreciationAccountId", Name = "ix_fa_reval_line_accum_depr_account")]
[Index("AssetAccountId", Name = "ix_fa_reval_line_asset_account")]
[Index("RevaluationDocId", "FaAssetId", Name = "ux_fa_revaluation_doc_line_doc_asset", IsUnique = true)]
public partial class FaRevaluationDocLine
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("revaluation_doc_id")]
    public long RevaluationDocId { get; set; }

    [Column("fa_asset_id")]
    public long FaAssetId { get; set; }

    [Column("old_value")]
    [Precision(18, 2)]
    public decimal OldValue { get; set; }

    [Column("new_value")]
    [Precision(18, 2)]
    public decimal NewValue { get; set; }

    [Column("revaluation_amount")]
    [Precision(18, 2)]
    public decimal RevaluationAmount { get; set; }

    [Column("note")]
    [StringLength(500)]
    public string? Note { get; set; }

    [Column("asset_account_id")]
    public int? AssetAccountId { get; set; }

    [Column("accumulated_depreciation_account_id")]
    public int? AccumulatedDepreciationAccountId { get; set; }

    [ForeignKey("AccumulatedDepreciationAccountId")]
    [InverseProperty("FaRevaluationDocLineAccumulatedDepreciationAccounts")]
    public virtual AccChartAccount? AccumulatedDepreciationAccount { get; set; }

    [ForeignKey("AssetAccountId")]
    [InverseProperty("FaRevaluationDocLineAssetAccounts")]
    public virtual AccChartAccount? AssetAccount { get; set; }

    [ForeignKey("FaAssetId")]
    [InverseProperty("FaRevaluationDocLines")]
    public virtual FaAsset FaAsset { get; set; } = null!;

    [ForeignKey("RevaluationDocId")]
    [InverseProperty("FaRevaluationDocLines")]
    public virtual FaRevaluationDoc RevaluationDoc { get; set; } = null!;
}
