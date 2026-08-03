using Application.Abstractions.Integration.Edo;
using SharedKernel.Exceptions;

namespace Integration.Edo.Persistence;

// Active provider jadvali/entitysi tayyor bo'lmaguncha default yoki memory qiymat
// bermaydi; foydalanish aniq persistence ishini talab qiladigan xatoga tugaydi.
public sealed class UnavailableActiveEdoProviderStore : IActiveEdoProviderStore
{
    public Task<EdoProviderCode?> GetAsync(
        int organizationId,
        CancellationToken ct = default) =>
        throw new EdoActiveProviderStorageUnavailableException();

    public Task SetAsync(
        int organizationId,
        EdoProviderCode providerCode,
        CancellationToken ct = default) =>
        throw new EdoActiveProviderStorageUnavailableException();
}
