namespace Application.Abstractions.Integration.Edo;

public interface IEdoProviderRegistry
{
    IReadOnlyCollection<EdoProviderCapabilityDto> GetProviders();
    IEdoProvider Resolve(EdoProviderCode providerCode);
    IEdoHistoricalDocumentSource ResolveHistoricalSource(EdoProviderCode providerCode);
}
