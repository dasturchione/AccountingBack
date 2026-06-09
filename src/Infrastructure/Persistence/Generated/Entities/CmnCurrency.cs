using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class CmnCurrency
{
    public short Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Symbol { get; set; }

    public short StateId { get; set; }

    public virtual ICollection<AccRegEntry> AccRegEntries { get; set; } = new List<AccRegEntry>();

    public virtual ICollection<BankOperation> BankOperations { get; set; } = new List<BankOperation>();

    public virtual ICollection<CashBox> CashBoxes { get; set; } = new List<CashBox>();

    public virtual ICollection<CashOperation> CashOperations { get; set; } = new List<CashOperation>();

    public virtual ICollection<CounterpartyBankAccount> CounterpartyBankAccounts { get; set; } = new List<CounterpartyBankAccount>();

    public virtual ICollection<CounterpartyRegBalance> CounterpartyRegBalances { get; set; } = new List<CounterpartyRegBalance>();

    public virtual ICollection<InvProductPrice> InvProductPrices { get; set; } = new List<InvProductPrice>();

    public virtual ICollection<MoneyRegBalance> MoneyRegBalances { get; set; } = new List<MoneyRegBalance>();

    public virtual ICollection<OrgBankAccount> OrgBankAccounts { get; set; } = new List<OrgBankAccount>();

    public virtual ICollection<PurDoc> PurDocs { get; set; } = new List<PurDoc>();

    public virtual ICollection<SaleDoc> SaleDocs { get; set; } = new List<SaleDoc>();

    public virtual CmnState State { get; set; } = null!;
}
