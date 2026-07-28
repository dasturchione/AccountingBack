namespace Integration.Faktura.Configs;

public sealed class FakturaOptions
{
    // Bo'lim kaliti ataylab eski nomda qoldirilgan: appsettings.json va
    // HostConfiguration.Extensions.cs bu kalitni literal satr bilan o'qiydi.
    public const string SectionName = "FakturaAuthSettings";

    public string BaseUrl { get; set; } = "https://api.faktura.uz";

    public string AuthUrl { get; set; } = "https://account.faktura.uz/token";

    public int TimeoutSeconds { get; set; } = 30;

    public string GrantType { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string ClientId { get; set; } = null!;

    public string ClientSecret { get; set; } = null!;
}
