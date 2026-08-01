namespace Application.Abstractions.Integration.Edo;

// Organization bo'yicha active provider qiymatini saqlash uchun persistence port.
// Bu bosqichda uning database modeli va implementatsiyasi mavjud emas.
public interface IActiveEdoProviderStore
{
    Task<EdoProviderCode?> GetAsync(
        int organizationId,
        CancellationToken ct = default);

    Task SetAsync(
        int organizationId,
        EdoProviderCode providerCode,
        CancellationToken ct = default);
}
