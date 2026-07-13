namespace Integration.Edocs;

public sealed class EdocsOptions
{
    public const string SectionName = "Edocs";
    public const string HttpClientName = "Edocs";

    // Intentionally empty by default so an unconfigured environment cannot call E-DOCS.
    public string BaseUrl { get; set; } = string.Empty;
    public int ChallengeTtlSeconds { get; set; } = 120;
    public int TokenTtlMinutes { get; set; } = 1380;
    public int GetRetryCount { get; set; } = 2;
}
