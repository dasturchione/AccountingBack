using SharedKernel.Filters;

namespace Application.Features.Rnt.RentalAccruals;

public sealed class RentalAccrualListFilter : ISearchFilter, IPaginationFilter
{
    public long? ContractId { get; set; }
    public short? StatusId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
