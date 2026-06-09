using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class InvWarehouse
{
    public int Id { get; set; }

    public int OrganizationId { get; set; }

    public int? BranchId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int? ResponsibleUserId { get; set; }

    public short StateId { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual OrgBranch? Branch { get; set; }

    public virtual ICollection<InvRegBalance> InvRegBalances { get; set; } = new List<InvRegBalance>();

    public virtual OrgOrganization Organization { get; set; } = null!;

    public virtual ICollection<PurDoc> PurDocs { get; set; } = new List<PurDoc>();

    public virtual SysUser? ResponsibleUser { get; set; }

    public virtual ICollection<SaleDoc> SaleDocs { get; set; } = new List<SaleDoc>();

    public virtual CmnState State { get; set; } = null!;
}
