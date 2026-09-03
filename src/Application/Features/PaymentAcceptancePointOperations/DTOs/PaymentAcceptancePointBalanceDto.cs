namespace Application.Features.PaymentAcceptancePointOperations;

public sealed class PaymentAcceptancePointBalanceDto
{
    public int PaymentAcceptancePointId { get; set; }
    public short CurrencyId { get; set; }
    public DateTime AsOfDate { get; set; }
    public decimal Balance { get; set; }
}
