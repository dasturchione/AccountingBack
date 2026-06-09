using System.Text.Json.Serialization;

namespace Integration.Faktura.Models;

public class TokenRequestDto
{
    [JsonPropertyName("grant_type")]
    public string GrantType { get; set; } = "password";

    [JsonPropertyName("username")]
    public string Username { get; set; } = null!;

    [JsonPropertyName("password")]
    public string Password { get; set; } = null!;

    [JsonPropertyName("client_id")]
    public string ClientId { get; set; } = null!;

    [JsonPropertyName("client_secret")]
    public string ClientSecret { get; set; } = null!;
}
