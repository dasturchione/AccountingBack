using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("cmn_fa_depreciation_method")]
[Index("Code", Name = "idx_cmn_fa_depreciation_method_code", IsUnique = true)]
[Index("StateId", Name = "idx_cmn_fa_depreciation_method_state_id")]
public partial class CmnFaDepreciationMethod
{
    [Key]
    [Column("id")]
    public short Id { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    [Column("state_id")]
    public short StateId { get; set; }

    [InverseProperty("DepreciationMethod")]
    public virtual ICollection<FaAsset> FaAssets { get; set; } = new List<FaAsset>();

    [InverseProperty("DepreciationMethod")]
    public virtual ICollection<FaReceiptDocAsset> FaReceiptDocAssets { get; set; } = new List<FaReceiptDocAsset>();

    [ForeignKey("StateId")]
    [InverseProperty("CmnFaDepreciationMethods")]
    public virtual CmnState State { get; set; } = null!;
}
