namespace Application.Abstractions.Integration.Edo;

public interface IEdoProviderConfiguration
{
    Task<bool> IsConfiguredAsync(
        EdoProviderCode providerCode,
        int organizationId,
        CancellationToken ct = default);
}
