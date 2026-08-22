using SharedKernel.Filters;

namespace Application.Features.Contracts;

public class ContractListFilter : ISearchFilter, IPaginationFilter
{
    internal int? OrganizationId { get; set; }
    public int? CounterpartyId { get; set; }
    public short? ContractTypeId { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
