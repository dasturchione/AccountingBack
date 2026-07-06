using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_fa_asset_status")]
[Index("Code", Name = "cmn_fa_asset_status_code_key", IsUnique = true)]
public partial class FaAssetStatus
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

    [InverseProperty("Status")]
    public virtual ICollection<FaAsset> FaAssets { get; set; } = new List<FaAsset>();

    [ForeignKey("StateId")]
    [InverseProperty("FaAssetStatuses")]
    public virtual State State { get; set; } = null!;
}
