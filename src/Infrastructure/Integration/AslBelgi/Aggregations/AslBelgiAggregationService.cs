using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.AuditLogs;
using Application.Features.Integration.AslBelgi.Aggregations;
using Application.Features.Integration.AslBelgi.Parsing;
using Domain.Entities;
using Infrastructure.Persistence;
using Integration.AslBelgi.Http;
using Integration.Shared.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Exceptions;
using SharedKernel.Security;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Integration.AslBelgi.Aggregations;

public sealed class AslBelgiAggregationService : IAslBelgiAggregationService
{
    private const string AggregationOperationType = "aggregation";

    // idempotency_record.operation_type — marking_aslbelgi_document.operation_type ("aggregation")
    // dan ATAYLAB alohida.
    private const string IdempotencyOperationType = "ASLBELGI_AGGREGATION";

    // §5.3 documentDate misoli offset bilan keladi ("2025-02-03T01:16:10+05:00").
    // packing_date bazada faqat DATE (vaqtsiz) sifatida saqlanadi — shuning uchun
    // O'zbekiston vaqt zonasi (doimiy UTC+5, DST yo'q) va yarim tunga qadar to'ldiriladi.
    private const string TashkentUtcOffset = "+05:00";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly AppDbContext _context;
    private readonly IUserContext _userContext;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAuditLogService _auditLogService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AslBelgiAggregationService> _logger;
    private readonly IHostEnvironment? _environment;

    public AslBelgiAggregationService(
        AppDbContext context,
        IUserContext userContext,
        IHttpClientFactory httpClientFactory,
        IAuditLogService auditLogService,
        IUnitOfWork unitOfWork,
        ILogger<AslBelgiAggregationService> logger,
        IHostEnvironment? environment = null)
    {
        _context = context;
        _userContext = userContext;
        _httpClientFactory = httpClientFactory;
        _auditLogService = auditLogService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _environment = environment;
    }

