namespace Integration.Edocs.Configs;

public sealed class EdocsOptions
{
    public const string SectionName = "Edocs";

    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
    public string Product { get; set; } = string.Empty;
    public string PartnerId { get; set; } = string.Empty;
    public int ChallengeTtlSeconds { get; set; } = 120;
}
