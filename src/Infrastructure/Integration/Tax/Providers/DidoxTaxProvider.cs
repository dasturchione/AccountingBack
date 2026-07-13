using Application.Abstractions.Integration;
using Application.Features.Cmn.Taxes.Integration.DTOs;
using Integration.Tax.Configs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Http;
using System.Text;
using System.Text.Json;

namespace Integration.Tax.Providers;

public sealed class DidoxTaxProvider : TaxProviderBase, ITaxDocumentProvider, IDidoxDocumentClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public DidoxTaxProvider(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        IOptions<TaxIntegrationSettings> options,
        ILogger<DidoxTaxProvider> logger)
        : base(httpClientFactory, httpContextAccessor, options, logger)
    {
    }

    public override string Code => "DIDOX";

    public override string Name => "Didox";

    protected override TaxIntegrationSettings.ProviderSettings ResolveProviderSettings() => Settings.Didox;

    public Task<TaxProviderOperationResultDto> SubmitAsync(TaxProviderOperationRequestDto request, CancellationToken ct = default)
    {
        var providerSettings = ResolveProviderSettings();

        if (string.IsNullOrWhiteSpace(providerSettings.FacturaDocType))
            return Task.FromResult(Failure("submit", "Didox FacturaDocType is not configured (confirm the ЭСФ docType code from the Didox documentation)."));
        if (string.IsNullOrWhiteSpace(request.CompanyToken))
            return Task.FromResult(Failure("submit", "Didox company token (user-key) is missing. Authenticate via /api/taxes/didox/auth first."));
        if (IsMissingSecret(providerSettings.PartnerToken))
            return Task.FromResult(Failure("submit", "Didox Partner-Authorization token is not configured (set TaxIntegration:Didox:PartnerToken via environment)."));

        var invoice = ParseInvoicePayload(request.Payload);
        if (invoice is null)
            return Task.FromResult(Failure("submit", "Didox invoice payload is missing or invalid."));

        var validationMessage = ValidateInvoice(invoice);
        if (validationMessage is not null)
            return Task.FromResult(Failure("submit", validationMessage));

        var path = providerSettings.CreateDocumentPath
            .Replace("{docType}", Uri.EscapeDataString(providerSettings.FacturaDocType))
            .Replace("{locale}", Uri.EscapeDataString(providerSettings.Locale));

        return ExecuteAsync<DidoxInvoiceRequest, SubmitDidoxResponse>(path, invoice, request.CompanyToken, ct, "submit");
    }

    public Task<TaxProviderOperationResultDto> GetDocumentStatusAsync(TaxProviderOperationRequestDto request, CancellationToken ct = default)
    {
        var didoxRequest = BuildStatusRequest(request);
        return ExecuteAsync<StatusDidoxRequest, StatusDidoxResponse>(Settings.Didox.StatusQueryPath, didoxRequest, request.CompanyToken, ct, "status");
    }

    public Task<TaxProviderOperationResultDto> CancelAsync(TaxProviderOperationRequestDto request, CancellationToken ct = default)
    {
        var didoxRequest = BuildCancelRequest(request);
        return ExecuteAsync<CancelDidoxRequest, CancelDidoxResponse>(Settings.Didox.CancelPath, didoxRequest, request.CompanyToken, ct, "cancel");
    }

    public Task<TaxProviderOperationResultDto> SignAsync(string documentId, string signature, string? companyToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(documentId))
            return Task.FromResult(Failure("sign", "Didox document id is required for signing."));

        // The E-IMZO signature is produced on the frontend; the backend only relays it.
        if (string.IsNullOrWhiteSpace(signature))
            return Task.FromResult(Failure("sign", "Signature is required (produced by the frontend E-IMZO flow)."));

        var path = ResolveProviderSettings().SignDocumentPath.Replace("{docId}", Uri.EscapeDataString(documentId));
        var body = new DidoxSignRequest { Signature = signature };
        return ExecuteAsync<DidoxSignRequest, SubmitDidoxResponse>(path, body, companyToken, ct, "sign");
    }

    private async Task<TaxProviderOperationResultDto> ExecuteAsync<TRequest, TResponse>(string path, TRequest request, string? companyToken, CancellationToken ct, string operation)
        where TRequest : class
        where TResponse : class
    {
        var providerSettings = ResolveProviderSettings();

        if (string.IsNullOrWhiteSpace(companyToken))
            return Failure(operation, "Didox company token (user-key) is missing. Authenticate via /api/taxes/didox/auth first.");

        if (IsMissingSecret(providerSettings.PartnerToken))
            return Failure(operation, "Didox Partner-Authorization token is not configured (set TaxIntegration:Didox:PartnerToken via environment).");

        var serializedBody = JsonSerializer.Serialize(request, JsonOptions);

        var response = await SendWithHeadersAsync<TResponse>(path, serializedBody, companyToken!, providerSettings, ct);

        var mapped = response is SubmitDidoxResponse submit
            ? MapFromSubmitResponse(submit)
            : response is StatusDidoxResponse status
                ? MapFromStatusResponse(status)
                : response is CancelDidoxResponse cancel
                    ? MapFromCancelResponse(cancel)
                    : null;

        if (mapped is null)
            return Failure(operation, "Unsupported Didox response payload.");

        return new TaxProviderOperationResultDto
        {
            ProviderCode = Code,
            Operation = operation,
            RequestedAt = DateTime.Now,
            ExternalDocumentId = mapped.ExternalDocumentId,
            StatusCode = mapped.StatusCode,
            StatusName = mapped.StatusName,
            IsSuccessful = mapped.IsSuccessful,
            Message = mapped.Message
        };
    }

    private static DidoxInvoiceRequest? ParseInvoicePayload(string? payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
            return null;

        try
        {
            return JsonSerializer.Deserialize<DidoxInvoiceRequest>(payload, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? ValidateInvoice(DidoxInvoiceRequest invoice)
    {
        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(invoice.SellerTin))
            missing.Add("SellerTin");
        if (string.IsNullOrWhiteSpace(invoice.BuyerTin))
            missing.Add("BuyerTin");
        if (invoice.Seller.VatRegStatus is null || string.IsNullOrWhiteSpace(invoice.Seller.VatRegCode))
            missing.Add("Seller.VatRegCode and Seller.VatRegStatus");
        if (string.IsNullOrWhiteSpace(invoice.Seller.Account) || string.IsNullOrWhiteSpace(invoice.Seller.BankId))
            missing.Add("Seller.Account and Seller.BankId");
        if (invoice.Buyer.VatRegStatus is null || string.IsNullOrWhiteSpace(invoice.Buyer.VatRegCode))
            missing.Add("Buyer.VatRegCode and Buyer.VatRegStatus");
        if (string.IsNullOrWhiteSpace(invoice.Buyer.Address))
            missing.Add("Buyer.Address");
        if (string.IsNullOrWhiteSpace(invoice.Buyer.Account) || string.IsNullOrWhiteSpace(invoice.Buyer.BankId))
            missing.Add("Buyer.Account and Buyer.BankId");
        if (invoice.ProductList.Products.Count == 0)
            missing.Add("ProductList.Products must contain at least one product");

        for (var index = 0; index < invoice.ProductList.Products.Count; index++)
        {
            var product = invoice.ProductList.Products[index];
            var path = $"ProductList.Products[{index}]";

            if (string.IsNullOrWhiteSpace(product.CatalogName))
                missing.Add($"{path}.CatalogName");
            if (string.IsNullOrWhiteSpace(product.PackageCode))
                missing.Add($"{path}.PackageCode");
            if (product.Origin is null)
                missing.Add($"{path}.Origin");
        }

        return missing.Count == 0
            ? null
            : $"Didox preflight failed. Required fields: {string.Join("; ", missing)}.";
    }

    private StatusDidoxRequest BuildStatusRequest(TaxProviderOperationRequestDto request)
    {
        return new StatusDidoxRequest
        {
            ProviderCode = request.ProviderCode,
            OrganizationId = request.OrganizationId,
            DocumentNumber = request.DocumentNumber,
            ExternalDocumentId = request.ExternalDocumentId,
            Payload = ParsePayload<StatusDidoxPayload>(request.Payload)
        };
    }

    private CancelDidoxRequest BuildCancelRequest(TaxProviderOperationRequestDto request)
    {
        return new CancelDidoxRequest
        {
            ProviderCode = request.ProviderCode,
            OrganizationId = request.OrganizationId,
            DocumentNumber = request.DocumentNumber,
            ExternalDocumentId = request.ExternalDocumentId,
            Payload = ParsePayload<CancelDidoxPayload>(request.Payload)
        };
    }

    private static TPayload ParsePayload<TPayload>(string? payload) where TPayload : new()
    {
        if (string.IsNullOrWhiteSpace(payload))
            return new TPayload();

        try
        {
            var parsed = JsonSerializer.Deserialize<TPayload>(payload, JsonOptions);
            return parsed ?? new TPayload();
        }
        catch
        {
            return new TPayload();
        }
    }

    private static TaxProviderOperationResultDto MapFromSubmitResponse(SubmitDidoxResponse response)
    {
        return new TaxProviderOperationResultDto
        {
            ExternalDocumentId = response.ExternalDocumentId
                ?? response.Data?.ExternalDocumentId
                ?? ExtractFromData(response.Data?.Content, "externalDocumentId")
                ?? ExtractFromData(response.Data?.Content, "documentId"),
            StatusCode = response.StatusCode ?? response.Code,
            StatusName = response.StatusName,
            IsSuccessful = IsSuccessfulResponse(response.IsSuccessful, response.Status),
            Message = response.Message
                ?? response.ErrorMessage
                ?? ExtractErrorsText(response.Errors)
                ?? ExtractFromData(response.Data?.Content, "message")
                ?? ExtractFromData(response.Data?.Content, "error")
        };
    }

    private static TaxProviderOperationResultDto MapFromStatusResponse(StatusDidoxResponse response)
    {
        return new TaxProviderOperationResultDto
        {
            ExternalDocumentId = response.ExternalDocumentId
                ?? response.Data?.ExternalDocumentId
                ?? ExtractFromData(response.Data?.Content, "externalDocumentId")
                ?? ExtractFromData(response.Data?.Content, "documentId"),
            StatusCode = response.StatusCode ?? response.Code,
            StatusName = response.StatusName,
            IsSuccessful = IsSuccessfulResponse(response.IsSuccessful, response.Status),
            Message = response.Message
                ?? response.ErrorMessage
                ?? ExtractErrorsText(response.Errors)
                ?? ExtractFromData(response.Data?.Content, "message")
                ?? ExtractFromData(response.Data?.Content, "error")
        };
    }

    private static TaxProviderOperationResultDto MapFromCancelResponse(CancelDidoxResponse response)
    {
        return new TaxProviderOperationResultDto
        {
            ExternalDocumentId = response.ExternalDocumentId
                ?? response.Data?.ExternalDocumentId
                ?? ExtractFromData(response.Data?.Content, "externalDocumentId")
                ?? ExtractFromData(response.Data?.Content, "documentId"),
            StatusCode = response.StatusCode ?? response.Code,
            StatusName = response.StatusName,
            IsSuccessful = IsSuccessfulResponse(response.IsSuccessful, response.Status),
            Message = response.Message
                ?? response.ErrorMessage
                ?? ExtractErrorsText(response.Errors)
                ?? ExtractFromData(response.Data?.Content, "message")
                ?? ExtractFromData(response.Data?.Content, "error")
        };
    }

    private TaxProviderOperationResultDto Failure(string operation, string message) => new()
    {
        ProviderCode = Code,
        Operation = operation,
        IsSuccessful = false,
        RequestedAt = DateTime.Now,
        Message = message
    };

    private static bool IsMissingSecret(string? value)
        => string.IsNullOrWhiteSpace(value)
            || value.Contains("SET_VIA_ENVIRONMENT", StringComparison.OrdinalIgnoreCase);

    private async Task<TResponse> SendWithHeadersAsync<TResponse>(
        string path,
        string payload,
        string companyToken,
        TaxIntegrationSettings.ProviderSettings providerSettings,
        CancellationToken ct)
    {
        var client = CreateClient();
        using var response = await SendWithRetryAsync(() =>
        {
            var request = new HttpRequestMessage(HttpMethod.Post, BuildUri(path))
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            // Didox requires both headers on every document request.
            request.Headers.TryAddWithoutValidation(providerSettings.UserKeyHeaderName, companyToken);
            request.Headers.TryAddWithoutValidation(providerSettings.PartnerAuthHeaderName, providerSettings.PartnerToken);

            Logger.LogDebug(
                "Didox request to {Path} with {UserKeyHeader} (len:{UserKeyLength}) and {PartnerHeader}.",
                request.RequestUri?.AbsolutePath,
                providerSettings.UserKeyHeaderName,
                companyToken.Length,
                providerSettings.PartnerAuthHeaderName);

            return client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        }, ct, retrySafe: false);

        await EnsureSuccessStatusOrThrowAsync(response);
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        var result = await JsonSerializer.DeserializeAsync<TResponse>(stream, JsonOptions, ct);
        if (result is null)
            return Activator.CreateInstance<TResponse>();

        return result;
    }

    private static bool IsSuccessfulResponse(bool? isSuccessful, string? status)
    {
        if (isSuccessful is not null)
            return isSuccessful.Value;

        if (string.IsNullOrWhiteSpace(status))
            return false;

        return string.Equals(status, "success", StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, "ok", StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, "completed", StringComparison.OrdinalIgnoreCase);
    }

    private static string? ExtractErrorsText(JsonElement? errors)
    {
        if (errors is null)
            return null;

        if (errors.Value.ValueKind == JsonValueKind.Null || errors.Value.ValueKind == JsonValueKind.Undefined)
            return null;

        if (errors.Value.ValueKind == JsonValueKind.Array)
        {
            var parts = new List<string>();
            foreach (var item in errors.Value.EnumerateArray())
            {
                var text = JsonValueToText(item);
                if (!string.IsNullOrWhiteSpace(text))
                    parts.Add(text);
            }

            if (parts.Count > 0)
                return string.Join("; ", parts);

            return null;
        }

        if (errors.Value.ValueKind == JsonValueKind.Object)
        {
            if (errors.Value.TryGetProperty("message", out var message) && !string.IsNullOrWhiteSpace(message.GetRawText()))
                return JsonValueToText(message);

            if (errors.Value.TryGetProperty("messages", out var messages) && messages.ValueKind == JsonValueKind.Array)
                return ExtractErrorsText(messages);
        }

        return JsonValueToText(errors.Value);
    }

    private static string? JsonValueToText(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.GetRawText(),
            JsonValueKind.False => "false",
            JsonValueKind.True => "true",
            _ => value.GetRawText()
        };
    }

    private static string? ExtractFromData(JsonElement? data, string propertyName)
    {
        if (data is null || data.Value.ValueKind != JsonValueKind.Object)
            return null;

        if (!data.Value.TryGetProperty(propertyName, out var json))
            return null;

        return json.ValueKind switch
        {
            JsonValueKind.String => json.GetString(),
            JsonValueKind.Number => json.GetRawText().Trim('"', '\\', ' '),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => json.GetRawText()
        };
    }
}