    public async Task<MarkingAggregationCreateResultDto> CreateAggregationAsync(MarkingAggregationCreateRequestDto request, CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();

        // Idempotentlik tekshiruvi — eng birinchi, boshqa hech qanday validatsiya yoki DB
        // yozuvidan oldin. Xuddi shu kalit bilan COMPLETED natija bo'lsa, CRPT'ga umuman
        // murojaat qilinmasdan o'sha natija qaytariladi.
        var requestHash = ComputeRequestHash(request);
        var replay = await TryCreateIdempotencyRecordAsync(organizationId, request.IdempotencyKey, requestHash, ct);
        if (replay is not null)
            return replay;

        if (!MarkingCodeParser.TryParse(request.ParentCode, out var parentParts) || parentParts is null)
            throw new MarkingCodeFormatException(
                $"Container (parent) code did not match the expected household appliances format (length {request.ParentCode.Length}).");

        var parentAlreadyExists = await _context.MarkingCodes.AnyAsync(
            m => m.OrganizationId == organizationId && m.Gtin == parentParts.Gtin && m.SerialNumber == parentParts.SerialNumber, ct);
        if (parentAlreadyExists)
            throw new InvalidOperationException("A marking_code row already exists for this container code — it cannot be created as a new one.");

        var innerParts = new List<MarkingCodeParts>();
        foreach (var rawCode in request.InnerCodes)
        {
            if (!MarkingCodeParser.TryParse(rawCode, out var parts) || parts is null)
                throw new MarkingCodeFormatException(
                    $"An inner code did not match the expected household appliances format (length {rawCode.Length}).");
            innerParts.Add(parts);
        }

        // Monotovarlilik: hujjatda agregatsiya (§5.3) uchun bu talab ANIQ yozilmagan (faqat
        // utilizatsiya, §5.1, uchun "bitta tovar guruhiga tegishli" deyilgan) — bu topshiriqda
        // so'ralgan mahalliy xavfsizlik qoidasi sifatida amalga oshirildi, hujjatdan olingan
        // majburiyat sifatida emas.
        var distinctGtins = innerParts.Select(p => p.Gtin).Distinct().ToList();
        if (distinctGtins.Count > 1)
            throw new InvalidOperationException(
                $"All inner codes must belong to the same GTIN (monotovarlilik). Found: {string.Join(", ", distinctGtins)}.");

        var sharedGtin = distinctGtins[0];

        var innerCodes = new List<MarkingCode>();
        foreach (var parts in innerParts)
        {
            var code = await _context.MarkingCodes.SingleOrDefaultAsync(
                m => m.OrganizationId == organizationId && m.Gtin == parts.Gtin && m.SerialNumber == parts.SerialNumber, ct)
                ?? throw new InvalidOperationException(
                    $"Inner code (gtin={parts.Gtin}, serial={parts.SerialNumber}) does not exist in marking_code yet.");

            if (code.Status != "in_circulation")
                throw new InvalidOperationException(
                    $"Inner code (gtin={parts.Gtin}, serial={parts.SerialNumber}) is in status '{code.Status}'; only 'in_circulation' codes can be aggregated.");

            if (code.ParentMarkingCodeId is not null)
                throw new InvalidOperationException(
                    $"Inner code (gtin={parts.Gtin}, serial={parts.SerialNumber}) already belongs to another container (parent_marking_code_id is set).");

            innerCodes.Add(code);
        }

        var innerProduct = await _context.Products.SingleOrDefaultAsync(
            p => p.OrganizationId == organizationId && p.Gtin == sharedGtin, ct)
            ?? throw new InvalidOperationException($"No inv_product with GTIN '{sharedGtin}' was found in the current organization.");

        // Konteyner kodining o'z GTIN'i ichki kodlarnikidan farq qilishi mumkin (masalan
        // guruh qadog'i uchun alohida GTIN bo'lsa) — shuning uchun mahsulot alohida topiladi.
        var parentProduct = parentParts.Gtin == sharedGtin
            ? innerProduct
            : await _context.Products.SingleOrDefaultAsync(p => p.OrganizationId == organizationId && p.Gtin == parentParts.Gtin, ct)
                ?? throw new InvalidOperationException($"No inv_product with GTIN '{parentParts.Gtin}' (container code) was found in the current organization.");

        var businessPlace = await ResolveBusinessPlaceAsync(request, organizationId, ct);

        if (!int.TryParse(businessPlace.ExternalId, out var crptBusinessPlaceId))
            throw new InvalidOperationException(
                $"marking_business_place.external_id ('{businessPlace.ExternalId}') is not a valid CRPT businessPlaceId — must be numeric.");

        // TRANZAKSIYA 1 — konteyner marking_code (yangi qator) + marking_aggregation PENDING,
        // tashqi chaqiruvdan oldin alohida commit qilinadi.
        var parentCode = new MarkingCode
        {
            OrganizationId = organizationId,
            ProductId = parentProduct.Id,
            Gtin = parentParts.Gtin,
            SerialNumber = parentParts.SerialNumber,
            CheckKey = parentParts.CheckKey,
            CheckCode = parentParts.CheckCode,
            Status = "in_circulation",
            CreatedAt = DateTime.UtcNow
        };

        MarkingAggregation aggregation;
        await _unitOfWork.BeginAsync(ct);
        try
        {
            _context.MarkingCodes.Add(parentCode);
            await _context.SaveChangesAsync(ct);

            aggregation = new MarkingAggregation
            {
                OrganizationId = organizationId,
                BusinessPlaceId = businessPlace.Id,
                ParentMarkingCodeId = parentCode.Id,
                PackingDate = request.PackingDate,
                PlannedCapacity = request.PlannedCapacity,
                ActualCapacity = innerCodes.Count,
                Status = "PENDING",
                CreatedAt = DateTime.UtcNow
            };
            _context.MarkingAggregations.Add(aggregation);
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
            var document = new CrptAggregationDocument
            {
                AggregationUnits =
                [
                    new CrptAggregationUnit
                    {
                        AggregationItemsCount = innerCodes.Count,
                        AggregationUnitCapacity = request.PlannedCapacity,
                        Codes = innerParts.Select(MarkingCodeParser.ComposeIdentification).ToList(),
                        UnitSerialNumber = MarkingCodeParser.ComposeIdentification(parentParts)
                    }
                ],
                BusinessPlaceId = crptBusinessPlaceId,
                DocumentDate = $"{request.PackingDate:yyyy-MM-dd}T00:00:00{TashkentUtcOffset}"
            };

            var documentJson = JsonSerializer.Serialize(document, JsonOptions);
            var documentBody = Convert.ToBase64String(Encoding.UTF8.GetBytes(documentJson));

            response = await SendJsonAsync(organizationId, HttpMethod.Post, "public/api/v1/doc/aggregation", new { documentBody }, ct);
        }
        catch (Exception ex)
        {
            await MarkFailedAsync(aggregation, organizationId, request.IdempotencyKey, ex.Message);
            throw;
        }

        var crptDocumentId = response.TryGetProperty("documentId", out var documentIdProperty) && documentIdProperty.ValueKind == JsonValueKind.String
            ? documentIdProperty.GetString()
            : null;

        if (string.IsNullOrWhiteSpace(crptDocumentId))
        {
            const string message = "CRPT aggregation response did not include a documentId.";
            await MarkFailedAsync(aggregation, organizationId, request.IdempotencyKey, message);
            throw new IntegrationHttpException(message, StatusCodes.Status502BadGateway);
        }

        // TRANZAKSIYA 2 — ichki kodlarning parent_marking_code_id si o'rnatiladi,
        // marking_aggregation.status=COMPLETED, idempotentlik yakuni, audit yozuvi. Alohida commit.
        try
        {
            await _unitOfWork.BeginAsync(CancellationToken.None);

            aggregation.CrptDocumentId = crptDocumentId;
            aggregation.Status = "COMPLETED";

            foreach (var code in innerCodes)
            {
                code.ParentMarkingCodeId = parentCode.Id;
                code.UpdatedAt = DateTime.UtcNow;
            }

            _context.MarkingAslBelgiDocuments.Add(new MarkingAslBelgiDocument
            {
                OrganizationId = organizationId,
                OperationType = AggregationOperationType,
                InternalDocumentType = "marking_aggregation",
                InternalDocumentId = aggregation.Id,
                ProviderDocumentId = crptDocumentId,
                Status = "COMPLETED",
                CreatedAt = DateTime.UtcNow
            });

            await CompleteIdempotencyAsync(organizationId, request.IdempotencyKey, crptDocumentId, CancellationToken.None);
            await _context.SaveChangesAsync(CancellationToken.None);

            _auditLogService.SetNewValues(new { aggregation.Id, aggregation.CrptDocumentId, aggregation.Status, ParentMarkingCodeId = parentCode.Id });
            await _auditLogService.CreateAsync("marking_aggregation", aggregation.Id.ToString(), AuditLogOperationTypeConst.Create);

            await _unitOfWork.CommitAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            await TryRollbackAsync();
            // CRPT tomonda hisobot allaqachon qabul qilingan — bu holatni orqaga qaytarib
            // bo'lmaydi. Idempotentlik yozuvi FAILED + resultDocumentId bilan qoldiriladi.
            await HandleRemoteSuccessLocalFailureAsync(organizationId, request.IdempotencyKey, crptDocumentId, ex, aggregation.Id);
            throw;
        }

        return new MarkingAggregationCreateResultDto
        {
            AggregationId = aggregation.Id,
            ParentMarkingCodeId = parentCode.Id,
            CrptDocumentId = aggregation.CrptDocumentId,
            Status = aggregation.Status,
            IsReplay = false
        };
    }

