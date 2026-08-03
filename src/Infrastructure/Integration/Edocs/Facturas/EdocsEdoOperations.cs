using Application.Abstractions.Authentication;
using Application.Abstractions.Integration.Edo;
using Integration.Edo.Http;
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

    public async Task<EdoInboxListDto> ListInboxAsync(EdoInboxQueryDto request, CancellationToken ct)
    {
        var query = $"documents?io=in&page={request.Page}&limit={request.PageSize}";
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

            var items = data.EnumerateArray().Select(item => ParseDocument(item, usesDocsShape)).ToList();
            var total = root.ValueKind == JsonValueKind.Object
                && root.TryGetProperty("total", out var totalProperty)
                && totalProperty.TryGetInt32(out var totalValue) ? totalValue : (int?)null;
            return new EdoInboxListDto { Items = items, Page = request.Page, PageSize = request.PageSize, TotalCount = total };
        }
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
            return new EdoDocumentStatusDto { Code = EdoDocumentStatusCode.UNKNOWN, ProviderStatusCode = "UNKNOWN" };

        return EdoProviderStatusMapper.MapEdocsStatus(statusProperty.GetString());
    }

    private EdoDocumentDto ParseDocument(JsonElement item, bool usesDocsShape)
    {
        var providerDocumentId = ReadRequiredString(item, usesDocsShape ? "_id" : "id");
        var documentType = ReadRequiredString(item, "type");

        var status = item.TryGetProperty("status", out var statusProperty) && statusProperty.ValueKind == JsonValueKind.String
            ? EdoProviderStatusMapper.MapEdocsStatus(statusProperty.GetString())
            : new EdoDocumentStatusDto { Code = EdoDocumentStatusCode.UNKNOWN, ProviderStatusCode = "UNKNOWN" };

        return new EdoDocumentDto
        {
            ProviderDocumentId = providerDocumentId,
            Direction = EdoDirection.INBOX,
            DocumentType = documentType,
            DocumentNumber = usesDocsShape ? null : ReadNestedString(item, "FacturaDoc", "FacturaNo"),
            DocumentDate = usesDocsShape ? null : ReadNestedDate(item, "FacturaDoc", "FacturaDate"),
            Status = status,
            Seller = usesDocsShape ? null : ReadParty(item, "Seller"),
            Buyer = usesDocsShape ? null : ReadParty(item, "Buyer"),
            TotalAmount = usesDocsShape ? null : ReadDecimal(item, "TotalAmount"),
            CreatedAt = usesDocsShape ? ReadDateTimeOffset(item, "createdAt") : null
        };
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, CancellationToken ct)
    {
        var request = new HttpRequestMessage(method, path);
        request.Options.Set(IntegrationHttpRequestOptions.OrganizationId, RequireOrganization());
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

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, string operation)
    {
        if (response.IsSuccessStatusCode)
            return;
        var status = (int)response.StatusCode;
        throw response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => new IntegrationUnauthorizedException($"{operation} was rejected by Edocs."),
            HttpStatusCode.Forbidden => new IntegrationForbiddenException($"{operation} was denied by Edocs."),
            _ => new IntegrationHttpException($"{operation} failed with HTTP status {status}.", status)
        };
    }

    private static string? ReadNestedString(JsonElement item, string parent, string child) =>
        item.TryGetProperty(parent, out var parentProperty)
            && parentProperty.TryGetProperty(child, out var childProperty)
            && childProperty.ValueKind == JsonValueKind.String
            ? childProperty.GetString()
            : null;

    private static DateOnly? ReadNestedDate(JsonElement item, string parent, string child) =>
        DateOnly.TryParse(ReadNestedString(item, parent, child), CultureInfo.InvariantCulture, DateTimeStyles.None, out var value) ? value : null;

    private static decimal? ReadDecimal(JsonElement item, string name) =>
        item.TryGetProperty(name, out var property) && property.TryGetDecimal(out var value) ? value : null;

    private static DateTimeOffset? ReadDateTimeOffset(JsonElement item, string name) =>
        item.TryGetProperty(name, out var property)
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
        if (!item.TryGetProperty(name, out var property) || property.ValueKind != JsonValueKind.Object)
            return null;
        return new EdoPartyDto
        {
            Name = ReadString(property, "Name") ?? string.Empty,
            TaxIdentifier = ReadString(property, "VatRegCode") ?? string.Empty,
            BankCode = ReadString(property, "BankId"),
            AccountNumber = ReadString(property, "Account"),
            Address = ReadString(property, "Address"),
            DirectorName = ReadString(property, "Director"),
            AccountantName = ReadString(property, "Accountant"),
            DistrictId = ReadString(property, "DistrictId")
        };
    }

    private static string? ReadString(JsonElement item, string name) =>
        item.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
}
