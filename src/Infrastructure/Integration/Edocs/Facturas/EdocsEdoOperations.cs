using Application.Abstractions.Authentication;
using Application.Abstractions.Integration.Edo;
using Domain.Entities;
using Integration.Edo.Http;
using Integration.Edo.Historical;
using Integration.Edocs.Http;
using Integration.Edo.Providers;
using Integration.Shared.Http;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using SharedKernel.Exceptions;

namespace Integration.Edocs.Facturas;

public sealed class EdocsEdoOperations(
    IUserContext userContext,
    IHttpClientFactory httpClientFactory)
{
    private const string BlobResponseMode = "false";
    private const int MaxInboxResponseBodyBytes = 1 * 1024 * 1024;
    private const int MaxDiagnosticSummaryLength = 512;
    private static readonly Regex SensitiveValueRegex = new(
        @"(?i)[""']?\b(?:pkcs7(?:_64)?|signature(?:hex)?|private(?:\s|_)?key|partner[-_]?authorization|authorization|access[_\s-]?token|auth[_\s-]?token|user[_\s-]?key|token|inn|tin|tax[_-]?id|(?:document|doc|factura|invoice)[-_]?(?:id|number|no|date)?|id)\b[""']?\s*[:=]\s*(?:""[^"" ]*""|'[^']*'|[^\s,;}\]]+)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

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
                EdoProviderCode.EDOCS.ToString(),
                EdoCapabilityKind.ListAll.ToString(),
                EdoCapabilityStatus.UNKNOWN.ToString());

        if (request.Search is not null
            || request.HasMarks is not null
            || request.DateFrom is not null
            || request.DateTo is not null
            || request.ProviderFilters.Count > 0)
            throw new EdoCapabilityUnavailableException(
                EdoProviderCode.EDOCS.ToString(),
                EdoCapabilityKind.SearchFilter.ToString(),
                EdoCapabilityStatus.UNKNOWN.ToString());

        if (request.Scope == EdoDocumentQueryScope.INBOX
            && request.Category == EdoDocumentCategory.DELETED_ARCHIVED)
            throw new EdoCapabilityUnavailableException(
                EdoProviderCode.EDOCS.ToString(),
                $"{EdoCapabilityKind.ListInbox}:{EdoDocumentCategory.DELETED_ARCHIVED}",
                EdoCapabilityStatus.UNKNOWN.ToString());

        var queryParameters = new List<string>
        {
            $"sort={(request.Category == EdoDocumentCategory.DELETED_ARCHIVED ? "updatedAt" : "createdAt")}",
            "order=-1",
            $"page={request.Page}",
            $"limit={request.Limit}",
            $"io={(request.Scope == EdoDocumentQueryScope.OUTBOX ? "out" : "in")}",
            "type=all"
        };
        var providerStatus = MapListStatus(request.Status, request.Category);
        if (providerStatus is not null)
            queryParameters.Add($"status={Uri.EscapeDataString(providerStatus)}");

        var query = $"documents?{string.Join('&', queryParameters)}";
        using var response = await SendAsync(HttpMethod.Get, query, ct);
        await EnsureSuccessAsync(response, "Edocs inbox list");
        var body = await ReadBoundedResponseBodyAsync(response, ct);
        JsonDocument json;
        try
        {
            json = JsonDocument.Parse(body);
        }
        catch (JsonException)
        {
            throw new IntegrationHttpException(
                $"Edocs inbox response shape unsupported: rootKind=InvalidJson, " +
                $"status={(int)response.StatusCode}, " +
                $"contentType={response.Content.Headers.ContentType?.ToString() ?? "none"}, " +
                $"contentEncoding={FormatContentEncoding(response)}, " +
                $"body={RedactAndTruncate(body) ?? "empty"}.",
                StatusCodes.Status502BadGateway);
        }

        using (json)
        {
            var root = json.RootElement;
            var usesDocsShape = false;
            JsonElement data;
            if (root.ValueKind == JsonValueKind.Array)
            {
                data = root;
            }
            else if (root.ValueKind == JsonValueKind.Object
                     && root.TryGetProperty("data", out var dataProperty)
                     && dataProperty.ValueKind == JsonValueKind.Array)
            {
                data = dataProperty;
            }
            else if (root.ValueKind == JsonValueKind.Object
                     && root.TryGetProperty("docs", out var docsProperty)
                     && docsProperty.ValueKind == JsonValueKind.Array)
            {
                data = docsProperty;
                usesDocsShape = true;
            }
            else
            {
                throw CreateUnsupportedInboxShapeException(root);
            }

            var items = data.EnumerateArray()
                .Select(item => ParseDocument(item, usesDocsShape, request.Scope == EdoDocumentQueryScope.OUTBOX
                    ? EdoDirection.OUTBOX
                    : EdoDirection.INBOX, requestedCategory: request.Category))
                .ToList();
            var totalDocs = ReadInt(root, "totalDocs");
            var limit = ReadInt(root, "limit");
            return new EdoInboxListDto
            {
                Items = items,
                Page = ReadInt(root, "page") ?? request.Page,
                PageSize = limit ?? request.Limit,
                TotalCount = totalDocs,
                TotalDocs = totalDocs,
                TotalPages = ReadInt(root, "totalPages"),
                HasNextPage = ReadBool(root, "hasNextPage"),
                HasPrevPage = ReadBool(root, "hasPrevPage"),
                HasPreviousPage = ReadBool(root, "hasPrevPage"),
                Limit = limit,
                NextPage = ReadInt(root, "nextPage"),
                PrevPage = ReadInt(root, "prevPage"),
                PreviousPage = ReadInt(root, "prevPage"),
                PagingCounter = ReadInt(root, "pagingCounter")
            };
        }
    }

    public async Task<EdoHistoricalPageResultDto> ListHistoricalSignedInboxAsync(
        int organizationId,
        EdoHistoricalPageRequestDto request,
        CancellationToken ct)
    {
        ValidateHistoricalPageRequest(organizationId, request);
        var query = string.Join('&',
            "sort=createdAt",
            "order=-1",
            $"page={request.Page}",
            $"limit={request.PageSize}",
            "io=in",
            "status=signed",
            "type=all");

        using var response = await SendAsync(
            organizationId,
            HttpMethod.Get,
            $"documents?{query}",
            ct);
        await EnsureSuccessAsync(response, "Edocs historical inbox list", preserveProviderStatus: true);

        try
        {
            var body = await ReadBoundedResponseBodyAsync(response, ct);
            using var json = JsonDocument.Parse(body);
            var root = json.RootElement;
            var usesDocsShape = false;
            JsonElement data;
            if (root.ValueKind == JsonValueKind.Array)
            {
                data = root;
            }
            else if (root.ValueKind == JsonValueKind.Object
                     && root.TryGetProperty("data", out var dataProperty)
                     && dataProperty.ValueKind == JsonValueKind.Array)
            {
                data = dataProperty;
            }
            else if (root.ValueKind == JsonValueKind.Object
                     && root.TryGetProperty("docs", out var docsProperty)
                     && docsProperty.ValueKind == JsonValueKind.Array)
            {
                data = docsProperty;
                usesDocsShape = true;
            }
            else
            {
                throw new EdoHistoricalMappingException("EDOCS_HISTORICAL_DOCUMENT_ARRAY_REQUIRED");
            }

            var items = data.EnumerateArray()
                .Select(item => MapHistoricalSummary(ParseDocument(
                    item,
                    usesDocsShape,
                    EdoDirection.INBOX,
                    fallbackDocumentType: "UNKNOWN")))
                .ToArray();
            var providerTotal = ReadInt(root, "totalDocs");
            var page = ReadInt(root, "page") ?? request.Page;
            var pageSize = ReadInt(root, "limit") ?? request.PageSize;
            _ = ReadInt(root, "totalPages");
            var hasNextPage = ReadBool(root, "hasNextPage");
            var nextPage = ReadInt(root, "nextPage");
            hasNextPage ??= nextPage.HasValue ? true : null;
            var hasCompleteMetadata = providerTotal.HasValue
                && hasNextPage.HasValue
                && (hasNextPage != true || nextPage.HasValue);

            return EdoHistoricalSourceSupport.BuildSuccessfulPage(
                EdoProviderCode.EDOCS,
                request,
                page,
                pageSize,
                providerTotal,
                hasNextPage,
                nextPage,
                hasCompleteMetadata,
                providerRequiresOverlapRescan: false,
                items);
        }
        catch (EdoHistoricalMappingException)
        {
            throw;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            throw new EdoHistoricalMappingException("EDOCS_HISTORICAL_RESPONSE_INVALID", exception);
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

        var type = MapStatusDocumentType(providerDocumentType);
        var id = RequireStatusProviderDocumentId(providerDocumentId);
        using var response = await SendAsync(
            organizationId,
            HttpMethod.Get,
            $"documents/{Uri.EscapeDataString(type)}/{Uri.EscapeDataString(id)}",
            ct);
        await EnsureSuccessAsync(response, "Edocs historical document details", preserveProviderStatus: true);

        try
        {
            using var json = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(ct),
                cancellationToken: ct);
            var root = json.RootElement;
            var responseIdentity = ReadOptionalString(root, "_id")
                ?? ReadOptionalString(root, "id");
            if (string.IsNullOrWhiteSpace(responseIdentity)
                || !string.Equals(responseIdentity, id, StringComparison.Ordinal))
            {
                throw new EdoHistoricalMappingException("EDOCS_DETAIL_IDENTITY_MISMATCH");
            }

            return ParseDocument(
                root,
                usesDocsShape: root.TryGetProperty("_id", out _),
                EdoDirection.INBOX,
                fallbackDocumentType: providerDocumentType,
                strictHistoricalDecimals: true);
        }
        catch (EdoHistoricalMappingException)
        {
            throw;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            throw new EdoHistoricalMappingException("EDOCS_HISTORICAL_DETAIL_INVALID", exception);
        }
    }

    public async Task<EdoInboxSummaryDto> GetInboxSummaryAsync(CancellationToken ct)
    {
        using var response = await SendAsync(HttpMethod.Get, "documents/all/get/stats", ct);
        await EnsureSuccessAsync(response, "Edocs document statistics");
        using var json = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(ct),
            cancellationToken: ct);

        return new EdoInboxSummaryDto
        {
            Provider = EdoProviderCode.EDOCS,
            Inbox = ReadDirectionStatusCounts(json.RootElement, "in"),
            Outbox = ReadDirectionStatusCounts(json.RootElement, "out")
        };
    }

    public async Task<EdoInboxRejectDto> RejectInboxAsync(
        string providerDocumentType,
        string providerDocumentId,
        EdoInboxRejectRequestDto request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.PreparedPkcs7))
            throw new InvalidOperationException("Edocs inbox rejection requires PreparedPkcs7.");

        var type = MapDocumentType(providerDocumentType);
        var id = RequireProviderDocumentId(providerDocumentId);
        using var response = await SendJsonAsync(
            HttpMethod.Post,
            $"documents/{type}/{Uri.EscapeDataString(id)}/reject",
            new { pkcs7 = request.PreparedPkcs7 },
            ct);
        await EnsureSuccessAsync(response, "Edocs inbox reject");

        // TAXMIN: The official contract does not document a response JSON schema;
        // HTTP success is the only accepted success signal here.
        return new EdoInboxRejectDto
        {
            Document = new EdoDocumentDto
            {
                ProviderDocumentId = id,
                Direction = EdoDirection.INBOX,
                DocumentType = providerDocumentType,
                Status = EdoProviderStatusMapper.MapEdocsStatus("rejected")
            }
        };
    }

    public async Task<EdoFileDto> GetFileAsync(
        string providerDocumentType,
        string providerDocumentId,
        CancellationToken ct)
    {
        var type = MapFileDocumentType(providerDocumentType);
        var id = RequireProviderDocumentId(providerDocumentId);
        var path = $"documents/{Uri.EscapeDataString(type)}/{Uri.EscapeDataString(id)}/{Uri.EscapeDataString(BlobResponseMode)}/file";
        var response = await SendAsync(HttpMethod.Get, path, ct);

        try
        {
            await EnsureSuccessAsync(response, "Edocs file download");
            var stream = await response.Content.ReadAsStreamAsync(ct);
            return new EdoFileDto
            {
                ProviderFileId = id,
                FileName = $"edocs-{id}",
                ContentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream",
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

    public async Task<EdoDocumentStatusDto> GetStatusAsync(
        string providerDocumentType,
        string providerDocumentId,
        CancellationToken ct)
    {
        var type = MapStatusDocumentType(providerDocumentType);
        var id = RequireStatusProviderDocumentId(providerDocumentId);
        using var response = await SendAsync(
            HttpMethod.Get,
            $"documents/{Uri.EscapeDataString(type)}/{Uri.EscapeDataString(id)}",
            ct);
        await EnsureSuccessAsync(response, "Edocs document status");
        using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        if (!json.RootElement.TryGetProperty("status", out var statusProperty)
            || statusProperty.ValueKind != JsonValueKind.String)
            return new EdoDocumentStatusDto
            {
                Code = EdoDocumentStatusCode.UNKNOWN,
                LocalCode = EdoDocumentStatusCode.UNKNOWN,
                ProviderStatusCode = "UNKNOWN"
            };

        return EdoProviderStatusMapper.MapEdocsStatus(statusProperty.GetString());
    }

    public async Task<EdoDocumentDto> GetDocumentDetailsAsync(
        EdoDirection direction,
        string providerDocumentType,
        string providerDocumentId,
        CancellationToken ct)
    {
        var type = MapStatusDocumentType(providerDocumentType);
        var id = RequireStatusProviderDocumentId(providerDocumentId);
        using var response = await SendAsync(
            HttpMethod.Get,
            $"documents/{Uri.EscapeDataString(type)}/{Uri.EscapeDataString(id)}",
            ct);
        await EnsureSuccessAsync(response, "Edocs document details");
        using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);

        var root = json.RootElement;
        return ParseDocument(root, usesDocsShape: root.TryGetProperty("_id", out _), direction,
            fallbackProviderDocumentId: id, fallbackDocumentType: providerDocumentType);
    }

    private EdoDocumentDto ParseDocument(
        JsonElement item,
        bool usesDocsShape,
        EdoDirection direction,
        string? fallbackProviderDocumentId = null,
        string? fallbackDocumentType = null,
        EdoDocumentCategory? requestedCategory = null,
        bool strictHistoricalDecimals = false)
    {
        var providerDocumentId = usesDocsShape
            ? ReadOptionalString(item, "_id") ?? fallbackProviderDocumentId
            : ReadOptionalString(item, "id") ?? fallbackProviderDocumentId;
        var documentType = ReadOptionalString(item, "type") ?? fallbackDocumentType;
        if (string.IsNullOrWhiteSpace(providerDocumentId) || string.IsNullOrWhiteSpace(documentType))
            throw new IntegrationHttpException(
                "Edocs document response did not contain a provider document ID and type.",
                StatusCodes.Status502BadGateway);

        EdoDocumentStatusDto status;
        if (!item.TryGetProperty("status", out var statusProperty)
            || statusProperty.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            status = new EdoDocumentStatusDto
            {
                Code = EdoDocumentStatusCode.UNKNOWN,
                LocalCode = EdoDocumentStatusCode.UNKNOWN,
                ProviderStatusCode = "UNKNOWN",
                ProviderRawStatus = "UNKNOWN"
            };
        }
        else if (statusProperty.ValueKind == JsonValueKind.String)
        {
            status = EdoProviderStatusMapper.MapEdocsStatus(statusProperty.GetString());
        }
        else
        {
            throw new IntegrationHttpException(
                "Edocs document response field 'status' must be a string.",
                StatusCodes.Status502BadGateway);
        }

        var seller = ReadSellerParty(item, documentType);
        var documentDateTime = ReadDateTime(item, "docDate")
            ?? ReadDateTime(item, "documentDate")
            ?? ReadPathDateTime(item, "data", "facturadoc", "facturadate")
            ?? ReadNestedDateTime(item, "FacturaDoc", "FacturaDate");

        return new EdoDocumentDto
        {
            ProviderCode = EdoProviderCode.EDOCS,
            ProviderDocumentId = providerDocumentId,
            Direction = direction,
            Category = EdoProviderStatusMapper.MapCategory(direction, status.Code, requestedCategory),
            DocumentType = documentType,
            DocumentNumber = ReadOptionalString(item, "docNumber")
                ?? ReadOptionalString(item, "documentNumber")
                ?? ReadPathString(item, "data", "facturadoc", "facturano")
                ?? ReadNestedString(item, "FacturaDoc", "FacturaNo"),
            DocumentDate = ReadDate(item, "docDate")
                ?? ReadDate(item, "documentDate")
                ?? ReadPathDate(item, "data", "facturadoc", "facturadate")
                ?? ReadNestedDate(item, "FacturaDoc", "FacturaDate"),
            DocumentDateTime = documentDateTime,
            Status = status,
            Seller = seller,
            Buyer = ReadBuyerParty(item),
            TotalAmount = ReadDecimal(item, "totalSumWithVat", strictHistoricalDecimals)
                ?? ReadDecimal(item, "totalWithVat", strictHistoricalDecimals)
                ?? ReadDecimal(item, "TotalAmount", strictHistoricalDecimals),
            CreatedAt = usesDocsShape ? ReadDateTimeOffset(item, "createdAt") : null,
            PreviewSellerTin = seller?.TaxIdentifier,
            PreviewContractNumber = ReadPathString(item, "data", "contractdoc", "contractno")
                ?? ReadOptionalString(item, "contractNumber"),
            PreviewContractDate = ReadPathDate(item, "data", "contractdoc", "contractdate")
                ?? ReadDate(item, "contractDate"),
            PreviewLines = ReadPreviewLines(item, strictHistoricalDecimals),
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
        return await httpClientFactory.CreateClient(EdocsHttpClientNames.Client)
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
    }

    private async Task<HttpResponseMessage> SendJsonAsync(
        HttpMethod method,
        string path,
        object body,
        CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, path)
        {
            Content = JsonContent.Create(body)
        };
        request.Options.Set(IntegrationHttpRequestOptions.OrganizationId, RequireOrganization());
        return await httpClientFactory.CreateClient(EdocsHttpClientNames.Client)
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
    }

    private static async Task<string> ReadBoundedResponseBodyAsync(
        HttpResponseMessage response,
        CancellationToken ct)
    {
        if (response.Content.Headers.ContentLength is > MaxInboxResponseBodyBytes)
            throw new IntegrationHttpException(
                $"Edocs inbox response exceeded the bounded body limit of {MaxInboxResponseBodyBytes} bytes.",
                StatusCodes.Status502BadGateway);

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        var buffer = new byte[MaxInboxResponseBodyBytes];
        var offset = 0;

        while (offset < buffer.Length)
        {
            var bytesRead = await stream.ReadAsync(buffer.AsMemory(offset), ct);
            if (bytesRead == 0)
                break;

            offset += bytesRead;
        }

        if (offset == buffer.Length)
        {
            var probe = new byte[1];
            var additionalBytes = await stream.ReadAsync(probe.AsMemory(), ct);
            if (additionalBytes > 0)
                throw new IntegrationHttpException(
                    $"Edocs inbox response exceeded the bounded body limit of {MaxInboxResponseBodyBytes} bytes.",
                    StatusCodes.Status502BadGateway);
        }

        return System.Text.Encoding.UTF8.GetString(buffer, 0, offset);
    }

    private static IntegrationHttpException CreateUnsupportedInboxShapeException(JsonElement root)
    {
        var properties = root.ValueKind == JsonValueKind.Object
            ? root.EnumerateObject().Select(property => property.Name).ToList()
            : [];
        var arrayProperties = root.ValueKind == JsonValueKind.Object
            ? root.EnumerateObject()
                .Where(property => property.Value.ValueKind == JsonValueKind.Array)
                .Select(property => property.Name)
                .ToList()
            : [];
        var itemProperties = FindFirstArrayObject(root) is { } item
            ? item.EnumerateObject().Select(property => property.Name).ToList()
            : [];

        var message =
            $"Edocs inbox response shape unsupported: rootKind={root.ValueKind}, " +
            $"properties={FormatPropertyNames(properties)}, " +
            $"arrayProperties={FormatPropertyNames(arrayProperties)}, " +
            $"itemProperties={FormatPropertyNames(itemProperties)}.";

        return new IntegrationHttpException(message, StatusCodes.Status502BadGateway);
    }

    private static JsonElement? FindFirstArrayObject(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in root.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.Object)
                    return item;
            }

            return null;
        }

        if (root.ValueKind != JsonValueKind.Object)
            return null;

        foreach (var property in root.EnumerateObject())
        {
            if (property.Value.ValueKind != JsonValueKind.Array)
                continue;

            foreach (var item in property.Value.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.Object)
                    return item;
            }
        }

        return null;
    }

    private static string FormatPropertyNames(IEnumerable<string> names)
    {
        var formatted = string.Join(",", names
            .Take(64)
            .Select(name => name.Replace("\r", string.Empty).Replace("\n", string.Empty)));

        if (string.IsNullOrWhiteSpace(formatted))
            return "none";

        return formatted.Length <= 512
            ? formatted
            : formatted[..512] + "...";
    }

    private static string FormatContentEncoding(HttpResponseMessage response) =>
        response.Content.Headers.ContentEncoding.Count == 0
            ? "none"
            : string.Join(",", response.Content.Headers.ContentEncoding);

    private static string? RedactAndTruncate(string value)
    {
        var redacted = SensitiveValueRegex.Replace(value, "<redacted>");
        var compact = Regex.Replace(redacted, @"\s+", " ").Trim();

        return string.IsNullOrWhiteSpace(compact)
            ? null
            : compact.Length <= MaxDiagnosticSummaryLength
                ? compact
                : compact[..MaxDiagnosticSummaryLength] + "...";
    }

    private int RequireOrganization() => userContext.OrganizationId
        ?? throw new InvalidOperationException("An active organization is required for Edocs EDO operations.");

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        string operation,
        bool preserveProviderStatus = false)
    {
        if (response.IsSuccessStatusCode)
            return;
        var status = (int)response.StatusCode;
        throw response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => new IntegrationUnauthorizedException($"{operation} was rejected by Edocs."),
            HttpStatusCode.Forbidden => new IntegrationForbiddenException($"{operation} was denied by Edocs."),
            HttpStatusCode.UnprocessableEntity when !preserveProviderStatus => new IntegrationHttpException(
                $"{operation} was rejected by Edocs with an invalid provider request.",
                StatusCodes.Status502BadGateway),
            _ => new IntegrationHttpException($"{operation} failed with HTTP status {status}.", status)
        };
    }

    private static string? ReadNestedString(JsonElement item, string parent, string child) =>
        TryGetPropertyIgnoreCase(item, parent, out var parentProperty)
            && TryGetPropertyIgnoreCase(parentProperty, child, out var childProperty)
            && childProperty.ValueKind == JsonValueKind.String
            ? childProperty.GetString()
            : null;

    private static DateOnly? ReadNestedDate(JsonElement item, string parent, string child) =>
        ParseDate(ReadNestedString(item, parent, child));

    private static DateTime? ReadNestedDateTime(JsonElement item, string parent, string child) =>
        ParseDocumentDateTime(ReadNestedString(item, parent, child));

    private static DateOnly? ReadDate(JsonElement item, string name) =>
        ParseDate(ReadOptionalString(item, name));

    private static DateTime? ReadDateTime(JsonElement item, string name) =>
        ParseDocumentDateTime(ReadOptionalString(item, name));

    private static decimal? ReadDecimal(
        JsonElement item,
        string name,
        bool strictHistoricalDecimal = false)
    {
        if (!TryGetPropertyIgnoreCase(item, name, out var property)
            || property.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        if (property.ValueKind == JsonValueKind.Number
            && property.TryGetDecimal(out var numericValue))
        {
            return numericValue;
        }

        if (property.ValueKind == JsonValueKind.String)
        {
            var stringValue = property.GetString();
            if (strictHistoricalDecimal)
            {
                if (IsStrictInvariantDecimal(stringValue)
                    && decimal.TryParse(
                        stringValue,
                        NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                        CultureInfo.InvariantCulture,
                        out var strictValue))
                {
                    return strictValue;
                }

                throw new EdoHistoricalMappingException(
                    "EDOCS_HISTORICAL_DETAIL_DECIMAL_INVALID");
            }

            return decimal.TryParse(
                stringValue,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var lenientValue)
                ? lenientValue
                : null;
        }

        if (strictHistoricalDecimal)
        {
            throw new EdoHistoricalMappingException(
                "EDOCS_HISTORICAL_DETAIL_DECIMAL_INVALID");
        }

        return null;
    }

    private static bool IsStrictInvariantDecimal(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        var index = value[0] == '-' ? 1 : 0;
        if (index == value.Length || !char.IsAsciiDigit(value[index]))
            return false;

        while (index < value.Length && char.IsAsciiDigit(value[index]))
            index++;

        if (index == value.Length)
            return true;
        if (value[index] != '.' || ++index == value.Length)
            return false;

        while (index < value.Length && char.IsAsciiDigit(value[index]))
            index++;

        return index == value.Length;
    }

    private static DateTimeOffset? ReadDateTimeOffset(JsonElement item, string name) =>
        TryGetPropertyIgnoreCase(item, name, out var property)
        && property.ValueKind == JsonValueKind.String
        && DateTimeOffset.TryParse(property.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var value)
            ? value
            : null;

    private static string ReadRequiredString(JsonElement item, string name) =>
        item.TryGetProperty(name, out var property)
            && property.ValueKind == JsonValueKind.String
            && !string.IsNullOrWhiteSpace(property.GetString())
            ? property.GetString()!
            : throw new IntegrationHttpException(
                $"Edocs inbox item did not contain the documented '{name}' field.",
                StatusCodes.Status502BadGateway);

    private static string MapDocumentType(string documentType) =>
        documentType.Trim().ToUpperInvariant() switch
        {
            "FACTURA" => "factura",
            _ => throw new InvalidOperationException($"Edocs inbox rejection does not support document type '{documentType}'.")
        };

    private static string MapFileDocumentType(string documentType)
    {
        if (string.IsNullOrWhiteSpace(documentType))
            throw new IntegrationHttpException(
                "Edocs file download requires a stored provider document type.",
                StatusCodes.Status422UnprocessableEntity);

        var type = documentType.Trim();
        return string.Equals(type, "FACTURA", StringComparison.OrdinalIgnoreCase)
            ? "factura"
            : type;
    }

    private static string RequireProviderDocumentId(string providerDocumentId) =>
        string.IsNullOrWhiteSpace(providerDocumentId)
            ? throw new InvalidOperationException("Edocs inbox rejection requires a provider document ID.")
            : providerDocumentId;

    private static string RequireStatusProviderDocumentId(string providerDocumentId) =>
        string.IsNullOrWhiteSpace(providerDocumentId)
            ? throw new InvalidOperationException("Edocs document status requires a provider document ID.")
            : providerDocumentId;

    private static string MapStatusDocumentType(string providerDocumentType)
    {
        if (string.IsNullOrWhiteSpace(providerDocumentType))
            throw new IntegrationHttpException(
                "Edocs document status requires a stored provider document type.",
                StatusCodes.Status422UnprocessableEntity);

        var type = providerDocumentType.Trim();
        return string.Equals(type, "FACTURA", StringComparison.OrdinalIgnoreCase)
            ? "factura"
            : type;
    }

    private static EdoPartyDto? ReadParty(JsonElement item, string name)
    {
        if (!TryGetPropertyIgnoreCase(item, name, out var property))
            return null;

        return ReadPartyObject(property);
    }

    private static EdoPartyDto? ReadPartyObject(
        JsonElement property,
        string? taxIdentifierOverride = null,
        bool useTaxIdentifierOverride = false)
    {
        if (property.ValueKind != JsonValueKind.Object)
            return null;

        return new EdoPartyDto
        {
            Name = ReadString(property, "Name") ?? string.Empty,
            TaxIdentifier = useTaxIdentifierOverride
                ? taxIdentifierOverride ?? string.Empty
                : ReadString(property, "VatRegCode") ?? string.Empty,
            BankCode = ReadString(property, "BankId"),
            AccountNumber = ReadString(property, "Account"),
            Address = ReadString(property, "Address"),
            DirectorName = ReadString(property, "Director"),
            AccountantName = ReadString(property, "Accountant"),
            DistrictId = ReadString(property, "DistrictId")
        };
    }

    private static EdoPartyDto? ReadBuyerParty(JsonElement item)
    {
        var rootBuyer = TryGetPropertyIgnoreCase(item, "Buyer", out var rootBuyerProperty)
            && rootBuyerProperty.ValueKind == JsonValueKind.Object
                ? rootBuyerProperty
                : (JsonElement?)null;
        var nestedBuyerProperty = ReadPath(item, "data", "buyer");
        var nestedBuyer = nestedBuyerProperty is { ValueKind: JsonValueKind.Object }
            ? nestedBuyerProperty
            : null;
        var preferredBuyer = rootBuyer ?? nestedBuyer;

        if (TryReadNormalizedTin(item, "buyertin", out var buyerTin)
            || TryReadPathNormalizedTin(item, out buyerTin, "data", "buyertin"))
        {
            return BuildBuyerParty(preferredBuyer, buyerTin);
        }

        if (TryReadBuyerObjectTin(rootBuyer, out buyerTin)
            || TryReadBuyerObjectTin(nestedBuyer, out buyerTin))
        {
            return BuildBuyerParty(preferredBuyer, buyerTin);
        }

        var targetBuyer = ReadTargetParty(item, "buyer");
        if (targetBuyer is not null)
            return targetBuyer;

        if (TryReadNormalizedTin(rootBuyer, "VatRegCode", out buyerTin)
            || TryReadNormalizedTin(nestedBuyer, "VatRegCode", out buyerTin))
        {
            return BuildBuyerParty(preferredBuyer, buyerTin);
        }

        return preferredBuyer is { } buyer
            ? ReadPartyObject(buyer, taxIdentifierOverride: null, useTaxIdentifierOverride: true)
            : null;
    }

    private static EdoPartyDto? ReadSellerParty(JsonElement item, string? documentType)
    {
        var rootSeller = TryGetPropertyIgnoreCase(item, "Seller", out var rootSellerProperty)
            && rootSellerProperty.ValueKind == JsonValueKind.Object
                ? rootSellerProperty
                : (JsonElement?)null;
        var nestedSellerProperty = ReadPath(item, "data", "seller");
        var nestedSeller = nestedSellerProperty is { ValueKind: JsonValueKind.Object }
            ? nestedSellerProperty
            : null;
        var preferredSeller = rootSeller ?? nestedSeller;

        if (TryReadNormalizedTin(item, "sellertin", out var sellerTin)
            || TryReadPathNormalizedTin(item, out sellerTin, "data", "sellertin"))
        {
            return BuildSellerParty(preferredSeller, sellerTin);
        }

        if (TryReadSellerObjectTin(rootSeller, out sellerTin)
            || TryReadSellerObjectTin(nestedSeller, out sellerTin))
        {
            return BuildSellerParty(preferredSeller, sellerTin);
        }

        if (TryReadPathNormalizedTin(item, out sellerTin, "data", "productlist", "tin")
            || TryReadPathNormalizedTin(item, out sellerTin, "productlist", "tin"))
        {
            return BuildSellerParty(preferredSeller, sellerTin);
        }

        var targetSeller = ReadTargetParty(item, "seller");
        if (targetSeller is not null)
        {
            return preferredSeller.HasValue
                ? BuildSellerParty(preferredSeller, targetSeller.TaxIdentifier)
                : targetSeller;
        }

        if (IsFacturaDocumentType(documentType)
            && TryReadNormalizedTin(item, "ownerTin", out sellerTin))
        {
            return preferredSeller.HasValue
                ? BuildSellerParty(preferredSeller, sellerTin)
                : new EdoPartyDto
                {
                    Name = ReadOptionalString(item, "ownerName") ?? string.Empty,
                    TaxIdentifier = sellerTin ?? string.Empty
                };
        }

        if (TryReadNormalizedTin(rootSeller, "VatRegCode", out sellerTin)
            || TryReadNormalizedTin(nestedSeller, "VatRegCode", out sellerTin))
        {
            return BuildSellerParty(preferredSeller, sellerTin);
        }

        return preferredSeller is { } seller
            ? ReadPartyObject(seller, taxIdentifierOverride: null, useTaxIdentifierOverride: true)
            : null;
    }

    private static bool TryReadSellerObjectTin(JsonElement? seller, out string? normalizedTin)
    {
        foreach (var fieldName in new[] { "tin", "sellerTin", "taxIdentifier", "inn" })
        {
            if (TryReadNormalizedTin(seller, fieldName, out normalizedTin))
                return true;
        }

        normalizedTin = null;
        return false;
    }

    private static bool TryReadBuyerObjectTin(JsonElement? buyer, out string? normalizedTin)
    {
        foreach (var fieldName in new[] { "tin", "buyerTin", "taxIdentifier", "inn" })
        {
            if (TryReadNormalizedTin(buyer, fieldName, out normalizedTin))
                return true;
        }

        normalizedTin = null;
        return false;
    }

    private static bool TryReadPathNormalizedTin(
        JsonElement item,
        out string? normalizedTin,
        params string[] path)
    {
        if (path.Length == 0)
        {
            normalizedTin = null;
            return false;
        }

        var parentPath = path[..^1];
        var parent = parentPath.Length == 0 ? item : ReadPath(item, parentPath);
        return TryReadNormalizedTin(parent, path[^1], out normalizedTin);
    }

    private static bool TryReadNormalizedTin(
        JsonElement? item,
        string name,
        out string? normalizedTin)
    {
        if (item is not { } value
            || !TryGetPropertyIgnoreCase(value, name, out var property))
        {
            normalizedTin = null;
            return false;
        }

        normalizedTin = property.ValueKind == JsonValueKind.String
            ? NormalizeTin(property.GetString())
            : null;
        return true;
    }

    private static string? NormalizeTin(string? value)
    {
        var normalized = value?.Trim();
        return !string.IsNullOrEmpty(normalized)
            && normalized.All(char.IsAsciiDigit)
                ? normalized
                : null;
    }

    private static EdoPartyDto BuildBuyerParty(
        JsonElement? buyer,
        string? normalizedTin)
    {
        if (buyer is { } buyerProperty)
        {
            return ReadPartyObject(
                buyerProperty,
                normalizedTin,
                useTaxIdentifierOverride: true)!;
        }

        return new EdoPartyDto
        {
            TaxIdentifier = normalizedTin ?? string.Empty
        };
    }

    private static EdoPartyDto BuildSellerParty(
        JsonElement? seller,
        string? normalizedTin)
    {
        if (seller is { } sellerProperty)
        {
            return ReadPartyObject(
                sellerProperty,
                normalizedTin,
                useTaxIdentifierOverride: true)!;
        }

        return new EdoPartyDto
        {
            TaxIdentifier = normalizedTin ?? string.Empty
        };
    }

    private static bool IsFacturaDocumentType(string? documentType) =>
        string.Equals(documentType, "FACTURA", StringComparison.OrdinalIgnoreCase);

    private static string? ReadString(JsonElement item, string name) =>
        TryGetPropertyIgnoreCase(item, name, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;

    private static string? ReadOptionalString(JsonElement item, string name) => ReadString(item, name);

    private static string? ReadStringFromParty(JsonElement item, string partyName, string fieldName) =>
        TryGetPropertyIgnoreCase(item, partyName, out var party)
        && party.ValueKind == JsonValueKind.Object
        && TryGetPropertyIgnoreCase(party, fieldName, out var field)
        && field.ValueKind == JsonValueKind.String
            ? field.GetString()
            : null;

    private static IReadOnlyCollection<EdoDocumentPreviewLineDto> ReadPreviewLines(
        JsonElement item,
        bool strictHistoricalDecimals = false)
    {
        if (ReadPath(item, "data", "productlist", "products") is { } products
            && products.ValueKind == JsonValueKind.Array)
        {
            var lines = new List<EdoDocumentPreviewLineDto>();
            foreach (var product in products.EnumerateArray())
            {
                if (product.ValueKind != JsonValueKind.Object)
                    throw new IntegrationHttpException(
                        "Edocs product list contains a non-object line.",
                        StatusCodes.Status502BadGateway);

                var number = ReadInteger(product, "ordno")
                    ?? throw new EdoHistoricalMappingException("PROVIDER_LINE_NUMBER_INVALID");

                lines.Add(new EdoDocumentPreviewLineDto
                {
                    Number = number,
                    CatalogCode = ReadOptionalString(product, "catalogcode"),
                    CatalogName = ReadOptionalString(product, "catalogname"),
                    PackageCode = ReadOptionalString(product, "packagecode"),
                    PackageName = ReadOptionalString(product, "packagename"),
                    Quantity = ReadDecimal(product, "count", strictHistoricalDecimals),
                    UnitPrice = ReadDecimal(product, "summa", strictHistoricalDecimals),
                    NetAmount = ReadDecimal(product, "deliverysum", strictHistoricalDecimals),
                    VatRate = ReadDecimal(product, "vatrate", strictHistoricalDecimals),
                    VatAmount = ReadDecimal(product, "vatsum", strictHistoricalDecimals),
                    TotalWithVat = ReadDecimal(product, "deliverysumwithvat", strictHistoricalDecimals),
                    MarkingCodes = ReadMarkingCodes(product)
                });
            }

            return lines;
        }

        var line = new EdoDocumentPreviewLineDto
        {
            Number = 1,
            CatalogCode = ReadOptionalString(item, "catalogCode"),
            PackageCode = ReadOptionalString(item, "packageCode"),
            PackageName = ReadOptionalString(item, "packageName"),
            Quantity = ReadDecimal(item, "quantity", strictHistoricalDecimals),
            UnitPrice = ReadDecimal(item, "unitPrice", strictHistoricalDecimals),
            VatRate = ReadDecimal(item, "vatRate", strictHistoricalDecimals),
            TotalWithVat = ReadDecimal(item, "totalWithVat", strictHistoricalDecimals),
            MarkingCodes = ReadMarkingCodes(item)
        };

        return line.CatalogCode is null
            && line.PackageCode is null
            && line.Quantity is null
            && line.UnitPrice is null
            && line.TotalWithVat is null
            ? []
            : [line];
    }

    private static IReadOnlyCollection<string> ReadMarkingCodes(JsonElement product)
    {
        var result = new List<string>();
        if (TryGetPropertyIgnoreCase(product, "marks", out var marks)
            && marks.ValueKind is not (JsonValueKind.Null or JsonValueKind.Undefined))
        {
            if (marks.ValueKind != JsonValueKind.Object)
                throw new EdoHistoricalMappingException(EdoImportMarkingPolicy.ProviderDataRequired);
            AppendMarkingArray(marks, "identtransupak", result);
            AppendMarkingArray(marks, "kiz", result);
            AppendMarkingArray(marks, "nomupak", result);
        }

        AppendMarkingArray(product, "identtransupak", result);
        AppendMarkingArray(product, "kiz", result);
        AppendMarkingArray(product, "nomupak", result);
        return result;
    }

    private static void AppendMarkingArray(JsonElement owner, string propertyName, List<string> target)
    {
        if (!TryGetPropertyIgnoreCase(owner, propertyName, out var values)
            || values.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            return;
        if (values.ValueKind != JsonValueKind.Array)
            throw new EdoHistoricalMappingException(EdoImportMarkingPolicy.ProviderDataRequired);

        foreach (var value in values.EnumerateArray())
        {
            if (value.ValueKind != JsonValueKind.String)
                throw new EdoHistoricalMappingException(EdoImportMarkingPolicy.ProviderDataRequired);
            var normalized = value.GetString()?.Trim();
            if (!string.IsNullOrWhiteSpace(normalized))
                target.Add(normalized);
        }
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

    private static EdoPartyDto? ReadTargetParty(JsonElement item, string side)
    {
        if (!TryGetPropertyIgnoreCase(item, "targetTins", out var targetTins)
            || targetTins.ValueKind != JsonValueKind.Array)
            return null;

        foreach (var target in targetTins.EnumerateArray())
        {
            if (target.ValueKind != JsonValueKind.Object
                || !string.Equals(ReadOptionalString(target, "side"), side, StringComparison.OrdinalIgnoreCase))
                continue;

            _ = TryReadNormalizedTin(target, "tin", out var tin);

            return new EdoPartyDto
            {
                Name = ReadOptionalString(target, "name") ?? string.Empty,
                TaxIdentifier = tin ?? string.Empty
            };
        }

        return null;
    }

    private static JsonElement? ReadPath(JsonElement item, params string[] path)
    {
        var current = item;
        foreach (var name in path)
        {
            if (!TryGetPropertyIgnoreCase(current, name, out var next))
                return null;

            current = next;
        }

        return current;
    }

    private static string? ReadPathString(JsonElement item, params string[] path) =>
        ReadPath(item, path) is { } property && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;

    private static DateOnly? ReadPathDate(JsonElement item, params string[] path) =>
        ParseDate(ReadPathString(item, path));

    private static DateTime? ReadPathDateTime(JsonElement item, params string[] path) =>
        ParseDocumentDateTime(ReadPathString(item, path));

    private static EdoPartyDto? ReadPathParty(JsonElement item, params string[] path) =>
        ReadPath(item, path) is { } property
            ? ReadPartyObject(property)
            : null;

    private static DateOnly? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return date;

        return DateTimeOffset.TryParse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind,
            out var timestamp)
            ? DateOnly.FromDateTime(timestamp.DateTime)
            : null;
    }

    internal static DateTime? ParseDocumentDateTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return date.ToDateTime(TimeOnly.MinValue);

        return DateTimeOffset.TryParse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind,
            out var timestamp)
            ? DateTime.SpecifyKind(timestamp.DateTime, DateTimeKind.Unspecified)
            : null;
    }

    private static int? ReadInteger(JsonElement item, string name)
    {
        if (!TryGetPropertyIgnoreCase(item, name, out var property))
            return null;

        if (property.ValueKind == JsonValueKind.Number
            && property.TryGetInt32(out var value))
            return value;

        if (property.ValueKind == JsonValueKind.String
            && int.TryParse(
                property.GetString(),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var stringValue))
        {
            return stringValue;
        }

        throw new EdoHistoricalMappingException("PROVIDER_LINE_NUMBER_INVALID");
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement item, string name, out JsonElement property)
    {
        if (item.ValueKind == JsonValueKind.Object)
        {
            foreach (var candidate in item.EnumerateObject())
            {
                if (string.Equals(candidate.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    property = candidate.Value;
                    return true;
                }
            }
        }

        property = default;
        return false;
    }

    private static IReadOnlyDictionary<string, int> ReadDirectionStatusCounts(JsonElement root, string direction)
    {
        if (root.ValueKind == JsonValueKind.Object
            && root.TryGetProperty(direction, out var directionProperty)
            && directionProperty.ValueKind == JsonValueKind.Object)
        {
            return ReadStatusCounts(directionProperty);
        }

        return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    }

    private static EdoHistoricalDocumentSummaryDto MapHistoricalSummary(EdoDocumentDto document)
    {
        var isConfirmedSigned = string.Equals(
            document.Status.ProviderStatusCode,
            "signed",
            StringComparison.OrdinalIgnoreCase);
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
            DocumentDateTime = document.DocumentDateTime,
            SellerTin = document.Seller?.TaxIdentifier ?? document.PreviewSellerTin,
            BuyerTin = document.Buyer?.TaxIdentifier,
            SellerName = document.Seller?.Name,
            Total = document.TotalAmount,
            UpdatedAt = document.UpdatedAt
        };
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

    private static IReadOnlyDictionary<string, int> ReadStatusCounts(JsonElement root)
    {
        var current = root;
        for (var depth = 0; depth < 4; depth++)
        {
            if (current.ValueKind != JsonValueKind.Object)
                break;

            var directCounts = current.EnumerateObject()
                .Where(property => property.Value.ValueKind == JsonValueKind.Number
                                   && property.Value.TryGetInt32(out _))
                .ToDictionary(
                    property => property.Name,
                    property => property.Value.GetInt32(),
                    StringComparer.OrdinalIgnoreCase);
            if (directCounts.Count > 0)
                return directCounts;

            var nested = new[] { "data", "stats", "counts", "in", "out" }
                .Select(name => current.TryGetProperty(name, out var property)
                    && property.ValueKind == JsonValueKind.Object
                    ? property
                    : (JsonElement?)null)
                .FirstOrDefault(property => property is not null);
            if (nested is null)
                break;

            current = nested.Value;
        }

        return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    }

    private static int? ReadInt(JsonElement root, string name)
    {
        if (root.ValueKind != JsonValueKind.Object
            || !root.TryGetProperty(name, out var property))
            return null;

        if (property.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            return null;

        if (property.ValueKind == JsonValueKind.Number
            && property.TryGetInt32(out var value))
            return value;

        throw new IntegrationHttpException(
            $"Edocs response field '{name}' must be an integer.",
            StatusCodes.Status502BadGateway);
    }

    private static bool? ReadBool(JsonElement root, string name) =>
        root.ValueKind == JsonValueKind.Object
        && root.TryGetProperty(name, out var property)
        && (property.ValueKind == JsonValueKind.True || property.ValueKind == JsonValueKind.False)
            ? property.GetBoolean()
            : null;

    private static string? MapListStatus(
        EdoDocumentStatusCode? status,
        EdoDocumentCategory? category)
    {
        if (status is null)
        {
            return category switch
            {
                EdoDocumentCategory.DRAFTS => "drafts",
                EdoDocumentCategory.REJECTED => "rejected",
                EdoDocumentCategory.DELETED_ARCHIVED => "deleted",
                _ => null
            };
        }

        return status.Value switch
        {
            EdoDocumentStatusCode.DRAFT => "drafts",
            EdoDocumentStatusCode.SENT => "sended",
            EdoDocumentStatusCode.SIGNED => "signed",
            EdoDocumentStatusCode.REJECTED => "rejected",
            EdoDocumentStatusCode.DELETED
                or EdoDocumentStatusCode.ARCHIVED
                or EdoDocumentStatusCode.CANCELLED => "deleted",
            _ => throw new EdoCapabilityUnavailableException(
                "EDOCS",
                "StatusFilter",
                EdoCapabilityStatus.UNKNOWN.ToString())
        };
    }
}
