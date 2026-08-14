using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_fa_depreciation_method")]
[Index("Code", Name = "idx_cmn_fa_depreciation_method_code", IsUnique = true)]
[Index("StateId", Name = "idx_cmn_fa_depreciation_method_state_id")]
public partial class FaDepreciationMethod
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

    [InverseProperty(nameof(FaAssetAccounting.DepreciationMethod))]
    public virtual ICollection<FaAssetAccounting> FaAssetAccountings { get; set; } = new List<FaAssetAccounting>();

    [InverseProperty(nameof(FaCommissioningDocLine.DepreciationMethod))]
    public virtual ICollection<FaCommissioningDocLine> FaCommissioningDocLines { get; set; } = new List<FaCommissioningDocLine>();

    [ForeignKey("StateId")]
    [InverseProperty("FaDepreciationMethods")]
    public virtual State State { get; set; } = null!;
}
