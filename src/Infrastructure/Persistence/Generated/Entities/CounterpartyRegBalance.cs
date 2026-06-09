using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class CounterpartyRegBalance
{
    public long Id { get; set; }

    public int OrganizationId { get; set; }

    public short DocumentTypeId { get; set; }

    public long DocumentId { get; set; }

    public int CounterpartyId { get; set; }

    public short OperationTypeId { get; set; }

    public short CurrencyId { get; set; }

    public decimal Amount { get; set; }

    public DateTime DocDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual CounterpartyCard Counterparty { get; set; } = null!;

    public virtual CmnCurrency Currency { get; set; } = null!;

    public virtual CmnDocumentType DocumentType { get; set; } = null!;

    public virtual CmnOperationType OperationType { get; set; } = null!;

    public virtual OrgOrganization Organization { get; set; } = null!;
}
