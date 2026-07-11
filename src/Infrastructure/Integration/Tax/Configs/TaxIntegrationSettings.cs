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
    public ProviderSettings Didox { get; set; } = new();
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

        // Didox 2-header auth (official partner API).
        // PartnerToken is a secret: keep placeholder in appsettings, override via environment.
        public string PartnerToken { get; set; } = string.Empty;
        public string UserKeyHeaderName { get; set; } = "user-key";
        public string PartnerAuthHeaderName { get; set; } = "Partner-Authorization";

        // Locale + auth endpoints for company-token exchange (E-IMZO signature flow runs on the frontend).
        public string Locale { get; set; } = "ru";
        public string AuthTokenPath { get; set; } = "/v1/auth/{taxId}/token/{locale}";
        public string AuthPasswordPath { get; set; } = "/v1/auth/{taxId}/password/{locale}";

        // Create-document endpoint (ЭСФ submit). Body is wrapped as { "document_json": {...} }.
        public string CreateDocumentPath { get; set; } = "/v1/documents/{docType}/create/{locale}";

        // Sign endpoint. Body is { "signature": "<pkcs7 timestamp b64>" } — the signature is produced
        // by the frontend E-IMZO; the backend only forwards it (never signs server-side).
        public string SignDocumentPath { get; set; } = "/v1/documents/{docId}/sign";

        // TODO(Didox doc): confirm the exact docType code for ЭСФ (счёт-фактура). Left empty so a
        // real submit fails fast with a clear message instead of guessing a wrong document type.
        public string FacturaDocType { get; set; } = string.Empty;
    }
}
