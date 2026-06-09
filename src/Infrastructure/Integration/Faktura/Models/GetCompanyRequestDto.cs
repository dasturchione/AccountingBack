using System.Text.Json.Serialization;

namespace Integration.Faktura.Models;

public class GetCompanyRequestDto
{
    [JsonPropertyName("companyInn")]
    public string CompanyInn { get; set; } = null!;
}
