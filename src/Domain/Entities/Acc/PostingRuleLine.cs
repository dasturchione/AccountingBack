namespace Domain.Entities;

public partial class PostingRuleLine
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

    public virtual PostingRule Rule { get; set; } = null!;
    public virtual ChartAccount? DebitAccount { get; set; }
    public virtual ChartAccount? CreditAccount { get; set; }
    public virtual State State { get; set; } = null!;
}
