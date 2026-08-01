namespace Application.Abstractions.Integration.Edo;

public interface IActiveEdoProviderResolver
{
    Task<EdoProviderCode> GetActiveProviderCodeAsync(CancellationToken ct = default);

    Task<IEdoProvider> GetActiveProviderAsync(CancellationToken ct = default);

    Task SetActiveProviderAsync(
        EdoProviderCode providerCode,
        CancellationToken ct = default);
}
