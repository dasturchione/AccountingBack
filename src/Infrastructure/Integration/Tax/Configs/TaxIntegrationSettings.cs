namespace Integration.Tax.Configs;

public sealed class TaxIntegrationSettings
{
    public bool Enabled { get; set; } = true;
    public string DefaultProviderCode { get; set; } = "MXIK";
    public int TimeoutSeconds { get; set; } = 30;
    public int RetryCount { get; set; } = 3;
    public bool EnableCorrelationPropagation { get; set; } = true;
    public ProviderSettings Mxik { get; set; } = new();
    public ProviderSettings SoliqApi { get; set; } = new();
    public ProviderSettings EFaktura { get; set; } = new();
    public string? ClientName { get; set; } = "TaxIntegration";

    public sealed class ProviderSettings
    {
        public bool Enabled { get; set; } = true;
        public string BaseUrl { get; set; } = string.Empty;
        public string StatusPath { get; set; } = "/status";
        public string SearchPath { get; set; } = "/search";
        public string LookupPath { get; set; } = "/lookup";
        public string SubmitPath { get; set; } = "/submit";
        public string StatusQueryPath { get; set; } = "/status";
        public string CancelPath { get; set; } = "/cancel";
        public int? TimeoutSeconds { get; set; }
        public int? RetryCount { get; set; }

    }
}
