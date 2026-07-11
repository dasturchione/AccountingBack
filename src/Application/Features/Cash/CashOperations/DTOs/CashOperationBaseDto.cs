namespace Application.Features.CashOperations;

public class CashOperationBaseDto
{
    public int CashBoxId { get; set; }
    public int? DestinationCashBoxId { get; set; }
    public short OperationTypeId { get; set; }
    public short? PaymentTypeId { get; set; }
    public int? CashChartAccountId { get; set; }
    public int? OffsetAccountId { get; set; }
    public int? CounterpartyId { get; set; }
    public DateTime DocDate { get; set; }
    public short CurrencyId { get; set; }
    public decimal Amount { get; set; }
    public decimal ExchangeRate { get; set; } = 1m;
    public string? Comment { get; set; }
}
