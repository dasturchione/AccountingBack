using Application.Abstractions.Integration.Edo;
using Application.Abstractions.Integration.Faktura;

namespace Application.Features.Integration.Edo;

public interface IEdoAuthenticationService
{
    Task<EdoAuthChallengeDto> GetChallengeAsync(
        EdoAuthChallengeRequestDto request,
        CancellationToken ct = default);

    Task<EdoAuthCompleteDto> CompleteAsync(
        EdoAuthCompleteRequestDto request,
        CancellationToken ct = default);

    Task<EdoAuthCompleteDto> CompleteFakturaAsync(
        FakturaAuthCompleteRequestDto request,
        CancellationToken ct = default);
}
