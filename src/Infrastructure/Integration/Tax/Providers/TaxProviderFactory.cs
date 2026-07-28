using Application.Abstractions.Integration;
using Integration.Tax.Configs;
using Microsoft.Extensions.Options;

namespace Integration.Tax.Providers;

public sealed class TaxProviderFactory : ITaxProviderFactory
{
    private readonly IReadOnlyCollection<ITaxProvider> _providers;
    private readonly TaxIntegrationOptions _settings;

    public TaxProviderFactory(IEnumerable<ITaxProvider> providers, IOptions<TaxIntegrationOptions> options)
    {
        _providers = providers.ToList();
        _settings = options.Value;
    }

    public ITaxProvider? Resolve(string? providerCode)
    {
        var code = string.IsNullOrWhiteSpace(providerCode) ? _settings.DefaultProviderCode : providerCode;
        return _providers.FirstOrDefault(x => x.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
    }

    public IReadOnlyCollection<TaxProviderInfoDto> GetSupportedProviders() =>
        _providers
            .Select(x => new TaxProviderInfoDto { Code = x.Code, Name = x.Name, SupportsStatus = true })
            .ToList();

    public IReadOnlyCollection<TaxProviderInfoDto> GetSupportedLookupProviders() =>
        _providers
            .Where(x => x is ITaxLookupProvider)
            .Select(x => new TaxProviderInfoDto { Code = x.Code, Name = x.Name, SupportsStatus = true })
            .ToList();

    public IReadOnlyCollection<TaxProviderInfoDto> GetSupportedDocumentProviders() =>
        _providers
            .Where(x => x is ITaxDocumentProvider)
            .Select(x => new TaxProviderInfoDto { Code = x.Code, Name = x.Name, SupportsStatus = true })
            .ToList();
}
