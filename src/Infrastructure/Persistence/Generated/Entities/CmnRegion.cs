using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class CmnRegion
{
    public int Id { get; set; }

    public string ShortName { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public short StateId { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<CounterpartyCard> CounterpartyCards { get; set; } = new List<CounterpartyCard>();

    public virtual ICollection<OrgBranch> OrgBranches { get; set; } = new List<OrgBranch>();

    public virtual ICollection<OrgOrganization> OrgOrganizations { get; set; } = new List<OrgOrganization>();

    public virtual CmnState State { get; set; } = null!;
}
