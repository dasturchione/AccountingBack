using Application.Abstractions.Integration;
using Application.Features.Edocs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Results;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Integration.Edocs;

public sealed class EdocsClient : IEdocsClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly EdocsOptions _options;
    private readonly EdocsResponseSanitizer _sanitizer;
    private readonly ILogger<EdocsClient> _logger;

    public EdocsClient(IHttpClientFactory httpClientFactory, IOptions<EdocsOptions> options, EdocsResponseSanitizer sanitizer, ILogger<EdocsClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _sanitizer = sanitizer;
        _logger = logger;
    }

    public async Task<Result<string>> GetAuthIdAsync(string serialNumber, CancellationToken ct = default)
    {
        var response = await SendGetAsync($"authId/{Uri.EscapeDataString(serialNumber)}", null, "auth challenge", ct);
        if (!response.IsSuccess)
            return Result.Failure<string>(response.Error);

        using var document = response.Value;
        var authId = GetString(document.RootElement, "authId");
        return string.IsNullOrWhiteSpace(authId)
            ? Result.Failure<string>(Error.Problem("Edocs.ChallengeResponseInvalid", _sanitizer.SafeClientMessage()))
            : Result.Success(authId);
    }

    public async Task<Result<EdocsLoginResult>> LoginAsync(string serialNumber, string pkcs7, CancellationToken ct = default)
    {
        var uri = TryBuildUri("login");
        if (!uri.IsSuccess)
            return Result.Failure<EdocsLoginResult>(uri.Error);

        using var request = new HttpRequestMessage(HttpMethod.Post, uri.Value)
        {
            Content = new StringContent(JsonSerializer.Serialize(new { serialNumber, pkcs7 }, JsonOptions), Encoding.UTF8, "application/json")
        };

        // Login intentionally does not use the GET retry helper.
        return await SendAndParseAsync(request, "login", ParseLogin, ct);
    }

    public async Task<Result<EdocsExternalProfile>> GetProfileAsync(string bearerToken, CancellationToken ct = default)
    {
        var response = await SendGetAsync("profile?type=buyer", bearerToken, "profile", ct);
        if (!response.IsSuccess)
            return Result.Failure<EdocsExternalProfile>(response.Error);

        using var document = response.Value;
        var root = UnwrapData(document.RootElement);
        var tin = GetString(root, "tin");
        return string.IsNullOrWhiteSpace(tin)
            ? Result.Failure<EdocsExternalProfile>(Error.Problem("Edocs.ProfileResponseInvalid", _sanitizer.SafeClientMessage()))
            : Result.Success(new EdocsExternalProfile(tin, GetString(root, "name")));
    }

    public async Task<Result<EdocsExternalDocumentPage>> GetDocumentsAsync(string bearerToken, EdocsDocumentListQuery query, CancellationToken ct = default)
    {
        var path = "documents" + BuildQuery(query);
        var response = await SendGetAsync(path, bearerToken, "document list", ct);
        if (!response.IsSuccess)
            return Result.Failure<EdocsExternalDocumentPage>(response.Error);

        using var document = response.Value;
        var root = UnwrapData(document.RootElement);
        var items = ResolveItems(root).Select(ParseDocument).ToList();
        return Result.Success(new EdocsExternalDocumentPage(items, GetInt(root, "total") ?? GetInt(document.RootElement, "total")));
    }

    private async Task<Result<JsonDocument>> SendGetAsync(string path, string? bearerToken, string operation, CancellationToken ct)
    {
        var uri = TryBuildUri(path);
        if (!uri.IsSuccess)
            return Result.Failure<JsonDocument>(uri.Error);

        var attempts = Math.Clamp(_options.GetRetryCount, 1, 3);
        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, uri.Value);
            if (!string.IsNullOrWhiteSpace(bearerToken))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            try
            {
                using var response = await CreateClient().SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(ct);
                    return TryParseDocument(content, operation);
                }

                if (IsTransient(response.StatusCode) && attempt < attempts)
                {
                    _logger.LogWarning("E-DOCS {Operation} returned transient status {StatusCode}; retrying GET attempt {Attempt}/{Attempts}.", operation, (int)response.StatusCode, attempt, attempts);
                    await Task.Delay(TimeSpan.FromMilliseconds(100 * attempt), ct);
                    continue;
                }

                await LogFailureAsync(response, operation, ct);
                return Result.Failure<JsonDocument>(ToError(response.StatusCode));
            }
            catch (HttpRequestException) when (attempt < attempts)
            {
                _logger.LogWarning("E-DOCS {Operation} transport failure; retrying GET attempt {Attempt}/{Attempts}.", operation, attempt, attempts);
                await Task.Delay(TimeSpan.FromMilliseconds(100 * attempt), ct);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (HttpRequestException)
            {
                return Result.Failure<JsonDocument>(Error.Problem("Edocs.TransportFailure", _sanitizer.SafeClientMessage()));
            }
        }

        return Result.Failure<JsonDocument>(Error.Problem("Edocs.TransportFailure", _sanitizer.SafeClientMessage()));
    }

    private async Task<Result<T>> SendAndParseAsync<T>(HttpRequestMessage request, string operation, Func<JsonElement, Result<T>> parser, CancellationToken ct)
    {
        try
        {
            using var response = await CreateClient().SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            if (!response.IsSuccessStatusCode)
            {
                await LogFailureAsync(response, operation, ct);
                return Result.Failure<T>(ToError(response.StatusCode));
            }

            var content = await response.Content.ReadAsStringAsync(ct);
            using var document = JsonDocument.Parse(content);
            return parser(document.RootElement);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException)
        {
            return Result.Failure<T>(Error.Problem("Edocs.TransportFailure", _sanitizer.SafeClientMessage()));
        }
        catch (JsonException)
        {
            return Result.Failure<T>(Error.Problem("Edocs.ResponseInvalid", _sanitizer.SafeClientMessage()));
        }
    }

    private Result<Uri> TryBuildUri(string path)
    {
        if (!Uri.TryCreate(_options.BaseUrl, UriKind.Absolute, out var baseUri) || baseUri.Scheme != Uri.UriSchemeHttps)
            return Result.Failure<Uri>(Error.Problem("Edocs.NotConfigured", "E-DOCS is not configured."));

        return Result.Success(new Uri(new Uri(baseUri.AbsoluteUri.TrimEnd('/') + "/"), path));
    }

    private HttpClient CreateClient() => _httpClientFactory.CreateClient(EdocsOptions.HttpClientName);

    private Task LogFailureAsync(HttpResponseMessage response, string operation, CancellationToken ct)
    {
        _logger.LogWarning("E-DOCS {Operation} failed with status {StatusCode}; external response body omitted.", operation, (int)response.StatusCode);
        return Task.CompletedTask;
    }

    private Result<JsonDocument> TryParseDocument(string content, string operation)
    {
        try
        {
            return Result.Success(JsonDocument.Parse(content));
        }
        catch (JsonException)
        {
            _logger.LogWarning("E-DOCS {Operation} returned invalid JSON.", operation);
            return Result.Failure<JsonDocument>(Error.Problem("Edocs.ResponseInvalid", _sanitizer.SafeClientMessage()));
        }
    }

    private Result<EdocsLoginResult> ParseLogin(JsonElement root)
    {
        var token = GetString(root, "token");
        if (string.IsNullOrWhiteSpace(token))
            return Result.Failure<EdocsLoginResult>(Error.Problem("Edocs.LoginResponseInvalid", _sanitizer.SafeClientMessage()));

        var tins = new List<string>();
        AddIfPresent(tins, root, "entityTin");
        AddIfPresent(tins, root, "tin");
        if (root.TryGetProperty("user", out var user))
        {
            AddIfPresent(tins, user, "tin");
            if (user.TryGetProperty("entities", out var entities) && entities.ValueKind == JsonValueKind.Array)
                foreach (var entity in entities.EnumerateArray())
                    AddIfPresent(tins, entity, "tin");
        }

        return Result.Success(new EdocsLoginResult(token, tins.Distinct(StringComparer.Ordinal).ToList()));
    }

    private static string BuildQuery(EdocsDocumentListQuery query)
    {
        var values = new List<(string Key, string? Value)>
        {
            ("page", query.Page.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            ("limit", query.Limit.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            ("sort", query.Sort), ("order", query.Order?.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            ("filter", query.Filter), ("fields", query.Fields), ("io", query.Io), ("status", query.Status), ("type", query.Type)
        };
        return "?" + string.Join("&", values.Where(x => !string.IsNullOrWhiteSpace(x.Value)).Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value!)}"));
    }

    private static JsonElement UnwrapData(JsonElement root) => root.ValueKind == JsonValueKind.Object && root.TryGetProperty("data", out var data) ? data : root;
    private static IEnumerable<JsonElement> ResolveItems(JsonElement root) => root.ValueKind == JsonValueKind.Array ? root.EnumerateArray() : root.ValueKind == JsonValueKind.Object && root.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array ? items.EnumerateArray() : [];
    private static EdocsExternalDocument ParseDocument(JsonElement item) => new(GetString(item, "id"), GetString(item, "number"), GetString(item, "type"), GetString(item, "status"), GetDate(item, "date") ?? GetDate(item, "docDate"));
    private static string? GetString(JsonElement element, string property) => element.ValueKind == JsonValueKind.Object && element.TryGetProperty(property, out var value) && value.ValueKind is JsonValueKind.String or JsonValueKind.Number ? value.ToString() : null;
    private static int? GetInt(JsonElement element, string property) => element.ValueKind == JsonValueKind.Object && element.TryGetProperty(property, out var value) && value.TryGetInt32(out var result) ? result : null;
    private static DateTimeOffset? GetDate(JsonElement element, string property) => DateTimeOffset.TryParse(GetString(element, property), out var result) ? result : null;
    private static void AddIfPresent(List<string> values, JsonElement element, string property) { var value = GetString(element, property); if (!string.IsNullOrWhiteSpace(value)) values.Add(value); }
    private static bool IsTransient(HttpStatusCode statusCode) => statusCode is HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests or HttpStatusCode.BadGateway or HttpStatusCode.ServiceUnavailable or HttpStatusCode.GatewayTimeout;
    private static Error ToError(HttpStatusCode statusCode) => statusCode switch { HttpStatusCode.Unauthorized => Error.Unauthorized("Edocs.Unauthorized", "E-DOCS authorization is no longer valid."), HttpStatusCode.Forbidden => Error.Forbidden("Edocs.Forbidden", "E-DOCS denied the request."), _ => Error.Problem("Edocs.ExternalFailure", "E-DOCS request failed.") };
}
