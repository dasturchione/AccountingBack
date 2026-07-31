using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.AuditLogs;
using Application.Features.Integration.AslBelgi.Parsing;
using Application.Features.Integration.AslBelgi.Utilizations;
using Domain.Entities;
using Infrastructure.Persistence;
using Integration.AslBelgi.Configs;
using Integration.AslBelgi.Http;
using Integration.Shared.Http;
using Microsoft.AspNetCore.Http;
using SharedKernel.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Exceptions;
using SharedKernel.Security;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Integration.AslBelgi.Utilizations;

public sealed class AslBelgiUtilizationService : IAslBelgiUtilizationService
{
    private const string UtilizationOperationType = "utilization";
    private const string DateFormat = "yyyy-MM-dd";

    // idempotency_record.operation_type — marking_aslbelgi_document.operation_type ("utilization")
    // dan ATAYLAB alohida.
    private const string IdempotencyOperationType = "ASLBELGI_UTILIZATION";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly AppDbContext _context;
    private readonly IUserContext _userContext;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAuditLogService _auditLogService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AslBelgiUtilizationService> _logger;
    private readonly IHostEnvironment? _environment;
    private readonly AslBelgiOptions? _options;

    public AslBelgiUtilizationService(
        AppDbContext context,
        IUserContext userContext,
        IHttpClientFactory httpClientFactory,
        IAuditLogService auditLogService,
        IUnitOfWork unitOfWork,
        ILogger<AslBelgiUtilizationService> logger,
        IHostEnvironment? environment = null,
        IOptions<AslBelgiOptions>? options = null)
    {
        _context = context;
        _userContext = userContext;
        _httpClientFactory = httpClientFactory;
        _auditLogService = auditLogService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _environment = environment;
        _options = options?.Value;
    }

