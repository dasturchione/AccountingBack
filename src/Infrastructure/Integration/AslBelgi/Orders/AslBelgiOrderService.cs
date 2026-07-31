using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.AuditLogs;
using Application.Features.Integration.AslBelgi.Orders;
using Application.Features.Integration.AslBelgi.Parsing;
using Domain.Entities;
using Infrastructure.Persistence;
using Integration.AslBelgi.Configs;
using Integration.AslBelgi.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Constants;
using SharedKernel.Exceptions;
using SharedKernel.Security;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Integration.AslBelgi.Orders;

public sealed class AslBelgiOrderService : IAslBelgiOrderService
{
    // Bu bosqichda hardcode qilingan taxminlar (task kirish shartnomasida faqat product_id/quantity/
    // business_place_id bor — releaseMethodType/serialNumberType/cisType berilmagan):
    //   releaseMethodType = PRIMARY  — birlamchi markirovka (ishlab chiqarish/import), §13.2
    //   serialNumberType  = OPERATOR — seriya raqami tizim tomonidan generatsiya qilinadi, §13.4
    //   cisType           = UNIT     — iste'mol qadog'i (maishiy texnika birlik holida), §13.13
    // Operator tasdig'i kerak bo'lsa, bu uchtasi keyinchalik so'rov parametriga aylantirilishi mumkin.
    private const string ReleaseMethodType = "PRIMARY";
    private const string SerialNumberType = "OPERATOR";
    private const string CisType = "UNIT";
    private const string OrderOperationType = "order";

    // idempotency_record.operation_type qiymati — marking_aslbelgi_document.operation_type
    // ("order") dan ATAYLAB alohida: ikkalasi bir xil ustunga o'xshamasin va aralashmasin.
    private const string IdempotencyOperationType = "ASLBELGI_ORDER";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly AppDbContext _context;
    private readonly IUserContext _userContext;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAuditLogService _auditLogService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AslBelgiOrderService> _logger;
    private readonly IHostEnvironment? _environment;
    private readonly AslBelgiOptions? _options;
    private readonly string? _apiKey;

    public AslBelgiOrderService(
        AppDbContext context,
        IUserContext userContext,
        IHttpClientFactory httpClientFactory,
        IAuditLogService auditLogService,
        IUnitOfWork unitOfWork,
        ILogger<AslBelgiOrderService> logger,
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
        _apiKey = options?.Value.ApiKey;
    }

