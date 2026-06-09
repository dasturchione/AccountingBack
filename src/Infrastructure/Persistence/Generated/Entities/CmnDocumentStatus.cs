using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class CmnDocumentStatus
{
    public short Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public short StateId { get; set; }

    public virtual ICollection<BankOperation> BankOperations { get; set; } = new List<BankOperation>();

    public virtual ICollection<CashOperation> CashOperations { get; set; } = new List<CashOperation>();

    public virtual ICollection<PurDoc> PurDocs { get; set; } = new List<PurDoc>();

    public virtual ICollection<SaleDoc> SaleDocs { get; set; } = new List<SaleDoc>();

    public virtual CmnState State { get; set; } = null!;
}
