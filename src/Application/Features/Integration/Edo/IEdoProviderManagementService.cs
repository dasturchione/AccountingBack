using Application.Abstractions.Integration.Edo;

namespace Application.Features.Integration.Edo;

public interface IEdoProviderManagementService
{
    Task<EdoProviderDto> GetActiveProviderAsync(CancellationToken ct = default);

    Task<EdoProviderDto> SetActiveProviderAsync(
        EdoActiveProviderRequestDto request,
        CancellationToken ct = default);

    Task<EdoCapabilitiesResponseDto> GetCapabilitiesAsync(
        CancellationToken ct = default);
}
