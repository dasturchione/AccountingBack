namespace Domain.Entities;

public partial class BankOperation
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public int BankAccountId { get; set; }
    public short OperationTypeId { get; set; }
    public short? PaymentTypeId { get; set; }
    public int? CounterpartyId { get; set; }
    public string DocNumber { get; set; } = null!;
    public DateTime DocDate { get; set; }
    public short CurrencyId { get; set; }
    public decimal Amount { get; set; }
    public string? Comment { get; set; }
    public short StatusId { get; set; }
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
}
