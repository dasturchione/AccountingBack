namespace Domain.Entities;

public partial class AccountingRegisterEntry
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
}
