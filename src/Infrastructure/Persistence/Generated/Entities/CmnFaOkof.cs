using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("cmn_fa_okof")]
[Index("Code", Name = "idx_cmn_fa_okof_code", IsUnique = true)]
[Index("StateId", Name = "idx_cmn_fa_okof_state_id")]
public partial class CmnFaOkof
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

    [InverseProperty("Okof")]
    public virtual ICollection<FaReceiptDocAsset> FaReceiptDocAssets { get; set; } = new List<FaReceiptDocAsset>();

    [ForeignKey("StateId")]
    [InverseProperty("CmnFaOkofs")]
    public virtual CmnState State { get; set; } = null!;
}