    public async Task<MarkingOrderCreateResultDto> CreateOrderAsync(MarkingOrderCreateRequestDto request, CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();

        // Idempotentlik tekshiruvi — eng birinchi bo'lib, boshqa hech qanday validatsiya yoki
        // DB yozuvidan oldin. Xuddi shu kalit bilan COMPLETED natija bo'lsa, CRPT'ga
        // umuman murojaat qilinmasdan o'sha natija qaytariladi.
        var requestHash = ComputeRequestHash(request);
        var replay = await TryCreateIdempotencyRecordAsync(organizationId, request.IdempotencyKey, requestHash, ct);
        if (replay is not null)
            return replay;

        var product = await _context.Products.SingleOrDefaultAsync(
            p => p.Id == request.ProductId && p.OrganizationId == organizationId, ct)
            ?? throw new InvalidOperationException("The product was not found in the current organization.");

        if (string.IsNullOrWhiteSpace(product.Gtin))
            throw new InvalidOperationException("GTIN yo'q, avval mahsulotga GTIN kiriting.");

        var businessPlace = await ResolveBusinessPlaceAsync(request, organizationId, ct);

        if (!int.TryParse(businessPlace.ExternalId, out var crptBusinessPlaceId))
            throw new InvalidOperationException(
                $"marking_business_place.external_id ('{businessPlace.ExternalId}') is not a valid CRPT businessPlaceId — must be numeric.");

        var productGroup = _options?.ProductGroup;
        if (string.IsNullOrWhiteSpace(productGroup))
            throw new InvalidOperationException("AslBelgi:ProductGroup is not configured.");

        // TRANZAKSIYA 1 — mahalliy PENDING yozuv, tashqi chaqiruvdan oldin alohida commit qilinadi.
        // Bu qulf: CRPT hali chaqirilmasdan turib buyurtma mavjudligi bazada ko'rinadi.
        var order = new MarkingOrder
        {
            OrganizationId = organizationId,
            BusinessPlaceId = businessPlace.Id,
            ProductId = product.Id,
            Gtin = product.Gtin,
            Quantity = request.Quantity,
            Status = "PENDING",
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.BeginAsync(ct);
        try
        {
            _context.MarkingOrders.Add(order);
            await _context.SaveChangesAsync(ct);
            await _unitOfWork.CommitAsync(ct);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(ct);
            throw;
        }

        // Tashqi chaqiruv ataylab hech qanday tranzaksiya ichida emas — CRPT tomonda yaratilgan
        // buyurtmani "rollback" qilib bo'lmaydi.
        JsonElement response;
        try
        {
            var payload = new CrptOrderRequest
            {
                ProductGroup = productGroup,
                BusinessPlaceId = crptBusinessPlaceId,
                ReleaseMethodType = ReleaseMethodType,
                Products = [new CrptOrderProduct
                {
                    Gtin = product.Gtin,
                    Quantity = request.Quantity,
                    SerialNumberType = SerialNumberType,
                    CisType = CisType
                }]
            };

            response = await SendJsonAsync(HttpMethod.Post, "api/orders", payload, ct);
        }
        catch (Exception ex)
        {
            // CRPT so'rovi bajarilmadi (masalan MK emitent bloki). Aniq xato: operator buni ko'rishi kerak.
            await MarkFailedAsync(order, organizationId, request.IdempotencyKey, ex.Message);
            throw;
        }

        var crptOrderId = response.TryGetProperty("orderId", out var orderIdProperty) && orderIdProperty.ValueKind == JsonValueKind.String
            ? orderIdProperty.GetString()
            : null;

        if (string.IsNullOrWhiteSpace(crptOrderId))
        {
            const string message = "CRPT order response did not include an orderId.";
            await MarkFailedAsync(order, organizationId, request.IdempotencyKey, message);
            throw new IntegrationHttpException(message, StatusCodes.Status502BadGateway);
        }

        // TRANZAKSIYA 2 — natija yozuvi (crpt_order_id, status), idempotentlik yakuni va audit
        // log, alohida commit.
        try
        {
            await _unitOfWork.BeginAsync(CancellationToken.None);

            order.CrptOrderId = crptOrderId;
            order.Status = "SUBMITTED";

            _context.MarkingAslBelgiDocuments.Add(new MarkingAslBelgiDocument
            {
                OrganizationId = organizationId,
                OperationType = OrderOperationType,
                InternalDocumentType = "marking_order",
                InternalDocumentId = order.Id,
                ProviderDocumentId = crptOrderId,
                Status = "SUBMITTED",
                CreatedAt = DateTime.UtcNow
            });

            await CompleteIdempotencyAsync(organizationId, request.IdempotencyKey, crptOrderId, CancellationToken.None);
            await _context.SaveChangesAsync(CancellationToken.None);

            _auditLogService.SetNewValues(new { order.Id, order.CrptOrderId, order.Status });
            await _auditLogService.CreateAsync("marking_order", order.Id.ToString(), AuditLogOperationTypeConst.Create);

            await _unitOfWork.CommitAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            await TryRollbackAsync();
            // CRPT tomonda buyurtma allaqachon yaratilgan (crptOrderId bor) — bu holatni orqaga
            // qaytarib bo'lmaydi. Idempotentlik yozuvi FAILED + resultDocumentId bilan qoldiriladi
            // (jimgina yo'qotish o'rniga qo'lda tiklashga majburlaydi — xuddi eski
            // AslBelgiTransferService.HandleRemoteSuccessLocalFailureAsync naqshi).
            await HandleRemoteSuccessLocalFailureAsync(organizationId, request.IdempotencyKey, crptOrderId, ex, order.Id);
            throw;
        }

        return new MarkingOrderCreateResultDto { OrderId = order.Id, CrptOrderId = order.CrptOrderId, Status = order.Status, IsReplay = false };
    }

    // §4.4 (GET /api/codes) — "Ost-buyurtmadan MC larni yuklab olish". Yo'l tanlovi: §4.5
    // (/codes/packs) EMAS, chunki u faqat allaqachon yuklab olingan PAKETLAR ro'yxatini
    // qaytaradi (packs[]), haqiqiy kodlarni emas. §4.5 ning o'zi PDF ichida ikki xil yozilgan
    // (K1, hal qilinmagan holda qoladi — /codes/packs yoki /api/codes/packs), lekin bu bosqichda
    // ishlatilmaydi, shuning uchun K1 amaliy jihatdan bu yerga ta'sir qilmaydi. §4.4 ning o'z yo'li
    // (`/api/codes`) hujjat ichida ziddiyatsiz — jadvalda ham, majburiy parametrlar ro'yxatida ham
    // (orderId, gtin, quantity — UCHALASI HAM majburiy, §11-X-07) bir xil yozilgan.
    //
    // Ma'lum cheklov: §4.4 paketlab (pack) qaytaradi va "keyingi paketni olish uchun lastPackId
    // ko'rsatish shart, aks holda birinchi paket qayta yuklanadi" (INT_ASLBELGI.md, ochiq band).
    // marking_order da lastPackId saqlaydigan ustun yo'q, shuning uchun bu metod faqat BIRINCHI
    // paketni oladi — agar buyurtma bir nechta paketga bo'lingan bo'lsa, qolganlari olinmaydi.
    //
    // Yana bir taxmin: "Ost-buyurtma" atamasi CRPT'da alohida ID (§4.3 sub-orders) bilan farqlanishi
    // mumkin, lekin bu hujjatda aniqlashtirilmagan va vazifa qamrovida emas — shuning uchun
    // marking_order.crpt_order_id to'g'ridan-to'g'ri `orderId` query parametriga uzatiladi.
    public async Task<MarkingOrderFetchCodesResultDto> FetchCodesAsync(long orderId, CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();

        var order = await _context.MarkingOrders.SingleOrDefaultAsync(o => o.Id == orderId && o.OrganizationId == organizationId, ct)
            ?? throw new InvalidOperationException("The specified marking order was not found in the current organization.");

        if (order.Status != "SUBMITTED")
            throw new InvalidOperationException(
                $"MarkingOrder {orderId} is in status '{order.Status}'; codes can only be fetched for an order in 'SUBMITTED' status.");

        var path = $"api/codes?orderId={Uri.EscapeDataString(order.CrptOrderId ?? string.Empty)}&gtin={Uri.EscapeDataString(order.Gtin)}&quantity={order.Quantity}";

        JsonElement response;
        try
        {
            response = await SendJsonAsync(HttpMethod.Get, path, body: null, ct);
        }
        catch (Exception ex)
        {
            // Fetch — qayta urinsa bo'ladigan operatsiya (order o'zi hali SUBMITTED holida
            // qoladi, FAILED qilinmaydi). Faqat urinish muvaffaqiyatsizligi audit sifatida
            // yoziladi va aniq exception operatorga qaytariladi.
            await RecordFetchFailureAsync(order, organizationId, ex.Message);
            throw;
        }

        var codes = ExtractCodes(response);

        if (codes.Count == 0)
        {
            // CRPT hali kod tayyorlamagan. Hujjatda bu holat uchun aniq xato kodi topilmadi —
            // shuning uchun bo'sh codes[] "hali tayyor emas" holati sifatida talqin qilinadi
            // (xato emas, oddiy oraliq holat).
            return new MarkingOrderFetchCodesResultDto
            {
                OrderId = order.Id,
                OrderStatus = order.Status,
                IsReady = false,
                CodesFetched = 0
            };
        }

        var packId = ExtractPackId(response);
        var warehouseId = await ResolveSingleWarehouseIdAsync(order.BusinessPlaceId, organizationId, CancellationToken.None);

        try
        {
            await _unitOfWork.BeginAsync(CancellationToken.None);

            var inserted = 0;
            foreach (var rawCode in codes)
            {
                if (!MarkingCodeParser.TryParse(rawCode, out var parts) || parts is null)
                {
                    _logger.LogWarning(
                        "Skipping a code returned by CRPT for MarkingOrder {OrderId}: it did not match the expected format (length {Length}).",
                        order.Id, rawCode.Length);
                    continue;
                }

                var existing = await _context.MarkingCodes.SingleOrDefaultAsync(
                    m => m.OrganizationId == organizationId && m.Gtin == parts.Gtin && m.SerialNumber == parts.SerialNumber,
                    CancellationToken.None);

                if (existing is not null)
                {
                    existing.OrderId = order.Id;
                    if (existing.Status != "in_circulation")
                    {
                        existing.Status = "in_circulation";
                        existing.UpdatedAt = DateTime.UtcNow;
                    }
                    continue;
                }

                _context.MarkingCodes.Add(new MarkingCode
                {
                    OrganizationId = organizationId,
                    ProductId = order.ProductId,
                    Gtin = parts.Gtin,
                    SerialNumber = parts.SerialNumber,
                    CheckKey = parts.CheckKey,
                    CheckCode = parts.CheckCode,
                    Status = "in_circulation",
                    OrderId = order.Id,
                    WarehouseId = warehouseId,
                    CreatedAt = DateTime.UtcNow
                });
                inserted++;
            }

            order.Status = "COMPLETED";

            _context.MarkingAslBelgiDocuments.Add(new MarkingAslBelgiDocument
            {
                OrganizationId = organizationId,
                OperationType = OrderOperationType,
                InternalDocumentType = "marking_order",
                InternalDocumentId = order.Id,
                ProviderDocumentId = packId,
                Status = "COMPLETED",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(CancellationToken.None);

            _auditLogService.SetNewValues(new { order.Id, order.Status, CodesFetched = inserted });
            await _auditLogService.CreateAsync("marking_order", order.Id.ToString(), AuditLogOperationTypeConst.Update, "CRPT codes fetched");

            await _unitOfWork.CommitAsync(CancellationToken.None);

            return new MarkingOrderFetchCodesResultDto
            {
                OrderId = order.Id,
                OrderStatus = order.Status,
                IsReady = true,
                CodesFetched = inserted
            };
        }
        catch (Exception ex)
        {
            await TryRollbackAsync();
            // CRPT allaqachon kodlarni qaytargan — bu holatni orqaga qaytarib bo'lmaydi
            // (kodlar CRPT tomonda "yetkazilgan" hisoblanadi). Qo'lda tekshirish kerak.
            _logger.LogCritical(
                ex,
                "CRPT returned {Count} codes for MarkingOrder {OrderId} but local persistence failed. Manual reconciliation required — codes may need to be re-fetched.",
                codes.Count,
                order.Id);
            throw;
        }
    }

    private static List<string> ExtractCodes(JsonElement response)
    {
        if (!response.TryGetProperty("codes", out var codesProperty) || codesProperty.ValueKind != JsonValueKind.Array)
            return [];

        var result = new List<string>();
        foreach (var item in codesProperty.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String)
                continue;

            var value = item.GetString();
            if (!string.IsNullOrWhiteSpace(value))
                result.Add(value);
        }

        return result;
    }

    private static string? ExtractPackId(JsonElement response)
    {
        if (!response.TryGetProperty("packId", out var packIdProperty))
            return null;

        return packIdProperty.ValueKind switch
        {
            JsonValueKind.String => packIdProperty.GetString(),
            JsonValueKind.Number => packIdProperty.GetRawText(),
            _ => null
        };
    }

    private async Task<int?> ResolveSingleWarehouseIdAsync(int businessPlaceId, int organizationId, CancellationToken ct)
    {
        var warehouseIds = await _context.Warehouses
            .Where(w => w.OrganizationId == organizationId && w.BusinessPlaceId == businessPlaceId)
            .Select(w => w.Id)
            .ToListAsync(ct);

        // Bir nechta ombor shu biznes-joyga bog'langan bo'lsa, qaysi birini tanlash noaniq —
        // bunday holda warehouse_id bo'sh qoldiriladi (majburiy emas).
        return warehouseIds.Count == 1 ? warehouseIds[0] : null;
    }

    private async Task<MarkingBusinessPlace> ResolveBusinessPlaceAsync(MarkingOrderCreateRequestDto request, int organizationId, CancellationToken ct)
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

    private async Task RecordFetchFailureAsync(MarkingOrder order, int organizationId, string errorMessage)
    {
        try
        {
            await _unitOfWork.BeginAsync(CancellationToken.None);

            _context.MarkingAslBelgiDocuments.Add(new MarkingAslBelgiDocument
            {
                OrganizationId = organizationId,
                OperationType = OrderOperationType,
                InternalDocumentType = "marking_order",
                InternalDocumentId = order.Id,
                Status = "FAILED",
                ErrorMessage = errorMessage,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(CancellationToken.None);
            await _unitOfWork.CommitAsync(CancellationToken.None);
        }
        catch (Exception compensationEx)
        {
            await TryRollbackAsync();
            _logger.LogCritical(
                compensationEx,
                "Failed to record a failed code-fetch attempt for MarkingOrder {OrderId}. Original CRPT error: {OriginalError}",
                order.Id,
                errorMessage);
        }
    }

    private async Task MarkFailedAsync(MarkingOrder order, int organizationId, string idempotencyKey, string errorMessage)
    {
        try
        {
            await _unitOfWork.BeginAsync(CancellationToken.None);

            order.Status = "FAILED";

            _context.MarkingAslBelgiDocuments.Add(new MarkingAslBelgiDocument
            {
                OrganizationId = organizationId,
                OperationType = OrderOperationType,
                InternalDocumentType = "marking_order",
                InternalDocumentId = order.Id,
                Status = "FAILED",
                ErrorMessage = errorMessage,
                CreatedAt = DateTime.UtcNow
            });

            // Idempotentlik yozuvi ham FAILED qilinadi (bitta tranzaksiyada, marking_order bilan
            // birga) — shu kalit bilan keyingi urinishga ruxsat berish uchun.
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
                "Failed to record FAILED status for MarkingOrder {OrderId}. Original CRPT error: {OriginalError}",
                order.Id,
                errorMessage);
        }
    }

    // --- Idempotentlik: AslBelgiTransferService (o'chirilgan, 2C.5) dagi naqsh qayta ishlatildi ---

    private static string ComputeRequestHash(MarkingOrderCreateRequestDto request) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
            $"{IdempotencyOperationType}|{request.ProductId}|{request.Quantity}|{request.BusinessPlaceId}|{request.WarehouseId}")));

