namespace Integration.Shared.Http;

// Uchala provayder (AslBelgi, Edocs, Faktura) uchun umumiy — chaqiruvchi tashkilot ID sini
// HttpRequestMessage.Options ga shu kalit bilan qo'yadi, DelegatingHandler o'sha yerdan
// o'qiydi. SendAsync(HttpRequestMessage, CancellationToken) imzosi o'zgarmaydi — kalit
// allaqachon parametr sifatida kelayotgan request obyektining o'zida saqlanadi.
public static class IntegrationHttpRequestOptions
{
    public static readonly HttpRequestOptionsKey<int> OrganizationId = new("OrganizationId");
}
