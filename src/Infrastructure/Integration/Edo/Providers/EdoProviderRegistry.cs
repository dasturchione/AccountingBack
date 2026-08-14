using Application.Abstractions.Integration.Edo;
using SharedKernel.Exceptions;

namespace Integration.Edo.Providers;

public sealed class EdoProviderRegistry : IEdoProviderRegistry
{
    private readonly IReadOnlyDictionary<EdoProviderCode, IEdoProvider> _providers;
    private readonly IReadOnlyDictionary<EdoProviderCode, IEdoHistoricalDocumentSource> _historicalSources;

    public EdoProviderRegistry(
        IEnumerable<IEdoProvider> providers,
        IEnumerable<IEdoHistoricalDocumentSource> historicalSources)
    {
        var providerList = providers.ToList();
        var duplicate = providerList
            .GroupBy(provider => provider.Code)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicate is not null)
            throw new InvalidOperationException($"EDO provider '{duplicate.Key}' is registered more than once.");

        _providers = providerList.ToDictionary(provider => provider.Code);

        var historicalSourceList = historicalSources.ToList();
        var duplicateHistoricalSource = historicalSourceList
            .GroupBy(source => source.ProviderCode)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateHistoricalSource is not null)
            throw new InvalidOperationException(
                $"EDO historical source '{duplicateHistoricalSource.Key}' is registered more than once.");

        _historicalSources = historicalSourceList.ToDictionary(source => source.ProviderCode);
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

    public IEdoHistoricalDocumentSource ResolveHistoricalSource(EdoProviderCode providerCode) =>
        _historicalSources.TryGetValue(providerCode, out var source)
            ? source
            : throw new EdoCapabilityUnavailableException(
                providerCode.ToString(),
                nameof(IEdoHistoricalDocumentSource),
                EdoCapabilityStatus.NOT_SUPPORTED.ToString());
}
