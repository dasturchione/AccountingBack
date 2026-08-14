using Application.Abstractions.Authentication;
using Application.Abstractions.Integration.Edo;
using Integration.Didox.Http;
using Integration.Didox.Services;
using Integration.Edo.Http;
using Integration.Edo.Historical;
using Integration.Edo.Providers;
using Integration.Shared.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using SharedKernel.Exceptions;
using SharedKernel.Text;

namespace Integration.Didox.Facturas;

public sealed class DidoxEdoOperations(
    IUserContext userContext,
    IHttpClientFactory httpClientFactory,
    DidoxTimestampClient timestampClient,
    ILogger<DidoxEdoOperations>? logger = null)
{
    private const int MaxFileLength = 25 * 1024 * 1024;
    private const int MaxHistoricalEmbeddedJsonLength = 1024 * 1024;
    private const int MaxHistoricalEmbeddedJsonDepth = 32;
    private const int MaxHistoricalEmbeddedJsonUnwrapDepth = 4;
    private static readonly JsonSerializerOptions JsonOptions = new();
    private static readonly string[] HistoricalDetailPayloadPropertyNames = ["json", "document_json", "document"];

    public Task<EdoInboxListDto> ListInboxAsync(EdoInboxQueryDto request, CancellationToken ct) =>
        ListDocumentsAsync(new EdoDocumentQueryDto
        {
            Scope = EdoDocumentQueryScope.INBOX,
            Page = request.Page,
            Limit = request.PageSize,
            Search = request.Search,
            HasMarks = request.HasMarks,
            Status = request.Status,
            DateFrom = request.DateFrom,
            DateTo = request.DateTo
        }, ct);

    public async Task<EdoInboxListDto> ListDocumentsAsync(EdoDocumentQueryDto request, CancellationToken ct)
    {
        if (request.Scope == EdoDocumentQueryScope.ALL)
            throw new EdoCapabilityUnavailableException(
                EdoProviderCode.DIDOX.ToString(),
                EdoCapabilityKind.ListAll.ToString(),
                EdoCapabilityStatus.UNKNOWN.ToString());

        if (request.ProviderFilters.Count > 0)
            throw new EdoCapabilityUnavailableException(
                EdoProviderCode.DIDOX.ToString(),
                EdoCapabilityKind.SearchFilter.ToString(),
                EdoCapabilityStatus.UNKNOWN.ToString());

        var query = new List<string>
        {
            $"owner={(request.Scope == EdoDocumentQueryScope.OUTBOX ? "1" : "0")}",
            $"page={request.Page}",
            $"limit={request.Limit}"
        };
        AddQuery(query, "name", request.Search);
        AddQuery(query, "hasMarks", request.HasMarks?.ToString().ToLowerInvariant());
        AddQuery(query, "dateFromCreated", request.DateFrom?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        AddQuery(query, "dateToCreated", request.DateTo?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        AddQuery(query, "status", MapStatusFilter(request.Status, request.Category));

        using var response = await SendAsync(HttpMethod.Get, $"v2/documents?{string.Join('&', query)}", ct);
        await EnsureSuccessAsync(response, "Didox inbox list");
        using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        if (!json.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
            throw new IntegrationHttpException("Didox inbox response did not contain the documented data array.", StatusCodes.Status502BadGateway);

        var direction = request.Scope == EdoDocumentQueryScope.OUTBOX
            ? EdoDirection.OUTBOX
            : EdoDirection.INBOX;
        var items = data.EnumerateArray()
            .Select(item => ParseDocument(item, direction, requestedCategory: request.Category))
            .ToList();
        int? total = null;
        if (json.RootElement.TryGetProperty("total", out var totalProperty))
        {
            if (totalProperty.ValueKind != JsonValueKind.Number
                || !totalProperty.TryGetInt32(out var totalValue))
            {
                throw new IntegrationHttpException(
                    "Didox response field 'total' must be an integer.",
                    StatusCodes.Status502BadGateway);
            }

            total = totalValue;
        }

        var page = ReadOptionalInt(json.RootElement, "page") ?? request.Page;
        var limit = ReadOptionalInt(json.RootElement, "limit") ?? request.Limit;
        var hasPreviousPage = ReadOptionalBool(json.RootElement, "hasPreviousPage")
            ?? ReadOptionalBool(json.RootElement, "hasPrevPage");

        return new EdoInboxListDto
        {
            Items = items,
            Page = page,
            PageSize = limit,
            TotalCount = total,
            TotalPages = ReadOptionalInt(json.RootElement, "totalPages"),
            HasNextPage = ReadOptionalBool(json.RootElement, "hasNextPage"),
            HasPrevPage = hasPreviousPage,
            HasPreviousPage = hasPreviousPage,
            Limit = limit,
            NextPage = ReadOptionalInt(json.RootElement, "nextPage"),
            PrevPage = ReadOptionalInt(json.RootElement, "prevPage"),
            PreviousPage = ReadOptionalInt(json.RootElement, "previousPage")
                ?? ReadOptionalInt(json.RootElement, "prevPage"),
            PagingCounter = ReadOptionalInt(json.RootElement, "pagingCounter")
        };
    }

    public async Task<EdoHistoricalPageResultDto> ListHistoricalSignedInboxAsync(
        int organizationId,
        EdoHistoricalPageRequestDto request,
        CancellationToken ct)
    {
        ValidateHistoricalPageRequest(organizationId, request);
        var query = string.Join('&',
            $"page={request.Page}",
            $"limit={request.PageSize}",
            "owner=0",
            "status=3",
            "doctype=002");

        using var response = await SendAsync(
            organizationId,
            HttpMethod.Get,
            $"v2/documents?{query}",
            ct);
        await EnsureSuccessAsync(response, "Didox historical inbox list");

        try
        {
            using var json = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(ct),
                cancellationToken: ct);
            if (!json.RootElement.TryGetProperty("data", out var data)
                || data.ValueKind != JsonValueKind.Array)
            {
                throw new EdoHistoricalMappingException("DIDOX_HISTORICAL_DATA_ARRAY_REQUIRED");
            }

            var items = data.EnumerateArray()
                .Select(item => MapHistoricalSummary(ParseDocument(
                    item,
                    EdoDirection.INBOX,
                    fallbackDocumentType: "002")))
                .ToArray();
            var providerTotal = ReadOptionalInt(json.RootElement, "total");
            var page = ReadOptionalInt(json.RootElement, "page") ?? request.Page;
            var pageSize = ReadOptionalInt(json.RootElement, "limit") ?? request.PageSize;
            var hasNextUrlMetadata = TryGetPropertyIgnoreCase(
                json.RootElement,
                "next_page_url",
                out var nextPageUrlProperty);
            string? nextPageUrl = null;
            if (hasNextUrlMetadata
                && nextPageUrlProperty.ValueKind is not JsonValueKind.Null and not JsonValueKind.Undefined)
            {
                if (nextPageUrlProperty.ValueKind != JsonValueKind.String)
                    throw new EdoHistoricalMappingException("DIDOX_NEXT_PAGE_URL_INVALID");

                nextPageUrl = nextPageUrlProperty.GetString();
            }

            bool? hasNextPage = hasNextUrlMetadata
                ? !string.IsNullOrWhiteSpace(nextPageUrl)
                : providerTotal.HasValue
                    ? page * pageSize < providerTotal.Value
                    : null;
            int? nextPage = !string.IsNullOrWhiteSpace(nextPageUrl)
                ? ReadPageFromUrl(nextPageUrl) ?? page + 1
                : hasNextPage == true
                    ? page + 1
                    : null;

            return EdoHistoricalSourceSupport.BuildSuccessfulPage(
                EdoProviderCode.DIDOX,
                request,
                page,
                pageSize,
                providerTotal,
                hasNextPage,
                nextPage,
                hasCompleteMetadata: providerTotal.HasValue || hasNextUrlMetadata,
                providerRequiresOverlapRescan: true,
                items);
        }
        catch (EdoHistoricalMappingException)
        {
            throw;
        }
        catch (Exception exception) when (exception is JsonException or IntegrationHttpException)
        {
            throw new EdoHistoricalMappingException("DIDOX_HISTORICAL_RESPONSE_INVALID", exception);
        }
    }

    public async Task<EdoDocumentDto> GetHistoricalDocumentDetailsAsync(
        int organizationId,
        string providerDocumentType,
        string providerDocumentId,
        CancellationToken ct)
    {
        if (organizationId <= 0)
            throw new ArgumentOutOfRangeException(nameof(organizationId));

        var id = RequireProviderDocumentId(providerDocumentId);
        using var response = await SendAsync(
            organizationId,
            HttpMethod.Get,
            $"v1/documents/{Uri.EscapeDataString(id)}?owner=0",
            ct);
        await EnsureSuccessAsync(response, "Didox historical document details");

        try
        {
            using var json = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(ct),
                cancellationToken: ct);
            var root = json.RootElement;
            try
            {
                ValidateHistoricalDetailEnvelope(root);
                ValidateHistoricalDetailIdentity(root, id);
                ValidateHistoricalDetailStatus(root);

                return ParseDocument(root, EdoDirection.INBOX, providerDocumentType, id);
            }
            catch (EdoHistoricalMappingException exception)
            {
                LogHistoricalDetailDiagnostic(exception.SafeFailureCode, root);
                throw;
            }
        }
        catch (JsonException)
        {
            LogHistoricalDetailDiagnostic("DIDOX_DETAIL_ENVELOPE_INVALID", root: null);
            throw new EdoHistoricalMappingException("DIDOX_DETAIL_ENVELOPE_INVALID");
        }
    }

    public async Task<EdoDocumentDto> GetDocumentDetailsAsync(
        EdoDirection direction,
        string providerDocumentType,
        string providerDocumentId,
        CancellationToken ct)
    {
        var id = RequireProviderDocumentId(providerDocumentId);
        using var response = await SendAsync(
            HttpMethod.Get,
            $"v1/documents/{Uri.EscapeDataString(id)}?owner={(direction == EdoDirection.OUTBOX ? "1" : "0")}",
            ct);
        await EnsureSuccessAsync(response, "Didox document details");
        using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        var root = json.RootElement;
        return ParseDocument(root, direction, providerDocumentType, id);
    }

    public async Task<EdoInboxRejectDto> RejectInboxAsync(string providerDocumentId, EdoInboxRejectRequestDto request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.PreparedPkcs7) && string.IsNullOrWhiteSpace(request.SignatureHex))
        {
            using var challengeResponse = await SendJsonAsync(
                HttpMethod.Post,
                $"v1/documents/{Uri.EscapeDataString(providerDocumentId)}/tosign",
                new { comment = request.Reason, action = "reject" },
                ct);
            await EnsureSuccessAsync(challengeResponse, "Didox inbox reject signing challenge");
            using var challengeJson = await JsonDocument.ParseAsync(await challengeResponse.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
            if (!challengeJson.RootElement.TryGetProperty("data", out var data))
                throw new IntegrationHttpException("Didox reject challenge response did not contain data.", StatusCodes.Status502BadGateway);

            var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(data.GetRawText()));
            return new EdoInboxRejectDto
            {
                Document = new EdoDocumentDto { ProviderDocumentId = providerDocumentId, Direction = EdoDirection.INBOX, DocumentType = "FACTURA" },
                SigningSession = new EdoSigningSessionDto
                {
                    SigningMode = EdoSigningMode.TimestampedSignature,
                    DocumentId = providerDocumentId,
                    Payload = payload,
                    PayloadFormat = "JsonBase64",
                    ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5)
                }
            };
        }

        if (string.IsNullOrWhiteSpace(request.PreparedPkcs7) || string.IsNullOrWhiteSpace(request.SignatureHex))
            throw new InvalidOperationException("Didox reject requires both PreparedPkcs7 and SignatureHex.");

        var timestamp = await timestampClient.GetTimeStampTokenForSigningAsync(
            RequireOrganization(), request.PreparedPkcs7, request.SignatureHex, ct);
        using var response = await SendJsonAsync(
            HttpMethod.Post,
            $"v1/documents/{Uri.EscapeDataString(providerDocumentId)}/reject",
            new { signature = timestamp },
            ct);
        await EnsureSuccessAsync(response, "Didox inbox reject");

        return new EdoInboxRejectDto
        {
            Document = new EdoDocumentDto
            {
                ProviderDocumentId = providerDocumentId,
                Direction = EdoDirection.INBOX,
                DocumentType = "FACTURA",
                Status = new EdoDocumentStatusDto
                {
                    Code = EdoDocumentStatusCode.REJECTED,
                    ProviderStatusCode = "REJECTED",
                    IsTerminal = true
                }
            }
        };
    }

    public async Task<EdoFileDto> GetFileAsync(string providerDocumentId, CancellationToken ct)
    {
        var response = await SendAsync(
            HttpMethod.Get,
            $"v1/documents/view/{Uri.EscapeDataString(providerDocumentId)}/pdf/ru",
            ct);
        await EnsureSuccessAsync(response, "Didox file download");
        EnsureFileLength(response);
        var stream = await response.Content.ReadAsStreamAsync(ct);
        return new EdoFileDto
        {
            ProviderFileId = providerDocumentId,
            FileName = $"didox-{providerDocumentId}.pdf",
            ContentType = response.Content.Headers.ContentType?.MediaType ?? "application/pdf",
            Length = response.Content.Headers.ContentLength ?? -1,
            Content = new EdoProviderResponseStream(stream, response, MaxFileLength)
        };
    }

    public Task<EdoDocumentStatusDto> GetStatusAsync(
        string providerDocumentId,
        CancellationToken ct) =>
        GetStatusAsync(providerDocumentId, null, ct);

    public async Task<EdoDocumentStatusDto> GetStatusAsync(
        string providerDocumentId,
        EdoDirection? direction,
        CancellationToken ct)
    {
        using var response = await SendAsync(
            HttpMethod.Get,
            $"v1/documents/{Uri.EscapeDataString(providerDocumentId)}{(direction is null ? string.Empty : $"?owner={(direction == EdoDirection.OUTBOX ? "1" : "0")}")}",
            ct);
        await EnsureSuccessAsync(response, "Didox document status");
        using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        var root = json.RootElement;
        if (!TryReadDidoxStatus(root, out var status))
            return new EdoDocumentStatusDto
            {
                Code = EdoDocumentStatusCode.UNKNOWN,
                LocalCode = EdoDocumentStatusCode.UNKNOWN
            };

        return MapDidoxProviderStatus(status);
    }

    private EdoDocumentDto ParseDocument(
        JsonElement item,
        EdoDirection direction,
        string? fallbackDocumentType = null,
        string? fallbackProviderDocumentId = null,
        EdoDocumentCategory? requestedCategory = null)
    {
        var detail = ReadDetailPayload(item);
        var previewLines = ReadPreviewLines(item);
        var providerDocumentId = ReadStringFromPayloads(item, "doc_id")
            ?? fallbackProviderDocumentId;
        if (string.IsNullOrWhiteSpace(providerDocumentId))
            throw new IntegrationHttpException(
                "Didox document response did not contain the documented doc_id field.",
                StatusCodes.Status502BadGateway);
        var status = TryReadDidoxStatus(item, out var statusCode)
            ? MapDidoxProviderStatus(statusCode)
            : new EdoDocumentStatusDto
            {
                Code = EdoDocumentStatusCode.UNKNOWN,
                LocalCode = EdoDocumentStatusCode.UNKNOWN
            };
        var seller = ReadDidoxParty(detail, "Seller", "SellerTin")
            ?? ReadDidoxParty(item, "Seller", "SellerTin");
        var buyer = ReadDidoxParty(detail, "Buyer", "BuyerTin")
            ?? ReadDidoxParty(item, "Buyer", "BuyerTin");
        var markingCodes = DidoxDocumentResponseMapper.ReadMarkingCodes(item)
            .Concat(DidoxDocumentResponseMapper.ReadMarkingCodes(detail))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return new EdoDocumentDto
        {
            ProviderCode = EdoProviderCode.DIDOX,
            ProviderDocumentId = providerDocumentId,
            Direction = direction,
            Category = EdoProviderStatusMapper.MapCategory(direction, status.Code, requestedCategory),
            DocumentType = ReadString(item, "doctype")
                ?? ReadString(detail, "doctype")
                ?? fallbackDocumentType
                ?? "UNKNOWN",
            DocumentNumber = ReadStringFromPayloads(item, "documentNumber")
                ?? ReadString(item, "name")
                ?? ReadNestedString(detail, "FacturaDoc", "FacturaNo"),
            DocumentDate = ReadDateFromPayloads(item, "documentDate")
                ?? ReadDate(item, "doc_date")
                ?? ReadNestedDate(detail, "FacturaDoc", "FacturaDate"),
            Status = status,
            Seller = seller,
            Buyer = buyer ?? ReadDidoxListPartner(item),
            TotalAmount = ReadDecimalFromPayloads(item, "totalWithVat")
                ?? SumLineTotals(previewLines)
                ?? ReadDecimalFromPayloads(item, "total_sum"),
            CreatedAt = ReadDateTime(item, "created"),
            UpdatedAt = ReadDateTime(item, "updated"),
            MarkingCodes = markingCodes,
            PreviewSellerTin = ReadStringFromPayloads(item, "sellerTin")
                ?? seller?.TaxIdentifier,
            PreviewContractNumber = ReadContractNumberFromPayloads(item),
            PreviewContractDate = ReadContractDateFromPayloads(item),
            PreviewLines = previewLines,
            ProviderFields = ReadProviderFields(item)
        };
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, CancellationToken ct)
    {
        return await SendAsync(RequireOrganization(), method, path, ct);
    }

    private async Task<HttpResponseMessage> SendAsync(
        int organizationId,
        HttpMethod method,
        string path,
        CancellationToken ct)
    {
        var request = new HttpRequestMessage(method, path);
        request.Options.Set(IntegrationHttpRequestOptions.OrganizationId, organizationId);
        return await httpClientFactory.CreateClient(DidoxHttpClientNames.Client)
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
    }

    private async Task<HttpResponseMessage> SendJsonAsync(HttpMethod method, string path, object body, CancellationToken ct)
    {
        var request = new HttpRequestMessage(method, path) { Content = JsonContent.Create(body, options: JsonOptions) };
        request.Options.Set(IntegrationHttpRequestOptions.OrganizationId, RequireOrganization());
        return await httpClientFactory.CreateClient(DidoxHttpClientNames.Client)
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
    }

    private int RequireOrganization() => userContext.OrganizationId
        ?? throw new InvalidOperationException("An active organization is required for Didox EDO operations.");

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, string operation)
    {
        if (response.IsSuccessStatusCode)
            return;
        var status = (int)response.StatusCode;
        throw response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => new IntegrationUnauthorizedException($"{operation} was rejected by Didox."),
            HttpStatusCode.Forbidden => new IntegrationForbiddenException($"{operation} was denied by Didox."),
            _ => new IntegrationHttpException($"{operation} failed with HTTP status {status}.", status)
        };
    }

    private static void EnsureFileLength(HttpResponseMessage response)
    {
        if (response.Content.Headers.ContentLength > MaxFileLength)
        {
            response.Dispose();
            throw new IntegrationHttpException("The provider file exceeds the configured maximum size.", StatusCodes.Status413PayloadTooLarge);
        }
    }

    private static void AddQuery(ICollection<string> query, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            query.Add($"{name}={Uri.EscapeDataString(value)}");
    }

    private static string? MapStatusFilter(
        EdoDocumentStatusCode? status,
        EdoDocumentCategory? category)
    {
        if (status is null)
        {
            return category switch
            {
                EdoDocumentCategory.DRAFTS => "0",
                EdoDocumentCategory.REJECTED => "4",
                EdoDocumentCategory.DELETED_ARCHIVED => "5,50,55",
                _ => null
            };
        }

        return status.Value switch
        {
            EdoDocumentStatusCode.DRAFT => "0",
            EdoDocumentStatusCode.PENDING_SIGNATURE => "2",
            EdoDocumentStatusCode.PARTNER_SIGNATURE_PENDING => "1",
            EdoDocumentStatusCode.AGENT_SIGNATURE_PENDING => "60",
            EdoDocumentStatusCode.SENT => "1,2,6,60",
            EdoDocumentStatusCode.SIGNED => "3",
            EdoDocumentStatusCode.REJECTED => "4",
            EdoDocumentStatusCode.DELETED => "5,55",
            EdoDocumentStatusCode.ARCHIVED => "50",
            EdoDocumentStatusCode.CANCELLED => "5,50,55",
            EdoDocumentStatusCode.FAILED => "40",
            _ => throw new EdoCapabilityUnavailableException(
                "DIDOX",
                "StatusFilter",
                EdoCapabilityStatus.UNKNOWN.ToString())
        };
    }

    private static string ReadRequiredString(JsonElement item, string propertyName) =>
        ReadString(item, propertyName) is { Length: > 0 } value
            ? value
            : throw new IntegrationHttpException($"Didox inbox item did not contain the documented '{propertyName}' field.", StatusCodes.Status502BadGateway);

    private static string? ReadString(JsonElement item, string propertyName) =>
        TryGetPropertyIgnoreCase(item, propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? Utf8MojibakeNormalizer.Normalize(property.GetString())
            : null;

    private static EdoDocumentStatusDto MapDidoxProviderStatus(int statusCode)
    {
        var mapped = EdoProviderStatusMapper.MapDidoxStatus(statusCode);
        return new EdoDocumentStatusDto
        {
            Code = mapped.Code,
            LocalCode = mapped.LocalCode,
            ProviderStatusCode = mapped.ProviderStatusCode,
            ProviderRawStatus = mapped.ProviderRawStatus,
            Description = mapped.Description,
            IsTerminal = mapped.IsTerminal,
            IsSuccessful = mapped.IsSuccessful,
            CheckedAt = DateTimeOffset.UtcNow,
            IsReconciliationRequired = mapped.IsReconciliationRequired
        };
    }

    private static bool TryReadDidoxStatus(JsonElement item, out int status)
    {
        int? resolvedStatus = null;
        foreach (var payload in EnumeratePayloads(item))
        {
            foreach (var propertyName in new[] { "doc_status", "status" })
            {
                if (!TryGetPropertyIgnoreCase(payload, propertyName, out var property)
                    || property.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
                    continue;

                if (!TryReadStrictInteger(property, out var parsedStatus))
                    throw new FormatException("Didox status must be an integer or an invariant numeric string.");

                if (resolvedStatus.HasValue && resolvedStatus.Value != parsedStatus)
                    throw new FormatException("Didox status fields are inconsistent.");

                resolvedStatus = parsedStatus;
            }
        }

        status = resolvedStatus.GetValueOrDefault();
        return resolvedStatus.HasValue;
    }

    private static JsonElement ReadDetailPayload(JsonElement root)
    {
        if (!TryGetPropertyIgnoreCase(root, "data", out var data))
            return root;

        if (data.ValueKind != JsonValueKind.Object)
            throw new EdoHistoricalMappingException("DIDOX_DETAIL_ENVELOPE_INVALID");

        foreach (var propertyName in HistoricalDetailPayloadPropertyNames)
        {
            if (TryGetPropertyIgnoreCase(data, propertyName, out var payload)
                && payload.ValueKind is not JsonValueKind.Null and not JsonValueKind.Undefined)
                return ReadEmbeddedDetailPayload(payload);
        }

        return data;
    }

    private static EdoPartyDto? ReadDidoxParty(
        JsonElement document,
        string partyPropertyName,
        string taxIdentifierPropertyName)
    {
        if (!TryGetPropertyIgnoreCase(document, partyPropertyName, out var party)
            || party.ValueKind != JsonValueKind.Object)
            return null;

        var name = ReadString(party, "Name");
        var taxIdentifier = ReadString(document, taxIdentifierPropertyName);
        return string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(taxIdentifier)
            ? null
            : new EdoPartyDto
            {
                Name = name,
                TaxIdentifier = taxIdentifier
            };
    }

    private static EdoPartyDto? ReadDidoxListPartner(JsonElement item)
    {
        var name = ReadString(item, "partnerCompany");
        var taxIdentifier = ReadString(item, "partnerTin");
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(taxIdentifier))
            return null;

        return new EdoPartyDto
        {
            Name = name,
            TaxIdentifier = taxIdentifier
        };
    }

    private static string? ReadNestedString(JsonElement root, string objectPropertyName, string valuePropertyName) =>
        TryGetPropertyIgnoreCase(root, objectPropertyName, out var nested)
        && nested.ValueKind == JsonValueKind.Object
            ? ReadString(nested, valuePropertyName)
            : null;

    private static DateOnly? ReadNestedDate(JsonElement root, string objectPropertyName, string valuePropertyName) =>
        DateOnly.TryParse(
            ReadNestedString(root, objectPropertyName, valuePropertyName),
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var value)
            ? value
            : null;

    private static bool TryGetPropertyIgnoreCase(
        JsonElement element,
        string propertyName,
        out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }
        }

        value = default;
        return false;
    }

    private static int? ReadOptionalInt(JsonElement root, string propertyName)
    {
        if (!TryGetPropertyIgnoreCase(root, propertyName, out var property)
            || property.ValueKind == JsonValueKind.Null
            || property.ValueKind == JsonValueKind.Undefined)
            return null;

        if (TryReadStrictInteger(property, out var value))
            return value;

        throw new IntegrationHttpException(
            $"Didox response field '{propertyName}' must be an integer.",
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
            $"Didox response field '{propertyName}' must be a boolean.",
            StatusCodes.Status502BadGateway);
    }

    private static string RequireProviderDocumentId(string providerDocumentId) =>
        string.IsNullOrWhiteSpace(providerDocumentId)
            ? throw new InvalidOperationException("Didox document details requires a provider document ID.")
            : providerDocumentId;

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

    private static DateOnly? ReadDate(JsonElement item, string propertyName) =>
        DateOnly.TryParse(ReadString(item, propertyName), CultureInfo.InvariantCulture, DateTimeStyles.None, out var value) ? value : null;

    private static DateOnly? ReadDateFromPayloads(JsonElement root, string propertyName)
    {
        foreach (var payload in EnumeratePayloads(root))
        {
            var value = ReadDate(payload, propertyName);
            if (value.HasValue)
                return value;
        }

        return null;
    }

    private static DateTimeOffset? ReadDateTime(JsonElement item, string propertyName) =>
        DateTimeOffset.TryParse(ReadString(item, propertyName), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var value) ? value : null;

    private static decimal? ReadDecimal(JsonElement item, string propertyName)
    {
        if (!TryGetPropertyIgnoreCase(item, propertyName, out var property)
            || property.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        if (property.ValueKind == JsonValueKind.Number
            && property.TryGetDecimal(out var numericValue))
        {
            return numericValue;
        }

        if (property.ValueKind == JsonValueKind.String
            && decimal.TryParse(
                property.GetString(),
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var stringValue))
        {
            return stringValue;
        }

        return null;
    }

    private static decimal? ReadDecimalFromPayloads(JsonElement root, string propertyName)
    {
        foreach (var payload in EnumeratePayloads(root))
        {
            var value = ReadDecimal(payload, propertyName);
            if (value.HasValue)
                return value;
        }

        return null;
    }

    private static string? ReadStringFromPayloads(JsonElement root, string propertyName)
    {
        foreach (var payload in EnumeratePayloads(root))
        {
            var value = ReadString(payload, propertyName);
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        return null;
    }

    private static string? ReadContractNumberFromPayloads(JsonElement root)
    {
        foreach (var payload in EnumeratePayloads(root))
        {
            var direct = ReadString(payload, "contractNumber")
                ?? ReadString(payload, "ContractNo");
            if (!string.IsNullOrWhiteSpace(direct))
                return direct;

            var nested = ReadNestedString(payload, "ContractDoc", "ContractNo");
            if (!string.IsNullOrWhiteSpace(nested))
                return nested;
        }

        return null;
    }

    private static DateOnly? ReadContractDateFromPayloads(JsonElement root)
    {
        foreach (var payload in EnumeratePayloads(root))
        {
            var direct = ReadDate(payload, "contractDate")
                ?? ReadDate(payload, "ContractDate");
            if (direct.HasValue)
                return direct;

            var nested = ReadNestedDate(payload, "ContractDoc", "ContractDate");
            if (nested.HasValue)
                return nested;
        }

        return null;
    }

    private static EdoHistoricalDocumentSummaryDto MapHistoricalSummary(EdoDocumentDto document)
    {
        var isConfirmedSigned = string.Equals(
            document.Status.ProviderStatusCode,
            "3",
            StringComparison.Ordinal);
        return new EdoHistoricalDocumentSummaryDto
        {
            ProviderDocumentId = document.ProviderDocumentId ?? string.Empty,
            Direction = EdoDirection.INBOX,
            Status = isConfirmedSigned
                ? EdoDocumentStatusCode.SIGNED
                : EdoDocumentStatusCode.UNKNOWN,
            DocumentType = document.DocumentType,
            DocumentNumber = document.DocumentNumber,
            DocumentDate = document.DocumentDate,
            SellerTin = document.Seller?.TaxIdentifier ?? document.PreviewSellerTin,
            BuyerTin = document.Buyer?.TaxIdentifier,
            SellerName = document.Seller?.Name,
            Total = document.TotalAmount,
            UpdatedAt = document.UpdatedAt
        };
    }

    private static int? ReadPageFromUrl(string url)
    {
        var queryIndex = url.IndexOf('?', StringComparison.Ordinal);
        if (queryIndex < 0 || queryIndex == url.Length - 1)
            return null;

        foreach (var pair in url[(queryIndex + 1)..].Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = pair.IndexOf('=', StringComparison.Ordinal);
            if (separator <= 0
                || !string.Equals(pair[..separator], "page", StringComparison.OrdinalIgnoreCase))
                continue;

            return int.TryParse(
                Uri.UnescapeDataString(pair[(separator + 1)..]),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var page)
                ? page
                : null;
        }

        return null;
    }

    private static void ValidateHistoricalPageRequest(
        int organizationId,
        EdoHistoricalPageRequestDto request)
    {
        if (organizationId <= 0)
            throw new ArgumentOutOfRangeException(nameof(organizationId));
        if (request.Page < 1)
            throw new ArgumentOutOfRangeException(nameof(request.Page));
        if (request.PageSize is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(request.PageSize));
    }

    private static IReadOnlyCollection<EdoDocumentPreviewLineDto> ReadPreviewLines(JsonElement root)
    {
        foreach (var payload in EnumeratePayloads(root))
        {
            if (TryGetPropertyIgnoreCase(payload, "productlist", out var productList))
            {
                if (productList.ValueKind != JsonValueKind.Object)
                    throw new EdoHistoricalMappingException("DIDOX_DETAIL_LINES_INVALID");

                if (!TryGetPropertyIgnoreCase(productList, "products", out var products))
                    continue;
                if (products.ValueKind != JsonValueKind.Array)
                    throw new EdoHistoricalMappingException("DIDOX_DETAIL_LINES_INVALID");

                var lines = new List<EdoDocumentPreviewLineDto>();
                foreach (var product in products.EnumerateArray())
                {
                    if (product.ValueKind != JsonValueKind.Object)
                        throw new EdoHistoricalMappingException("DIDOX_DETAIL_LINES_INVALID");

                    if (!TryGetPropertyIgnoreCase(product, "ordno", out var ordinal)
                        || !TryReadStrictInteger(ordinal, out var number))
                    {
                        throw new EdoHistoricalMappingException("DIDOX_DETAIL_LINES_INVALID");
                    }
                    var packageName = ReadString(product, "packagename");

                    lines.Add(new EdoDocumentPreviewLineDto
                    {
                        Number = number,
                        CatalogCode = ReadString(product, "catalogcode"),
                        CatalogName = ReadString(product, "catalogname"),
                        PackageCode = ReadString(product, "packagecode"),
                        PackageName = packageName,
                        IsService = string.Equals(
                            packageName?.Trim(),
                            "услуга (сум)",
                            StringComparison.OrdinalIgnoreCase),
                        NetAmount = ReadDecimal(product, "deliverysum"),
                        VatAmount = ReadDecimal(product, "vatsum"),
                        Quantity = ReadDecimal(product, "count"),
                        UnitPrice = ReadDecimal(product, "summa"),
                        VatRate = ReadDecimal(product, "vatrate"),
                        TotalWithVat = ReadDecimal(product, "deliverysumwithvat"),
                        MarkingCodes = DidoxDocumentResponseMapper.ReadMarkingCodes(product)
                    });
                }

                return lines;
            }

            var line = new EdoDocumentPreviewLineDto
            {
                Number = 1,
                CatalogCode = ReadString(payload, "catalogCode"),
                CatalogName = ReadString(payload, "catalogName"),
                PackageCode = ReadString(payload, "packageCode"),
                PackageName = ReadString(payload, "packageName"),
                IsService = string.Equals(
                    ReadString(payload, "packageName")?.Trim(),
                    "услуга (сум)",
                    StringComparison.OrdinalIgnoreCase),
                Quantity = ReadDecimal(payload, "quantity"),
                UnitPrice = ReadDecimal(payload, "unitPrice"),
                VatRate = ReadDecimal(payload, "vatRate"),
                TotalWithVat = ReadDecimal(payload, "totalWithVat"),
                MarkingCodes = DidoxDocumentResponseMapper.ReadMarkingCodes(payload)
            };

            if (line.CatalogCode is not null
                || line.PackageCode is not null
                || line.Quantity.HasValue
                || line.UnitPrice.HasValue
                || line.TotalWithVat.HasValue)
                return [line];
        }

        return [];
    }

    private static decimal? SumLineTotals(IReadOnlyCollection<EdoDocumentPreviewLineDto> lines) =>
        lines.Count > 0 && lines.All(line => line.TotalWithVat.HasValue)
            ? lines.Sum(line => line.TotalWithVat!.Value)
            : null;

    private static void ValidateHistoricalDetailEnvelope(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object)
            throw new EdoHistoricalMappingException("DIDOX_DETAIL_ENVELOPE_INVALID");

        if (TryGetPropertyIgnoreCase(root, "data", out var data)
            && data.ValueKind != JsonValueKind.Object)
        {
            throw new EdoHistoricalMappingException("DIDOX_DETAIL_ENVELOPE_INVALID");
        }

        _ = EnumeratePayloads(root).ToArray();
    }

    private static void ValidateHistoricalDetailIdentity(JsonElement root, string requestedIdentity)
    {
        var responseIdentities = EnumeratePayloads(root)
            .Select(payload => ReadString(payload, "doc_id"))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (responseIdentities.Length == 0
            || responseIdentities.Any(value => !string.Equals(value, requestedIdentity, StringComparison.Ordinal)))
        {
            throw new EdoHistoricalMappingException("DIDOX_DETAIL_IDENTITY_MISMATCH");
        }
    }

    private static void ValidateHistoricalDetailStatus(JsonElement root)
    {
        try
        {
            if (!TryReadDidoxStatus(root, out _))
                throw new EdoHistoricalMappingException("DIDOX_DETAIL_STATUS_INVALID");
        }
        catch (FormatException)
        {
            throw new EdoHistoricalMappingException("DIDOX_DETAIL_STATUS_INVALID");
        }
    }

    private static JsonElement ReadEmbeddedDetailPayload(JsonElement payload)
    {
        if (payload.ValueKind == JsonValueKind.Object)
            return payload;
        if (payload.ValueKind != JsonValueKind.String)
            throw new EdoHistoricalMappingException("DIDOX_DETAIL_DOCUMENT_JSON_INVALID");

        var serializedPayload = payload.GetString();
        for (var unwrapDepth = 0; unwrapDepth < MaxHistoricalEmbeddedJsonUnwrapDepth; unwrapDepth++)
        {
            if (string.IsNullOrWhiteSpace(serializedPayload)
                || Encoding.UTF8.GetByteCount(serializedPayload) > MaxHistoricalEmbeddedJsonLength)
            {
                throw new EdoHistoricalMappingException("DIDOX_DETAIL_DOCUMENT_JSON_INVALID");
            }

            try
            {
                using var parsed = JsonDocument.Parse(
                    serializedPayload,
                    new JsonDocumentOptions { MaxDepth = MaxHistoricalEmbeddedJsonDepth });
                if (parsed.RootElement.ValueKind == JsonValueKind.Object)
                    return parsed.RootElement.Clone();
                if (parsed.RootElement.ValueKind == JsonValueKind.String)
                {
                    serializedPayload = parsed.RootElement.GetString();
                    continue;
                }
            }
            catch (JsonException)
            {
                throw new EdoHistoricalMappingException("DIDOX_DETAIL_DOCUMENT_JSON_INVALID");
            }

            throw new EdoHistoricalMappingException("DIDOX_DETAIL_DOCUMENT_JSON_INVALID");
        }

        throw new EdoHistoricalMappingException("DIDOX_DETAIL_DOCUMENT_JSON_INVALID");
    }

    private static bool TryReadStrictInteger(JsonElement property, out int value)
    {
        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out value))
            return true;

        if (property.ValueKind == JsonValueKind.String
            && int.TryParse(
                property.GetString(),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out value))
        {
            return true;
        }

        value = default;
        return false;
    }

    private void LogHistoricalDetailDiagnostic(string safeFailureCode, JsonElement? root)
    {
        if (logger is null)
            return;

        logger.LogWarning(
            "DIDOX historical detail normalization failed. Validation={Validation}; Structure={Structure}",
            safeFailureCode,
            DescribeHistoricalDetailStructure(root));
    }

    private static string DescribeHistoricalDetailStructure(JsonElement? root)
    {
        if (root is not { } value)
            return "ROOT=INVALID_JSON";

        var fields = new List<string> { $"ROOT={value.ValueKind}" };
        if (!TryGetPropertyIgnoreCase(value, "data", out var data))
            return string.Join(';', fields);

        fields.Add($"data={data.ValueKind}");
        if (data.ValueKind != JsonValueKind.Object)
            return string.Join(';', fields);

        foreach (var propertyName in HistoricalDetailPayloadPropertyNames)
        {
            if (TryGetPropertyIgnoreCase(data, propertyName, out var payload))
                fields.Add($"data.{propertyName}={payload.ValueKind}");
        }

        try
        {
            var payloadIndex = 0;
            foreach (var payload in EnumeratePayloads(value))
            {
                AddHistoricalDetailFieldKinds(fields, $"payload[{payloadIndex++}]", payload);
            }
        }
        catch (EdoHistoricalMappingException)
        {
            // The envelope shape above is sufficient when an embedded JSON payload cannot be decoded.
        }

        return string.Join(';', fields);
    }

    private static void AddHistoricalDetailFieldKinds(
        ICollection<string> fields,
        string prefix,
        JsonElement payload)
    {
        foreach (var propertyName in new[]
                 {
                     "doc_id", "doc_status", "status", "doctype", "documentDate",
                     "contractNumber", "ContractNo", "ContractDate", "ContractDoc", "productlist"
                 })
        {
            if (TryGetPropertyIgnoreCase(payload, propertyName, out var property))
                fields.Add($"{prefix}.{propertyName}={property.ValueKind}");
        }

        if (TryGetPropertyIgnoreCase(payload, "productlist", out var productList)
            && productList.ValueKind == JsonValueKind.Object
            && TryGetPropertyIgnoreCase(productList, "products", out var products))
        {
            fields.Add($"{prefix}.productlist.products={products.ValueKind}");
        }
    }

    private static IEnumerable<JsonElement> EnumeratePayloads(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object)
            throw new EdoHistoricalMappingException("DIDOX_DETAIL_ENVELOPE_INVALID");

        yield return root;

        if (!TryGetPropertyIgnoreCase(root, "data", out var data))
            yield break;

        if (data.ValueKind != JsonValueKind.Object)
            throw new EdoHistoricalMappingException("DIDOX_DETAIL_ENVELOPE_INVALID");

        yield return data;

        foreach (var propertyName in HistoricalDetailPayloadPropertyNames)
        {
            if (TryGetPropertyIgnoreCase(data, propertyName, out var payload)
                && payload.ValueKind is not JsonValueKind.Null and not JsonValueKind.Undefined)
            {
                yield return ReadEmbeddedDetailPayload(payload);
            }
        }
    }
}
