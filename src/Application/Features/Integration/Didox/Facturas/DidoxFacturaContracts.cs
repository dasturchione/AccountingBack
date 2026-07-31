namespace Application.Features.Integration.Didox.Facturas;

public sealed class DidoxFacturaPartyDto
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

public sealed class DidoxFacturaEmpowermentDto
{
    // BL-D07 (INT_DIDOX.md §5.1, §11.8): loyihada ishonchnoma (доверенность) tushunchasi
    // umuman yo'q — na jadval, na entity, na maydon. FacturaEmpowermentDoc bloki esa
    // ЭСФ (002) uchun MAJBURIY (*), ichidagi to'rt maydon ham * bilan belgilangan.
    // Shuning uchun bu ma'lumot chaqiruvchidan to'g'ridan-to'g'ri olinadi — xuddi
    // 6.4-bosqichda Edocs Seller/Buyer ma'lumotlari qanday olingan bo'lsa shunday.
    public string EmpowermentNo { get; init; } = string.Empty;
    public DateOnly EmpowermentDateOfIssue { get; init; }
    public string AgentFio { get; init; } = string.Empty;
    public string AgentPinfl { get; init; } = string.Empty;
}

public sealed class DidoxFacturaProductRequestDto
{
    public int OrdNo { get; init; }
    public string Name { get; init; } = string.Empty;
    public string CatalogCode { get; init; } = string.Empty;
    public string CatalogName { get; init; } = string.Empty;
    public string PackageCode { get; init; } = string.Empty;
    public string PackageName { get; init; } = string.Empty;

    // INT_DIDOX.md §5.1/§10.4: Count/Summa/VatRate/VatSum/DeliverySum/DeliverySumWithVat —
    // hujjatda tasdiqlangan holda SATR (string) turida, son emas.
    public string Count { get; init; } = string.Empty;
    public string Summa { get; init; } = string.Empty;
    public string VatRate { get; init; } = string.Empty;
    public string VatSum { get; init; } = string.Empty;
    public string? DeliverySum { get; init; }
    public string? DeliverySumWithVat { get; init; }

    public bool WithoutVat { get; init; }
    public int Origin { get; init; }

    // marking_code.id lar (agar shu qator markirovkali mahsulotga tegishli bo'lsa).
    // Xizmatlar yoki markirovkasiz mahsulotlar uchun bo'sh qoldiriladi.
    public List<long> MarkingCodeIds { get; init; } = [];
}

public sealed class DidoxFacturaCreateRequestDto
{
    // Ichki hujjat havolasi — marking_didox_document.internal_document_type/internal_document_id
    // ga yoziladi. Standart qiymat "sale_doc" (sotuv hujjati).
    public long InternalDocumentId { get; init; }
    public string InternalDocumentType { get; init; } = "sale_doc";

    public string SellerTin { get; init; } = string.Empty;
    public string BuyerTin { get; init; } = string.Empty;
    public DidoxFacturaPartyDto Seller { get; init; } = new();
    public DidoxFacturaPartyDto Buyer { get; init; } = new();

    public string FacturaNo { get; init; } = string.Empty;
    public DateOnly FacturaDate { get; init; }

    // INT_DIDOX.md §5.1: ildizdagi ContractDoc bloki * (majburiy) — Edocs'dagidan farqli
    // o'laroq bu yerda ixtiyoriy emas.
    public string ContractNo { get; init; } = string.Empty;
    public DateOnly ContractDate { get; init; }

    public DidoxFacturaEmpowermentDto Empowerment { get; init; } = new();

    public List<DidoxFacturaProductRequestDto> Products { get; init; } = [];

    // Chaqiruvchi tomonidan beriladi — xuddi shu kalit bilan qayta yuborilgan so'rov
    // Didox'ga ikkinchi marta yubormasdan, saqlangan natijani qaytaradi.
    public string IdempotencyKey { get; init; } = string.Empty;
}

public sealed class DidoxFacturaCreateResultDto
{
    public long MarkingDidoxDocumentId { get; init; }
    public string? ProviderDocumentId { get; init; }
    public string Status { get; init; } = string.Empty;
    public bool IsReplay { get; init; }
}

// INT_DIDOX.md §7.2 3-4-qadam: GET /v1/documents/{id}?owner=1 javobidan data.json,
// base64 ga o'girilgan holda frontendga (E-IMZO'ga imzolash uchun) qaytariladi.
public sealed class DidoxFacturaSignChallengeResultDto
{
    public long MarkingDidoxDocumentId { get; init; }
    public string DataBase64 { get; init; } = string.Empty;
}

public sealed class DidoxFacturaSignRequestDto
{
    public long MarkingDidoxDocumentId { get; init; }
    public string Pkcs7 { get; init; } = string.Empty;
    public string SignatureHex { get; init; } = string.Empty;
}

// Controller marshrutidan {id} allaqachon keladi — tana faqat E-IMZO natijasini o'z ichiga oladi.
public sealed class DidoxFacturaSignBodyDto
{
    public string Pkcs7 { get; init; } = string.Empty;
    public string SignatureHex { get; init; } = string.Empty;
}

public sealed class DidoxFacturaSignResultDto
{
    public long MarkingDidoxDocumentId { get; init; }
    public string Status { get; init; } = string.Empty;

    // Didox'dan HAQIQIY o'qilgan status kodi (INT_DIDOX.md §8.7) — agar GET
    // /v1/documents/{id} javobidan o'qib bo'lmasa (maydon nomi tasdiqlanmagan,
    // DidoxFacturaService.TryReadRealStatusAsync izohiga qarang), null qaytadi
    // va Status "SIGN_SENT" bo'ladi.
    public int? DidoxStatusCode { get; init; }
}

public interface IDidoxFacturaService
{
    Task<DidoxFacturaCreateResultDto> CreateFacturaDocumentAsync(DidoxFacturaCreateRequestDto request, CancellationToken ct = default);

    Task<DidoxFacturaSignChallengeResultDto> GetSignChallengeAsync(long markingDidoxDocumentId, CancellationToken ct = default);

    Task<DidoxFacturaSignResultDto> SignFacturaDocumentAsync(DidoxFacturaSignRequestDto request, CancellationToken ct = default);
}
