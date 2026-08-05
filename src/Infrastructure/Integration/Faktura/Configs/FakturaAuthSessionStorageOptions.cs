namespace Integration.Faktura.Configs;

public sealed class FakturaAuthSessionStorageOptions
{
    public const string SectionName = "FakturaAuthSessionStorage";

    public string RootPath { get; set; } = string.Empty;

    public int SessionLifetimeMinutes { get; set; } = 120;

    public int MaxSessionFileBytes { get; set; } = 64 * 1024;
}
