namespace Application.Features.Reposting;

public interface IRepostReadRepository
{
    Task<List<RepostCandidate>> GetCandidatesAsync(RepostReadRequest request, CancellationToken ct = default);
}
