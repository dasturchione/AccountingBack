using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class AccPostingRuleLine
{
    public int Id { get; set; }

    public int RuleId { get; set; }

    public int SortOrder { get; set; }

    public int? DebitAccountId { get; set; }

    public int? CreditAccountId { get; set; }

    public string AmountSource { get; set; } = null!;

    public string? QuantitySource { get; set; }

    public string? ContentTemplate { get; set; }

    public short StateId { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual AccChartAccount? CreditAccount { get; set; }

    public virtual AccChartAccount? DebitAccount { get; set; }

    public virtual AccPostingRule Rule { get; set; } = null!;

    public virtual CmnState State { get; set; } = null!;
}
