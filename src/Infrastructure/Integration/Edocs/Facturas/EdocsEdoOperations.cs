using Application.Abstractions.Authentication;
using Application.Abstractions.Integration.Edo;
using Integration.Edocs.Http;
using Integration.Edo.Providers;
using Integration.Shared.Http;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Net;
using System.Text.Json;
using SharedKernel.Exceptions;

namespace Integration.Edocs.Facturas;

public sealed class EdocsEdoOperations(
    IUserContext userContext,
    IHttpClientFactory httpClientFactory)
{
    public async Task<EdoInboxListDto> ListInboxAsync(EdoInboxQueryDto request, CancellationToken ct)
    {
        var query = $"documents?io=in&page={request.Page}&pageSize={request.PageSize}";
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

    public async Task<EdoDocumentStatusDto> GetStatusAsync(string providerDocumentId, CancellationToken ct)
    {
        using var response = await SendAsync(
            HttpMethod.Get,
            $"documents/factura/{Uri.EscapeDataString(providerDocumentId)}",
            ct);
        await EnsureSuccessAsync(response, "Edocs document status");
        using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        if (!json.RootElement.TryGetProperty("Status", out var statusProperty)
            || statusProperty.ValueKind != JsonValueKind.String)
            return new EdoDocumentStatusDto { Code = EdoDocumentStatusCode.UNKNOWN, ProviderStatusCode = "UNKNOWN" };

        return EdoProviderStatusMapper.MapEdocsStatus(statusProperty.GetString());
    }

    private EdoDocumentDto ParseDocument(JsonElement item)
    {
        if (!item.TryGetProperty("_id", out var idProperty) || idProperty.ValueKind != JsonValueKind.String
            || string.IsNullOrWhiteSpace(idProperty.GetString()))
            throw new IntegrationHttpException("Edocs inbox item did not contain the documented _id field.", StatusCodes.Status502BadGateway);

        var status = item.TryGetProperty("Status", out var statusProperty) && statusProperty.ValueKind == JsonValueKind.String
            ? EdoProviderStatusMapper.MapEdocsStatus(statusProperty.GetString())
            : new EdoDocumentStatusDto { Code = EdoDocumentStatusCode.UNKNOWN, ProviderStatusCode = "UNKNOWN" };

        return new EdoDocumentDto
        {
            ProviderDocumentId = idProperty.GetString(),
            Direction = EdoDirection.INBOX,
            DocumentType = "FACTURA",
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
