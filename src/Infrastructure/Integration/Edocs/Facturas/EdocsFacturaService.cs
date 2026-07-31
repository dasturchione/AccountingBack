using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.AuditLogs;
using Application.Features.Integration.AslBelgi.Parsing;
using Application.Features.Integration.Edocs.Facturas;
using Domain.Entities;
using Infrastructure.Persistence;
using Integration.Edocs.Configs;
using Integration.Edocs.Http;
using Integration.Shared.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Exceptions;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Integration.Edocs.Facturas;

public sealed class EdocsFacturaService : IEdocsFacturaService
{
    private const string OperationType = "factura_create";
    private const string DateFormat = "yyyy-MM-dd";

    // idempotency_record.operation_type — marking_edocs_document.operation_type ("factura_create")
    // dan ATAYLAB alohida (5.8/6-jarayondagi AslBelgi naqshi bilan bir xil).
    private const string IdempotencyOperationType = "EDOCS_FACTURA_CREATE";

    // E-DOCS.pdf ("POST Создание нового документа") namunasi PascalCase kalitlar bilan
    // ("ProductList", "SellerTin", "VatRate" va h.k.) — CRPT integratsiyalaridagi
    // JsonSerializerDefaults.Web (camelCase) BU YERDA ishlatilmaydi.
    private static readonly JsonSerializerOptions JsonOptions = new();

    private readonly AppDbContext _context;
    private readonly IUserContext _userContext;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAuditLogService _auditLogService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EdocsFacturaService> _logger;

