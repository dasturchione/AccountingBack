using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class CashBox
{
    public int Id { get; set; }

    public int OrganizationId { get; set; }

    public int? BranchId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public short CurrencyId { get; set; }

    public short StateId { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual OrgBranch? Branch { get; set; }

    public virtual ICollection<CashOperation> CashOperations { get; set; } = new List<CashOperation>();

    public virtual CmnCurrency Currency { get; set; } = null!;

    public virtual OrgOrganization Organization { get; set; } = null!;

    public virtual CmnState State { get; set; } = null!;
}
