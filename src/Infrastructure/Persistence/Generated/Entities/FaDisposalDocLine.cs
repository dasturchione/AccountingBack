using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("fa_disposal_doc_line")]
[Index("DisposalDocId", Name = "idx_fa_disposal_doc_line_doc_id")]
[Index("FaAssetId", Name = "idx_fa_disposal_doc_line_fa_asset_id")]
[Index("DisposalDocId", "FaAssetId", Name = "ux_fa_disposal_doc_line_doc_asset", IsUnique = true)]
public partial class FaDisposalDocLine
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("disposal_doc_id")]
    public long DisposalDocId { get; set; }

    [Column("fa_asset_id")]
    public long FaAssetId { get; set; }

    [Column("book_value")]
    [Precision(18, 2)]
    public decimal BookValue { get; set; }

    [Column("sale_amount")]
    [Precision(18, 2)]
    public decimal SaleAmount { get; set; }

    [Column("gain_loss")]
    [Precision(18, 2)]
    public decimal GainLoss { get; set; }

    [Column("note")]
    [StringLength(500)]
    public string? Note { get; set; }

    [ForeignKey("DisposalDocId")]
    [InverseProperty("FaDisposalDocLines")]
    public virtual FaDisposalDoc DisposalDoc { get; set; } = null!;

    [ForeignKey("FaAssetId")]
    [InverseProperty("FaDisposalDocLines")]
    public virtual FaAsset FaAsset { get; set; } = null!;
}
