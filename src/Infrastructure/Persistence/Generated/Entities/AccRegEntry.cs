using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class AccRegEntry
{
    public long Id { get; set; }

    public int OrganizationId { get; set; }

    public short DocumentTypeId { get; set; }

    public long DocumentId { get; set; }

    public int? DebitAccountId { get; set; }

    public int? CreditAccountId { get; set; }

    public short CurrencyId { get; set; }

    public decimal Amount { get; set; }

    public DateTime DocDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public short? OperationTypeId { get; set; }

    public decimal? DebitQuantity { get; set; }

    public decimal? CreditQuantity { get; set; }

    public string? Content { get; set; }

    public string? JournalNumber { get; set; }

    public virtual ICollection<AccRegEntrySubkonto> AccRegEntrySubkontos { get; set; } = new List<AccRegEntrySubkonto>();

    public virtual AccChartAccount? CreditAccount { get; set; }

    public virtual CmnCurrency Currency { get; set; } = null!;

    public virtual AccChartAccount? DebitAccount { get; set; }

    public virtual CmnDocumentType DocumentType { get; set; } = null!;

    public virtual CmnOperationType? OperationType { get; set; }

    public virtual OrgOrganization Organization { get; set; } = null!;
}
