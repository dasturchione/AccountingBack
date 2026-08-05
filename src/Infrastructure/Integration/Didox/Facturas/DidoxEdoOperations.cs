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

    public async Task<EdoInboxListDto> ListInboxAsync(EdoInboxQueryDto request, CancellationToken ct)
    {
        var query = new List<string>
        {
            "owner=0",
            $"page={request.Page}",
            $"limit={request.PageSize}"
        };
        AddQuery(query, "name", request.Search);
        AddQuery(query, "hasMarks", request.HasMarks?.ToString().ToLowerInvariant());
        AddQuery(query, "dateFromCreated", request.FromDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        AddQuery(query, "dateToCreated", request.ToDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        AddQuery(query, "status", MapStatusFilter(request.Status));

        using var response = await SendAsync(HttpMethod.Get, $"v2/documents?{string.Join('&', query)}", ct);
        await EnsureSuccessAsync(response, "Didox inbox list");
        using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        if (!json.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
            throw new IntegrationHttpException("Didox inbox response did not contain the documented data array.", StatusCodes.Status502BadGateway);

        var items = data.EnumerateArray().Select(ParseDocument).ToList();
        var total = json.RootElement.TryGetProperty("total", out var totalProperty)
            && totalProperty.TryGetInt32(out var totalValue) ? totalValue : (int?)null;

        return new EdoInboxListDto { Items = items, Page = request.Page, PageSize = request.PageSize, TotalCount = total };
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
        if (!json.RootElement.TryGetProperty("doc_status", out var statusProperty)
            || !statusProperty.TryGetInt32(out var status))
            return new EdoDocumentStatusDto { Code = EdoDocumentStatusCode.UNKNOWN, ProviderStatusCode = "UNKNOWN" };

        return EdoProviderStatusMapper.MapDidoxStatus(status);
    }

    private EdoDocumentDto ParseDocument(JsonElement item)
    {
        var providerDocumentId = ReadRequiredString(item, "doc_id");
        var status = item.TryGetProperty("doc_status", out var statusProperty) && statusProperty.TryGetInt32(out var statusCode)
            ? EdoProviderStatusMapper.MapDidoxStatus(statusCode)
            : new EdoDocumentStatusDto { Code = EdoDocumentStatusCode.UNKNOWN, ProviderStatusCode = "UNKNOWN" };

        return new EdoDocumentDto
        {
            ProviderDocumentId = providerDocumentId,
            Direction = EdoDirection.INBOX,
            DocumentType = ReadString(item, "doctype") ?? "UNKNOWN",
            DocumentNumber = ReadString(item, "name"),
            DocumentDate = ReadDate(item, "doc_date"),
            Status = status,
            Buyer = new EdoPartyDto
            {
                Name = ReadString(item, "partnerCompany") ?? string.Empty,
                TaxIdentifier = ReadString(item, "partnerTin") ?? string.Empty
            },
            TotalAmount = ReadDecimal(item, "total_sum"),
            CreatedAt = ReadDateTime(item, "created"),
            UpdatedAt = ReadDateTime(item, "updated"),
            MarkingCodes = DidoxDocumentResponseMapper.ReadMarkingCodes(item)
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

    private static string? MapStatusFilter(EdoDocumentStatusCode? status) => status switch
    {
        EdoDocumentStatusCode.DRAFT => "0",
        EdoDocumentStatusCode.SENT => "1,2,6,60",
        EdoDocumentStatusCode.SIGNED => "3",
        EdoDocumentStatusCode.REJECTED => "4",
        EdoDocumentStatusCode.CANCELLED => "5,50,55",
        EdoDocumentStatusCode.FAILED => "40",
        _ => null
    };

    private static string ReadRequiredString(JsonElement item, string propertyName) =>
        ReadString(item, propertyName) is { Length: > 0 } value
            ? value
            : throw new IntegrationHttpException($"Didox inbox item did not contain the documented '{propertyName}' field.", StatusCodes.Status502BadGateway);

    private static string? ReadString(JsonElement item, string propertyName) =>
        item.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;

    private static DateOnly? ReadDate(JsonElement item, string propertyName) =>
        DateOnly.TryParse(ReadString(item, propertyName), CultureInfo.InvariantCulture, DateTimeStyles.None, out var value) ? value : null;

    private static DateTimeOffset? ReadDateTime(JsonElement item, string propertyName) =>
        DateTimeOffset.TryParse(ReadString(item, propertyName), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var value) ? value : null;

    private static decimal? ReadDecimal(JsonElement item, string propertyName)
    {
        if (!item.TryGetProperty(propertyName, out var property)
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
}
