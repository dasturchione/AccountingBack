using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class PurDoc
{
    public long Id { get; set; }

    public int OrganizationId { get; set; }

    public string DocNumber { get; set; } = null!;

    public DateTime DocDate { get; set; }

    public int CounterpartyId { get; set; }

    public int WarehouseId { get; set; }

    public short CurrencyId { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal VatAmount { get; set; }

    public decimal FinalAmount { get; set; }

    public short StatusId { get; set; }

    public string? Comment { get; set; }

    public short StateId { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual CounterpartyCard Counterparty { get; set; } = null!;

    public virtual CmnCurrency Currency { get; set; } = null!;

    public virtual OrgOrganization Organization { get; set; } = null!;

    public virtual ICollection<PurDocTable> PurDocTables { get; set; } = new List<PurDocTable>();

    public virtual CmnState State { get; set; } = null!;

    public virtual CmnDocumentStatus Status { get; set; } = null!;

    public virtual InvWarehouse Warehouse { get; set; } = null!;
}
