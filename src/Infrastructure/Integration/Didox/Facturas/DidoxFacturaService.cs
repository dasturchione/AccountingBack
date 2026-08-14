using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.AuditLogs;
using Application.Features.Integration.AslBelgi.Parsing;
using Application.Features.Integration.Didox.Facturas;
using Domain.Entities;
using Infrastructure.Persistence;
using Integration.Didox.Http;
using Integration.Didox.Services;
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

namespace Integration.Didox.Facturas;

public sealed class DidoxFacturaService : IDidoxFacturaService
{
    private const string OperationType = "factura_create";
    private const string DateFormat = "yyyy-MM-dd";

    // idempotency_record.operation_type — marking_didox_document.operation_type
    // ("factura_create") dan ATAYLAB alohida (Edocs 6.4/AslBelgi 5.8 naqshi bilan bir xil).
    private const string IdempotencyOperationType = "DIDOX_FACTURA";

    // INT_DIDOX.md §4.1 va §10.6 (X-02): rasmiy kod uch xonali — "002". Eski kodda
    // ikki xonali "02" ishlatilgan va bu ANIQ NOTO'G'RI deb qayd etilgan — shu xatoni
    // TAKRORLAMASLIK uchun bu yerda literal "002" ishlatiladi.
    private const string DocType = "002";

    // INT_DIDOX.md §2.3: "locale ⬜ (ru default yoki uz)" — hujjatning o'zi tasdiqlagan
    // standart qiymat, taxmin emas.
    private const string DefaultLocale = "ru";

    // INT_DIDOX.md §5.1: "Version — JSON tuzilma versiyasi. Joriy qiymat: 1".
    private const int SchemaVersion = 1;

    // INT_DIDOX.md §8.1: FacturaType — hisob-faktura turi, 0 = "Стандартный" (standart).
    // Bu bosqichda qattiq kodlangan (AslBelgiOrderService'dagi ReleaseMethodType/
    // SerialNumberType/CisType bilan bir xil yondashuv) — kelajakda boshqa turlar
    // (masalan "Тuzatilgan") kerak bo'lsa, so'rov parametriga aylantirilishi mumkin.
    private const int DefaultFacturaType = 0;

    private static readonly JsonSerializerOptions JsonOptions = new();

    // Sign muvaffaqiyatli o'tdi, lekin GET /v1/documents/{id} javobidan haqiqiy statusni
    // ishonchli o'qib bo'lmadi (maydon topilmadi/kutilmagan shaklda) — TryReadRealStatusAsync
    // izohiga qarang. Foydalanuvchi bilan 7.3-bosqichda kelishilgan: bu holatda xato
    // TASHLANMAYDI (Didox tomonda hujjat allaqachon imzolangan), lekin "SIGNED" ga
    // sukut bo'yicha ham aylanmaydi.
    private const string SignSentStatus = "SIGN_SENT";

    private readonly AppDbContext _context;
    private readonly IUserContext _userContext;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAuditLogService _auditLogService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DidoxFacturaService> _logger;
    private readonly DidoxTimestampClient _timestampClient;

    public DidoxFacturaService(
        AppDbContext context,
        IUserContext userContext,
        IHttpClientFactory httpClientFactory,
        IAuditLogService auditLogService,
        IUnitOfWork unitOfWork,
        ILogger<DidoxFacturaService> logger,
        DidoxTimestampClient timestampClient)
    {
        _context = context;
        _userContext = userContext;
        _httpClientFactory = httpClientFactory;
        _auditLogService = auditLogService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _timestampClient = timestampClient;
    }