    private async Task<MarkingOrderCreateResultDto?> TryCreateIdempotencyRecordAsync(int organizationId, string key, string hash, CancellationToken ct)
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
                // Concurrent so'rov bir vaqtda xuddi shu kalit bilan yozuv yaratgan bo'lishi
                // mumkin (uidx_idempotency_record_org_key) — qayta o'qib chiqamiz.
                _context.ChangeTracker.Clear();
                existing = await _context.IdempotencyRecords.SingleAsync(x => x.IdempotencyKey == key, ct);
            }
        }

        if (existing.OperationType != IdempotencyOperationType || existing.RequestHash != hash)
            throw new InvalidOperationException("The idempotency key has already been used for a different request.");

        if (existing.Status == "COMPLETED" && !string.IsNullOrWhiteSpace(existing.ResultDocumentId))
        {
            var existingOrder = await _context.MarkingOrders.SingleOrDefaultAsync(
                o => o.OrganizationId == organizationId && o.CrptOrderId == existing.ResultDocumentId, ct);

            return new MarkingOrderCreateResultDto
            {
                OrderId = existingOrder?.Id ?? 0,
                CrptOrderId = existing.ResultDocumentId,
                Status = existingOrder?.Status ?? "SUBMITTED",
                IsReplay = true
            };
        }

        if (existing.Status == "FAILED")
        {
            // Qayta urinishga ruxsat: yozuv PENDING ga qaytariladi, xuddi yangi urinish kabi
            // davom etadi (eski AslBelgiTransferService farqli o'laroq — u FAILED holatda
            // doim rad etar edi; bu yerda operatordan qo'lda aralashuv talab qilinmaydi).
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
            "CRPT order succeeded remotely (CrptOrderId={CrptOrderId}) but local status update failed for MarkingOrder {OrderId}. Manual reconciliation required.",
            crptResultId,
            localId);

        try
        {
            // Rollback'dan keyin tracker'da saqlanmagan obyektlar qoladi — tozalanadi.
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
                "Failed to record reconciliation state for CRPT OrderId={CrptOrderId}, IdempotencyKey={IdempotencyKey}.",
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
            _logger.LogError(rollbackEx, "Rollback failed while persisting a marking_order result.");
        }
    }

    private int RequireOrganization() => _userContext.OrganizationId
        ?? throw new InvalidOperationException("An active organization is required for CRPT order operations.");

    private async Task<JsonElement> SendJsonAsync(HttpMethod method, string path, object? body, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, path);
        if (body is not null)
            request.Content = JsonContent.Create(body, options: JsonOptions);

        using var response = await SendAsync(request, ct);
        await EnsureSuccessStatusOrThrowAsync(response);
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        return document.RootElement.Clone();
    }

    private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
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
            var detail = SensitiveDataRedactor.Redact(await response.Content.ReadAsStringAsync(), _apiKey);
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

    private sealed class CrptOrderRequest
    {
        public string ProductGroup { get; init; } = string.Empty;
        public int BusinessPlaceId { get; init; }
        public string ReleaseMethodType { get; init; } = string.Empty;
        public List<CrptOrderProduct> Products { get; init; } = [];
    }

    private sealed class CrptOrderProduct
    {
        public string Gtin { get; init; } = string.Empty;
        public int Quantity { get; init; }
        public string SerialNumberType { get; init; } = string.Empty;
        public string CisType { get; init; } = string.Empty;
    }
}
