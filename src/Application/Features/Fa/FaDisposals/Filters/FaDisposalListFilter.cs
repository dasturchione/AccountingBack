using SharedKernel.Filters;

namespace Application.Features.FaDisposals;

public class FaDisposalListFilter : ISearchFilter, IPaginationFilter
{
    public short? StatusId { get; set; }
    public short? DisposalTypeId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
