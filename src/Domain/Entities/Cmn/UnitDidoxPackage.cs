using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_unit_didox_package")]
[Index("UnitId", Name = "idx_cmn_unit_didox_package_unit_id")]
[Index("EffectiveFrom", "EffectiveTo", Name = "idx_cmn_unit_didox_package_effective_dates")]
[Index("StateId", Name = "idx_cmn_unit_didox_package_state_id")]
[Index("UnitId", "EffectiveFrom", Name = "ux_cmn_unit_didox_package_unit_effective_from", IsUnique = true)]
public partial class UnitDidoxPackage
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("unit_id")]
    public short UnitId { get; set; }

    [Column("package_code")]
    [StringLength(50)]
    public string PackageCode { get; set; } = null!;

    [Column("package_name")]
    [StringLength(250)]
    public string PackageName { get; set; } = null!;

    [Column("effective_from")]
    public DateOnly EffectiveFrom { get; set; }

    [Column("effective_to")]
    public DateOnly? EffectiveTo { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey(nameof(StateId))]
    public virtual State State { get; set; } = null!;

    [ForeignKey(nameof(UnitId))]
    [InverseProperty(nameof(Unit.DidoxPackages))]
    public virtual Unit Unit { get; set; } = null!;
}