    public async Task<MarkingUtilizationCreateResultDto> CreateUtilizationAsync(MarkingUtilizationCreateRequestDto request, CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();

        // Idempotentlik tekshiruvi — eng birinchi, boshqa hech qanday validatsiya yoki DB
        // yozuvidan oldin. Xuddi shu kalit bilan COMPLETED natija bo'lsa, CRPT'ga umuman
        // murojaat qilinmasdan o'sha natija qaytariladi.
        var requestHash = ComputeRequestHash(request);
        var replay = await TryCreateIdempotencyRecordAsync(organizationId, request.IdempotencyKey, requestHash, ct);
        if (replay is not null)
            return replay;

        var productGroup = _options?.ProductGroup;
        if (string.IsNullOrWhiteSpace(productGroup))
            throw new InvalidOperationException("AslBelgi:ProductGroup is not configured.");

        // Kodlarni CRPT ga yuborishdan oldin (T1 dan ham oldin) hal qilamiz — noto'g'ri
        // formatdagi kod yoki topilmagan buyurtma so'rovni CRPT ga yetib bormasdan to'xtatadi.
        var resolvedCodes = await ResolveMarkingCodesAsync(request, organizationId, ct);
        var businessPlace = await ResolveBusinessPlaceAsync(request, organizationId, ct);

        if (!int.TryParse(businessPlace.ExternalId, out var crptBusinessPlaceId))
            throw new InvalidOperationException(
                $"marking_business_place.external_id ('{businessPlace.ExternalId}') is not a valid CRPT businessPlaceId — must be numeric.");

        // TRANZAKSIYA 1 — mahalliy PENDING yozuv, tashqi chaqiruvdan oldin alohida commit qilinadi.
        var utilization = new MarkingUtilization
        {
            OrganizationId = organizationId,
            BusinessPlaceId = businessPlace.Id,
            ProductionDate = request.ProductionDate,
            ExpirationDate = request.ExpirationDate,
            Status = "PENDING",
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.BeginAsync(ct);
        try
        {
            _context.MarkingUtilizations.Add(utilization);
            await _context.SaveChangesAsync(ct);
            await _unitOfWork.CommitAsync(ct);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(ct);
            throw;
        }

        // Tashqi chaqiruv ataylab hech qanday tranzaksiya ichida emas — CRPT tomonda qabul
        // qilingan hisobotni "rollback" qilib bo'lmaydi.
        JsonElement response;
        try
        {
            var payload = new CrptUtilisationRequest
            {
                Sntins = resolvedCodes.Select(x => x.Sntin).ToList(),
                BusinessPlaceId = crptBusinessPlaceId,
                ReleaseType = request.ReleaseType,
                ManufacturerCountry = request.ManufacturerCountry,
                ProductionDate = request.ProductionDate.ToString(DateFormat),
                ExpirationDate = request.ExpirationDate?.ToString(DateFormat)
            };

            var path = $"api/utilisation?productGroup={Uri.EscapeDataString(productGroup)}";
            response = await SendJsonAsync(organizationId, HttpMethod.Post, path, payload, ct);
        }
        catch (Exception ex)
        {
            // CRPT so'rovi bajarilmadi. Aniq xato: operator buni ko'rishi kerak.
            await MarkFailedAsync(utilization, organizationId, request.IdempotencyKey, ex.Message);
            throw;
        }

        var crptDocumentId = response.TryGetProperty("reportId", out var reportIdProperty) && reportIdProperty.ValueKind == JsonValueKind.String
            ? reportIdProperty.GetString()
            : null;

        if (string.IsNullOrWhiteSpace(crptDocumentId))
        {
            const string message = "CRPT utilisation response did not include a reportId.";
            await MarkFailedAsync(utilization, organizationId, request.IdempotencyKey, message);
            throw new IntegrationHttpException(message, StatusCodes.Status502BadGateway);
        }

        // TRANZAKSIYA 2 — natija yozuvi, idempotentlik yakuni, audit log va tegishli
        // marking_code larni "in_circulation" ga o'tkazish, alohida commit.
        try
        {
            await _unitOfWork.BeginAsync(CancellationToken.None);

            utilization.CrptDocumentId = crptDocumentId;
            utilization.Status = "SUBMITTED";

            _context.MarkingAslBelgiDocuments.Add(new MarkingAslBelgiDocument
            {
                OrganizationId = organizationId,
                OperationType = UtilizationOperationType,
                InternalDocumentType = "marking_utilization",
                InternalDocumentId = utilization.Id,
                ProviderDocumentId = crptDocumentId,
                Status = "SUBMITTED",
                CreatedAt = DateTime.UtcNow
            });

            await ApplyInCirculationStatusAsync(resolvedCodes, organizationId, utilization.Id);
            await CompleteIdempotencyAsync(organizationId, request.IdempotencyKey, crptDocumentId, CancellationToken.None);

            await _context.SaveChangesAsync(CancellationToken.None);

            _auditLogService.SetNewValues(new { utilization.Id, utilization.CrptDocumentId, utilization.Status });
            await _auditLogService.CreateAsync("marking_utilization", utilization.Id.ToString(), AuditLogOperationTypeConst.Create);

            await _unitOfWork.CommitAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            await TryRollbackAsync();
            // CRPT tomonda hisobot allaqachon qabul qilingan (crptDocumentId bor) — bu holatni
            // orqaga qaytarib bo'lmaydi. Idempotentlik yozuvi FAILED + resultDocumentId bilan
            // qoldiriladi (jimgina yo'qotish o'rniga qo'lda tiklashga majburlaydi).
            await HandleRemoteSuccessLocalFailureAsync(organizationId, request.IdempotencyKey, crptDocumentId, ex, utilization.Id);
            throw;
        }

        return new MarkingUtilizationCreateResultDto
        {
            UtilizationId = utilization.Id,
            CrptDocumentId = utilization.CrptDocumentId,
            Status = utilization.Status,
            IsReplay = false
        };
    }

    private sealed record ResolvedCode(string Sntin, MarkingCodeParts Parts, MarkingCode? ExistingEntity);

    private async Task<List<ResolvedCode>> ResolveMarkingCodesAsync(MarkingUtilizationCreateRequestDto request, int organizationId, CancellationToken ct)
    {
        if (request.MarkingOrderId is { } orderId)
        {
            var orderExists = await _context.MarkingOrders.AnyAsync(o => o.Id == orderId && o.OrganizationId == organizationId, ct);
            if (!orderExists)
                throw new InvalidOperationException("The specified marking order was not found in the current organization.");

            var existingCodes = await _context.MarkingCodes
                .Where(c => c.OrganizationId == organizationId && c.OrderId == orderId)
                .ToListAsync(ct);

            if (existingCodes.Count == 0)
                throw new InvalidOperationException(
                    $"No marking_code rows are linked to marking_order {orderId} (order_id) yet. The code-fetch flow " +
                    "(GET /api/codes) is not implemented in this project — codes must already exist in marking_code " +
                    "before they can be utilised through an order reference. Use the 'codes' input instead.");

            return existingCodes
                .Select(code =>
                {
                    var parts = new MarkingCodeParts(code.Gtin, code.SerialNumber, code.CheckKey, code.CheckCode);
                    return new ResolvedCode(MarkingCodeParser.Compose(parts), parts, code);
                })
                .ToList();
        }

        if (request.Codes is { Count: > 0 })
        {
            var resolved = new List<ResolvedCode>();
            foreach (var rawCode in request.Codes)
            {
                if (!MarkingCodeParser.TryParse(rawCode, out var parts) || parts is null)
                    throw new MarkingCodeFormatException(
                        $"Code did not match the expected household appliances format (length {rawCode.Length}).");

                resolved.Add(new ResolvedCode(rawCode, parts, null));
            }

            return resolved;
        }

        throw new InvalidOperationException("Either markingOrderId or codes must be provided.");
    }

    private async Task ApplyInCirculationStatusAsync(List<ResolvedCode> resolvedCodes, int organizationId, long utilizationId)
    {
        foreach (var resolved in resolvedCodes)
        {
            if (resolved.ExistingEntity is not null)
            {
                resolved.ExistingEntity.UtilizationId = utilizationId;
                if (resolved.ExistingEntity.Status != "in_circulation")
                {
                    resolved.ExistingEntity.Status = "in_circulation";
                    resolved.ExistingEntity.UpdatedAt = DateTime.UtcNow;
                }
                continue;
            }

            var existingByKey = await _context.MarkingCodes.SingleOrDefaultAsync(
                m => m.OrganizationId == organizationId && m.Gtin == resolved.Parts.Gtin && m.SerialNumber == resolved.Parts.SerialNumber,
                CancellationToken.None);

            if (existingByKey is not null)
            {
                existingByKey.UtilizationId = utilizationId;
                if (existingByKey.Status != "in_circulation")
                {
                    existingByKey.Status = "in_circulation";
                    existingByKey.UpdatedAt = DateTime.UtcNow;
                }
                continue;
            }

            var product = await _context.Products.SingleOrDefaultAsync(
                p => p.OrganizationId == organizationId && p.Gtin == resolved.Parts.Gtin, CancellationToken.None);

            if (product is null)
            {
                _logger.LogWarning(
                    "CRPT utilisation succeeded for code {Gtin}/{SerialNumber}, but no inv_product with this GTIN exists in organization {OrganizationId} — the marking_code row was not created.",
                    resolved.Parts.Gtin, resolved.Parts.SerialNumber, organizationId);
                continue;
            }

            _context.MarkingCodes.Add(new MarkingCode
            {
                OrganizationId = organizationId,
                ProductId = product.Id,
                Gtin = resolved.Parts.Gtin,
                SerialNumber = resolved.Parts.SerialNumber,
                CheckKey = resolved.Parts.CheckKey,
                CheckCode = resolved.Parts.CheckCode,
                Status = "in_circulation",
                UtilizationId = utilizationId,
                CreatedAt = DateTime.UtcNow
            });
        }
    }

    private async Task<MarkingBusinessPlace> ResolveBusinessPlaceAsync(MarkingUtilizationCreateRequestDto request, int organizationId, CancellationToken ct)
    {
        if (request.BusinessPlaceId is { } businessPlaceId)
        {
            return await _context.MarkingBusinessPlaces.SingleOrDefaultAsync(
                b => b.Id == businessPlaceId && b.OrganizationId == organizationId, ct)
                ?? throw new InvalidOperationException("The specified business place was not found in the current organization.");
        }

        if (request.WarehouseId is { } warehouseId)
        {
            var warehouse = await _context.Warehouses.SingleOrDefaultAsync(
                w => w.Id == warehouseId && w.OrganizationId == organizationId, ct)
                ?? throw new InvalidOperationException("The specified warehouse was not found in the current organization.");

            if (warehouse.BusinessPlaceId is not { } resolvedBusinessPlaceId)
                throw new InvalidOperationException(
                    $"Warehouse '{warehouse.Name}' has no business place linked (inv_warehouse.business_place_id is empty).");

            return await _context.MarkingBusinessPlaces.SingleOrDefaultAsync(
                b => b.Id == resolvedBusinessPlaceId && b.OrganizationId == organizationId, ct)
                ?? throw new InvalidOperationException("The warehouse's linked business place no longer exists.");
        }

        throw new InvalidOperationException("Either businessPlaceId or warehouseId must be provided.");
    }

    private async Task MarkFailedAsync(MarkingUtilization utilization, int organizationId, string idempotencyKey, string errorMessage)
    {
        try
        {
            await _unitOfWork.BeginAsync(CancellationToken.None);

            utilization.Status = "FAILED";

            _context.MarkingAslBelgiDocuments.Add(new MarkingAslBelgiDocument
            {
                OrganizationId = organizationId,
                OperationType = UtilizationOperationType,
                InternalDocumentType = "marking_utilization",
                InternalDocumentId = utilization.Id,
                Status = "FAILED",
                ErrorMessage = errorMessage,
                CreatedAt = DateTime.UtcNow
            });

            // Idempotentlik yozuvi ham FAILED qilinadi (bitta tranzaksiyada) — shu kalit bilan
            // keyingi urinishga ruxsat berish uchun.
            var idempotencyRecord = await _context.IdempotencyRecords.SingleOrDefaultAsync(
                x => x.OrganizationId == organizationId && x.IdempotencyKey == idempotencyKey, CancellationToken.None);
            if (idempotencyRecord is not null && idempotencyRecord.Status != "COMPLETED")
            {
                idempotencyRecord.Status = "FAILED";
                idempotencyRecord.UpdatedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(CancellationToken.None);
            await _unitOfWork.CommitAsync(CancellationToken.None);
        }
        catch (Exception compensationEx)
        {
            await TryRollbackAsync();
            _logger.LogCritical(
                compensationEx,
                "Failed to record FAILED status for MarkingUtilization {UtilizationId}. Original CRPT error: {OriginalError}",
                utilization.Id,
                errorMessage);
        }
    }

    // --- Idempotentlik: AslBelgiTransferService (o'chirilgan, 2C.5) dagi naqsh qayta ishlatildi ---

    private static string ComputeRequestHash(MarkingUtilizationCreateRequestDto request) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
            $"{IdempotencyOperationType}|{request.MarkingOrderId}|{string.Join(",", request.Codes ?? [])}|{request.BusinessPlaceId}|{request.WarehouseId}|{request.ProductionDate:O}|{request.ExpirationDate:O}|{request.ReleaseType}|{request.ManufacturerCountry}")));

    private async Task<MarkingUtilizationCreateResultDto?> TryCreateIdempotencyRecordAsync(int organizationId, string key, string hash, CancellationToken ct)
    {
        var existing = await _context.IdempotencyRecords.SingleOrDefaultAsync(x => x.IdempotencyKey == key, ct);

        if (existing is null)
        {
            _context.IdempotencyRecords.Add(new IdempotencyRecord
            {
                OrganizationId = organizationId,
                IdempotencyKey = key,
                OperationType = IdempotencyOperationType,
                RequestHash = hash,
                Status = "PENDING",
                CreatedDate = DateTime.UtcNow
            });

            try
            {
                await _context.SaveChangesAsync(ct);
                return null;
            }
            catch (DbUpdateException)
            {
                _context.ChangeTracker.Clear();
                existing = await _context.IdempotencyRecords.SingleAsync(x => x.IdempotencyKey == key, ct);
            }
        }

        if (existing.OperationType != IdempotencyOperationType || existing.RequestHash != hash)
            throw new InvalidOperationException("The idempotency key has already been used for a different request.");

        if (existing.Status == "COMPLETED" && !string.IsNullOrWhiteSpace(existing.ResultDocumentId))
        {
            var existingUtilization = await _context.MarkingUtilizations.SingleOrDefaultAsync(
                u => u.OrganizationId == organizationId && u.CrptDocumentId == existing.ResultDocumentId, ct);

            return new MarkingUtilizationCreateResultDto
            {
                UtilizationId = existingUtilization?.Id ?? 0,
                CrptDocumentId = existing.ResultDocumentId,
                Status = existingUtilization?.Status ?? "SUBMITTED",
                IsReplay = true
            };
        }

        if (existing.Status == "FAILED")
        {
            existing.Status = "PENDING";
            existing.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
            return null;
        }

        throw new InvalidOperationException("A request with this idempotency key is already in progress.");
    }

    private async Task CompleteIdempotencyAsync(int organizationId, string key, string resultId, CancellationToken ct)
    {
        var record = await _context.IdempotencyRecords.SingleAsync(x => x.OrganizationId == organizationId && x.IdempotencyKey == key, ct);
        record.ResultDocumentId = resultId;
        record.Status = "COMPLETED";
        record.UpdatedDate = DateTime.UtcNow;
    }

    private async Task HandleRemoteSuccessLocalFailureAsync(int organizationId, string idempotencyKey, string crptResultId, Exception ex, long localId)
    {
        _logger.LogCritical(
            ex,
            "CRPT utilisation succeeded remotely (CrptDocumentId={CrptDocumentId}) but local status update failed for MarkingUtilization {UtilizationId}. Manual reconciliation required.",
            crptResultId,
            localId);

        try
        {
            _context.ChangeTracker.Clear();

            var record = await _context.IdempotencyRecords.SingleOrDefaultAsync(
                x => x.OrganizationId == organizationId && x.IdempotencyKey == idempotencyKey, CancellationToken.None);
            if (record is null)
                return;

            record.Status = "FAILED";
            record.ResultDocumentId = crptResultId;
            record.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception compensationEx)
        {
            _logger.LogCritical(
                compensationEx,
                "Failed to record reconciliation state for CRPT DocumentId={CrptDocumentId}, IdempotencyKey={IdempotencyKey}.",
                crptResultId,
                idempotencyKey);
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
            _logger.LogError(rollbackEx, "Rollback failed while persisting a marking_utilization result.");
        }
    }

    private int RequireOrganization() => _userContext.OrganizationId
        ?? throw new InvalidOperationException("An active organization is required for CRPT utilisation operations.");

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

    private sealed class CrptUtilisationRequest
    {
        public List<string> Sntins { get; init; } = [];
        public int BusinessPlaceId { get; init; }
        public string ReleaseType { get; init; } = string.Empty;
        public string ManufacturerCountry { get; init; } = string.Empty;
        public string? ProductionDate { get; init; }
        public string? ExpirationDate { get; init; }
    }
}
