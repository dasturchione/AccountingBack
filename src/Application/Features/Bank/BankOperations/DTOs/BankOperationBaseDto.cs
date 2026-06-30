namespace Application.Features.BankOperations;

public class BankOperationBaseDto
{
    public int BankAccountId { get; set; }
    public short OperationTypeId { get; set; }
    public int? CounterpartyId { get; set; }
    public DateTime DocDate { get; set; }
    public short CurrencyId { get; set; }
    public decimal Amount { get; set; }
    public decimal ExchangeRate { get; set; } = 1m;
    public string? Comment { get; set; }
}
