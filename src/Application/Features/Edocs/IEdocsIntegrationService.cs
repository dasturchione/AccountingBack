using SharedKernel.Results;

namespace Application.Features.Edocs;

public interface IEdocsIntegrationService
{
    Task<Result<EdocsChallengeResponse>> CreateChallengeAsync(EdocsChallengeRequest request, CancellationToken ct = default);
    Task<Result<EdocsLoginResponse>> LoginAsync(EdocsLoginRequest request, CancellationToken ct = default);
    Task<Result<EdocsProfileResponse>> GetProfileAsync(CancellationToken ct = default);
    Task<Result<EdocsDocumentListResponse>> GetDocumentsAsync(EdocsDocumentListQuery query, CancellationToken ct = default);
}
