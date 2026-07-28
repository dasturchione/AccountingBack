using Application.Abstractions.Integration;
using Application.Abstractions.Integration.Models;
using Integration.Faktura.Http;
using System.Net.Http.Json;

namespace Integration.Faktura.Services;

public sealed class FakturaService : IFakturaService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public FakturaService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<CompanyBasicDetailsDto> GetCompanyDataAsync(string companyInn)
    {
        var client = _httpClientFactory.CreateClient(FakturaHttpClientNames.Client);

        using var response = await client.GetAsync(
            $"Api/Company/GetCompanyBasicDetails?companyInn={Uri.EscapeDataString(companyInn)}");

        if (!response.IsSuccessStatusCode)
        {
            // Javob tanasi istisno matniga qo'shilmaydi — u maxfiy qiymatlarni
            // o'z ichiga olishi mumkin. Diagnostika uchun status kodi yetarli.
            throw new InvalidOperationException(
                $"Faktura dan kompaniya ma'lumotlarini olishda xatolik: HTTP {(int)response.StatusCode}.");
        }

        var companyDetails = await response.Content.ReadFromJsonAsync<CompanyBasicDetailsDto>();

        return companyDetails
            ?? throw new InvalidOperationException("Kompaniya ma'lumotlarini o'qib bo'lmadi.");
    }
}
