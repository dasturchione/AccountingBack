using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class InvProductTable
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int OrganizationId { get; set; }

    public string Name { get; set; } = null!;

    public string? Code { get; set; }

    public string? Barcode { get; set; }

    public short StateId { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual OrgOrganization Organization { get; set; } = null!;

    public virtual InvProduct Product { get; set; } = null!;

    public virtual ICollection<PurDocTable> PurDocTables { get; set; } = new List<PurDocTable>();

    public virtual ICollection<SaleDocTable> SaleDocTables { get; set; } = new List<SaleDocTable>();

    public virtual CmnState State { get; set; } = null!;
}