    public async Task<DidoxFacturaCreateResultDto> CreateFacturaDocumentAsync(DidoxFacturaCreateRequestDto request, CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();

        // Idempotentlik tekshiruvi — eng birinchi, boshqa hech qanday validatsiya yoki
        // DB yozuvidan oldin (5.8/6.4-bosqich naqshi).
        var requestHash = ComputeRequestHash(request);
        var replay = await TryCreateIdempotencyRecordAsync(organizationId, request.IdempotencyKey, requestHash, ct);
        if (replay is not null)
            return replay;

        // Markirovka kodlarini Didox'ga yuborishdan oldin (T1 dan ham oldin) tekshiramiz —
        // band/sotilgan kod bilan so'rov Didox'ga yetib bormasdan to'xtaydi.
        var markingCodesById = await ResolveMarkingCodesAsync(request, organizationId, ct);

        // TRANZAKSIYA 1 — mahalliy PENDING yozuv, tashqi chaqiruvdan oldin alohida commit qilinadi.
        var document = new MarkingDidoxDocument
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
            _context.MarkingDidoxDocuments.Add(document);
            await _context.SaveChangesAsync(ct);
            await _unitOfWork.CommitAsync(ct);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(ct);
            throw;
        }

        // Tashqi chaqiruv ataylab hech qanday tranzaksiya ichida emas — Didox tomonda
        // yaratilgan hujjatni "rollback" qilib bo'lmaydi.
        JsonElement response;
        try
        {
            var payload = BuildPayload(request, markingCodesById);
            response = await SendJsonAsync(organizationId, HttpMethod.Post, $"v1/documents/{DocType}/create/{DefaultLocale}", payload, ct);
        }
        catch (Exception ex)
        {
            await MarkFailedAsync(document, organizationId, request.IdempotencyKey, ex.Message);
            throw;
        }

        var providerDocumentId = ExtractDocumentId(response);
        if (string.IsNullOrWhiteSpace(providerDocumentId))
        {
            const string message = "Didox factura creation response did not include an _id.";
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
            await _auditLogService.CreateAsync("marking_didox_document", document.Id.ToString(), AuditLogOperationTypeConst.Create);

            await _unitOfWork.CommitAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            await TryRollbackAsync();
            // Didox tomonda hujjat allaqachon yaratilgan (providerDocumentId bor) — bu
            // holatni orqaga qaytarib bo'lmaydi. Idempotentlik yozuvi FAILED + resultDocumentId
            // bilan qoldiriladi (qo'lda tiklashga majburlaydi — Edocs/AslBelgi naqshi bilan bir xil).
            await HandleRemoteSuccessLocalFailureAsync(organizationId, request.IdempotencyKey, providerDocumentId, ex, document.Id);
            throw;
        }

