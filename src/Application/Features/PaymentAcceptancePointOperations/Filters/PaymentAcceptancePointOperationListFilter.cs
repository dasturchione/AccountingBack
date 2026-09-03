using SharedKernel.Filters;

namespace Application.Features.PaymentAcceptancePointOperations;

public sealed class PaymentAcceptancePointOperationListFilter : ISearchFilter, IPaginationFilter
{
    public int? PaymentAcceptancePointId { get; set; }
    public short? DirectionId { get; set; }
    public short? CurrencyId { get; set; }
    public short? StatusId { get; set; }
    public long? RelatedDocumentId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
