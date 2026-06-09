using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class InvProductPrice
{
    public long Id { get; set; }

    public int OrganizationId { get; set; }

    public int ProductId { get; set; }

    public short CurrencyId { get; set; }

    public decimal Price { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public short StateId { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual CmnCurrency Currency { get; set; } = null!;

    public virtual OrgOrganization Organization { get; set; } = null!;

    public virtual InvProduct Product { get; set; } = null!;

    public virtual CmnState State { get; set; } = null!;
}
