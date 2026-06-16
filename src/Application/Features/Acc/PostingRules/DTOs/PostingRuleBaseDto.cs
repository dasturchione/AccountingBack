namespace Application.Features.Acc.PostingRules
{
    public class PostingRuleBaseDto
    {
        public short? OperationTypeId { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
    }

    public class PostingRuleLineBaseDto
    {
        public int SortOrder { get; set; }
        public int? DebitAccountId { get; set; }
        public int? CreditAccountId { get; set; }
        public string AmountSource { get; set; } = null!;
        public string? QuantitySource { get; set; }
        public string? ContentTemplate { get; set; }
    }
}
