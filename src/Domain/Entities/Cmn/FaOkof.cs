using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_fa_okof")]
[Index("Code", Name = "idx_cmn_fa_okof_code", IsUnique = true)]
[Index("StateId", Name = "idx_cmn_fa_okof_state_id")]
public partial class FaOkof
{
    [Key]
    [Column("id")]
    public short Id { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("state_id")]
    public short StateId { get; set; }

    [InverseProperty("Okof")]
    public virtual ICollection<FaAsset> FaAssets { get; set; } = new List<FaAsset>();

    [ForeignKey("StateId")]
    [InverseProperty("FaOkofs")]
    public virtual State State { get; set; } = null!;
}
