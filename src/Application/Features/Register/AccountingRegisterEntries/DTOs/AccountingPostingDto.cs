namespace Application.Features.Register.AccountingRegisterEntries
{
    public class AccountingPostingDto
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
        public long? PostingBatchId { get; set; }
        public long? SourceLineId { get; set; }
        public long? ReversalEntryId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? DebitAccountName { get; set; }
        public string? CreditAccountName { get; set; }
        public string? DebitAccountCode { get; set; }
        public string? DebitAccountNumber { get; set; }
        public string? CreditAccountCode { get; set; }
        public string? CreditAccountNumber { get; set; }
        public string CurrencyName { get; set; } = null!;
        public string CurrencyCode { get; set; } = null!;
        public decimal? DebitQuantity { get; set; }
        public decimal? CreditQuantity { get; set; }
        public List<AccountingPostingTableDto> Tables { get; set; } = new();
    }

    public class AccountingPostingTableDto
    {
        public long Id { get; set; }
        public long EntryId { get; set; }
        public string Side { get; set; } = null!;
        public short SubkontoTypeId { get; set; }
        public string SubkontoTypeCode { get; set; } = null!;
        public string SubkontoTypeName { get; set; } = null!;
        public int SortOrder { get; set; }
        public long? EntityId { get; set; }
        public string? DisplayValue { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
