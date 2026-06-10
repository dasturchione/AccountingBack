namespace Domain.Entities;

public partial class AccountingRegisterEntry
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public short DocumentTypeId { get; set; }
    public long DocumentId { get; set; }
    public int? DebitAccountId { get; set; }
    public int? CreditAccountId { get; set; }
    public short? OperationTypeId { get; set; }
    public short CurrencyId { get; set; }
    public decimal Amount { get; set; }
    public decimal? DebitQuantity { get; set; }
    public decimal? CreditQuantity { get; set; }
    public string? Content { get; set; }
    public string? JournalNumber { get; set; }
    public DateTime DocDate { get; set; }
    public DateTime CreatedDate { get; set; }

    public virtual Organization Organization { get; set; } = null!;
    public virtual DocumentType DocumentType { get; set; } = null!;
    public virtual ChartAccount? DebitAccount { get; set; }
    public virtual ChartAccount? CreditAccount { get; set; }
    public virtual OperationType? OperationType { get; set; }
    public virtual Currency Currency { get; set; } = null!;
    public virtual ICollection<RegisterEntrySubkonto> RegisterEntrySubkontos { get; set; } = new List<RegisterEntrySubkonto>();
}
