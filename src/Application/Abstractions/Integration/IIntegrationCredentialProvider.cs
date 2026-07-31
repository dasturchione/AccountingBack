using Domain.Entities;

namespace Application.Abstractions.Integration;

public interface IIntegrationCredentialProvider
{
    // Faqat is_active = true bo'lgan yozuvni qaytaradi; topilmasa null.
    Task<IntegrationCredential?> GetAsync(int organizationId, string provider, CancellationToken ct = default);
}
