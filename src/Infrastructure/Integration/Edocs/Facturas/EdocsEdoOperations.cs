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
using SharedKernel.Exceptions;

namespace Integration.Edocs.Facturas;

public sealed class EdocsEdoOperations(
    IUserContext userContext,
    IHttpClientFactory httpClientFactory)
{
    private const string BlobResponseMode = "false";

    public async Task<EdoInboxListDto> ListInboxAsync(EdoInboxQueryDto request, CancellationToken ct)
    {
        var query = $"documents?io=in&page={request.Page}&limit={request.PageSize}";
        using var response = await SendAsync(HttpMethod.Get, query, ct);
        await EnsureSuccessAsync(response, "Edocs inbox list");
        using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        var data = json.RootElement.ValueKind == JsonValueKind.Array
            ? json.RootElement
            : json.RootElement.TryGetProperty("data", out var dataProperty) && dataProperty.ValueKind == JsonValueKind.Array
                ? dataProperty
                : throw new IntegrationHttpException("Edocs inbox response did not contain a documented data array.", StatusCodes.Status502BadGateway);

        var items = data.EnumerateArray().Select(ParseDocument).ToList();
        var total = json.RootElement.TryGetProperty("total", out var totalProperty)
            && totalProperty.TryGetInt32(out var totalValue) ? totalValue : (int?)null;
        return new EdoInboxListDto { Items = items, Page = request.Page, PageSize = request.PageSize, TotalCount = total };
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
        var type = MapDocumentType(providerDocumentType);
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

    public async Task<EdoDocumentStatusDto> GetStatusAsync(string providerDocumentId, CancellationToken ct)
    {
        using var response = await SendAsync(
            HttpMethod.Get,
            $"documents/factura/{Uri.EscapeDataString(providerDocumentId)}",
            ct);
        await EnsureSuccessAsync(response, "Edocs document status");
        using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        if (!json.RootElement.TryGetProperty("status", out var statusProperty)
            || statusProperty.ValueKind != JsonValueKind.String)
            return new EdoDocumentStatusDto { Code = EdoDocumentStatusCode.UNKNOWN, ProviderStatusCode = "UNKNOWN" };

        return EdoProviderStatusMapper.MapEdocsStatus(statusProperty.GetString());
    }

    private EdoDocumentDto ParseDocument(JsonElement item)
    {
        var providerDocumentId = ReadRequiredString(item, "id");
        var documentType = ReadRequiredString(item, "type");

        var status = item.TryGetProperty("status", out var statusProperty) && statusProperty.ValueKind == JsonValueKind.String
            ? EdoProviderStatusMapper.MapEdocsStatus(statusProperty.GetString())
            : new EdoDocumentStatusDto { Code = EdoDocumentStatusCode.UNKNOWN, ProviderStatusCode = "UNKNOWN" };

        return new EdoDocumentDto
        {
            ProviderDocumentId = providerDocumentId,
            Direction = EdoDirection.INBOX,
            DocumentType = documentType,
            DocumentNumber = ReadNestedString(item, "FacturaDoc", "FacturaNo"),
            DocumentDate = ReadNestedDate(item, "FacturaDoc", "FacturaDate"),
            Status = status,
            Seller = ReadParty(item, "Seller"),
            Buyer = ReadParty(item, "Buyer"),
            TotalAmount = ReadDecimal(item, "TotalAmount")
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

    private static string RequireProviderDocumentId(string providerDocumentId) =>
        string.IsNullOrWhiteSpace(providerDocumentId)
            ? throw new InvalidOperationException("Edocs inbox rejection requires a provider document ID.")
            : providerDocumentId;

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
