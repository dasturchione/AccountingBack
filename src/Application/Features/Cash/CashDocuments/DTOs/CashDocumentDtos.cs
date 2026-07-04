using Application.Common.Pagination;
using Application.Features.CashOperations;
using SharedKernel.Filters;

namespace Application.Features.CashDocuments;

public class CashDocumentBaseDto
{
    public int CashBoxId { get; set; }
    public short PaymentPurposeId { get; set; }
    public short? PaymentTypeId { get; set; }
    public int? CounterpartyId { get; set; }
    public DateTime DocDate { get; set; }
    public short CurrencyId { get; set; }
    public decimal Amount { get; set; }
    public decimal ExchangeRate { get; set; } = 1m;
    public string? Comment { get; set; }
}

public sealed class CashDocumentCreateDto : CashDocumentBaseDto;

public sealed class CashDocumentUpdateDto : CashDocumentBaseDto;

public sealed class CashDocumentListFilter : ISearchFilter, IPaginationFilter
{
    public int? CashBoxId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}

public sealed class CashBookFilter : IPaginationFilter
{
    public int CashBoxId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}

public sealed class CashBookDto
{
    public int CashBoxId { get; set; }
    public string CashBoxName { get; set; } = null!;
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public decimal TotalReceipt { get; set; }
    public decimal TotalPayment { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
    public IReadOnlyCollection<CashBookEntryDto> Items { get; set; } = [];
}

public sealed class CashBookEntryDto
{
    public long MoneyRegisterEntryId { get; set; }
    public long CashOperationId { get; set; }
    public DateTime DocDate { get; set; }
    public string DocNumber { get; set; } = null!;
    public string DocumentKind { get; set; } = null!;
    public short PaymentPurposeId { get; set; }
    public string PaymentPurposeName { get; set; } = null!;
    public int? CounterpartyId { get; set; }
    public string? CounterpartyName { get; set; }
    public string? Comment { get; set; }
    public short CurrencyId { get; set; }
    public string CurrencyName { get; set; } = null!;
    public decimal Receipt { get; set; }
    public decimal Payment { get; set; }
    public decimal RunningBalance { get; set; }
}
