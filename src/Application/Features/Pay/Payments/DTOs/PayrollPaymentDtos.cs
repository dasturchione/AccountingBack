using SharedKernel.Filters;

namespace Application.Features.Pay.Payments;

public sealed class PayrollPaymentCreateDto
{
    public long PeriodId { get; set; }
    public long? PayrollDocId { get; set; }
    public DateTime DocDate { get; set; }
    public string PaymentKind { get; set; } = null!;
    public string SourceType { get; set; } = null!;
    public int? BankAccountId { get; set; }
    public int? CashBoxId { get; set; }
    public int SourceChartAccountId { get; set; }
    public int OffsetAccountId { get; set; }
    public short CurrencyId { get; set; }
    public string? Note { get; set; }
    public List<PayrollPaymentLineCreateDto> Lines { get; set; } = [];
}

public sealed class PayrollPaymentLineCreateDto
{
    public long EmployeeId { get; set; }
    public decimal Amount { get; set; }
    public string? Note { get; set; }
}

public sealed class PayrollPaymentListFilter : ISearchFilter, IPaginationFilter
{
    public string? Search { get; set; }
    public long? PeriodId { get; set; }
    public string? PaymentKind { get; set; }
    public string? SourceType { get; set; }
    public short? StatusId { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}

public class PayrollPaymentListDto
{
    public long Id { get; set; }
    public string DocNumber { get; set; } = null!;
    public DateTime DocDate { get; set; }
    public long PeriodId { get; set; }
    public string PeriodName { get; set; } = null!;
    public string PaymentKind { get; set; } = null!;
    public string SourceType { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public short StatusId { get; set; }
    public string StatusName { get; set; } = null!;
    public long? BankOperationId { get; set; }
    public long? CashOperationId { get; set; }
}

public sealed class PayrollPaymentDto : PayrollPaymentListDto
{
    public int OrganizationId { get; set; }
    public long? PayrollDocId { get; set; }
    public int? BankAccountId { get; set; }
    public int? CashBoxId { get; set; }
    public int SourceChartAccountId { get; set; }
    public int? OffsetAccountId { get; set; }
    public short CurrencyId { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? PostedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public List<PayrollPaymentLineDto> Lines { get; set; } = [];
}

public sealed class PayrollPaymentLineDto
{
    public long Id { get; set; }
    public long EmployeeId { get; set; }
    public string EmployeeNumber { get; set; } = null!;
    public string EmployeeName { get; set; } = null!;
    public long? PayrollLineId { get; set; }
    public decimal Amount { get; set; }
    public string? Note { get; set; }
}
