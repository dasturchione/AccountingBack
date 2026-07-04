using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("cmn_region")]
public partial class CmnRegion
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

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [InverseProperty("Region")]
    public virtual ICollection<CounterpartyCard> CounterpartyCards { get; set; } = new List<CounterpartyCard>();

    [InverseProperty("Region")]
    public virtual ICollection<OrgBranch> OrgBranches { get; set; } = new List<OrgBranch>();

    [InverseProperty("Region")]
    public virtual ICollection<OrgOrganization> OrgOrganizations { get; set; } = new List<OrgOrganization>();

    [ForeignKey("StateId")]
    [InverseProperty("CmnRegions")]
    public virtual CmnState State { get; set; } = null!;
}
