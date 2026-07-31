using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.Integration.AslBelgi.DTOs;
using Application.Features.Integration.AslBelgi.Parsing;
using Application.Features.Integration.AslBelgi.Services;
using Domain.Entities;
using Infrastructure.Persistence;
using Integration.AslBelgi.Http;
using Integration.Shared.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharedKernel.Exceptions;
using SharedKernel.Security;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Integration.AslBelgi.Services;

public sealed class AslBelgiVerificationService : IAslBelgiVerificationService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AppDbContext _context;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AslBelgiVerificationService> _logger;
    private readonly IHostEnvironment? _environment;

    public AslBelgiVerificationService(
        IHttpClientFactory httpClientFactory,
        AppDbContext context,
        IUserContext userContext,
        IUnitOfWork unitOfWork,
        ILogger<AslBelgiVerificationService> logger,
        IHostEnvironment? environment = null)
    {
        _httpClientFactory = httpClientFactory;
        _context = context;
        _userContext = userContext;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _environment = environment;
    }

    public async Task<JsonElement> GetPublicCodeInformationAsync(MarkingCodeCheckRequestDto request, CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();
        var response = await SendJsonAsync(organizationId, HttpMethod.Post, "public/api/cod/public/codes", request, ct);
        await PersistCodeResultsAsync(response);
        return response;
    }

    public async Task<JsonElement> GetPrivateCodeInformationAsync(MarkingCodeCheckRequestDto request, CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();
        var response = await SendJsonAsync(organizationId, HttpMethod.Post, "public/api/cod/private/codes", request, ct);
        await PersistCodeResultsAsync(response);
        return response;
    }

    public Task<JsonElement> GetProductsByGtinAsync(ProductRegistryByGtinRequestDto request, CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();
        var path = $"public/api/v1/product-registry/product?productGroup={Uri.EscapeDataString(request.ProductGroup)}&gtin={Uri.EscapeDataString(request.Gtin)}";
        return SendJsonAsync(organizationId, HttpMethod.Get, path, body: null, ct);
    }

    public async Task<CounterpartyStatusResponseDto?> GetCounterpartyStatusAsync(string tin, CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"public/api/v1/party/parties/{Uri.EscapeDataString(tin)}/status");
        using var response = await SendAsync(organizationId, request, ct);

        if (response.StatusCode == HttpStatusCode.NoContent)
            return null;

        await EnsureSuccessStatusOrThrowAsync(response);
        var content = await response.Content.ReadAsStringAsync(ct);

        if (string.IsNullOrWhiteSpace(content))
            return null;

        return JsonSerializer.Deserialize<CounterpartyStatusResponseDto>(content, JsonOptions);
    }

    // Bu qism CRPT so'rovi MUVAFFAQIYATLI bo'lgandan KEYIN ishga tushadi (Yo'l 2: UoW faqat
    // mahalliy yozuvni o'rab turadi, tashqi chaqiruvni emas). Agar mahalliy yozuv muvaffaqiyatsiz
    // bo'lsa, xato faqat log qilinadi va yutiladi — CRPT allaqachon javob bergan so'rovni
    // "bekor qilib" bo'lmaydi, shuning uchun chaqiruvchiga baribir asl CRPT javobi qaytariladi.
    private async Task PersistCodeResultsAsync(JsonElement response)
    {
        var organizationId = _userContext.OrganizationId;
        if (organizationId is null)
        {
            _logger.LogWarning("Skipping marking_code persistence for CRPT code check: no active organization in the current request context.");
            return;
        }

        try
        {
            await _unitOfWork.BeginAsync(CancellationToken.None);

            foreach (var entry in EnumerateCodeEntries(response))
                await UpsertMarkingCodeAsync(organizationId.Value, entry);

            await _context.SaveChangesAsync(CancellationToken.None);
            await _unitOfWork.CommitAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            await TryRollbackAsync();
            _logger.LogError(ex, "Failed to persist marking_code rows after a successful CRPT code check response. The CRPT response is still returned to the caller.");
        }
    }

    private async Task TryRollbackAsync()
    {
        try
        {
            await _unitOfWork.RollbackAsync(CancellationToken.None);
        }
        catch (Exception rollbackEx)
        {
            _logger.LogError(rollbackEx, "Rollback failed while persisting marking_code rows after a CRPT code check.");
        }
    }

    private static IEnumerable<JsonElement> EnumerateCodeEntries(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in root.EnumerateArray())
                yield return item;
            yield break;
        }

        if (root.ValueKind != JsonValueKind.Object)
            yield break;

        foreach (var property in root.EnumerateObject())
        {
            if (property.Value.ValueKind != JsonValueKind.Array)
                continue;

            foreach (var item in property.Value.EnumerateArray())
                yield return item;
            yield break;
        }

        yield return root;
    }

    private async Task UpsertMarkingCodeAsync(int organizationId, JsonElement entry)
    {
        if (entry.ValueKind != JsonValueKind.Object)
            return;

        if (!entry.TryGetProperty("code", out var codeProperty) || codeProperty.ValueKind != JsonValueKind.String)
            return;

        var rawCode = codeProperty.GetString();
        if (string.IsNullOrWhiteSpace(rawCode))
            return;

        if (!MarkingCodeParser.TryParse(rawCode, out var parts) || parts is null)
        {
            _logger.LogWarning(
                "Skipping marking_code persistence: code did not match the expected household appliances format (length {Length}).",
                rawCode.Length);
            return;
        }

        var crptStatus = entry.TryGetProperty("status", out var statusProperty) && statusProperty.ValueKind == JsonValueKind.String
            ? statusProperty.GetString()
            : null;

        if (!TryMapStatus(crptStatus, out var status))
        {
            _logger.LogWarning(
                "Skipping marking_code {Gtin}/{SerialNumber}: unmapped or missing CRPT status '{CrptStatus}'.",
                parts.Gtin, parts.SerialNumber, crptStatus);
            return;
        }

        var product = await _context.Products.SingleOrDefaultAsync(
            p => p.OrganizationId == organizationId && p.Gtin == parts.Gtin, CancellationToken.None);

        if (product is null)
        {
            _logger.LogWarning(
                "Skipping marking_code {Gtin}/{SerialNumber}: no inv_product with this GTIN in organization {OrganizationId}.",
                parts.Gtin, parts.SerialNumber, organizationId);
            return;
        }

        var existing = await _context.MarkingCodes.SingleOrDefaultAsync(
            m => m.OrganizationId == organizationId && m.Gtin == parts.Gtin && m.SerialNumber == parts.SerialNumber,
            CancellationToken.None);

        if (existing is null)
        {
            _context.MarkingCodes.Add(new MarkingCode
            {
                OrganizationId = organizationId,
                ProductId = product.Id,
                Gtin = parts.Gtin,
                SerialNumber = parts.SerialNumber,
                CheckKey = parts.CheckKey,
                CheckCode = parts.CheckCode,
                Status = status,
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            existing.Status = status;
            existing.UpdatedAt = DateTime.UtcNow;
        }
    }

    // CRPT §13.15 MC statusi (RECEIVED, APPLIED, INTRODUCED, WITHDRAWN, WRITTEN_OFF) loyihaning
    // marking_code.status cheklovi (in_circulation/withdrawn/utilized/sold) bilan bir xil emas.
    // Bu moslik — operator tomonidan tasdiqlanishi kerak bo'lgan taxmin (PROCESS5_AUDIT.md, 5-band):
    // "sold" bu yerda hech qachon o'rnatilmaydi — u faqat ichki sotuv oqimi orqali belgilanadi.
    private static bool TryMapStatus(string? crptStatus, out string status)
    {
        switch (crptStatus)
        {
            case "RECEIVED":
            case "APPLIED":
            case "INTRODUCED":
                status = "in_circulation";
                return true;
            case "WITHDRAWN":
                status = "withdrawn";
                return true;
            case "WRITTEN_OFF":
                status = "utilized";
                return true;
            default:
                status = string.Empty;
                return false;
        }
    }

    private int RequireOrganization() => _userContext.OrganizationId
        ?? throw new InvalidOperationException("An active organization is required for CRPT verification operations.");

    private async Task<JsonElement> SendJsonAsync(int organizationId, HttpMethod method, string path, object? body, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, path);
        if (body is not null)
            request.Content = JsonContent.Create(body, options: JsonOptions);

        using var response = await SendAsync(organizationId, request, ct);
        await EnsureSuccessStatusOrThrowAsync(response);
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        return document.RootElement.Clone();
    }

    private async Task<HttpResponseMessage> SendAsync(int organizationId, HttpRequestMessage request, CancellationToken ct)
    {
        request.Options.Set(IntegrationHttpRequestOptions.OrganizationId, organizationId);

        try
        {
            var client = _httpClientFactory.CreateClient(AslBelgiHttpClientNames.Client);
            return await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new IntegrationHttpException("CRPT request timed out.", StatusCodes.Status504GatewayTimeout);
        }
        catch (HttpRequestException)
        {
            throw new IntegrationHttpException("CRPT request could not be completed.", StatusCodes.Status502BadGateway);
        }
    }

    private async Task EnsureSuccessStatusOrThrowAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        if (response.StatusCode == HttpStatusCode.BadRequest && _environment?.IsDevelopment() == true)
        {
            var detail = SensitiveDataRedactor.Redact(await response.Content.ReadAsStringAsync());
            throw new IntegrationHttpException($"CRPT request failed with HTTP status 400. Detail: {detail}", 400);
        }

        throw response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => new IntegrationUnauthorizedException("CRPT credentials were rejected."),
            HttpStatusCode.Forbidden => new IntegrationForbiddenException("CRPT denied the request."),
            _ => new IntegrationHttpException(
                $"CRPT request failed with HTTP status {(int)response.StatusCode}.",
                (int)response.StatusCode)
        };
    }

}
