using SharedKernel.Constants;
using System.Text.Json.Serialization;

namespace Application.Features.BankParsers;

public class BankExportDto
{
    [JsonPropertyName("accounts")]
    public List<AccountStatementDto> Accounts { get; set; } = new();
}

public class AccountStatementDto
{
    [JsonPropertyName("bankMfo")]
    public string BankMfo { get; set; } = "";

    [JsonPropertyName("bankName")]
    public string BankName { get; set; } = "";

    [JsonPropertyName("accountNumber")]
    public string AccountNumber { get; set; } = "";

    [JsonPropertyName("bankAccountId")]
    public int? BankAccountId { get; set; }

    [JsonPropertyName("bankId")]
    public int? BankId { get; set; }

    [JsonPropertyName("bankBranchId")]
    public int? BankBranchId { get; set; }

    [JsonPropertyName("bankInn")]
    public string? BankInn { get; set; }

    [JsonPropertyName("companyName")]
    public string CompanyName { get; set; } = "";

    [JsonPropertyName("companyInn")]
    public string CompanyInn { get; set; } = "";

    [JsonPropertyName("periodFrom")]
    public DateTime PeriodFrom { get; set; }

    [JsonPropertyName("periodTo")]
    public DateTime PeriodTo { get; set; }

    [JsonPropertyName("openingBalance")]
    public decimal OpeningBalance { get; set; }

    [JsonPropertyName("closingBalance")]
    public decimal ClosingBalance { get; set; }

    [JsonPropertyName("totalDebit")]
    public decimal TotalDebit { get; set; }

    [JsonPropertyName("totalCredit")]
    public decimal TotalCredit { get; set; }

    [JsonPropertyName("hasActivity")]
    public bool HasActivity { get; set; }

    [JsonPropertyName("transactions")]
    public List<TransactionDto> Transactions { get; set; } = new();
}

public class TransactionDto
{
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("docNumber")]
    public string DocNumber { get; set; } = "";

    [JsonPropertyName("bankDocumentNumber")]
    public string BankDocumentNumber { get; set; } = "";

    [JsonPropertyName("directionId")]
    public short DirectionId => Debit > Credit ? MovementDirectionIdConst.IN : MovementDirectionIdConst.OUT;

    [JsonPropertyName("operationCode")]
    public string OperationCode { get; set; } = "";

    [JsonPropertyName("mfoCounterparty")]
    public string MfoCounterparty { get; set; } = "";

    [JsonPropertyName("counterpartyAccount")]
    public string CounterpartyAccount { get; set; } = "";

    [JsonPropertyName("counterpartyInn")]
    public string CounterpartyInn { get; set; } = "";

    [JsonPropertyName("counterpartyName")]
    public string CounterpartyName { get; set; } = "";

    [JsonPropertyName("counterpartyId")]
    public int? CounterpartyId { get; set; }

    [JsonPropertyName("counterpartyBankAccountId")]
    public int? CounterpartyBankAccountId { get; set; }

    [JsonPropertyName("debit")]
    public decimal Debit { get; set; }

    [JsonPropertyName("credit")]
    public decimal Credit { get; set; }

    [JsonPropertyName("purpose")]
    public string Purpose { get; set; } = "";

    [JsonPropertyName("direction")]
    public string Direction { get; set; } = "";

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("classificationCategoryId")]
    public short? ClassificationCategoryId { get; set; }

    [JsonPropertyName("classificationCode")]
    public string? ClassificationCode { get; set; }

    [JsonPropertyName("classificationName")]
    public string? ClassificationName { get; set; }

    [JsonPropertyName("classificationRuleId")]
    public int? ClassificationRuleId { get; set; }

    [JsonPropertyName("classificationRuleCode")]
    public string? ClassificationRuleCode { get; set; }

    [JsonPropertyName("requiresReview")]
    public bool RequiresReview { get; set; }

    //[JsonPropertyName("paymentPurposeHints")]
}
