namespace Application.Features.BankOperations;

public class BankOperationBaseDto
{
    public int BankAccountId { get; set; }
    public short DirectionId { get; set; }
    public short? PaymentTypeId { get; set; }
    public int? BankChartAccountId { get; set; }
    public int? OffsetAccountId { get; set; }
    public int? CounterpartyId { get; set; }
    public int? CounterpartyBankAccountId { get; set; }
    public string? BankDocumentNumber { get; set; }
    public short? ClassificationCategoryId { get; set; }
    public int? ClassificationRuleId { get; set; }
    public DateTime DocDate { get; set; }
    public short CurrencyId { get; set; }
    public decimal Amount { get; set; }
    public decimal ExchangeRate { get; set; } = 1m;
    public string? Comment { get; set; }
    public long? ContractId { get; set; }
}
