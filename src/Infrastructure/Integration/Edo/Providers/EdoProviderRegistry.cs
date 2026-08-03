using Application.Abstractions.Integration.Edo;
using SharedKernel.Exceptions;

namespace Integration.Edo.Providers;

public sealed class EdoProviderRegistry : IEdoProviderRegistry
{
    private readonly IReadOnlyDictionary<EdoProviderCode, IEdoProvider> _providers;

    public EdoProviderRegistry(IEnumerable<IEdoProvider> providers)
    {
        var providerList = providers.ToList();
        var duplicate = providerList
            .GroupBy(provider => provider.Code)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicate is not null)
            throw new InvalidOperationException($"EDO provider '{duplicate.Key}' is registered more than once.");

        _providers = providerList.ToDictionary(provider => provider.Code);
    }

    public IReadOnlyCollection<EdoProviderCapabilityDto> GetProviders() =>
        _providers.Values
            .OrderBy(provider => provider.Code)
            .Select(provider => provider.Capabilities)
            .ToList();

    public IEdoProvider Resolve(EdoProviderCode providerCode) =>
        _providers.TryGetValue(providerCode, out var provider)
            ? provider
            : throw new EdoProviderNotFoundException(providerCode.ToString());
}
