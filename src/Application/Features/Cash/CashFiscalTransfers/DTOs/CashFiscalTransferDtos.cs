namespace Application.Features.CashFiscalTransfers;

public class CashFiscalTransferBaseDto
{
    public int FiscalCashRegisterId { get; set; }
    public int CashBoxId { get; set; }

    /// <summary>-1: fiscal register to cash box; 1: cash box to fiscal register.</summary>
    public short DirectionId { get; set; }

    public DateTime DocDate { get; set; }
    public short CurrencyId { get; set; }
    public decimal Amount { get; set; }
    public decimal ExchangeRate { get; set; } = 1m;
    public int? FiscalCashAccountId { get; set; }
    public int? CashBoxAccountId { get; set; }
    public string? Comment { get; set; }
}

public sealed class CashFiscalTransferCreateDto : CashFiscalTransferBaseDto;
public sealed class CashFiscalTransferUpdateDto : CashFiscalTransferBaseDto;

public class CashFiscalTransferDto
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public string DocNumber { get; set; } = null!;
    public DateTime DocDate { get; set; }
    public int FiscalCashRegisterId { get; set; }
    public string FiscalCashRegisterName { get; set; } = null!;
    public int CashBoxId { get; set; }
    public string CashBoxName { get; set; } = null!;
    public short DirectionId { get; set; }
    public string DirectionCode { get; set; } = null!;
    public string DirectionName { get; set; } = null!;
    public short CurrencyId { get; set; }
    public string CurrencyName { get; set; } = null!;
    public decimal Amount { get; set; }
    public decimal ExchangeRate { get; set; }
    public int? FiscalCashAccountId { get; set; }
    public string? FiscalCashAccountNumber { get; set; }
    public string? FiscalCashAccountName { get; set; }
    public int? CashBoxAccountId { get; set; }
    public string? CashBoxAccountNumber { get; set; }
    public string? CashBoxAccountName { get; set; }
    public short StatusId { get; set; }
    public string StatusName { get; set; } = null!;
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public string? Comment { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? PostedAt { get; set; }
    public int? PostedByUserId { get; set; }
    public DateTime? CancelledAt { get; set; }
    public int? CancelledByUserId { get; set; }
}

public sealed class CashFiscalTransferListDto : CashFiscalTransferDto;
