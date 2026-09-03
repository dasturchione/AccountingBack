namespace Application.Features.PaymentAcceptancePointOperations;

public class PaymentAcceptancePointOperationBaseDto
{
    public int PaymentAcceptancePointId { get; set; }
    public short DirectionId { get; set; }
    public DateTime DocDate { get; set; }
    public short CurrencyId { get; set; }
    public decimal Amount { get; set; }
    public decimal ExchangeRate { get; set; } = 1m;
    public string? ExternalTransactionNumber { get; set; }
    public string? Comment { get; set; }
}
