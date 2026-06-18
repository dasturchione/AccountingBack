using SharedKernel.Filters;

namespace Application.Features.Departments;

public class DepartmentListFilter : ISearchFilter, IPaginationFilter
{
    public int? BranchId { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
