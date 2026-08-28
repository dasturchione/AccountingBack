using SharedKernel.Filters;

namespace Application.Features.BankOperations;

public class BankOperationListFilter : ISearchFilter, IPaginationFilter
{
    public int? BankAccountId { get; set; }
    public short? DirectionId { get; set; }
    public long? RelatedDocumentId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
