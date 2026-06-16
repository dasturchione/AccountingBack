using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.Acc.PostingRules;

public interface IPostingRuleService
{
    Task<Result<List<PostingRuleListDto>>> GetAllAsync(PostingRuleListFilter filter, CancellationToken ct = default);
    Task<Result<PostingRuleDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<int>> CreateAsync(PostingRuleCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(int id, PostingRuleUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}
