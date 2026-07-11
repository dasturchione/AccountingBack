using Application.Abstractions.Integration;
using Integration.Tax.Configs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace Integration.Tax.Providers;

/// <summary>
/// Exchanges an E-IMZO signature (or password) for a Didox company token (user-key).
/// The signature itself is produced by the client via E-IMZO on the frontend — this backend
/// client never generates a signature, it only relays it to Didox.
/// </summary>
public sealed class DidoxAuthClient : IDidoxAuthClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TaxIntegrationSettings _settings;
    private readonly ILogger<DidoxAuthClient> _logger;

    public DidoxAuthClient(
        IHttpClientFactory httpClientFactory,
        IOptions<TaxIntegrationSettings> options,
        ILogger<DidoxAuthClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settings = options.Value;
        _logger = logger;
    }

    public Task<DidoxTokenResultDto> GetTokenBySignatureAsync(string taxId, string signature, string? locale, CancellationToken ct = default)
        => ExchangeAsync(_settings.Didox.AuthTokenPath, taxId, locale, new { signature }, ct);

    public Task<DidoxTokenResultDto> GetTokenByPasswordAsync(string taxId, string password, string? locale, CancellationToken ct = default)
        => ExchangeAsync(_settings.Didox.AuthPasswordPath, taxId, locale, new { password }, ct);

    private async Task<DidoxTokenResultDto> ExchangeAsync(string pathTemplate, string taxId, string? locale, object body, CancellationToken ct)
    {
        var providerSettings = _settings.Didox;

        if (string.IsNullOrWhiteSpace(providerSettings.BaseUrl))
            return new DidoxTokenResultDto { IsSuccessful = false, Message = "Didox BaseUrl is not configured." };

        if (string.IsNullOrWhiteSpace(taxId))
            return new DidoxTokenResultDto { IsSuccessful = false, Message = "Tax id is required for Didox authentication." };

        var effectiveLocale = string.IsNullOrWhiteSpace(locale) ? providerSettings.Locale : locale!;
        var path = pathTemplate
            .Replace("{taxId}", Uri.EscapeDataString(taxId))
            .Replace("{locale}", Uri.EscapeDataString(effectiveLocale));

        var client = _httpClientFactory.CreateClient(_settings.ClientName ?? "TaxIntegration");
        client.Timeout = TimeSpan.FromSeconds(Math.Max(5, providerSettings.TimeoutSeconds ?? _settings.TimeoutSeconds));

        using var request = new HttpRequestMessage(HttpMethod.Post, BuildUri(providerSettings.BaseUrl, path))
        {
            Content = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json")
        };

        // The partner API namespace is partner-scoped; forward the partner token when it is configured.
        if (!IsMissingSecret(providerSettings.PartnerToken))
            request.Headers.TryAddWithoutValidation(providerSettings.PartnerAuthHeaderName, providerSettings.PartnerToken);

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        var responseText = await ReadBodySafelyAsync(response, ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Didox auth request to {Path} failed with status {StatusCode}.", path, (int)response.StatusCode);
            return new DidoxTokenResultDto
            {
                IsSuccessful = false,
                Message = string.IsNullOrWhiteSpace(responseText)
                    ? $"Didox authentication failed with status {(int)response.StatusCode}."
                    : responseText
            };
        }

        var token = ExtractToken(responseText);
        if (string.IsNullOrWhiteSpace(token))
            return new DidoxTokenResultDto { IsSuccessful = false, Message = "Didox authentication response did not contain a token." };

        return new DidoxTokenResultDto { IsSuccessful = true, Token = token };
    }

    private static Uri BuildUri(string baseUrl, string path)
        => new(new Uri(baseUrl.TrimEnd('/') + "/"), path.TrimStart('/'));

    private static bool IsMissingSecret(string? value)
        => string.IsNullOrWhiteSpace(value)
            || value.Contains("SET_VIA_ENVIRONMENT", StringComparison.OrdinalIgnoreCase);

    private static string? ExtractToken(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return null;

        try
        {
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.ValueKind == JsonValueKind.Object
                && document.RootElement.TryGetProperty("token", out var token)
                && token.ValueKind == JsonValueKind.String)
            {
                return token.GetString();
            }
        }
        catch (JsonException)
        {
            // fall through
        }

        return null;
    }

    private static async Task<string> ReadBodySafelyAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            return await response.Content.ReadAsStringAsync(ct);
        }
        catch
        {
            return string.Empty;
        }
    }
}
