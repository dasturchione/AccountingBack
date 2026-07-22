namespace Integration.AslBelgi.Configs;

public sealed class AslBelgiOptions
{
    public const string SectionName = "AslBelgi";

    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string Tin { get; set; } = string.Empty;
}
