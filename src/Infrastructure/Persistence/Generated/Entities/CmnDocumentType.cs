using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class CmnDocumentType
{
    public short Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public short StateId { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<AccPostingRule> AccPostingRules { get; set; } = new List<AccPostingRule>();

    public virtual ICollection<AccRegEntry> AccRegEntries { get; set; } = new List<AccRegEntry>();

    public virtual ICollection<CounterpartyRegBalance> CounterpartyRegBalances { get; set; } = new List<CounterpartyRegBalance>();

    public virtual ICollection<InvRegBalance> InvRegBalances { get; set; } = new List<InvRegBalance>();

    public virtual ICollection<MoneyRegBalance> MoneyRegBalances { get; set; } = new List<MoneyRegBalance>();

    public virtual CmnState State { get; set; } = null!;
}
