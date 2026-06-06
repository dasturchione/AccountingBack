using SharedKernel.Filters;

namespace Application.Features.CashOperations;

public class CashOperationListFilter : ISearchFilter, IPaginationFilter
{
    public int? OrganizationId { get; set; }
    public int? CashBoxId { get; set; }
    public short? OperationTypeId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