    public EdocsFacturaService(
        AppDbContext context,
        IUserContext userContext,
        IHttpClientFactory httpClientFactory,
        IAuditLogService auditLogService,
        IUnitOfWork unitOfWork,
        ILogger<EdocsFacturaService> logger)
    {
        _context = context;
        _userContext = userContext;
        _httpClientFactory = httpClientFactory;
        _auditLogService = auditLogService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<EdocsFacturaCreateResultDto> CreateFacturaDocumentAsync(EdocsFacturaCreateRequestDto request, CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();

        // Idempotentlik tekshiruvi — eng birinchi, boshqa hech qanday validatsiya yoki DB
        // yozuvidan oldin (5.8/AslBelgi naqshi).
        var requestHash = ComputeRequestHash(request);
        var replay = await TryCreateIdempotencyRecordAsync(organizationId, request.IdempotencyKey, requestHash, ct);
        if (replay is not null)
            return replay;

        // Markirovka kodlarini CRPT/Edocs'ga yuborishdan oldin (T1 dan ham oldin) tekshiramiz —
        // band/sotilgan kod bilan so'rov Edocs'ga yetib bormasdan to'xtaydi.
        var markingCodesById = await ResolveMarkingCodesAsync(request, organizationId, ct);

        // TRANZAKSIYA 1 — mahalliy PENDING yozuv, tashqi chaqiruvdan oldin alohida commit qilinadi.
        var document = new MarkingEdocsDocument
        {
            OrganizationId = organizationId,
            OperationType = OperationType,
            InternalDocumentType = request.InternalDocumentType,
            InternalDocumentId = request.InternalDocumentId,
            Status = "PENDING",
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.BeginAsync(ct);
        try
        {
            _context.MarkingEdocsDocuments.Add(document);
            await _context.SaveChangesAsync(ct);
            await _unitOfWork.CommitAsync(ct);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(ct);
            throw;
        }

        // Tashqi chaqiruv ataylab hech qanday tranzaksiya ichida emas — Edocs tomonda
        // yaratilgan hujjatni "rollback" qilib bo'lmaydi.
        JsonElement response;
        try
        {
            var payload = BuildPayload(request, markingCodesById);
            response = await SendJsonAsync(organizationId, HttpMethod.Post, "documents/factura", payload, ct);
        }
        catch (Exception ex)
        {
            await MarkFailedAsync(document, organizationId, request.IdempotencyKey, ex.Message);
            throw;
        }

        var providerDocumentId = ExtractDocumentId(response);
        if (string.IsNullOrWhiteSpace(providerDocumentId))
        {
            const string message = "Edocs factura creation response did not include a document id.";
            await MarkFailedAsync(document, organizationId, request.IdempotencyKey, message);
            throw new IntegrationHttpException(message, StatusCodes.Status502BadGateway);
        }

        // TRANZAKSIYA 2 — natija yozuvi, idempotentlik yakuni, audit log va ishlatilgan
        // marking_code larni "sold" ga o'tkazish, alohida commit.
        try
        {
            await _unitOfWork.BeginAsync(CancellationToken.None);

            document.Status = "SUBMITTED";
            document.ProviderDocumentId = providerDocumentId;
            document.UpdatedAt = DateTime.UtcNow;

            foreach (var code in markingCodesById.Values)
            {
                code.Status = "sold";
                code.UpdatedAt = DateTime.UtcNow;
            }

            await CompleteIdempotencyAsync(organizationId, request.IdempotencyKey, providerDocumentId, CancellationToken.None);
            await _context.SaveChangesAsync(CancellationToken.None);

            _auditLogService.SetNewValues(new { document.Id, document.ProviderDocumentId, document.Status });
            await _auditLogService.CreateAsync("marking_edocs_document", document.Id.ToString(), AuditLogOperationTypeConst.Create);

            await _unitOfWork.CommitAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            await TryRollbackAsync();
            // Edocs tomonda hujjat allaqachon yaratilgan (providerDocumentId bor) — bu holatni
            // orqaga qaytarib bo'lmaydi. Idempotentlik yozuvi FAILED + resultDocumentId bilan
            // qoldiriladi (qo'lda tiklashga majburlaydi — AslBelgi naqshi bilan bir xil).
            await HandleRemoteSuccessLocalFailureAsync(organizationId, request.IdempotencyKey, providerDocumentId, ex, document.Id);
            throw;
        }

        return new EdocsFacturaCreateResultDto
        {
            MarkingEdocsDocumentId = document.Id,
            ProviderDocumentId = document.ProviderDocumentId,
            Status = document.Status,
            IsReplay = false
        };
    }

    // ALOHIDA idempotency_record ATAYLAB ishlatilmaydi — bu operatsiya (create'dan farqli
    // o'laroq) yangi tashqi resurs yaratmaydi, allaqachon mavjud hujjatning holatini
    // o'zgartiradi. Mahalliy himoya ikki qavatli: (1) pastdagi status=SUBMITTED tekshiruvi —
    // hujjat allaqachon imzolangan/muvaffaqiyatsiz bo'lsa, qayta chaqiruv T1'ga yetib bormasdan
    // rad etiladi; (2) Edocs'ning o'z holat mashinasi (E-DOCS.pdf: "Может быть выполнено для
    // статуса drafts и sended... Из sended переводит документ в статус signed, при условии
    // что все получатели подписали") — noto'g'ri holatdan qayta imzolashga urinishni o'zi
    // rad etadi. 6.2-bosqichdagi /auth/complete ham xuddi shu sabab bilan idempotency_record
    // ishlatmagan edi (token almashish — yangi resurs yaratish emas).
    public async Task<EdocsFacturaSignResultDto> SignFacturaDocumentAsync(EdocsFacturaSignRequestDto request, CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();

        var document = await _context.MarkingEdocsDocuments.SingleOrDefaultAsync(
            d => d.Id == request.MarkingEdocsDocumentId && d.OrganizationId == organizationId, ct)
            ?? throw new InvalidOperationException("The specified marking_edocs_document was not found in the current organization.");

        if (document.Status != "SUBMITTED")
            throw new InvalidOperationException(
                $"marking_edocs_document {document.Id} is in status '{document.Status}'; it can only be signed while status is " +
                "'SUBMITTED' (it has either not been successfully created yet, or has already been signed/failed).");

        if (string.IsNullOrWhiteSpace(document.ProviderDocumentId))
            throw new InvalidOperationException($"marking_edocs_document {document.Id} has no provider_document_id — it was not successfully submitted to Edocs.");

        try
        {
            var payload = new EdocsSignPayload { Pkcs7 = request.Pkcs7 };
            await SendSignRequestAsync(organizationId, $"documents/factura/{Uri.EscapeDataString(document.ProviderDocumentId)}/sign", payload, ct);
        }
        catch (Exception ex)
        {
            await RecordSignFailureAsync(document, ex.Message);
            throw;
        }

        try
        {
            await _unitOfWork.BeginAsync(CancellationToken.None);

            // TASDIQLANMAGAN: Edocs'ning holat mashinasi bo'yicha bu chaqiruv drafts→sended
            // (birinchi tomon) YOKI sended→signed (barcha qabul qiluvchilar imzolagach)
            // bo'lishi mumkin — javob buni ajratib bermaydi (PDF'da namuna yo'q). Soddalik
            // uchun har ikkala holatda ham mahalliy "SIGNED" qo'yiladi; agar keyingi bosqichda
            // Edocs javobi haqiqiy holatni ("sended" vs "signed") qaytarishi tasdiqlansa, bu
            // yerga aniqlashtirish qo'shilishi kerak.
            document.Status = "SIGNED";
            document.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(CancellationToken.None);

            _auditLogService.SetNewValues(new { document.Id, document.Status });
            await _auditLogService.CreateAsync("marking_edocs_document", document.Id.ToString(), AuditLogOperationTypeConst.Update, "Edocs factura signed");

            await _unitOfWork.CommitAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            await TryRollbackAsync();
            _logger.LogCritical(
                ex,
                "Edocs sign succeeded remotely for MarkingEdocsDocument {DocumentId} (ProviderDocumentId={ProviderDocumentId}) but local status update failed. Manual reconciliation required.",
                document.Id,
                document.ProviderDocumentId);
            throw;
        }

        return new EdocsFacturaSignResultDto { MarkingEdocsDocumentId = document.Id, Status = document.Status };
    }

    private async Task RecordSignFailureAsync(MarkingEdocsDocument document, string errorMessage)
    {
        try
        {
            await _unitOfWork.BeginAsync(CancellationToken.None);

            document.ErrorMessage = errorMessage;
            document.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(CancellationToken.None);
            await _unitOfWork.CommitAsync(CancellationToken.None);
        }
        catch (Exception compensationEx)
        {
            await TryRollbackAsync();
            _logger.LogCritical(
                compensationEx,
                "Failed to record a failed sign attempt for MarkingEdocsDocument {DocumentId}. Original Edocs error: {OriginalError}",
                document.Id,
                errorMessage);
        }
    }

    private async Task<Dictionary<long, MarkingCode>> ResolveMarkingCodesAsync(EdocsFacturaCreateRequestDto request, int organizationId, CancellationToken ct)
    {
        var ids = request.Products.SelectMany(p => p.MarkingCodeIds).Distinct().ToList();
        if (ids.Count == 0)
            return [];

        var codes = await _context.MarkingCodes
            .Where(m => m.OrganizationId == organizationId && ids.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, ct);

        foreach (var id in ids)
        {
            if (!codes.TryGetValue(id, out var code))
                throw new InvalidOperationException($"marking_code {id} was not found in the current organization.");

            if (code.Status != "in_circulation")
                throw new InvalidOperationException(
                    $"marking_code {id} ({code.Gtin}/{code.SerialNumber}) is not available for sale — current status '{code.Status}' " +
                    "(it may already be sold or reserved by another document).");
        }

        return codes;
    }

    private static EdocsFacturaPayload BuildPayload(EdocsFacturaCreateRequestDto request, Dictionary<long, MarkingCode> markingCodesById)
    {
        var products = request.Products.Select(p =>
        {
            List<string>? markingCodes = null;
            if (p.MarkingCodeIds.Count > 0)
            {
                markingCodes = p.MarkingCodeIds
                    .Select(id => markingCodesById[id])
                    .Select(code => MarkingCodeParser.Compose(new MarkingCodeParts(code.Gtin, code.SerialNumber, code.CheckKey, code.CheckCode)))
                    .ToList();
            }

            return new EdocsProductPayload
            {
                OrdNo = p.OrdNo,
                Name = p.Name,
                MeasureId = p.MeasureId,
                Count = p.Count,
                Summa = p.Summa,
                VatRate = p.VatRate,
                WithoutVat = p.WithoutVat,
                HasVat = p.HasVat,
                MarkingCodes = markingCodes
            };
        }).ToList();

        return new EdocsFacturaPayload
        {
            ProductList = new EdocsProductListPayload { Products = products },
            SellerTin = request.SellerTin,
            BuyerTin = request.BuyerTin,
            FacturaDoc = new EdocsFacturaDocPayload
            {
                FacturaNo = request.FacturaNo,
                FacturaDate = request.FacturaDate.ToString(DateFormat)
            },
            ContractDoc = request.ContractNo is null
                ? null
                : new EdocsContractDocPayload
                {
                    ContractNo = request.ContractNo,
                    ContractDate = request.ContractDate?.ToString(DateFormat) ?? string.Empty
                },
            Seller = ToPartyPayload(request.Seller),
            Buyer = ToPartyPayload(request.Buyer)
        };
    }

    private static EdocsPartyPayload ToPartyPayload(EdocsFacturaPartyDto party) => new()
    {
        BankId = party.BankId,
        WorkPhone = party.WorkPhone,
        Name = party.Name,
        Account = party.Account,
        Address = party.Address,
        DistrictId = party.DistrictId,
        Mobile = party.Mobile,
        Accountant = party.Accountant,
        Director = party.Director,
        Oked = party.Oked,
        VatRegCode = party.VatRegCode
    };

    // TASDIQLANMAGAN (6.4-bosqich) — E-DOCS.pdf "POST Создание нового документа" bo'limida
    // yaratilgan hujjatning javob JSON tuzilishi ko'rsatilmagan (faqat so'rov namunasi bor).
    // Eng ehtimolli maydon nomlari navbat bilan sinaladi: "_id" (boshqa GET javoblarida
    // MongoDB uslubidagi ID sifatida ko'rinadi, masalan documents/actWorkPerformed/{id}),
    // keyin "id", keyin "FacturaId" (mavjud hujjat namunasida shu nom bilan uchraydi).
    private static string? ExtractDocumentId(JsonElement response)
    {
        foreach (var propertyName in new[] { "_id", "id", "FacturaId" })
        {
            if (response.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String)
            {
                var value = property.GetString();
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }
        }

        return null;
    }

    private async Task MarkFailedAsync(MarkingEdocsDocument document, int organizationId, string idempotencyKey, string errorMessage)
    {
        try
        {
            await _unitOfWork.BeginAsync(CancellationToken.None);

            document.Status = "FAILED";
            document.ErrorMessage = errorMessage;
            document.UpdatedAt = DateTime.UtcNow;

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
                "Failed to record FAILED status for MarkingEdocsDocument {DocumentId}. Original Edocs error: {OriginalError}",
                document.Id,
                errorMessage);
        }
    }

    // --- Idempotentlik: AslBelgi (5.8-bosqich) dagi naqsh qayta ishlatildi ---

    private static string ComputeRequestHash(EdocsFacturaCreateRequestDto request)
    {
        var productsPart = string.Join(";", request.Products.Select(p =>
            $"{p.OrdNo}|{p.Name}|{p.Count}|{p.Summa}|{p.VatRate}|{p.WithoutVat}|{p.HasVat}|{string.Join(",", p.MarkingCodeIds.OrderBy(x => x))}"));

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
            $"{IdempotencyOperationType}|{request.InternalDocumentType}|{request.InternalDocumentId}|{request.SellerTin}|{request.BuyerTin}|{request.FacturaNo}|{request.FacturaDate:O}|{productsPart}")));
    }

    private async Task<EdocsFacturaCreateResultDto?> TryCreateIdempotencyRecordAsync(int organizationId, string key, string hash, CancellationToken ct)
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
            var existingDocument = await _context.MarkingEdocsDocuments.SingleOrDefaultAsync(
                d => d.OrganizationId == organizationId && d.ProviderDocumentId == existing.ResultDocumentId, ct);

            return new EdocsFacturaCreateResultDto
            {
                MarkingEdocsDocumentId = existingDocument?.Id ?? 0,
                ProviderDocumentId = existing.ResultDocumentId,
                Status = existingDocument?.Status ?? "SUBMITTED",
                IsReplay = true
            };
        }

        if (existing.Status == "FAILED")
        {
            // Qayta urinishga ruxsat: yozuv PENDING ga qaytariladi, xuddi yangi urinish kabi
            // davom etadi.
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

    private async Task HandleRemoteSuccessLocalFailureAsync(int organizationId, string idempotencyKey, string providerDocumentId, Exception ex, long localId)
    {
        _logger.LogCritical(
            ex,
            "Edocs factura creation succeeded remotely (ProviderDocumentId={ProviderDocumentId}) but local status update failed for MarkingEdocsDocument {DocumentId}. Manual reconciliation required.",
            providerDocumentId,
            localId);

        try
        {
            _context.ChangeTracker.Clear();

            var record = await _context.IdempotencyRecords.SingleOrDefaultAsync(
                x => x.OrganizationId == organizationId && x.IdempotencyKey == idempotencyKey, CancellationToken.None);
            if (record is null)
                return;

            record.Status = "FAILED";
            record.ResultDocumentId = providerDocumentId;
            record.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception compensationEx)
        {
            _logger.LogCritical(
                compensationEx,
                "Failed to record reconciliation state for Edocs ProviderDocumentId={ProviderDocumentId}, IdempotencyKey={IdempotencyKey}.",
                providerDocumentId,
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
            _logger.LogError(rollbackEx, "Rollback failed while persisting a marking_edocs_document result.");
        }
    }

    private int RequireOrganization() => _userContext.OrganizationId
        ?? throw new InvalidOperationException("An active organization is required for Edocs factura operations.");

    private async Task<JsonElement> SendJsonAsync(int organizationId, HttpMethod method, string path, object body, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, path)
        {
            Content = JsonContent.Create(body, options: JsonOptions)
        };

        using var response = await SendAsync(organizationId, request, ct);
        await EnsureSuccessStatusOrThrowAsync(response);
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        return document.RootElement.Clone();
    }

    // Imzolash javobining JSON shakli E-DOCS.pdf'da ko'rsatilmagan (faqat so'rov namunasi
    // bor) — shuning uchun bu chaqiruv javob tanasini majburan JSON sifatida tahlil
    // qilmaydi, faqat HTTP status muvaffaqiyatli ekanligini tekshiradi.
    private async Task SendSignRequestAsync(int organizationId, string path, object body, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(body, options: JsonOptions)
        };

        using var response = await SendAsync(organizationId, request, ct);
        await EnsureSuccessStatusOrThrowAsync(response);
    }

    private async Task<HttpResponseMessage> SendAsync(int organizationId, HttpRequestMessage request, CancellationToken ct)
    {
        // x-product/x-partner sarlavhalari endi EdocsAuthorizationHandler ichida,
        // markazlashgan tarzda qo'shiladi (6.5.7-bosqich) — bu yerda emas.
        request.Options.Set(IntegrationHttpRequestOptions.OrganizationId, organizationId);

        try
        {
            var client = _httpClientFactory.CreateClient(EdocsHttpClientNames.Client);
            return await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new IntegrationHttpException("Edocs request timed out.", StatusCodes.Status504GatewayTimeout);
        }
        catch (HttpRequestException)
        {
            throw new IntegrationHttpException("Edocs request could not be completed.", StatusCodes.Status502BadGateway);
        }
    }

    private static async Task EnsureSuccessStatusOrThrowAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        throw response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => new IntegrationUnauthorizedException("Edocs credentials were rejected."),
            HttpStatusCode.Forbidden => new IntegrationForbiddenException("Edocs denied the request."),
            HttpStatusCode.NotFound => new IntegrationHttpException("Edocs endpoint or referenced resource was not found.", 404),
            _ => new IntegrationHttpException(
                $"Edocs request failed with HTTP status {(int)response.StatusCode}.",
                (int)response.StatusCode)
        };
    }

    private sealed class EdocsSignPayload
    {
        public string Pkcs7 { get; init; } = string.Empty;
    }

    private sealed class EdocsFacturaPayload
    {
        public EdocsProductListPayload ProductList { get; init; } = new();
        public string SellerTin { get; init; } = string.Empty;
        public string BuyerTin { get; init; } = string.Empty;
        public EdocsFacturaDocPayload FacturaDoc { get; init; } = new();
        public EdocsContractDocPayload? ContractDoc { get; init; }
        public EdocsPartyPayload Seller { get; init; } = new();
        public EdocsPartyPayload Buyer { get; init; } = new();
    }

    private sealed class EdocsProductListPayload
    {
        public List<EdocsProductPayload> Products { get; init; } = [];
    }

    private sealed class EdocsProductPayload
    {
        public int OrdNo { get; init; }
        public string Name { get; init; } = string.Empty;
        public int MeasureId { get; init; }
        public string Count { get; init; } = string.Empty;
        public string Summa { get; init; } = string.Empty;
        public decimal VatRate { get; init; }
        public bool WithoutVat { get; init; }
        public bool HasVat { get; init; }

        // TASDIQLANMAGAN (6.4-bosqich) — 6.3-bosqichda o'qib chiqilgan E-DOCS.pdf
        // namunasida markirovka kodlari uchun maydon YO'Q (namunadagi hujjat markirovkasiz
        // xizmat edi). Eng yaqin taxmin: "MarkingCodes" — har bir birlik uchun to'liq
        // sntin kod (marking_code.gtin+serial_number+check_key+check_code dan
        // MarkingCodeParser.Compose() bilan qayta yig'ilgan). Haqiqiy kalit nomi Edocs
        // bilan tasdiqlanmaguncha noaniq bo'lib qoladi — birinchi haqiqiy sinovda
        // to'g'rilanishi kerak.
        public List<string>? MarkingCodes { get; init; }
    }

    private sealed class EdocsFacturaDocPayload
    {
        public string FacturaNo { get; init; } = string.Empty;
        public string FacturaDate { get; init; } = string.Empty;
    }

    private sealed class EdocsContractDocPayload
    {
        public string ContractNo { get; init; } = string.Empty;
        public string ContractDate { get; init; } = string.Empty;
    }

    private sealed class EdocsPartyPayload
    {
        public string BankId { get; init; } = string.Empty;
        public string? WorkPhone { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Account { get; init; } = string.Empty;
        public string Address { get; init; } = string.Empty;
        public string DistrictId { get; init; } = string.Empty;
        public string? Mobile { get; init; }
        public string? Accountant { get; init; }
        public string? Director { get; init; }
        public string? Oked { get; init; }
        public string? VatRegCode { get; init; }
    }
}
