using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class CashOperation
{
    public long Id { get; set; }

    public int OrganizationId { get; set; }

    public int CashBoxId { get; set; }

    public short OperationTypeId { get; set; }

    public short? PaymentTypeId { get; set; }

    public int? CounterpartyId { get; set; }

    public string DocNumber { get; set; } = null!;

    public DateTime DocDate { get; set; }

    public short CurrencyId { get; set; }

    public decimal Amount { get; set; }

    public string? Comment { get; set; }

    public short StatusId { get; set; }

    public short StateId { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual CashBox CashBox { get; set; } = null!;

    public virtual CounterpartyCard? Counterparty { get; set; }

    public virtual CmnCurrency Currency { get; set; } = null!;

    public virtual CmnOperationType OperationType { get; set; } = null!;

    public virtual OrgOrganization Organization { get; set; } = null!;

    public virtual CmnPaymentType? PaymentType { get; set; }

    public virtual CmnState State { get; set; } = null!;

    public virtual CmnDocumentStatus Status { get; set; } = null!;
}
