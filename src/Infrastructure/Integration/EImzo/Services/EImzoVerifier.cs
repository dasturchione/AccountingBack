using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Application.Abstractions.Integration;
using Integration.EImzo.Configs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Results;

namespace Integration.EImzo.Services;

public sealed class EImzoVerifier : IEImzoVerifier
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly string[] SupportedDateFormats =
    [
        "yyyy-MM-dd HH:mm:ss",
        "yyyy.MM.dd HH:mm:ss",
        "yyyy-MM-ddTHH:mm:ss",
        "yyyy-MM-ddTHH:mm:ssZ",
        "yyyy-MM-ddTHH:mm:ss.fffZ"
    ];

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly EImzoSettings _settings;
    private readonly ILogger<EImzoVerifier> _logger;

    public EImzoVerifier(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        IOptions<EImzoSettings> options,
        ILogger<EImzoVerifier> logger)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
        _settings = options.Value;
        _logger = logger;
    }

    public Task<Result<Pkcs7VerifyResult>> VerifyAttachedAsync(byte[] pkcs7, CancellationToken ct = default)
    {
        if (pkcs7 is null || pkcs7.Length == 0)
            return Task.FromResult(Result.Failure<Pkcs7VerifyResult>(Error.Problem("EImzo.VerifyPayloadMissing", "PKCS#7 payload is empty.")));

        return VerifyAsync(
            "backend/pkcs7/verify/attached",
            Convert.ToBase64String(pkcs7),
            ct);
    }

    public Task<Result<Pkcs7VerifyResult>> VerifyDetachedAsync(byte[] document, byte[] pkcs7, CancellationToken ct = default)
    {
        if (document is null || document.Length == 0)
            return Task.FromResult(Result.Failure<Pkcs7VerifyResult>(Error.Problem("EImzo.VerifyDocumentMissing", "Detached verification document is empty.")));

        if (pkcs7 is null || pkcs7.Length == 0)
            return Task.FromResult(Result.Failure<Pkcs7VerifyResult>(Error.Problem("EImzo.VerifyPayloadMissing", "PKCS#7 payload is empty.")));

        var payload = $"{Convert.ToBase64String(document)}|{Convert.ToBase64String(pkcs7)}";

        return VerifyAsync(
            "backend/pkcs7/verify/detached",
            payload,
            ct);
    }

    private async Task<Result<Pkcs7VerifyResult>> VerifyAsync(string path, string payload, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(_settings.EImzoServerUrl))
        {
            return Result.Failure<Pkcs7VerifyResult>(Error.Problem(
                "EImzo.ServerUrlMissing",
                "e-imzo-server sozlanmagan."));
        }

        try
        {
            var client = CreateClient();
            using var response = await SendWithRetryAsync(() =>
            {
                var request = new HttpRequestMessage(HttpMethod.Post, path)
                {
                    Content = new StringContent(payload, Encoding.UTF8, "text/plain")
                };
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                ApplyRequestHeaders(request);

                return client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            }, ct);

            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "e-imzo-server verify request failed with status {StatusCode}. Response: {Response}",
                    (int)response.StatusCode,
                    body);

                return Result.Failure<Pkcs7VerifyResult>(Error.Problem(
                    "EImzo.VerifyHttpFailed",
                    $"e-imzo-server verify request failed with status {(int)response.StatusCode}."));
            }

            var verifyResponse = JsonSerializer.Deserialize<EImzoVerifyResponse>(body, JsonOptions);
            if (verifyResponse is null)
            {
                return Result.Failure<Pkcs7VerifyResult>(Error.Problem(
                    "EImzo.VerifyResponseInvalid",
                    "e-imzo-server returned an empty verification response."));
            }

            if (verifyResponse.Status != 1)
            {
                return Result.Failure<Pkcs7VerifyResult>(Error.Problem(
                    "EImzo.VerifyRejected",
                    string.IsNullOrWhiteSpace(verifyResponse.Message)
                        ? "e-imzo-server signature verification failed."
                        : verifyResponse.Message));
            }

            return MapVerifyResult(verifyResponse);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "e-imzo-server verification request failed for path {Path}", path);
            return Result.Failure<Pkcs7VerifyResult>(Error.Problem("EImzo.VerifyRequestFailed", ex.Message));
        }
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient("EImzoServer");
        client.BaseAddress = new Uri(_settings.EImzoServerUrl!.TrimEnd('/') + "/", UriKind.Absolute);
        client.Timeout = TimeSpan.FromSeconds(30);
        return client;
    }

    private async Task<HttpResponseMessage> SendWithRetryAsync(Func<Task<HttpResponseMessage>> action, CancellationToken ct)
    {
        const int attempts = 3;
        var delay = TimeSpan.FromMilliseconds(200);
        Exception? lastException = null;

        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                var response = await action();
                if (IsTransient(response.StatusCode) && attempt < attempts)
                {
                    _logger.LogWarning(
                        "e-imzo-server returned transient status {StatusCode} on attempt {Attempt}/{Attempts}",
                        (int)response.StatusCode,
                        attempt,
                        attempts);

                    response.Dispose();
                    await Task.Delay(delay, ct);
                    delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
                    continue;
                }

                return response;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                lastException = ex;
                if (attempt >= attempts)
                    break;

                _logger.LogWarning(ex, "e-imzo-server request attempt {Attempt}/{Attempts} failed", attempt, attempts);
                await Task.Delay(delay, ct);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
            }
        }

        throw lastException ?? new HttpRequestException("e-imzo-server request failed.");
    }

    private void ApplyRequestHeaders(HttpRequestMessage request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context is null)
            return;

        if (!string.IsNullOrWhiteSpace(context.Request.Host.Value))
            request.Headers.Host = context.Request.Host.Value;

        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault()
            ?? context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
            ?? context.Connection.RemoteIpAddress?.ToString();

        if (!string.IsNullOrWhiteSpace(realIp))
            request.Headers.TryAddWithoutValidation("X-Real-IP", realIp);
    }

    private Result<Pkcs7VerifyResult> MapVerifyResult(EImzoVerifyResponse response)
    {
        var signer = response.Pkcs7Info?.Signers?.FirstOrDefault();
        if (signer is null)
        {
            return Result.Failure<Pkcs7VerifyResult>(Error.Problem(
                "EImzo.VerifySignerMissing",
                "e-imzo-server response does not contain signer information."));
        }

        var certificate = signer.Certificate?.FirstOrDefault();
        var subjectInfo = certificate?.SubjectInfo ?? new Dictionary<string, string?>(StringComparer.Ordinal);
        var hasTimestamp = signer.TimeStampInfo is not null;
        var signedAt = ParseDateTime(hasTimestamp ? signer.TimeStampInfo!.Time : signer.SigningTime);
        var isTimeStampValid = !hasTimestamp
            || (signer.TimeStampInfo!.Verified && signer.TimeStampInfo.CertificateVerified);
        var isValid = response.Status == 1
            && signer.Verified
            && signer.CertificateVerified
            && signer.CertificateValidAtSigningTime
            && isTimeStampValid;

        byte[]? documentBytes = null;
        if (!string.IsNullOrWhiteSpace(response.Pkcs7Info?.DocumentBase64))
        {
            try
            {
                documentBytes = Convert.FromBase64String(response.Pkcs7Info.DocumentBase64);
            }
            catch (FormatException ex)
            {
                _logger.LogWarning(ex, "e-imzo-server returned invalid documentBase64 payload");
                return Result.Failure<Pkcs7VerifyResult>(Error.Problem(
                    "EImzo.VerifyDocumentInvalid",
                    "e-imzo-server returned invalid document payload."));
            }
        }

        return Result.Success(new Pkcs7VerifyResult
        {
            IsValid = isValid,
            SubjectName = certificate?.SubjectName,
            Pinfl = TryGetSubjectValue(subjectInfo, "1.2.860.3.16.1.2"),
            Inn = TryGetSubjectValue(subjectInfo, "1.2.860.3.16.1.1"),
            SignedAt = signedAt,
            SerialNumber = certificate?.SerialNumber,
            OcspStatus = BuildOcspStatus(signer),
            HasTimestamp = hasTimestamp,
            DocumentBytes = documentBytes,
            Error = isValid
                ? null
                : signer.Exception ?? response.Message ?? "e-imzo-server verification returned an invalid signature result."
        });
    }

    private static string? TryGetSubjectValue(IReadOnlyDictionary<string, string?> subjectInfo, string oid)
    {
        return subjectInfo.TryGetValue(oid, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : null;
    }

    private static string BuildOcspStatus(EImzoVerifySigner? signer)
    {
        if (signer is null)
            return "Unknown";

        if (!string.IsNullOrWhiteSpace(signer.RevokedStatusInfo))
            return signer.RevokedStatusInfo;

        return signer.CertificateVerified ? "Valid" : "Invalid";
    }

    private static DateTime? ParseDateTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (DateTime.TryParseExact(
                value,
                SupportedDateFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal,
                out var exact))
        {
            return exact;
        }

        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var parsed)
            ? parsed
            : null;
    }

    private static bool IsTransient(HttpStatusCode statusCode)
        => statusCode is HttpStatusCode.RequestTimeout
            or HttpStatusCode.TooManyRequests
            or HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.GatewayTimeout;

    private sealed class EImzoVerifyResponse
    {
        public int Status { get; set; }
        public string? Message { get; set; }
        public EImzoPkcs7Info? Pkcs7Info { get; set; }
    }

    private sealed class EImzoPkcs7Info
    {
        public List<EImzoVerifySigner>? Signers { get; set; }
        public string? DocumentBase64 { get; set; }
    }

    private sealed class EImzoVerifySigner
    {
        public string? SigningTime { get; set; }
        public bool Verified { get; set; }
        public bool CertificateVerified { get; set; }
        public bool CertificateValidAtSigningTime { get; set; }
        public string? Exception { get; set; }
        public string? RevokedStatusInfo { get; set; }
        public List<EImzoCertificate>? Certificate { get; set; }
        public EImzoTimeStampInfo? TimeStampInfo { get; set; }
    }

    private sealed class EImzoCertificate
    {
        public Dictionary<string, string?> SubjectInfo { get; set; } = new(StringComparer.Ordinal);
        public string? SubjectName { get; set; }
        public string? SerialNumber { get; set; }
    }

    private sealed class EImzoTimeStampInfo
    {
        public string? Time { get; set; }
        public bool Verified { get; set; }
        public bool CertificateVerified { get; set; }
    }
}
