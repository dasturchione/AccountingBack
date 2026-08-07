using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Abstractions.Integration.Edo;
using Application.Abstractions.Integration.Faktura;
using Domain.Entities;
using Integration.Faktura.Configs;
using Integration.Faktura.Dtos;
using Integration.Faktura.Http;
using Integration.Edo.Http;
using Integration.Edo.Providers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SharedKernel.Exceptions;
using SharedKernel.Query.Specifications;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Integration.Faktura.Edo;

public sealed class FakturaEdoOperations(
    IHttpClientFactory httpClientFactory,
    IFakturaTokenService tokenService,
    IFakturaAuthSessionStore authSessionStore,
    IOptions<FakturaOptions> options,
    IUserContext userContext,
    IQueryRepository<Organization> organizationQuery)
{
    private const int MaxErrorResponseBodyBytes = 4 * 1024;
    private const int MaxDiagnosticValueLength = 512;
    private static readonly HashSet<string> SafeErrorFieldNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "code",
        "message",
        "error",
        "detail"
    };
    private static readonly Regex SensitiveDiagnosticRegex = new(
        """(?i)(token|password|client[_-]?secret|authorization|pkcs7|signature|private[_-]?key|inn|tin|tax[_-]?id)\s*['"]?\s*[:=]\s*(?:"[^"]*"|'[^']*'|[^,;\s}\]]+)""",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly HashSet<string> SupportedSessionCookieNames =
    [
        ".AspNet.AccountFakturaApp",
        ".ASPXAUTH"
    ];

    private const int MaxRedirects = 8;

    public async Task<EdoAuthCompleteDto> CompleteAuthAsync(
        EdoAuthCompleteRequestDto request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.PreparedPkcs7))
        {
            _ = await tokenService.GetAccessTokenAsync(ct);
            return new EdoAuthCompleteDto { IsAuthenticated = true };
        }

        var preparedPkcs7 = RequireText(request.PreparedPkcs7, nameof(request.PreparedPkcs7));
        var userId = userContext.Id
            ?? throw new IntegrationUnauthorizedException(
                "Faktura E-IMZO authentication requires an authenticated user.");
        var organizationId = userContext.OrganizationId
            ?? throw new InvalidOperationException(
                "Faktura E-IMZO authentication requires an active organization.");
        var settings = options.Value;
        var client = httpClientFactory.CreateClient(FakturaHttpClientNames.AuthClient);

        using var attachRequest = new HttpRequestMessage(HttpMethod.Post, settings.SignatureAttachUrl)
        {
            Content = JsonContent.Create(new FakturaAttachTimestampTokenRequestDto
            {
                Pkcs7 = preparedPkcs7
            })
        };
        using var attachResponse = await client.SendAsync(
            attachRequest,
            HttpCompletionOption.ResponseHeadersRead,
            ct);
        await EnsureSuccessAsync(attachResponse, "Faktura timestamp attachment");

        var timestampResponse = await attachResponse.Content
            .ReadFromJsonAsync<FakturaAttachTimestampTokenResponseDto>(ct)
            ?? throw new IntegrationHttpException(
                "Faktura timestamp attachment response could not be parsed.",
                StatusCodes.Status502BadGateway);
        if (!timestampResponse.Success || string.IsNullOrWhiteSpace(timestampResponse.Data))
        {
            throw new IntegrationHttpException(
                "Faktura timestamp attachment response did not contain a successful data value.",
                StatusCodes.Status502BadGateway);
        }

        using var loginRequest = new HttpRequestMessage(HttpMethod.Post, settings.SignatureLoginUrl)
        {
            Content = JsonContent.Create(new FakturaLoginWithSignatureRequestDto
            {
                RememberMe = false,
                TimeStamp = timestampResponse.Data
            })
        };
        using var loginResponse = await client.SendAsync(
            loginRequest,
            HttpCompletionOption.ResponseHeadersRead,
            ct);

        var cookies = new Dictionary<string, FakturaAuthCookie>(StringComparer.Ordinal);
        CaptureCookies(loginResponse, cookies, DateTimeOffset.UtcNow);
        await EnsureSuccessAsync(loginResponse, "Faktura signature login");

        var loginResult = await loginResponse.Content
            .ReadFromJsonAsync<FakturaLoginWithSignatureResponseDto>(ct)
            ?? throw new IntegrationHttpException(
                "Faktura signature login response could not be parsed.",
                StatusCodes.Status502BadGateway);
        if (!loginResult.Success)
        {
            throw new IntegrationUnauthorizedException(
                "Faktura signature login was not successful.");
        }

        var authorizationUri = BuildAuthorizationUri(settings);
        using var authorizationRequest = new HttpRequestMessage(HttpMethod.Get, authorizationUri);
        AddCookieHeader(authorizationRequest, cookies, DateTimeOffset.UtcNow);
        using var authorizationResponse = await client.SendAsync(
            authorizationRequest,
            HttpCompletionOption.ResponseHeadersRead,
            ct);
        CaptureCookies(authorizationResponse, cookies, DateTimeOffset.UtcNow);

        using var redirectResponse = await FollowRedirectsAsync(
            client,
            authorizationResponse,
            cookies,
            settings,
            ct);
        if (redirectResponse is not null)
        {
            CaptureCookies(redirectResponse, cookies, DateTimeOffset.UtcNow);
            if (!IsRedirect(redirectResponse))
                await EnsureSuccessAsync(redirectResponse, "Faktura signature authorization redirect");
        }
        else
        {
            await EnsureSuccessAsync(authorizationResponse, "Faktura signature authorization");
        }

        if (!cookies.ContainsKey(".AspNet.AccountFakturaApp")
            || !cookies.ContainsKey(".ASPXAUTH"))
        {
            throw new IntegrationHttpException(
                $"Faktura signature authorization did not return the required session cookies (received: {string.Join(", ", cookies.Keys.OrderBy(name => name, StringComparer.Ordinal))}).",
                StatusCodes.Status502BadGateway);
        }

        var now = DateTimeOffset.UtcNow;
        var session = new FakturaAuthSession(
            new FakturaAuthSessionScope(userId, organizationId, EdoProviderCode.FAKTURA),
            cookies.Values.ToArray(),
            now,
            DateTimeOffset.MaxValue);
        await authSessionStore.SaveAsync(session, ct);

        return new EdoAuthCompleteDto { IsAuthenticated = true };
    }

    private static async Task<HttpResponseMessage?> FollowRedirectsAsync(
        HttpClient client,
        HttpResponseMessage response,
        IDictionary<string, FakturaAuthCookie> cookies,
        FakturaOptions settings,
        CancellationToken ct)
    {
        var allowedHosts = GetAllowedHosts(settings);
        var current = response;

        for (var redirectCount = 0; redirectCount < MaxRedirects; redirectCount++)
        {
            if (!IsRedirect(current))
                return null;

            var location = current.Headers.Location
                ?? throw new IntegrationHttpException(
                    "Faktura signature login redirect did not contain a Location header.",
                    StatusCodes.Status502BadGateway);
            var currentUri = current.RequestMessage?.RequestUri
                ?? throw new IntegrationHttpException(
                    "Faktura signature login redirect did not contain a request URI.",
                    StatusCodes.Status502BadGateway);
            var nextUri = location.IsAbsoluteUri
                ? location
                : new Uri(currentUri, location);
            EnsureAllowedRedirect(nextUri, allowedHosts);

            var currentMethod = current.RequestMessage?.Method ?? HttpMethod.Get;
            var nextMethod = GetRedirectMethod(current.StatusCode, currentMethod);
            current.Dispose();

            using var nextRequest = new HttpRequestMessage(nextMethod, nextUri);
            AddCookieHeader(nextRequest, cookies, DateTimeOffset.UtcNow);
            current = await client.SendAsync(
                nextRequest,
                HttpCompletionOption.ResponseHeadersRead,
                ct);
            CaptureCookies(current, cookies, DateTimeOffset.UtcNow);

            if (IsExternalLoginCallback(current)
                && HasUsableCookie(cookies, ".ASPXAUTH", DateTimeOffset.UtcNow))
            {
                return current;
            }
        }

        current.Dispose();
        throw new IntegrationHttpException(
            "Faktura signature login redirect chain exceeded the configured limit.",
            StatusCodes.Status502BadGateway);
    }

    private static Uri BuildAuthorizationUri(FakturaOptions settings)
    {
        var builder = new UriBuilder(RequireText(
            settings.AuthorizationUrl,
            $"{FakturaOptions.SectionName}:AuthorizationUrl"));
        var query = builder.Query.TrimStart('?');
        var parameters = new List<string>();
        if (!string.IsNullOrWhiteSpace(query))
            parameters.Add(query);

        parameters.Add("response_type=code");
        parameters.Add($"client_id={Uri.EscapeDataString(RequireText(
            settings.AuthorizationClientId,
            $"{FakturaOptions.SectionName}:AuthorizationClientId"))}");
        parameters.Add($"redirect_uri={Uri.EscapeDataString(RequireText(
            settings.AuthorizationRedirectUri,
            $"{FakturaOptions.SectionName}:AuthorizationRedirectUri"))}");
        parameters.Add($"state={Uri.EscapeDataString("/")}");
        parameters.Add($"scope={Uri.EscapeDataString(RequireText(
            settings.AuthorizationScope,
            $"{FakturaOptions.SectionName}:AuthorizationScope"))}");

        builder.Query = string.Join("&", parameters);
        return builder.Uri;
    }

    private static bool IsExternalLoginCallback(HttpResponseMessage response)
    {
        var requestUri = response.RequestMessage?.RequestUri;
        return requestUri is not null
            && string.Equals(
                requestUri.AbsolutePath,
                "/account/externallogin",
                StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRedirect(HttpResponseMessage response) =>
        response.StatusCode is HttpStatusCode.MovedPermanently
            or HttpStatusCode.Found
            or HttpStatusCode.SeeOther
            or HttpStatusCode.TemporaryRedirect
            or HttpStatusCode.PermanentRedirect;

    private static HttpMethod GetRedirectMethod(
        HttpStatusCode statusCode,
        HttpMethod currentMethod)
    {
        if (statusCode is HttpStatusCode.TemporaryRedirect or HttpStatusCode.PermanentRedirect)
        {
            if (currentMethod != HttpMethod.Get && currentMethod != HttpMethod.Head)
            {
                throw new IntegrationHttpException(
                    "Faktura signature login returned an unsupported redirect method.",
                    StatusCodes.Status502BadGateway);
            }

            return currentMethod;
        }

        return currentMethod == HttpMethod.Get || currentMethod == HttpMethod.Head
            ? currentMethod
            : HttpMethod.Get;
    }

    private static HashSet<string> GetAllowedHosts(FakturaOptions settings)
    {
        var hosts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        AddHost(settings.BaseUrl, hosts);
        AddHost(settings.AuthUrl, hosts);
        AddHost(settings.SignatureAttachUrl, hosts);
        AddHost(settings.SignatureLoginUrl, hosts);
        AddHost(settings.AuthorizationUrl, hosts);
        return hosts;
    }

    private static void AddHost(string value, ISet<string> hosts)
    {
        if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
            hosts.Add(uri.Host);
    }

    private static void EnsureAllowedRedirect(Uri uri, ISet<string> allowedHosts)
    {
        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
            || !allowedHosts.Contains(uri.Host))
        {
            throw new IntegrationHttpException(
                "Faktura signature login returned a redirect outside the configured HTTPS provider hosts.",
                StatusCodes.Status502BadGateway);
        }
    }

    private static void CaptureCookies(
        HttpResponseMessage response,
        IDictionary<string, FakturaAuthCookie> cookies,
        DateTimeOffset now)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
            return;

        foreach (var header in setCookieHeaders)
        {
            var segments = header.Split(';');
            var separator = segments[0].IndexOf('=');
            if (separator <= 0)
                continue;

            var name = segments[0][..separator].Trim();
            var value = segments[0][(separator + 1)..].Trim();
            if (!SupportedSessionCookieNames.Contains(name)
                || string.IsNullOrWhiteSpace(value)
                || value.Any(char.IsControl)
                || value.Contains(';', StringComparison.Ordinal))
            {
                continue;
            }

            DateTimeOffset? expiresAt = null;
            foreach (var attribute in segments.Skip(1))
            {
                var parts = attribute.Split('=', 2);
                var attributeName = parts[0].Trim();
                if (attributeName.Equals("expires", StringComparison.OrdinalIgnoreCase)
                    && parts.Length == 2
                    && DateTimeOffset.TryParse(
                        parts[1].Trim(),
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AllowWhiteSpaces,
                        out var parsedExpiry))
                {
                    expiresAt = parsedExpiry;
                }
                else if (attributeName.Equals("max-age", StringComparison.OrdinalIgnoreCase)
                    && parts.Length == 2
                    && long.TryParse(
                        parts[1].Trim(),
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out var maxAgeSeconds))
                {
                    expiresAt = maxAgeSeconds <= 0
                        ? now
                        : now.AddSeconds(maxAgeSeconds);
                }
            }

            if (string.IsNullOrWhiteSpace(value)
                || (expiresAt is not null && expiresAt <= now))
            {
                cookies.Remove(name);
                continue;
            }

            cookies[name] = new FakturaAuthCookie(name, value, expiresAt);
        }
    }

    private static bool HasUsableCookie(
        IDictionary<string, FakturaAuthCookie> cookies,
        string name,
        DateTimeOffset now) =>
        cookies.TryGetValue(name, out var cookie)
        && !string.IsNullOrWhiteSpace(cookie.Value)
        && (cookie.ExpiresAt is null || cookie.ExpiresAt > now);

    private static void AddCookieHeader(
        HttpRequestMessage request,
        IDictionary<string, FakturaAuthCookie> cookies,
        DateTimeOffset now)
    {
        var cookieHeader = string.Join(
            "; ",
            cookies.Values
                .Where(cookie => cookie.ExpiresAt is null || cookie.ExpiresAt > now)
                .Select(cookie => $"{cookie.Name}={cookie.Value}"));
        if (!string.IsNullOrWhiteSpace(cookieHeader))
            request.Headers.TryAddWithoutValidation("Cookie", cookieHeader);
    }

    public async Task<EdoOutboxCreateDto> CreateFacturaAsync(
        EdoOutboxFacturaCreateRequestDto request,
        CancellationToken ct = default)
    {
        // TAXMIN: umumiy outbox modelidagi Seller.TaxIdentifier Faktura.uz
        // companyInn query parametriga moslashtiriladi.
        var companyInn = RequireText(request.Seller.TaxIdentifier, "Seller.TaxIdentifier");
        var payload = new FakturaImportDocumentRequestDto
        {
            Invoices = [MapInvoice(request)]
        };

        var client = httpClientFactory.CreateClient(FakturaHttpClientNames.Client);
        var endpoint = $"Api/Document/ImportDocumentRegister?companyInn={Uri.EscapeDataString(companyInn)}";
        using var response = await client.PostAsJsonAsync(
            endpoint,
            payload,
            new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            },
            ct);

        await EnsureSuccessAsync(response, "Faktura factura import");

        var result = await response.Content.ReadFromJsonAsync<FakturaImportDocumentResponseDto>(ct)
            ?? throw new IntegrationHttpException(
                "Faktura import response could not be parsed.",
                StatusCodes.Status502BadGateway);

        if (result.ErrorCount != 0
            || result.SuccessCount != 1
            || result.SuccessItems is null
            || result.SuccessItems.Count != 1)
        {
            throw new IntegrationHttpException(
                $"Faktura import did not return exactly one successful document (success={result.SuccessCount}, errors={result.ErrorCount}).",
                StatusCodes.Status502BadGateway);
        }

        var item = result.SuccessItems.Single();
        // Faktura.uz UniqueId — provider hujjat identifikatori. Id ayrim javoblarda
        // string/null bo'lishi mumkin; u faqat numeric bo'lsa LegacyDocumentId sifatida
        // qo'shimcha mapping qilinadi. Ikkalasi ham mavjud bo'lmasa keyingi provider
        // operatsiyalarini xavfsiz davom ettirib bo'lmaydi.
        var providerDocumentId = RequireProviderDocumentId(
            string.IsNullOrWhiteSpace(item.UniqueId) ? item.Id : item.UniqueId,
            "SuccessItems.UniqueId/Id");
        var legacyDocumentId = ParseLegacyDocumentId(item.Id);

        return new EdoOutboxCreateDto
        {
            Document = new EdoDocumentDto
            {
                LegacyDocumentId = legacyDocumentId,
                ProviderDocumentId = providerDocumentId,
                Direction = EdoDirection.OUTBOX,
                DocumentType = "FACTURA",
                DocumentNumber = request.DocumentNumber,
                DocumentDate = request.DocumentDate,
                // Import response does not document a document status; do not infer success.
                Status = new EdoDocumentStatusDto
                {
                    Code = EdoDocumentStatusCode.UNKNOWN,
                    LocalCode = EdoDocumentStatusCode.UNKNOWN,
                    CheckedAt = DateTimeOffset.UtcNow
                }
            }
        };
    }

    public async Task<EdoOutboxSignDto> SignOutboxAsync(
        string providerDocumentId,
        EdoOutboxSignRequestDto request,
        CancellationToken ct = default)
    {
        var uniqueId = RequireText(providerDocumentId, nameof(providerDocumentId));
        var signedContent = RequireText(request.PreparedPkcs7, nameof(request.PreparedPkcs7));
        var certificateSerial = RequireText(
            request.CertificateSerialNumber,
            nameof(request.CertificateSerialNumber));

        var payload = new FakturaSignDocumentRequestDto
        {
            UniqueId = uniqueId,
            SignedContent = signedContent,
            CertificateSerial = certificateSerial
        };

        var client = httpClientFactory.CreateClient(FakturaHttpClientNames.Client);
        using var response = await client.PostAsJsonAsync(
            "Api/Sign/SignDocument",
            payload,
            new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            },
            ct);

        await EnsureSuccessAsync(response, "Faktura outbox sign");
        var resultCode = await ReadSignResultCodeAsync(response, ct);
        if (resultCode != 0)
        {
            throw new IntegrationHttpException(
                $"Faktura outbox sign returned code {resultCode} ({MapSignResultCode(resultCode)}).",
                StatusCodes.Status502BadGateway);
        }

        return new EdoOutboxSignDto
        {
            Document = new EdoDocumentDto
            {
                ProviderDocumentId = uniqueId,
                Direction = EdoDirection.OUTBOX,
                DocumentType = "FACTURA",
                Status = new EdoDocumentStatusDto
                {
                    Code = EdoDocumentStatusCode.UNKNOWN,
                    LocalCode = EdoDocumentStatusCode.UNKNOWN,
                    CheckedAt = DateTimeOffset.UtcNow
                }
            }
        };
    }

    public Task<EdoInboxListDto> ListInboxAsync(
        EdoInboxQueryDto request,
        CancellationToken ct = default) =>
        ListDocumentsAsync(new EdoDocumentQueryDto
        {
            Scope = EdoDocumentQueryScope.INBOX,
            Page = request.Page,
            Limit = request.PageSize,
            Search = request.Search,
            HasMarks = request.HasMarks,
            Category = request.Category,
            Status = request.Status,
            DateFrom = request.FromDate,
            DateTo = request.ToDate
        }, ct);

    public async Task<EdoInboxListDto> ListDocumentsAsync(
        EdoDocumentQueryDto request,
        CancellationToken ct = default)
    {
        var skip = checked((request.Page - 1) * request.Limit);
        if (request.Scope == EdoDocumentQueryScope.ALL)
            throw new EdoCapabilityUnavailableException(
                EdoProviderCode.FAKTURA.ToString(),
                EdoCapabilityKind.ListAll.ToString(),
                EdoCapabilityStatus.UNKNOWN.ToString());

        var isInbox = request.Scope == EdoDocumentQueryScope.INBOX;
        var companyInn = await RequireOrganizationInnAsync(ct);
        var requestedDirectionCategory = isInbox
            ? EdoDocumentCategory.INBOX
            : EdoDocumentCategory.OUTBOX;
        if (request.Category is not null && request.Category != requestedDirectionCategory)
            throw new EdoCapabilityUnavailableException(
                EdoProviderCode.FAKTURA.ToString(),
                EdoCapabilityKind.SearchFilter.ToString(),
                EdoCapabilityStatus.UNKNOWN.ToString());

        if (request.Search is not null
            || request.HasMarks is not null
            || request.DateFrom is not null
            || request.DateTo is not null
            || request.Status is not null
            || request.ProviderFilters.Count > 0)
            throw new EdoCapabilityUnavailableException(
                EdoProviderCode.FAKTURA.ToString(),
                EdoCapabilityKind.SearchFilter.ToString(),
                EdoCapabilityStatus.UNKNOWN.ToString());

        var query = string.Join(
            "&",
            $"CompanyInn={Uri.EscapeDataString(companyInn)}",
            $"Limit={request.Limit.ToString(CultureInfo.InvariantCulture)}",
            $"Skip={skip.ToString(CultureInfo.InvariantCulture)}",
            $"IsInbox={isInbox.ToString().ToLowerInvariant()}",
            $"IsSent={(!isInbox).ToString().ToLowerInvariant()}");

        var client = httpClientFactory.CreateClient(FakturaHttpClientNames.Client);
        var session = await TryGetAuthSessionAsync(ct);
        using var response = session is not null
            ? await client.PostAsJsonAsync(
                BuildSessionInboxUri(options.Value),
                new
                {
                    Limit = request.Limit,
                    Skip = skip,
                    IsInbox = isInbox,
                    IsSent = !isInbox,
                    IsSign = false,
                    Statuses = Array.Empty<string>()
                },
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = null
                },
                ct)
            : await client.GetAsync(
                $"Api/Document/GetDocuments?{query}",
                HttpCompletionOption.ResponseHeadersRead,
                ct);
        await EnsureSuccessAsync(response, "Faktura inbox list");

        using var json = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(ct),
            cancellationToken: ct);
        if (!json.RootElement.TryGetProperty("documents", out var documents)
            || documents.ValueKind != JsonValueKind.Array)
        {
            throw new IntegrationHttpException(
                "Faktura inbox response did not contain the documented documents array.",
                StatusCodes.Status502BadGateway);
        }

        if (!json.RootElement.TryGetProperty("totalCount", out var totalCountProperty)
            || !totalCountProperty.TryGetInt32(out var totalCount))
        {
            throw new IntegrationHttpException(
                "Faktura inbox response did not contain the documented totalCount field.",
                StatusCodes.Status502BadGateway);
        }

        var items = documents.EnumerateArray()
            .Select(item => ParseInboxDocument(
                item,
                isInbox ? EdoDirection.INBOX : EdoDirection.OUTBOX,
                request.Category))
            .ToList();
        return new EdoInboxListDto
        {
            Items = items,
            Page = ReadOptionalInt(json.RootElement, "page") ?? request.Page,
            PageSize = ReadOptionalInt(json.RootElement, "limit") ?? request.Limit,
            TotalCount = totalCount,
            TotalPages = ReadOptionalInt(json.RootElement, "totalPages"),
            HasNextPage = ReadOptionalBool(json.RootElement, "hasNextPage"),
            HasPrevPage = ReadOptionalBool(json.RootElement, "hasPrevPage")
                ?? ReadOptionalBool(json.RootElement, "hasPreviousPage"),
            HasPreviousPage = ReadOptionalBool(json.RootElement, "hasPreviousPage")
                ?? ReadOptionalBool(json.RootElement, "hasPrevPage"),
            Limit = ReadOptionalInt(json.RootElement, "limit") ?? request.Limit,
            NextPage = ReadOptionalInt(json.RootElement, "nextPage"),
            PrevPage = ReadOptionalInt(json.RootElement, "prevPage"),
            PreviousPage = ReadOptionalInt(json.RootElement, "previousPage")
                ?? ReadOptionalInt(json.RootElement, "prevPage"),
            PagingCounter = ReadOptionalInt(json.RootElement, "pagingCounter")
        };
    }

    public async Task<EdoInboxRejectDto> RejectInboxAsync(
        string providerDocumentId,
        string providerDocumentType,
        CancellationToken ct = default)
    {
        var documentUniqueId = RequireText(providerDocumentId, nameof(providerDocumentId));
        var companyInn = await RequireOrganizationInnAsync(ct);
        var payload = new FakturaRejectDocumentRequestDto
        {
            DocumentUniqueId = documentUniqueId,
            CompanyInn = companyInn
        };

        var client = httpClientFactory.CreateClient(FakturaHttpClientNames.Client);
        using var response = await client.PostAsJsonAsync(
            "Api/Document/RejectDocument",
            payload,
            new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            },
            ct);
        await EnsureSuccessAsync(response, "Faktura inbox reject");

        using var json = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(ct),
            cancellationToken: ct);
        var result = ParseRejectResult(json.RootElement);
        if (!result.Success || result.Code != 0)
        {
            throw new IntegrationHttpException(
                $"Faktura inbox reject returned code {result.Code} ({MapRejectResultCode(result.Code)}).",
                StatusCodes.Status502BadGateway);
        }

        return new EdoInboxRejectDto
        {
            Document = new EdoDocumentDto
            {
                ProviderDocumentId = documentUniqueId,
                Direction = EdoDirection.INBOX,
                DocumentType = string.IsNullOrWhiteSpace(providerDocumentType)
                    ? "UNKNOWN"
                    : providerDocumentType,
                Status = new EdoDocumentStatusDto
                {
                    Code = EdoDocumentStatusCode.REJECTED,
                    ProviderStatusCode = result.Code.ToString(CultureInfo.InvariantCulture),
                    IsTerminal = true,
                    IsSuccessful = true,
                    CheckedAt = DateTimeOffset.UtcNow
                }
            }
        };
    }

    public async Task<EdoFileDto> GetFileAsync(
        string providerDocumentId,
        CancellationToken ct = default)
    {
        var uniqueId = RequireText(providerDocumentId, nameof(providerDocumentId));
        if (await TryGetAuthSessionAsync(ct) is null)
        {
            throw new IntegrationUnauthorizedException(
                "Faktura PDF download requires an active E-IMZO session.");
        }

        var client = httpClientFactory.CreateClient(FakturaHttpClientNames.Client);
        var response = await client.GetAsync(
            BuildSessionFileUri(options.Value, uniqueId),
            HttpCompletionOption.ResponseHeadersRead,
            ct);

        try
        {
            await EnsureSuccessAsync(response, "Faktura PDF download");
            var contentType = response.Content.Headers.ContentType?.MediaType;
            if (!string.Equals(contentType, "application/pdf", StringComparison.OrdinalIgnoreCase))
            {
                throw new IntegrationHttpException(
                    "Faktura PDF response did not contain the documented application/pdf content type.",
                    StatusCodes.Status502BadGateway);
            }
            var pdfContentType = contentType!;

            if (!string.Equals(
                    response.Content.Headers.ContentDisposition?.DispositionType,
                    "attachment",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new IntegrationHttpException(
                    "Faktura PDF response did not contain the documented attachment disposition.",
                    StatusCodes.Status502BadGateway);
            }

            var stream = await response.Content.ReadAsStreamAsync(ct);
            return new EdoFileDto
            {
                ProviderFileId = uniqueId,
                FileName = "UNKNOWN",
                ContentType = pdfContentType,
                Length = response.Content.Headers.ContentLength ?? -1,
                Content = new EdoProviderResponseStream(stream, response, long.MaxValue)
            };
        }
        catch
        {
            response.Dispose();
            throw;
        }
    }

    public Task<EdoDocumentStatusDto> GetOutboxStatusAsync(
        string providerDocumentId,
        CancellationToken ct = default) =>
        GetStatusAsync(providerDocumentId, ct);

    public Task<EdoDocumentStatusDto> GetInboxStatusAsync(
        string providerDocumentId,
        CancellationToken ct = default) =>
        GetInboxStatusFromSessionAsync(providerDocumentId, ct);

    private async Task<EdoDocumentStatusDto> GetInboxStatusFromSessionAsync(
        string providerDocumentId,
        CancellationToken ct)
    {
        var uniqueId = RequireText(providerDocumentId, nameof(providerDocumentId));
        if (await TryGetAuthSessionAsync(ct) is null)
        {
            throw new IntegrationUnauthorizedException(
                "Faktura inbox status requires an active E-IMZO session.");
        }

        var client = httpClientFactory.CreateClient(FakturaHttpClientNames.Client);
        using var response = await client.PostAsJsonAsync(
            BuildSessionInboxUri(options.Value),
            new
            {
                Limit = 100,
                Skip = 0,
                IsInbox = true,
                IsSent = false,
                IsSign = false,
                Statuses = Array.Empty<string>()
            },
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = null
            },
            ct);
        await EnsureSuccessAsync(response, "Faktura inbox status");

        using var json = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(ct),
            cancellationToken: ct);
        if (!json.RootElement.TryGetProperty("documents", out var documents)
            || documents.ValueKind != JsonValueKind.Array)
        {
            throw new IntegrationHttpException(
                "Faktura inbox status response did not contain the documented documents array.",
                StatusCodes.Status502BadGateway);
        }

        var document = documents.EnumerateArray()
            .FirstOrDefault(item => string.Equals(
                ReadOptionalString(item, "uniqueId"),
                uniqueId,
                StringComparison.Ordinal));
        if (document.ValueKind == JsonValueKind.Undefined)
        {
            throw new IntegrationHttpException(
                "Faktura inbox status response did not contain the requested document.",
                StatusCodes.Status404NotFound);
        }

        if (!document.TryGetProperty("status", out var statusProperty)
            || statusProperty.ValueKind is not (JsonValueKind.String or JsonValueKind.Number))
        {
            throw new IntegrationHttpException(
                "Faktura inbox status item did not contain the documented status field.",
                StatusCodes.Status502BadGateway);
        }

        return new EdoDocumentStatusDto
        {
            Code = EdoDocumentStatusCode.UNKNOWN,
            LocalCode = EdoDocumentStatusCode.UNKNOWN,
            ProviderStatusCode = ReadProviderStatusCode(statusProperty),
            CheckedAt = DateTimeOffset.UtcNow
        };
    }

    private async Task<EdoDocumentStatusDto> GetStatusAsync(
        string providerDocumentId,
        CancellationToken ct)
    {
        var uniqueId = RequireText(providerDocumentId, nameof(providerDocumentId));
        var companyInn = await RequireOrganizationInnAsync(ct);
        var payload = new FakturaDocumentStatusRequestDto
        {
            DocumentUniqueIds = [uniqueId]
        };

        var client = httpClientFactory.CreateClient(FakturaHttpClientNames.Client);
        using var response = await client.PostAsJsonAsync(
            $"Api/GetDocumentStatus?companyInn={Uri.EscapeDataString(companyInn)}",
            payload,
            new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            },
            ct);
        await EnsureSuccessAsync(response, "Faktura document status");

        using var json = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(ct),
            cancellationToken: ct);
        return ParseDocumentStatus(json.RootElement, uniqueId);
    }

    private static FakturaInvoiceDto MapInvoice(EdoOutboxFacturaCreateRequestDto request) => new()
    {
        Head = new FakturaHeadDto
        {
            Sender = MapParty(request.Seller, true),
            Receiver = MapParty(request.Buyer, false)
        },
        Document = new FakturaDocumentDto
        {
            DocumentNumber = RequireText(request.DocumentNumber, nameof(request.DocumentNumber)),
            DocumentDate = request.DocumentDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
            ContractNumber = request.ContractNumber,
            ContractDate = request.ContractDate?.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
            Items = request.Lines.Select(MapItem).ToList(),
            ColumnSummaryValues = BuildSummary(request.Lines)
        }
    };

    private static FakturaPartyEnvelopeDto MapParty(EdoPartyDto party, bool sender) =>
        sender
            ? new FakturaPartyEnvelopeDto
            {
                SenderInfo = MapPartyInfo(party),
                ApproverAndSigners = MapSigners(party)
            }
            : new FakturaPartyEnvelopeDto
            {
                ReceiverInfo = MapPartyInfo(party),
                ApproverAndSigners = MapSigners(party)
            };

    private static FakturaPartyInfoDto MapPartyInfo(EdoPartyDto party) => new()
    {
        INN = RequireText(party.TaxIdentifier, "Party.TaxIdentifier"),
        CompanyName = RequireText(party.Name, "Party.Name"),
        Address = string.IsNullOrWhiteSpace(party.Address)
            ? null
            : new FakturaAddressDto { Street = party.Address },
        BankDetails = string.IsNullOrWhiteSpace(party.BankCode)
            && string.IsNullOrWhiteSpace(party.AccountNumber)
            ? null
            : new FakturaBankDetailsDto
            {
                AccountNumber = party.AccountNumber,
                BankCode = party.BankCode
            }
    };

    private static FakturaApproverAndSignersDto? MapSigners(EdoPartyDto party)
    {
        if (string.IsNullOrWhiteSpace(party.DirectorName)
            && string.IsNullOrWhiteSpace(party.AccountantName))
        {
            return null;
        }

        return new FakturaApproverAndSignersDto
        {
            Approver = MapEmployee(party.DirectorName),
            SignerAccountant = MapEmployee(party.AccountantName)
        };
    }

    private static FakturaEmployeeDto? MapEmployee(string? name) =>
        string.IsNullOrWhiteSpace(name)
            ? null
            : new FakturaEmployeeDto { FirstName = name };

    private static FakturaItemDto MapItem(EdoFacturaLineDto line)
    {
        if (line.Quantity <= 0 || line.Quantity != decimal.Truncate(line.Quantity))
            throw new InvalidOperationException("Line.Quantity must be a positive integer for Faktura import.");

        var unit = RequireText(line.UnitName ?? line.UnitCode, "Line.UnitName/UnitCode");
        var taxRate = line.TaxRate;
        var taxAmount = line.TaxAmount ?? 0m;

        return new FakturaItemDto
        {
            ItemNumber = line.Number.ToString(CultureInfo.InvariantCulture),
            Description = RequireText(line.Name, "Line.Name"),
            Volume = checked((int)line.Quantity),
            UnitPrice = line.Amount / line.Quantity,
            Subtotal = line.Amount,
            Vat = taxRate is null
                ? null
                : new FakturaVatDto
                {
                    VatRate = $"{taxRate.Value.ToString(CultureInfo.InvariantCulture)}%",
                    VatValue = taxAmount
                },
            SubtotalWithTaxes = line.Amount + taxAmount,
            MeasurementUnit = unit
        };
    }

    private static FakturaColumnSummaryValuesDto BuildSummary(
        IReadOnlyCollection<EdoFacturaLineDto> lines)
    {
        var subtotal = lines.Sum(line => line.Amount);
        var vat = lines.Sum(line => line.TaxAmount ?? 0m);
        return new FakturaColumnSummaryValuesDto
        {
            ColumnSubtotal = subtotal,
            ColumnVatValue = vat,
            ColumnSubtotalWithTaxes = subtotal + vat
        };
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, string operation)
    {
        if (response.IsSuccessStatusCode)
            return;

        var status = (int)response.StatusCode;
        var diagnostics = await ReadErrorDiagnosticsAsync(response);
        throw response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => new IntegrationUnauthorizedException(
                $"{operation} was rejected by Faktura. {diagnostics}"),
            HttpStatusCode.Forbidden => new IntegrationForbiddenException(
                $"{operation} was denied by Faktura. {diagnostics}"),
            _ => new IntegrationHttpException(
                $"{operation} failed with HTTP status {status}. {diagnostics}",
                status)
        };
    }

    private static async Task<string> ReadErrorDiagnosticsAsync(HttpResponseMessage response)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();
        var buffer = new byte[MaxErrorResponseBodyBytes];
        var offset = 0;
        while (offset < buffer.Length)
        {
            var bytesRead = await stream.ReadAsync(buffer.AsMemory(offset));
            if (bytesRead == 0)
                break;

            offset += bytesRead;
        }

        var body = System.Text.Encoding.UTF8.GetString(buffer, 0, offset);
        var fields = new List<string>();
        try
        {
            using var json = JsonDocument.Parse(body);
            CollectSafeErrorFields(json.RootElement, fields);
        }
        catch (JsonException)
        {
            // The bounded body is included only as a redacted summary below.
        }

        var fieldSummary = fields.Count == 0 ? "none" : string.Join(", ", fields);
        var bodySummary = RedactDiagnosticText(body);
        var requestUri = response.RequestMessage?.RequestUri;
        return $"status={(int)response.StatusCode}, "
            + $"host={requestUri?.Host ?? "none"}, "
            + $"path={requestUri?.AbsolutePath ?? "none"}, "
            + $"contentType={response.Content.Headers.ContentType?.ToString() ?? "none"}, "
            + $"bodyPresent={offset > 0}, "
            + $"fields={fieldSummary}, "
            + $"bodySummary={bodySummary ?? "none"}";
    }

    private static void CollectSafeErrorFields(JsonElement element, ICollection<string> fields, int depth = 0)
    {
        if (depth > 5 || fields.Count >= 16)
            return;

        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (SafeErrorFieldNames.Contains(property.Name)
                    && property.Value.ValueKind is JsonValueKind.String
                        or JsonValueKind.Number
                        or JsonValueKind.True
                        or JsonValueKind.False)
                {
                    var value = property.Value.ValueKind == JsonValueKind.String
                        ? property.Value.GetString()
                        : property.Value.GetRawText();
                    var safeValue = RedactDiagnosticText(value);
                    if (!string.IsNullOrWhiteSpace(safeValue))
                        fields.Add($"{property.Name}={safeValue}");
                }

                CollectSafeErrorFields(property.Value, fields, depth + 1);
                if (fields.Count >= 16)
                    return;
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                CollectSafeErrorFields(item, fields, depth + 1);
                if (fields.Count >= 16)
                    return;
            }
        }
    }

    private static string? RedactDiagnosticText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var redacted = SensitiveDiagnosticRegex.Replace(value, "$1=<redacted>");
        var compact = Regex.Replace(redacted, @"\s+", " ").Trim();
        return compact.Length <= MaxDiagnosticValueLength
            ? compact
            : compact[..MaxDiagnosticValueLength] + "...";
    }

    private static async Task<int> ReadSignResultCodeAsync(
        HttpResponseMessage response,
        CancellationToken ct)
    {
        var body = (await response.Content.ReadAsStringAsync(ct)).Trim();
        if (int.TryParse(body, NumberStyles.Integer, CultureInfo.InvariantCulture, out var rawCode))
            return rawCode;

        try
        {
            using var json = JsonDocument.Parse(body);
            var value = json.RootElement;
            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var numericCode))
                return numericCode;
            if (value.ValueKind == JsonValueKind.String
                && int.TryParse(value.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var stringCode))
                return stringCode;
        }
        catch (JsonException)
        {
            // The documented response wrapper is incomplete; the caller receives a controlled integration error below.
        }

        throw new IntegrationHttpException(
            "Faktura outbox sign response code could not be determined from the documented scalar response.",
            StatusCodes.Status502BadGateway);
    }

    private static string MapSignResultCode(int code) => code switch
    {
        1 => "BalanceIsLow",
        3 => "SignatureValidationFailed",
        4 => "InternalError",
        5 => "OfferNotSigned",
        6 => "NotAllowedNow",
        7 => "CreditLimitIsLow",
        _ => "UNKNOWN"
    };

    private static (bool Success, int Code) ParseRejectResult(JsonElement root)
    {
        if (!root.TryGetProperty("Success", out var successProperty)
            || successProperty.ValueKind is not (JsonValueKind.True or JsonValueKind.False)
            || !root.TryGetProperty("code", out var codeProperty)
            || !codeProperty.TryGetInt32(out var code))
        {
            throw new IntegrationHttpException(
                "Faktura reject response did not contain the documented Success and code fields.",
                StatusCodes.Status502BadGateway);
        }

        return (successProperty.GetBoolean(), code);
    }

    private static string MapRejectResultCode(int code) => code switch
    {
        0 => "Success",
        2 => "YouAreNotAMemberOfDocument",
        3 => "SignatureValidationFailed",
        4 => "InternalError",
        6 => "NotAllowedNow",
        _ => "UNKNOWN"
    };

    private static EdoDocumentStatusDto ParseDocumentStatus(
        JsonElement root,
        string providerDocumentId)
    {
        if (!root.TryGetProperty("Success", out var successProperty)
            || successProperty.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
        {
            throw new IntegrationHttpException(
                "Faktura status response did not contain the documented Success field.",
                StatusCodes.Status502BadGateway);
        }

        if (!successProperty.GetBoolean())
        {
            throw new IntegrationHttpException(
                "Faktura status response reported Success=false.",
                StatusCodes.Status502BadGateway);
        }

        if (!root.TryGetProperty("Data", out var data)
            || data.ValueKind != JsonValueKind.Object
            || !data.TryGetProperty("DocumentStatuses", out var statuses)
            || statuses.ValueKind != JsonValueKind.Array)
        {
            throw new IntegrationHttpException(
                "Faktura status response did not contain the documented Data.DocumentStatuses array.",
                StatusCodes.Status502BadGateway);
        }

        var statusItem = statuses.EnumerateArray()
            .FirstOrDefault(item => item.TryGetProperty("UniqueId", out var idProperty)
                && idProperty.ValueKind == JsonValueKind.String
                && string.Equals(idProperty.GetString(), providerDocumentId, StringComparison.Ordinal));
        if (statusItem.ValueKind == JsonValueKind.Undefined)
        {
            throw new IntegrationHttpException(
                "Faktura status response did not contain the requested document UniqueId.",
                StatusCodes.Status502BadGateway);
        }

        if (!statusItem.TryGetProperty("Status", out var statusProperty))
        {
            throw new IntegrationHttpException(
                "Faktura status item did not contain the documented Status field.",
                StatusCodes.Status502BadGateway);
        }

        var providerStatusCode = ReadProviderStatusCode(statusProperty);
        var description = ReadStatusDescription(statusItem);
        return new EdoDocumentStatusDto
        {
            Code = EdoDocumentStatusCode.UNKNOWN,
            LocalCode = EdoDocumentStatusCode.UNKNOWN,
            ProviderStatusCode = providerStatusCode,
            Description = description,
            CheckedAt = DateTimeOffset.UtcNow
        };
    }

    private static string ReadProviderStatusCode(JsonElement statusProperty) =>
        statusProperty.ValueKind switch
        {
            JsonValueKind.String => statusProperty.GetString() ?? "UNKNOWN",
            JsonValueKind.Number => statusProperty.GetRawText(),
            _ => "UNKNOWN"
        };

    private static string? ReadStatusDescription(JsonElement statusItem)
    {
        if (!statusItem.TryGetProperty("StatusDescription", out var descriptionProperty)
            || descriptionProperty.ValueKind != JsonValueKind.Array)
            return null;

        var descriptions = descriptionProperty.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString())
            .Where(value => !string.IsNullOrWhiteSpace(value));
        return string.Join("; ", descriptions);
    }

    private async Task<string> RequireOrganizationInnAsync(CancellationToken ct)
    {
        var organizationId = userContext.OrganizationId
            ?? throw new InvalidOperationException("An active organization is required for Faktura integration.");
        var organization = await organizationQuery.GetAsync(
            new QuerySpecification<Organization> { Criteria = x => x.Id == organizationId },
            ct);
        if (organization is null || string.IsNullOrWhiteSpace(organization.Inn))
        {
            throw new IntegrationHttpException(
                "The current organization is missing the INN required by the Faktura provider.",
                StatusCodes.Status502BadGateway);
        }

        return organization.Inn.Trim();
    }

    private async Task<FakturaAuthSession?> TryGetAuthSessionAsync(CancellationToken ct)
    {
        if (userContext.Id is not int userId
            || userContext.OrganizationId is not int organizationId)
        {
            return null;
        }

        try
        {
            return await authSessionStore.GetAsync(
                new FakturaAuthSessionScope(userId, organizationId, EdoProviderCode.FAKTURA),
                ct);
        }
        catch (Exception ex) when (ex is IOException
            or UnauthorizedAccessException
            or InvalidOperationException)
        {
            return null;
        }
    }

    private static Uri BuildSessionInboxUri(FakturaOptions settings)
    {
        return BuildSessionAppUri(settings, "ru/document/getdocuments");
    }

    private static Uri BuildSessionFileUri(FakturaOptions settings, string providerDocumentId)
    {
        var endpoint = new UriBuilder(
            BuildSessionAppUri(settings, "ru/document/downloaddocumentpdf"))
        {
            Query = $"uniqueid={Uri.EscapeDataString(providerDocumentId)}"
        };
        return endpoint.Uri;
    }

    private static Uri BuildSessionAppUri(FakturaOptions settings, string path)
    {
        var signatureAttachUri = new Uri(
            RequireText(
                settings.SignatureAttachUrl,
                $"{FakturaOptions.SectionName}:SignatureAttachUrl"),
            UriKind.Absolute);
        if (!string.Equals(
                signatureAttachUri.Scheme,
                Uri.UriSchemeHttps,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"{FakturaOptions.SectionName}:SignatureAttachUrl must use HTTPS.");
        }

        return new Uri(
            $"{signatureAttachUri.Scheme}://{signatureAttachUri.Authority}/{path.TrimStart('/')}",
            UriKind.Absolute);
    }

    private static EdoDocumentDto ParseInboxDocument(
        JsonElement item,
        EdoDirection direction,
        EdoDocumentCategory? requestedCategory)
    {
        var providerDocumentId = ReadOptionalString(item, "uniqueId")
            ?? ReadOptionalString(item, "UniqueId");
        if (string.IsNullOrWhiteSpace(providerDocumentId))
        {
            throw new IntegrationHttpException(
                "Faktura inbox item did not contain the required uniqueId field.",
                StatusCodes.Status502BadGateway);
        }

        var providerStatus = ReadOptionalString(item, "status");
        var status = EdoProviderStatusMapper.Map(providerStatus);
        return new EdoDocumentDto
        {
            ProviderCode = EdoProviderCode.FAKTURA,
            ProviderDocumentId = providerDocumentId,
            Direction = direction,
            Category = EdoProviderStatusMapper.MapCategory(direction, status.Code, requestedCategory),
            DocumentType = "UNKNOWN",
            TotalAmount = ReadOptionalDecimal(item, "totalPrice"),
            CreatedAt = ReadOptionalDateTimeOffset(item, "createdDateTime"),
            UpdatedAt = ReadOptionalDateTimeOffset(item, "updatedDateTime"),
            Status = new EdoDocumentStatusDto
            {
                Code = status.Code,
                LocalCode = status.LocalCode,
                ProviderStatusCode = status.ProviderStatusCode,
                ProviderRawStatus = status.ProviderRawStatus,
                IsTerminal = status.IsTerminal,
                IsSuccessful = status.IsSuccessful,
                CheckedAt = DateTimeOffset.UtcNow
            },
            ProviderFields = ReadProviderFields(item)
        };
    }

    private static IReadOnlyDictionary<string, JsonElement> ReadProviderFields(JsonElement item)
    {
        if (item.ValueKind != JsonValueKind.Object)
            return new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);

        var fields = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
        foreach (var property in item.EnumerateObject())
        {
            if (property.Name.Contains("token", StringComparison.OrdinalIgnoreCase)
                || property.Name.Contains("pkcs7", StringComparison.OrdinalIgnoreCase)
                || property.Name.Contains("signature", StringComparison.OrdinalIgnoreCase)
                || property.Name.Contains("authorization", StringComparison.OrdinalIgnoreCase)
                || property.Name.Contains("password", StringComparison.OrdinalIgnoreCase)
                || property.Name.Contains("secret", StringComparison.OrdinalIgnoreCase))
                continue;

            fields[property.Name] = property.Value.Clone();
        }

        return fields;
    }

    private static string? ReadOptionalString(JsonElement item, string propertyName)
    {
        if (!item.TryGetProperty(propertyName, out var property)
            || property.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        var value = property.GetString();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static int? ReadOptionalInt(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var property)
            || property.ValueKind == JsonValueKind.Null
            || property.ValueKind == JsonValueKind.Undefined)
            return null;

        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value))
            return value;

        throw new IntegrationHttpException(
            $"Faktura response field '{propertyName}' must be an integer.",
            StatusCodes.Status502BadGateway);
    }

    private static bool? ReadOptionalBool(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var property)
            || property.ValueKind == JsonValueKind.Null
            || property.ValueKind == JsonValueKind.Undefined)
            return null;

        if (property.ValueKind is JsonValueKind.True or JsonValueKind.False)
            return property.GetBoolean();

        throw new IntegrationHttpException(
            $"Faktura response field '{propertyName}' must be a boolean.",
            StatusCodes.Status502BadGateway);
    }

    private static decimal? ReadOptionalDecimal(JsonElement item, string propertyName)
    {
        if (!item.TryGetProperty(propertyName, out var property))
            return null;

        if (property.ValueKind == JsonValueKind.Number
            && property.TryGetDecimal(out var number))
        {
            return number;
        }

        return property.ValueKind == JsonValueKind.String
            && decimal.TryParse(
                property.GetString(),
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var parsed)
            ? parsed
            : null;
    }

    private static DateTimeOffset? ReadOptionalDateTimeOffset(
        JsonElement item,
        string propertyName)
    {
        var value = ReadOptionalString(item, propertyName);
        return DateTimeOffset.TryParse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind,
            out var parsed)
            ? parsed
            : null;
    }

    private static string RequireText(string? value, string name) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException($"{name} is required for Faktura integration.")
            : value;

    private static string RequireProviderDocumentId(string? value, string name) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new IntegrationHttpException(
                $"Faktura import response did not contain a provider document ID ({name}).",
                StatusCodes.Status502BadGateway)
            : value;

    private static long? ParseLegacyDocumentId(string? value) =>
        long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id)
            && id > 0
            ? id
            : null;
}
