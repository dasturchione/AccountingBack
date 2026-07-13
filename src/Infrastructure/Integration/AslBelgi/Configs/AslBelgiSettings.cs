namespace Integration.AslBelgi.Configs;

public sealed class AslBelgiSettings
{
    public Dictionary<int, string> OrganizationCapabilities { get; set; } = [];

    public string ServerBaseUrl { get; set; } = string.Empty;
    public string AuthenticatePath { get; set; } = "/api/users/authenticate";
    public string RefreshPath { get; set; } = "/api/users/tokens/refresh";
    public int TimeoutSeconds { get; set; } = 30;
    public int RetryCount { get; set; } = 3;
    public bool EnableCorrelationPropagation { get; set; } = true;

    // Auth mode: "technical" (login/password -> short-lived token, auto refresh) or
    // "business" (long-lived apiKey generated in ЛК via E-IMZO).
    public string AuthMode { get; set; } = "technical";

    // TECHNICAL credentials (secret). ⏳ WAITING POINT: supplied by the accountant, set via environment.
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    // BUSINESS apiKey (secret). ⏳ WAITING POINT: generated in ЛК with E-IMZO, never created by the backend.
    public string ApiKey { get; set; } = string.Empty;

    // Refresh the access token this many seconds before its real expiry.
    public int AccessTokenSafetyMarginSeconds { get; set; } = 60;

    // apiKey check is rate-limited to 100/24h — cache the result per TIN for this many minutes.
    public int ApiKeyCheckCacheMinutes { get; set; } = 60;

    // Reserved for next prompts
    public string CheckApiKeyPath { get; set; } = "/public/api/v1/party/parties/{tin}/api-keys/check";
    public string RefreshApiKeyPath { get; set; } = "/public/api/v1/party/parties/{tin}/api-keys/refresh";

    // KM order endpoints (Open API v1.18.2, section 4).
    public string OrdersPath { get; set; } = "/api/orders";
    public string CodesPath { get; set; } = "/api/codes";

    public string DocumentsPath { get; set; } = "/api/documents";
    public string StatusPath { get; set; } = "/api/status";
}