    private async Task<MarkingBusinessPlace> ResolveBusinessPlaceAsync(MarkingAggregationCreateRequestDto request, int organizationId, CancellationToken ct)
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

    private async Task MarkFailedAsync(MarkingAggregation aggregation, int organizationId, string idempotencyKey, string errorMessage)
    {
        try
        {
            await _unitOfWork.BeginAsync(CancellationToken.None);

            aggregation.Status = "FAILED";

            _context.MarkingAslBelgiDocuments.Add(new MarkingAslBelgiDocument
            {
                OrganizationId = organizationId,
                OperationType = AggregationOperationType,
                InternalDocumentType = "marking_aggregation",
                InternalDocumentId = aggregation.Id,
                Status = "FAILED",
                ErrorMessage = errorMessage,
                CreatedAt = DateTime.UtcNow
            });

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
                "Failed to record FAILED status for MarkingAggregation {AggregationId}. Original CRPT error: {OriginalError}",
                aggregation.Id,
                errorMessage);
        }
    }

    // --- Idempotentlik: AslBelgiTransferService (o'chirilgan, 2C.5) dagi naqsh qayta ishlatildi ---

    private static string ComputeRequestHash(MarkingAggregationCreateRequestDto request) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
            $"{IdempotencyOperationType}|{request.ParentCode}|{string.Join(",", request.InnerCodes)}|{request.BusinessPlaceId}|{request.WarehouseId}|{request.PackingDate:O}|{request.PlannedCapacity}")));

    private async Task<MarkingAggregationCreateResultDto?> TryCreateIdempotencyRecordAsync(int organizationId, string key, string hash, CancellationToken ct)
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
            var existingAggregation = await _context.MarkingAggregations.SingleOrDefaultAsync(
                a => a.OrganizationId == organizationId && a.CrptDocumentId == existing.ResultDocumentId, ct);

            return new MarkingAggregationCreateResultDto
            {
                AggregationId = existingAggregation?.Id ?? 0,
                ParentMarkingCodeId = existingAggregation?.ParentMarkingCodeId ?? 0,
                CrptDocumentId = existing.ResultDocumentId,
                Status = existingAggregation?.Status ?? "COMPLETED",
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
            "CRPT aggregation succeeded remotely (CrptDocumentId={CrptDocumentId}) but local status update failed for MarkingAggregation {AggregationId}. Manual reconciliation required.",
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
            _logger.LogError(rollbackEx, "Rollback failed while persisting a marking_aggregation result.");
        }
    }

    private int RequireOrganization() => _userContext.OrganizationId
        ?? throw new InvalidOperationException("An active organization is required for CRPT aggregation operations.");

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

    private sealed class CrptAggregationDocument
    {
        public List<CrptAggregationUnit> AggregationUnits { get; init; } = [];
        public int BusinessPlaceId { get; init; }
        public string DocumentDate { get; init; } = string.Empty;
    }

    private sealed class CrptAggregationUnit
    {
        public int AggregationItemsCount { get; init; }
        public int AggregationUnitCapacity { get; init; }
        public List<string> Codes { get; init; } = [];
        public string UnitSerialNumber { get; init; } = string.Empty;
    }
}
