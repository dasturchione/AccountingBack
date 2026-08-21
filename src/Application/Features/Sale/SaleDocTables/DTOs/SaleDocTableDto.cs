using System.Text.Json.Serialization;

namespace Application.Features.SaleDocTables;

public class SaleDocTableDto
{
    public long Id { get; set; }
    public long? OwnerId { get; set; }
    public int ProductTableId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public decimal CostPrice { get; set; }
    public decimal Amount { get; set; }
    public short? VatRateId { get; set; }
    public string? VatRateName { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }

    [JsonIgnore]
    public string? MarkingNumber { get; set; }
    [JsonIgnore]
    public string? SerialNumber { get; set; }
    public bool HasMarking { get; set; }
    public int MarkingCount { get; set; }
    public string? VerificationState { get; set; }
    public string? SourceType { get; set; }
}
