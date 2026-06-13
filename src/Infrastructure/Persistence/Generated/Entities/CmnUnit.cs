using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("cmn_unit")]
[Index("Code", Name = "idx_cmn_unit_code", IsUnique = true)]
public partial class CmnUnit
{
    [Key]
    [Column("id")]
    public short Id { get; set; }

    [Column("code")]
    [StringLength(20)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column("state_id")]
    public short StateId { get; set; }

    [InverseProperty("Unit")]
    public virtual ICollection<InvProduct> InvProducts { get; set; } = new List<InvProduct>();

    [ForeignKey("StateId")]
    [InverseProperty("CmnUnits")]
    public virtual CmnState State { get; set; } = null!;
}
