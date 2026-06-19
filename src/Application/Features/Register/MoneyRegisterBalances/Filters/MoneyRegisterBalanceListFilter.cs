using SharedKernel.Filters;

namespace Application.Features.MoneyRegisterBalances;

public class MoneyRegisterBalanceListFilter : IPaginationFilter
{
    public short? DocumentTypeId { get; set; }
    public long? DocumentId { get; set; }
    public string? SourceType { get; set; }
    public int? SourceId { get; set; }
    public short? OperationTypeId { get; set; }
    public short? CurrencyId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
