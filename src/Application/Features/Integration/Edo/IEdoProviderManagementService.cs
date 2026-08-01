using Application.Abstractions.Integration.Edo;

namespace Application.Features.Integration.Edo;

public interface IEdoProviderManagementService
{
    Task<IReadOnlyCollection<EdoProviderDto>> GetProvidersAsync(CancellationToken ct = default);

    Task<EdoProviderDto> GetActiveProviderAsync(CancellationToken ct = default);

    Task<EdoProviderDto> SetActiveProviderAsync(
        EdoActiveProviderRequestDto request,
        CancellationToken ct = default);
}
