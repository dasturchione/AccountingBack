using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("cmn_district")]
[Index("RegionId", Name = "idx_cmn_district_region_id")]
public partial class CmnDistrict
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("short_name")]
    [StringLength(250)]
    public string ShortName { get; set; } = null!;

    [Column("full_name")]
    [StringLength(250)]
    public string FullName { get; set; } = null!;

    [Column("region_id")]
    public int RegionId { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [InverseProperty("District")]
    public virtual ICollection<CounterpartyCard> CounterpartyCards { get; set; } = new List<CounterpartyCard>();

    [InverseProperty("District")]
    public virtual ICollection<OrgBranch> OrgBranches { get; set; } = new List<OrgBranch>();

    [InverseProperty("District")]
    public virtual ICollection<OrgOrganization> OrgOrganizations { get; set; } = new List<OrgOrganization>();
}
