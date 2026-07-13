namespace Application.Features.Register.PostingEngines
{
    public class PostingEntryContext
    {
        public int? DebitAccountId { get; set; }
        public int? CreditAccountId { get; set; }
        public decimal Amount { get; set; }
        public decimal? DebitQuantity { get; set; }
        public decimal? CreditQuantity { get; set; }
        public string? Content { get; set; }
        public long? SourceLineId { get; set; }
    }
}
