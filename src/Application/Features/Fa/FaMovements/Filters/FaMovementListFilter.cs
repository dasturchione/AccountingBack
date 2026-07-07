using SharedKernel.Filters;

namespace Application.Features.FaMovements;

public class FaMovementListFilter : ISearchFilter, IPaginationFilter
{
    public short? StatusId { get; set; }
    public int? DepartmentId { get; set; }
    public int? ResponsibleUserId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
