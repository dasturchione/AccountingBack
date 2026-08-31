using SharedKernel.Filters;

namespace Application.Features.Rnt.RentalContracts;

public sealed class RentalContractListFilter : ISearchFilter, IPaginationFilter
{
    public short? StatusId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
