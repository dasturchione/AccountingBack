namespace Application.Features.CashCollections;

public class CashCollectionBaseDto
{
    public int CashBoxId { get; set; }
    public int BankAccountId { get; set; }
    public DateTime DocDate { get; set; }
    public short CurrencyId { get; set; }
    public decimal Amount { get; set; }
    public decimal ExchangeRate { get; set; } = 1m;
    public int? CashChartAccountId { get; set; }
    public int? CashInTransitAccountId { get; set; }
    public int? BankChartAccountId { get; set; }
    public string? Comment { get; set; }
}

public sealed class CashCollectionCreateDto : CashCollectionBaseDto;

public sealed class CashCollectionUpdateDto : CashCollectionBaseDto;

public class CashCollectionDto
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public string DocNumber { get; set; } = null!;
    public DateTime DocDate { get; set; }
    public int CashBoxId { get; set; }
    public string CashBoxName { get; set; } = null!;
    public int BankAccountId { get; set; }
    public string BankAccountNumber { get; set; } = null!;
    public string BankName { get; set; } = null!;
    public short CurrencyId { get; set; }
    public string CurrencyCode { get; set; } = null!;
    public string CurrencyName { get; set; } = null!;
    public decimal Amount { get; set; }
    public decimal ExchangeRate { get; set; }
    public int? CashChartAccountId { get; set; }
    public string? CashChartAccountNumber { get; set; }
    public string? CashChartAccountName { get; set; }
    public int? CashInTransitAccountId { get; set; }
    public string? CashInTransitAccountNumber { get; set; }
    public string? CashInTransitAccountName { get; set; }
    public int? BankChartAccountId { get; set; }
    public string? BankChartAccountNumber { get; set; }
    public string? BankChartAccountName { get; set; }
    public long? BankOperationId { get; set; }
    public short StatusId { get; set; }
    public string StatusName { get; set; } = null!;
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public string? Comment { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? InTransitAt { get; set; }
    public int? InTransitByUserId { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? CompletedByUserId { get; set; }
    public DateTime? CancelledAt { get; set; }
    public int? CancelledByUserId { get; set; }
    public short? CancelledFromStatusId { get; set; }
}

public sealed class CashCollectionListDto : CashCollectionDto;

public sealed class CashCollectionInTransitDto
{
    public long Id { get; set; }
    public string DocNumber { get; set; } = null!;
    public DateTime DocDate { get; set; }
    public int CashBoxId { get; set; }
    public string CashBoxName { get; set; } = null!;
    public int BankAccountId { get; set; }
    public string BankAccountNumber { get; set; } = null!;
    public string BankName { get; set; } = null!;
    public short CurrencyId { get; set; }
    public string CurrencyCode { get; set; } = null!;
    public decimal Amount { get; set; }
}
