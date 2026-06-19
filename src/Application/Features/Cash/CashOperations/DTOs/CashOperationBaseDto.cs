namespace Application.Features.CashOperations;

public class CashOperationBaseDto
{
    public int CashBoxId { get; set; }
    public short OperationTypeId { get; set; }
    public short? PaymentTypeId { get; set; }
    public int? CounterpartyId { get; set; }
    public string DocNumber { get; set; } = null!;
    public DateTime DocDate { get; set; }
    public short CurrencyId { get; set; }
    public decimal Amount { get; set; }
    public string? Comment { get; set; }
}
