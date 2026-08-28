namespace Application.Features.PaymentAcceptancePointOperations;

public sealed class PaymentAcceptancePointOperationListDto
{
    public long Id { get; set; }
    public int PaymentAcceptancePointId { get; set; }
    public string PaymentAcceptancePointCode { get; set; } = null!;
    public string PaymentAcceptancePointName { get; set; } = null!;
    public short DirectionId { get; set; }
    public string DirectionName { get; set; } = null!;
    public string DocNumber { get; set; } = null!;
    public DateTime DocDate { get; set; }
    public short CurrencyId { get; set; }
    public string CurrencyCode { get; set; } = null!;
    public decimal Amount { get; set; }
    public string? ExternalTransactionNumber { get; set; }
    public short StatusId { get; set; }
    public string StatusName { get; set; } = null!;
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
}
