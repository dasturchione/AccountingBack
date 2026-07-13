using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("cmn_mxik_catalog")]
[Index("EffectiveFrom", "EffectiveTo", Name = "idx_cmn_mxik_catalog_effective_dates")]
[Index("MxikCode", Name = "idx_cmn_mxik_catalog_mxik_code")]
[Index("StateId", Name = "idx_cmn_mxik_catalog_state_id")]
[Index("MxikCode", "EffectiveFrom", Name = "ux_cmn_mxik_catalog_code_effective_from", IsUnique = true)]
public partial class CmnDidoxMxikCatalog
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("mxik_code")]
    [StringLength(17)]
    public string MxikCode { get; set; } = null!;

    [Column("name")]
    [StringLength(500)]
    public string Name { get; set; } = null!;

    [Column("source_name")]
    [StringLength(100)]
    public string SourceName { get; set; } = null!;

    [Column("source_updated_at", TypeName = "timestamp without time zone")]
    public DateTime? SourceUpdatedAt { get; set; }

    [Column("effective_from")]
    public DateOnly EffectiveFrom { get; set; }

    [Column("effective_to")]
    public DateOnly? EffectiveTo { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("StateId")]
    public virtual CmnState State { get; set; } = null!;
}
