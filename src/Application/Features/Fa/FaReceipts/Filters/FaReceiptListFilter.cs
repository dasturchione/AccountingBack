using SharedKernel.Filters;

namespace Application.Features.FaReceipts;

public class FaReceiptListFilter : ISearchFilter, IPaginationFilter
{
    public int? CounterpartyId { get; set; }
    public short? StatusId { get; set; }
    public short? ReceiptTypeId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
