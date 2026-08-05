namespace Integration.Didox.Configs;

public sealed class DidoxOptions
{
    public const string SectionName = "Didox";

    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;

    // api.didox.uz legacy/production oqimida user-key yetarli va
    // Partner-Authorization yuborilmaydi.
    public bool UsePartnerlessLegacyApi { get; set; }

    // Mavjud partner rejimi uchun ixtiyoriy platforma tokeni.
    public string PartnerToken { get; set; } = string.Empty;
}
