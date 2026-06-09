using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class InvRegBalance
{
    public long Id { get; set; }

    public int OrganizationId { get; set; }

    public short DocumentTypeId { get; set; }

    public long DocumentId { get; set; }

    public int WarehouseId { get; set; }

    public int ProductId { get; set; }

    public short OperationTypeId { get; set; }

    public decimal Quantity { get; set; }

    public decimal Amount { get; set; }

    public DateTime DocDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual CmnDocumentType DocumentType { get; set; } = null!;

    public virtual CmnOperationType OperationType { get; set; } = null!;

    public virtual OrgOrganization Organization { get; set; } = null!;

    public virtual InvProduct Product { get; set; } = null!;

    public virtual InvWarehouse Warehouse { get; set; } = null!;
}
