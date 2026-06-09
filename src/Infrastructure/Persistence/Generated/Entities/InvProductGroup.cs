using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class InvProductGroup
{
    public int Id { get; set; }

    public int OrganizationId { get; set; }

    public int? ParentId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public short StateId { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<InvProduct> InvProducts { get; set; } = new List<InvProduct>();

    public virtual ICollection<InvProductGroup> InverseParent { get; set; } = new List<InvProductGroup>();

    public virtual OrgOrganization Organization { get; set; } = null!;

    public virtual InvProductGroup? Parent { get; set; }

    public virtual CmnState State { get; set; } = null!;
}
