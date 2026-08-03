namespace Integration.Faktura.Configs;

public sealed class FakturaOptions
{
    // Bo'lim kaliti ataylab eski nomda qoldirilgan: appsettings.json va
    // HostConfiguration.Extensions.cs bu kalitni literal satr bilan o'qiydi.
    public const string SectionName = "FakturaAuthSettings";

    public string BaseUrl { get; set; } = string.Empty;

    public string AuthUrl { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 30;

    public string GrantType { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string ClientId { get; set; } = null!;

    public string ClientSecret { get; set; } = null!;
}
