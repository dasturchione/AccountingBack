namespace Integration.CentralBank.Configs;

public sealed class CentralBankOptions
{
    public const string SectionName = "CentralBank";

    public string BaseUrl { get; set; } = null!;
    public int TimeoutSeconds { get; set; } = 30;
    public int RetryCount { get; set; } = 3;
    public string DefaultProviderCode { get; set; } = "CENTRAL_BANK";
}
