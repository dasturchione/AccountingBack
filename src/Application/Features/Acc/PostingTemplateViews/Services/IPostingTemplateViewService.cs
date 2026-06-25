using SharedKernel.Results;

namespace Application.Features.Acc.PostingTemplateViews;

public interface IPostingTemplateViewService
{
    Task<Result<List<PostingTemplateViewListDto>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<PostingTemplateViewDto>> GetByIdAsync(short id, short policyId = 1, CancellationToken ct = default);
}
