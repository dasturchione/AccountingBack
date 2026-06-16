using SharedKernel.Filters;

namespace Application.Features.Acc.PostingRules;

public class PostingRuleListFilter : ISearchFilter, IPaginationFilter
{
    public short? DocumentTypeId { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
