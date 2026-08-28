namespace Application.Features.BankOperations;

public class BankOperationDto
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public int BankAccountId { get; set; }
    public string BankAccountNumber { get; set; } = null!;
    public string BankAccountName { get; set; } = null!;
    public string? BankAccountInn { get; set; }
    public int BankId { get; set; }
    public string BankName { get; set; } = null!;
    public string? BankMfo { get; set; }
    public string? BankInn { get; set; }
    public short DirectionId { get; set; }
    public string DirectionName { get; set; } = null!;
    public short? PaymentTypeId { get; set; }
    public string? PaymentTypeName { get; set; }
    public int? BankChartAccountId { get; set; }
    public string? BankChartAccountNumber { get; set; }
    public string? BankChartAccountName { get; set; }
    public int? OffsetAccountId { get; set; }
    public string? OffsetAccountNumber { get; set; }
    public string? OffsetAccountName { get; set; }
    public int? CounterpartyId { get; set; }
    public int? CounterpartyBankAccountId { get; set; }
    public string? CounterpartyBankAccountNumber { get; set; }
    public string? CounterpartyName { get; set; }
    public string? CounterpartyInn { get; set; }
    public string DocNumber { get; set; } = null!;
    public string? BankDocumentNumber { get; set; }
    public short? ClassificationCategoryId { get; set; }
    public string? ClassificationCode { get; set; }
    public string? ClassificationName { get; set; }
    public int? ClassificationRuleId { get; set; }
    public string? ClassificationRuleCode { get; set; }
    public long? RelatedDocumentId { get; set; }
    public short? RelatedDocumentTypeId { get; set; }
    public string? RelatedDocumentTypeName { get; set; }
    public long? RelatedDocumentEntityId { get; set; }
    public string? RelatedDocumentNumber { get; set; }
    public DateTime? RelatedDocumentDate { get; set; }
    public DateTime DocDate { get; set; }
    public short CurrencyId { get; set; }
    public string CurrencyName { get; set; } = null!;
    public decimal Amount { get; set; }
    public decimal ExchangeRate { get; set; }
    public DateTime? PostedAt { get; set; }
    public int? PostedByUserId { get; set; }
    public DateTime? CancelledAt { get; set; }
    public int? CancelledByUserId { get; set; }
    public string? Comment { get; set; }
    public short StatusId { get; set; }
    public string StatusName { get; set; } = null!;
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public long? ContractId { get; set; }
    public string? ContractNumber { get; set; }
}