        return new DidoxFacturaCreateResultDto
        {
            MarkingDidoxDocumentId = document.Id,
            ProviderDocumentId = document.ProviderDocumentId,
            Status = document.Status,
            IsReplay = false
        };
    }

    // ============================================================================
    // QISM 1 — Imzolash uchun challenge (INT_DIDOX.md §7.2, 1-4-qadamlar).
    // ============================================================================
    public async Task<DidoxFacturaSignChallengeResultDto> GetSignChallengeAsync(long markingDidoxDocumentId, CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();
        var document = await RequireSubmittedDocumentAsync(markingDidoxDocumentId, organizationId, ct);

        using var request = new HttpRequestMessage(HttpMethod.Get, $"v1/documents/{document.ProviderDocumentId}?owner=1");
        using var response = await SendAsync(organizationId, request, ct);
        await EnsureSuccessStatusOrThrowAsync(response);

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var responseDocument = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

        // INT_DIDOX.md §7.2, 3-qadam: "GET /v1/documents/{id}?owner=1 javobidan data.json" —
        // BU MAYDON TASDIQLANGAN (taxmin emas, hujjatda so'zma-so'z shunday yozilgan).
        // Nuqtali yozuv ("data.json") shu hujjatning o'zida ishlatilgan ichma-ich yo'l
        // konvensiyasi (masalan §5.1 dagi "pending_document.document_json" bilan bir xil) —
        // demak response.data.json (ichma-ich obyekt), literal "data.json" nomli maydon emas.
        if (!responseDocument.RootElement.TryGetProperty("data", out var dataProperty) ||
            !dataProperty.TryGetProperty("json", out var jsonProperty))
        {
            throw new IntegrationHttpException(
                $"Didox GET /v1/documents/{document.ProviderDocumentId}?owner=1 response did not contain data.json.",
                StatusCodes.Status502BadGateway);
        }

        // "data.json" JSON-satr (allaqachon serializatsiya qilingan matn) yoki JSON-obyekt
        // sifatida qaytishi mumkin — 4-qadam ("base64 ga o'girish") ikkala holatda ham
        // shu QIYMATga nisbatan bajariladi.
        var content = jsonProperty.ValueKind == JsonValueKind.String
            ? jsonProperty.GetString() ?? string.Empty
            : jsonProperty.GetRawText();

        return new DidoxFacturaSignChallengeResultDto
        {
            MarkingDidoxDocumentId = document.Id,
            DataBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(content))
        };
    }

    // ============================================================================
    // QISM 2 — Imzolashni yakunlash (INT_DIDOX.md §7.2, 5-7-qadamlar).
    // idempotency_record ATAYLAB ishlatilmaydi: bu holat-o'tishi (state transition),
    // resurs yaratish emas — mahalliy SUBMITTED tekshiruvi + Didox'ning o'z holat
    // mashinasi qayta yuborishlardan yetarlicha himoya qiladi (Edocs 6.5 bilan bir xil
    // asos).
    // ============================================================================
    public async Task<DidoxFacturaSignResultDto> SignFacturaDocumentAsync(DidoxFacturaSignRequestDto request, CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();
        var document = await RequireSubmittedDocumentAsync(request.MarkingDidoxDocumentId, organizationId, ct);

        string timeStampTokenB64;
        try
        {
            // Imzolash rejimi — tashkilot allaqachon autentifikatsiyadan o'tgan,
            // INT_DIDOX.md §2.1 bo'yicha user-key ham yuboriladi (8-bosqich,
            // PROCESS7_AUDIT #3 tuzatishi — login rejimidan ATAYLAB ajratilgan).
            timeStampTokenB64 = await _timestampClient.GetTimeStampTokenForSigningAsync(organizationId, request.Pkcs7, request.SignatureHex, ct);
        }
        catch (Exception ex)
        {
            await MarkSignFailedAsync(document, ex.Message);
            throw;
        }

        try
        {
            await SendJsonAsync(organizationId, HttpMethod.Post, $"v1/documents/{document.ProviderDocumentId}/sign",
                new { signature = timeStampTokenB64 }, ct);
        }
        catch (Exception ex)
        {
            await MarkSignFailedAsync(document, ex.Message);
            throw;
        }

        // Bu nuqtadan boshlab Didox tomonda hujjat ALLAQACHON imzolangan — qolgan
        // xatolar "remote success / local failure" holati (5/6-jarayonlardagi
        // HandleRemoteSuccessLocalFailureAsync bilan bir xil falsafa): hech narsa
        // qayta yuqoriga otilmaydi, faqat loglanadi va SIGN_SENT bilan qoldiriladi.
        var (localStatus, didoxStatusCode, diagnostics) = await TryReadRealStatusAsync(organizationId, document.ProviderDocumentId!, ct);

        try
        {
            await _unitOfWork.BeginAsync(CancellationToken.None);

            document.Status = localStatus;
            document.ErrorMessage = diagnostics;
            document.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(CancellationToken.None);

            _auditLogService.SetNewValues(new { document.Id, document.ProviderDocumentId, document.Status, DidoxStatusCode = didoxStatusCode });
            await _auditLogService.CreateAsync("marking_didox_document", document.Id.ToString(), AuditLogOperationTypeConst.Update);

            await _unitOfWork.CommitAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            await TryRollbackAsync();
            _logger.LogCritical(
                ex,
                "Didox sign succeeded remotely for MarkingDidoxDocument {DocumentId} (ProviderDocumentId={ProviderDocumentId}) but local status update failed. Manual reconciliation required.",
                document.Id,
                document.ProviderDocumentId);
            throw;
        }

        return new DidoxFacturaSignResultDto
        {
            MarkingDidoxDocumentId = document.Id,
            Status = document.Status,
            DidoxStatusCode = didoxStatusCode
        };
    }

    private async Task<MarkingDidoxDocument> RequireSubmittedDocumentAsync(long markingDidoxDocumentId, int organizationId, CancellationToken ct)
    {
        var document = await _context.MarkingDidoxDocuments.SingleOrDefaultAsync(
            d => d.Id == markingDidoxDocumentId && d.OrganizationId == organizationId, ct)
            ?? throw new InvalidOperationException($"marking_didox_document {markingDidoxDocumentId} was not found in the current organization.");

        if (document.Status != "SUBMITTED")
            throw new InvalidOperationException(
                $"marking_didox_document {document.Id} is not in SUBMITTED status (current: '{document.Status}') — signing is not allowed.");

        if (string.IsNullOrWhiteSpace(document.ProviderDocumentId))
            throw new InvalidOperationException($"marking_didox_document {document.Id} has no ProviderDocumentId.");

        return document;
    }

    private async Task MarkSignFailedAsync(MarkingDidoxDocument document, string errorMessage)
    {
        try
        {
            await _unitOfWork.BeginAsync(CancellationToken.None);

            // Status ATAYLAB o'zgartirilmaydi (SUBMITTED holida qoladi) — imzolash
            // muvaffaqiyatsiz tugadi, hujjat qayta imzolanishi mumkin bo'lishi kerak.
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
                "Failed to record sign failure for MarkingDidoxDocument {DocumentId}. Original Didox error: {OriginalError}",
                document.Id,
                errorMessage);
        }
    }

    // ============================================================================
    // Sign muvaffaqiyatli o'tgandan keyin HAQIQIY statusni o'qish.
    //
    // TASDIQLANMAGAN FARAZ: GET /v1/documents/{id} (yagona hujjat) javobi ham
    // GET /v2/documents (ro'yxat) javobidagi bilan bir xil "doc_status" maydonidan
    // foydalanadi deb FARAZ QILINMOQDA. INT_DIDOX.md §3.5 jadvalida GET /v1/documents/{id}
    // qatori faqat "Batafsil ma'lumot" deydi — javob sxemasi hujjatda umuman
    // KO'RSATILMAGAN (faqat ro'yxat elementi uchun, §3.5.2, doc_status bor).
    //
    // Foydalanuvchi bilan 7.3-bosqichda ANIQ kelishilgan qaror (Edocs 6.5 xatosini
    // takrorlamaslik uchun, lekin "remote success / local failure" holatini ham
    // yaratmaslik uchun):
    //   1. doc_status ni birinchi izlaydi;
    //   2. topilsa — MapDidoxStatusToLocal orqali mahalliy statusga o'giradi;
    //   3. topilmasa yoki kutilmagan shaklda bo'lsa — XATO TASHLAMAYDI: LogWarning
    //      yozadi va mahalliy statusni SIGN_SENT ga qo'yadi (SIGNED ga sukut
    //      bo'yicha AYLANMAYDI);
    //   4. provider javob tanasi log yoki document.ErrorMessage ga yozilmaydi;
    //      faqat xavfsiz diagnostika kodi saqlanadi.
    //
    // JONLI SINOVDA BIRINCHI TEKSHIRILADIGAN NUQTA — agar doc_status boshqa nom
    // bilan chiqsa, faqat shu metod ichini o'zgartirish kifoya.
    // ============================================================================
    private async Task<(string LocalStatus, int? DidoxStatusCode, string? Diagnostics)> TryReadRealStatusAsync(int organizationId, string providerDocumentId, CancellationToken ct)
    {
        string rawBody;
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"v1/documents/{providerDocumentId}");
            using var response = await SendAsync(organizationId, request, ct);
            rawBody = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Didox post-sign status read returned HTTP {StatusCode} for organization {OrganizationId}.",
                    (int)response.StatusCode, organizationId);
                return (SignSentStatus, null, "DIDOX_POST_SIGN_STATUS_HTTP_ERROR");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                "Didox post-sign status read failed for organization {OrganizationId}. FailureType={FailureType}.",
                organizationId, ex.GetType().Name);
            return (SignSentStatus, null, "DIDOX_POST_SIGN_STATUS_READ_FAILED");
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(rawBody);
        }
        catch (JsonException)
        {
            _logger.LogWarning(
                "Didox post-sign status response was not valid JSON for organization {OrganizationId}.",
                organizationId);
            return (SignSentStatus, null, "DIDOX_POST_SIGN_STATUS_INVALID_JSON");
        }

        using (document)
        {
            if (document.RootElement.TryGetProperty("doc_status", out var statusProperty)
                && TryReadDidoxStatus(statusProperty, out var docStatus))
            {
                return (MapDidoxStatusToLocal(docStatus), docStatus, null);
            }
        }

        _logger.LogWarning(
            "Didox post-sign status response did not contain a valid doc_status for organization {OrganizationId}.",
            organizationId);
        return (SignSentStatus, null, "DIDOX_POST_SIGN_STATUS_INVALID");
    }

    internal static bool TryReadDidoxStatus(JsonElement value, out int status)
    {
        if (value.ValueKind == JsonValueKind.Number)
            return value.TryGetInt32(out status);

        if (value.ValueKind == JsonValueKind.String)
            return int.TryParse(value.GetString(), out status);

        status = default;
        return false;
    }

    // ============================================================================
    // Didox status kodi → mahalliy status. YAGONA joy — boshqa hech qayerda bu
    // o'girish qo'lda takrorlanmaydi (7.3-bosqich talabi).
    //
    // INT_DIDOX.md §8.7 "Hujjat statuslari — barcha 7 turi", СФ (hisob-faktura)
    // jadvali — bu servis faqat ЭСФ (DocType="002") bilan ishlagani uchun aynan
    // shu jadval qo'llanildi (Akt/ТТН/Доверенность kabi boshqa hujjat turlari uchun
    // kodlar boshqacha, INT_DIDOX.md §8.7).
    // ============================================================================
    private static string MapDidoxStatusToLocal(int docStatus) => docStatus switch
    {
        0 => "DRAFT",                          // Черновик
        1 => "AWAITING_PARTNER_SIGNATURE",     // Ожидают подписи партнера
        2 => "AWAITING_YOUR_SIGNATURE",        // Ожидает вашей подписи
        3 => "SIGNED",                         // Подписан
        4 => "SIGNATURE_REJECTED",             // Отказ от подписи
        5 => "DELETED",                        // Удален
        40 => "INVALID",                       // Недействительный
        50 => "CANCELLED_BY_TAX_AUTHORITY",    // Аннулирован НК
        55 => "DRAFT_DELETED",                 // Черновик удален
        60 => "AWAITING_AGENT_SIGNATURE",      // Ожидают подписи агента
        // Hujjatda yo'q/noma'lum kod — sukut bo'yicha SIGNED emas, jonli sinovda
        // ko'rib chiqish uchun SIGN_SENT bilan qoldiriladi.
        _ => SignSentStatus
    };

    private async Task<Dictionary<long, MarkingCode>> ResolveMarkingCodesAsync(DidoxFacturaCreateRequestDto request, int organizationId, CancellationToken ct)
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

    private static DidoxFacturaPayload BuildPayload(DidoxFacturaCreateRequestDto request, Dictionary<long, MarkingCode> markingCodesById)
    {
        var products = request.Products.Select(p =>
        {
            var marks = string.Empty;
            if (p.MarkingCodeIds.Count > 0)
            {
                var composedCodes = p.MarkingCodeIds
                    .Select(id => markingCodesById[id])
                    .Select(code => MarkingCodeParser.Compose(new MarkingCodeParts(code.Gtin, code.SerialNumber, code.CheckKey, code.CheckCode)))
                    .ToList();
                marks = ComposeMarks(composedCodes);
            }

            return new DidoxProductPayload
            {
                OrdNo = p.OrdNo,
                Name = p.Name,
                CatalogCode = p.CatalogCode,
                CatalogName = p.CatalogName,
                Marks = marks,
                PackageCode = p.PackageCode,
                PackageName = p.PackageName,
                Count = p.Count,
                Summa = p.Summa,
                DeliverySum = p.DeliverySum,
                VatRate = p.VatRate,
                VatSum = p.VatSum,
                DeliverySumWithVat = p.DeliverySumWithVat,
                WithoutVat = p.WithoutVat,
                Origin = p.Origin
            };
        }).ToList();

        // HasMarking qattiq kodlanmaydi — haqiqatda markirovkali qator bor-yo'qligidan hisoblanadi.
        var hasMarking = request.Products.Any(p => p.MarkingCodeIds.Count > 0);
        var hasVat = request.Products.Any(p => !p.WithoutVat);

        return new DidoxFacturaPayload
        {
            Version = SchemaVersion,
            HasMarking = hasMarking,
            FacturaType = DefaultFacturaType,
            ProductList = new DidoxProductListPayload
            {
                Tin = request.SellerTin,
                HasVat = hasVat,
                Products = products
            },
            FacturaDoc = new DidoxFacturaDocPayload
            {
                FacturaNo = request.FacturaNo,
                FacturaDate = request.FacturaDate.ToString(DateFormat)
            },
            ContractDoc = new DidoxContractDocPayload
            {
                ContractNo = request.ContractNo,
                ContractDate = request.ContractDate.ToString(DateFormat)
            },
            SellerTin = request.SellerTin,
            Seller = ToPartyPayload(request.Seller),
            BuyerTin = request.BuyerTin,
            Buyer = ToPartyPayload(request.Buyer),
            FacturaEmpowermentDoc = new DidoxEmpowermentPayload
            {
                EmpowermentNo = request.Empowerment.EmpowermentNo,
                EmpowermentDateOfIssue = request.Empowerment.EmpowermentDateOfIssue.ToString(DateFormat),
                AgentFio = request.Empowerment.AgentFio,
                AgentPinfl = request.Empowerment.AgentPinfl
            }
        };
    }

    private static DidoxPartyPayload ToPartyPayload(DidoxFacturaPartyDto party) => new()
    {
        Name = party.Name,
        VatRegCode = party.VatRegCode,
        VatRegStatus = party.VatRegStatus,
        Account = party.Account,
        BankId = party.BankId,
        Address = party.Address,
        Director = party.Director,
        Accountant = party.Accountant,
        BranchCode = party.BranchCode,
        BranchName = party.BranchName
    };

    // ============================================================================
    // BL-D03 (INT_DIDOX.md §6.3) — OCHIQ BLOKER.
    //
    // `ProductList.Products[].Marks` bir nechta markirovka kodini o'zida saqlashi
    // mumkin bo'lgan SATR (string) — buni hujjatning o'zi tasdiqlaydi (izoh
    // "//Маркировки", ko'plikda). Lekin satr ICHIDA kodlar qanday ajratilishi
    // (vergul? probel? nuqtali vergul? ASCII 29/GS ajratgichi?) hujjatda umuman
    // YOZILMAGAN va jonli sinovsiz aniqlanmaydi.
    //
    // Shu noaniqlik BUTUN oqimga tarqalib ketmasligi uchun kodlarni satrga yig'ish
    // MANTIG'I FAQAT shu yagona metodda joylashgan — boshqa hech qanday joyda
    // (BuildPayload ichida ham) Marks satri qo'lda yasalmaydi.
    //
    // TANLANGAN AJRATGICH: vergul (",").
    // NEGA: Didox API'sining o'zida "bitta satrda bir nechta qiymat" uchun
    // KUZATILGAN, TASDIQLANGAN yagona konvensiya — vergul. Masalan
    // GET /v2/documents so'rovining `doctype` va `status` parametrlari xuddi shu
    // API hujjatida "Vergul bilan bir nechta" deb aniq yozilgan (INT_DIDOX.md §3.5.1).
    // ASCII 29 (GS) ishlatilmadi: bu ko'rinmas boshqaruv belgisi, JSON matn
    // maydonida yuborilishi hech qayerda tasdiqlanmagan va odatiy emas.
    //
    // JONLI SINOVDA TEKSHIRILADI: (1) Didox bu formatdagi satrni xato qaytarmasdan
    // qabul qiladimi; (2) hujjat imzolangandan keyin markirovka kodlari haqiqatan
    // ham to'g'ri o'qiladi/bog'lanadimi (A.1 №3 — xaridor tasdiqlagach kodlar unga
    // o'tishi kerak). Agar Didox bu formatni rad etsa yoki noto'g'ri talqin qilsa —
    // faqat SHU metod ichidagi ajratgichni almashtirish kifoya, chaqiruvchi kod
    // (BuildPayload, servisning qolgan qismi) o'zgarmaydi.
    // ============================================================================
    private static string ComposeMarks(IReadOnlyList<string> codes)
    {
        if (codes.Count == 0)
            return string.Empty;

        // Bitta kod bo'lsa ajratgich umuman ishlatilmaydi — noaniq formatlash
        // ehtimolini eng keng tarqalgan holatda (bitta birlik sotilganda) yo'qqa chiqaradi.
        return codes.Count == 1 ? codes[0] : string.Join(",", codes);
    }

    // INT_DIDOX.md §5.1: "Javob (Response 200): ... ildizda `_id` va `created_date`."
    // — bu TASDIQLANGAN, Edocs 6.4-bosqichdagi 3-nomzodli taxmindan farqli o'laroq
    // bitta, aniq maydon nomi.
    private static string? ExtractDocumentId(JsonElement response) =>
        response.TryGetProperty("_id", out var idProperty) && idProperty.ValueKind == JsonValueKind.String
            ? idProperty.GetString()
            : null;

    private async Task MarkFailedAsync(MarkingDidoxDocument document, int organizationId, string idempotencyKey, string errorMessage)
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
                "Failed to record FAILED status for MarkingDidoxDocument {DocumentId}. Original Didox error: {OriginalError}",
                document.Id,
                errorMessage);
        }
    }

    // --- Idempotentlik: AslBelgi (5.8-bosqich)/Edocs (6.4-bosqich) dagi naqsh qayta ishlatildi ---

    private static string ComputeRequestHash(DidoxFacturaCreateRequestDto request)
    {
        var productsPart = string.Join(";", request.Products.Select(p =>
            $"{p.OrdNo}|{p.Name}|{p.Count}|{p.Summa}|{p.VatRate}|{p.VatSum}|{p.WithoutVat}|{string.Join(",", p.MarkingCodeIds.OrderBy(x => x))}"));

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
            $"{IdempotencyOperationType}|{request.InternalDocumentType}|{request.InternalDocumentId}|{request.SellerTin}|{request.BuyerTin}|{request.FacturaNo}|{request.FacturaDate:O}|{productsPart}")));
    }

    private async Task<DidoxFacturaCreateResultDto?> TryCreateIdempotencyRecordAsync(int organizationId, string key, string hash, CancellationToken ct)
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
            var existingDocument = await _context.MarkingDidoxDocuments.SingleOrDefaultAsync(
                d => d.OrganizationId == organizationId && d.ProviderDocumentId == existing.ResultDocumentId, ct);

            return new DidoxFacturaCreateResultDto
            {
                MarkingDidoxDocumentId = existingDocument?.Id ?? 0,
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
            "Didox factura creation succeeded remotely (ProviderDocumentId={ProviderDocumentId}) but local status update failed for MarkingDidoxDocument {DocumentId}. Manual reconciliation required.",
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
                "Failed to record reconciliation state for Didox ProviderDocumentId={ProviderDocumentId}, IdempotencyKey={IdempotencyKey}.",
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
            _logger.LogError(rollbackEx, "Rollback failed while persisting a marking_didox_document result.");
        }
    }

    private int RequireOrganization() => _userContext.OrganizationId
        ?? throw new InvalidOperationException("An active organization is required for Didox factura operations.");

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

    private async Task<HttpResponseMessage> SendAsync(int organizationId, HttpRequestMessage request, CancellationToken ct)
    {
        // Partner-Authorization va user-key ikkalasi ham DidoxAuthorizationHandler
        // ichida qo'yiladi (7.1-bosqich) — bu yerda faqat organizationId ni handler
        // o'qishi uchun request.Options ga qo'yamiz.
        request.Options.Set(IntegrationHttpRequestOptions.OrganizationId, organizationId);

        try
        {
            var client = _httpClientFactory.CreateClient(DidoxHttpClientNames.Client);
            return await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new IntegrationHttpException("Didox request timed out.", StatusCodes.Status504GatewayTimeout);
        }
        catch (HttpRequestException)
        {
            throw new IntegrationHttpException("Didox request could not be completed.", StatusCodes.Status502BadGateway);
        }
    }

    // INT_DIDOX.md §9 — tasdiqlangan xato kodlari (umumiy, hujjat yaratish metodlari
    // uchun alohida xato jadvali hujjatda YO'Q — §9: "Hujjat yaratish... metodlarining
    // xato kodlari — aniqlanmadi").
    private static async Task EnsureSuccessStatusOrThrowAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        throw response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => new IntegrationUnauthorizedException("Didox credentials were rejected."),
            HttpStatusCode.Forbidden => new IntegrationForbiddenException("Didox denied the request."),
            HttpStatusCode.UnprocessableEntity => new IntegrationHttpException("Didox rejected the request (422).", 422),
            HttpStatusCode.Locked => new IntegrationHttpException("Didox account is locked (423).", 423),
            (HttpStatusCode)429 => new IntegrationHttpException("Didox rate limit exceeded (429).", 429),
            _ => new IntegrationHttpException(
                $"Didox request failed with HTTP status {(int)response.StatusCode}.",
                (int)response.StatusCode)
        };
    }

    private sealed class DidoxFacturaPayload
    {
        public int Version { get; init; } = SchemaVersion;
        public bool HasMarking { get; init; }
        public int FacturaType { get; init; }
        public DidoxProductListPayload ProductList { get; init; } = new();
        public DidoxFacturaDocPayload FacturaDoc { get; init; } = new();
        public DidoxContractDocPayload ContractDoc { get; init; } = new();
        public string SellerTin { get; init; } = string.Empty;
        public DidoxPartyPayload Seller { get; init; } = new();
        public string BuyerTin { get; init; } = string.Empty;
        public DidoxPartyPayload Buyer { get; init; } = new();
        public DidoxEmpowermentPayload FacturaEmpowermentDoc { get; init; } = new();
    }

    private sealed class DidoxProductListPayload
    {
        public string Tin { get; init; } = string.Empty;
        public bool HasVat { get; init; }
        public List<DidoxProductPayload> Products { get; init; } = [];
    }

    private sealed class DidoxProductPayload
    {
        public int OrdNo { get; init; }
        public string Name { get; init; } = string.Empty;
        public string CatalogCode { get; init; } = string.Empty;
        public string CatalogName { get; init; } = string.Empty;
        public string Marks { get; init; } = string.Empty;
        public string PackageCode { get; init; } = string.Empty;
        public string PackageName { get; init; } = string.Empty;
        public string Count { get; init; } = string.Empty;
        public string Summa { get; init; } = string.Empty;
        public string? DeliverySum { get; init; }
        public string VatRate { get; init; } = string.Empty;
        public string VatSum { get; init; } = string.Empty;
        public string? DeliverySumWithVat { get; init; }
        public bool WithoutVat { get; init; }
        public int Origin { get; init; }
    }

    private sealed class DidoxFacturaDocPayload
    {
        public string FacturaNo { get; init; } = string.Empty;
        public string FacturaDate { get; init; } = string.Empty;
    }

    private sealed class DidoxContractDocPayload
    {
        public string ContractNo { get; init; } = string.Empty;
        public string ContractDate { get; init; } = string.Empty;
    }

    private sealed class DidoxPartyPayload
    {
        public string Name { get; init; } = string.Empty;
        public string VatRegCode { get; init; } = string.Empty;
        public string VatRegStatus { get; init; } = string.Empty;
        public string Account { get; init; } = string.Empty;
        public string BankId { get; init; } = string.Empty;
        public string Address { get; init; } = string.Empty;
        public string? Director { get; init; }
        public string? Accountant { get; init; }
        public string? BranchCode { get; init; }
        public string? BranchName { get; init; }
    }

    private sealed class DidoxEmpowermentPayload
    {
        public string EmpowermentNo { get; init; } = string.Empty;
        public string EmpowermentDateOfIssue { get; init; } = string.Empty;
        public string AgentFio { get; init; } = string.Empty;
        public string AgentPinfl { get; init; } = string.Empty;
    }
}
