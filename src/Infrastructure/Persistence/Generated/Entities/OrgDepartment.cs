using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class OrgDepartment
{
    public int Id { get; set; }

    public int OrganizationId { get; set; }

    public int? BranchId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public short StateId { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual OrgBranch? Branch { get; set; }

    public virtual OrgOrganization Organization { get; set; } = null!;

    public virtual CmnState State { get; set; } = null!;
}
