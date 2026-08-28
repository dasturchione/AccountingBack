namespace Application.Features.PaymentAcceptancePointOperations;

public sealed class PaymentAcceptancePointOperationDto
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public int PaymentAcceptancePointId { get; set; }
    public string PaymentAcceptancePointCode { get; set; } = null!;
    public string PaymentAcceptancePointName { get; set; } = null!;
    public short DirectionId { get; set; }
    public string DirectionName { get; set; } = null!;
    public string DocNumber { get; set; } = null!;
    public DateTime DocDate { get; set; }
    public short CurrencyId { get; set; }
    public string CurrencyCode { get; set; } = null!;
    public string CurrencyName { get; set; } = null!;
    public decimal Amount { get; set; }
    public decimal ExchangeRate { get; set; }
    public string? ExternalTransactionNumber { get; set; }
    public string? Comment { get; set; }
    public short StatusId { get; set; }
    public string StatusName { get; set; } = null!;
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public DateTime? PostedAt { get; set; }
    public int? PostedByUserId { get; set; }
    public DateTime? CancelledAt { get; set; }
    public int? CancelledByUserId { get; set; }
}
