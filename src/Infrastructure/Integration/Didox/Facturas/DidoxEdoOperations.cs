using Application.Abstractions.Authentication;
using Application.Abstractions.Integration.Edo;
using Integration.Didox.Http;
using Integration.Didox.Services;
using Integration.Edo.Http;
using Integration.Edo.Providers;
using Integration.Shared.Http;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using SharedKernel.Exceptions;

namespace Integration.Didox.Facturas;

public sealed class DidoxEdoOperations(
    IUserContext userContext,
    IHttpClientFactory httpClientFactory,
    DidoxTimestampClient timestampClient)
{
    private const int MaxFileLength = 25 * 1024 * 1024;
    private static readonly JsonSerializerOptions JsonOptions = new();

    public Task<EdoInboxListDto> ListInboxAsync(EdoInboxQueryDto request, CancellationToken ct) =>
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

    public async Task<EdoDocumentDto> GetDocumentDetailsAsync(
        EdoDirection direction,
        string providerDocumentType,
        string providerDocumentId,
        CancellationToken ct)
    {
        var id = RequireProviderDocumentId(providerDocumentId);
        using var response = await SendAsync(
            HttpMethod.Get,
            $"v1/documents/{Uri.EscapeDataString(id)}",
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

    public async Task<EdoDocumentStatusDto> GetStatusAsync(string providerDocumentId, CancellationToken ct)
    {
        using var response = await SendAsync(
            HttpMethod.Get,
            $"v1/documents/{Uri.EscapeDataString(providerDocumentId)}",
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
        var providerDocumentId = ReadString(item, "doc_id")
            ?? ReadString(detail, "doc_id")
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
            DocumentNumber = ReadString(item, "name")
                ?? ReadNestedString(detail, "FacturaDoc", "FacturaNo"),
            DocumentDate = ReadDate(item, "doc_date")
                ?? ReadNestedDate(detail, "FacturaDoc", "FacturaDate"),
            Status = status,
            Seller = seller,
            Buyer = buyer ?? ReadDidoxListPartner(item),
            TotalAmount = ReadDecimalFromPayloads(item, "total_sum"),
            CreatedAt = ReadDateTime(item, "created"),
            UpdatedAt = ReadDateTime(item, "updated"),
            MarkingCodes = markingCodes,
            ProviderFields = ReadProviderFields(item)
        };
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, CancellationToken ct)
    {
        var request = new HttpRequestMessage(method, path);
        request.Options.Set(IntegrationHttpRequestOptions.OrganizationId, RequireOrganization());
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
            _ => null
        };
    }

    private static string ReadRequiredString(JsonElement item, string propertyName) =>
        ReadString(item, propertyName) is { Length: > 0 } value
            ? value
            : throw new IntegrationHttpException($"Didox inbox item did not contain the documented '{propertyName}' field.", StatusCodes.Status502BadGateway);

    private static string? ReadString(JsonElement item, string propertyName) =>
        TryGetPropertyIgnoreCase(item, propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
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
        status = default;
        foreach (var payload in EnumeratePayloads(item))
        {
            foreach (var propertyName in new[] { "doc_status", "status" })
            {
                if (!TryGetPropertyIgnoreCase(payload, propertyName, out var property)
                    || property.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
                    continue;

                if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out status))
                    return true;

                throw new IntegrationHttpException(
                    $"Didox response field '{propertyName}' must be an integer.",
                    StatusCodes.Status502BadGateway);
            }
        }

        return false;
    }

    private static JsonElement ReadDetailPayload(JsonElement root)
    {
        if (TryGetPropertyIgnoreCase(root, "data", out var data)
            && data.ValueKind == JsonValueKind.Object)
        {
            if (TryGetPropertyIgnoreCase(data, "json", out var json)
                && json.ValueKind == JsonValueKind.Object)
                return json;

            if (TryGetPropertyIgnoreCase(data, "document_json", out var documentJson)
                && documentJson.ValueKind == JsonValueKind.Object)
                return documentJson;

            if (TryGetPropertyIgnoreCase(data, "document", out var document)
                && document.ValueKind == JsonValueKind.Object)
                return document;

            return data;
        }

        return root;
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
        if (!root.TryGetProperty(propertyName, out var property)
            || property.ValueKind == JsonValueKind.Null
            || property.ValueKind == JsonValueKind.Undefined)
            return null;

        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value))
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

    private static IEnumerable<JsonElement> EnumeratePayloads(JsonElement root)
    {
        yield return root;

        if (!TryGetPropertyIgnoreCase(root, "data", out var data)
            || data.ValueKind != JsonValueKind.Object)
            yield break;

        yield return data;

        if (TryGetPropertyIgnoreCase(data, "document", out var document)
            && document.ValueKind == JsonValueKind.Object)
            yield return document;

        if (TryGetPropertyIgnoreCase(data, "json", out var json)
            && json.ValueKind == JsonValueKind.Object)
            yield return json;

        if (TryGetPropertyIgnoreCase(data, "document_json", out var documentJson)
            && documentJson.ValueKind == JsonValueKind.Object)
            yield return documentJson;
    }
}
