namespace Integration.Faktura.Configs;

public class FakturaAuthSettings
{
    public string GrantType { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string ClientId { get; set; } = null!;

    public string ClientSecret { get; set; } = null!;
}
