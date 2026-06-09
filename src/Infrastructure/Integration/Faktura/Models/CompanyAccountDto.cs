using System.Text.Json.Serialization;

namespace Integration.Faktura.Models;

public class CompanyAccountDto
{
    [JsonPropertyName("BankName")]
    public string BankName { get; set; } = string.Empty;

    [JsonPropertyName("BankMfo")]
    public string BankMfo { get; set; } = string.Empty;

    [JsonPropertyName("AccountCode")]
    public string AccountCode { get; set; } = string.Empty;

    [JsonPropertyName("IsPrimary")]
    public bool IsPrimary { get; set; } = false;
}
