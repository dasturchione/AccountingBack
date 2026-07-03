using SharedKernel.Results;

namespace Application.Features.Reposting;

public interface IRepostService
{
    Task<Result<RepostDto>> RepostAsync(RepostFilter filter, CancellationToken ct = default);
}
