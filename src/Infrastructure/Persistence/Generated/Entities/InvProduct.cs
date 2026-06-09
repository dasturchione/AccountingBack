using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class InvProduct
{
    public int Id { get; set; }

    public int OrganizationId { get; set; }

    public int? ProductGroupId { get; set; }

    public short UnitId { get; set; }

    public string Code { get; set; } = null!;

    public string? Barcode { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsService { get; set; }

    public short StateId { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<InvProductPrice> InvProductPrices { get; set; } = new List<InvProductPrice>();

    public virtual ICollection<InvRegBalance> InvRegBalances { get; set; } = new List<InvRegBalance>();

    public virtual OrgOrganization Organization { get; set; } = null!;

    public virtual InvProductGroup? ProductGroup { get; set; }

    public virtual ICollection<PurDocTable> PurDocTables { get; set; } = new List<PurDocTable>();

    public virtual ICollection<SaleDocTable> SaleDocTables { get; set; } = new List<SaleDocTable>();

    public virtual CmnState State { get; set; } = null!;

    public virtual CmnUnit Unit { get; set; } = null!;
}
