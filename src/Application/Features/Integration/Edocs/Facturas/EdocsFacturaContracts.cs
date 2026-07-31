namespace Application.Features.Integration.Edocs.Facturas;

public sealed class EdocsFacturaPartyDto
{
    public string BankId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Account { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string DistrictId { get; init; } = string.Empty;
    public string? WorkPhone { get; init; }
    public string? Mobile { get; init; }
    public string? Accountant { get; init; }
    public string? Director { get; init; }
    public string? Oked { get; init; }
    public string? VatRegCode { get; init; }
}

public sealed class EdocsFacturaProductRequestDto
{
    public int OrdNo { get; init; }
    public string Name { get; init; } = string.Empty;
    public int MeasureId { get; init; }
    public string Count { get; init; } = string.Empty;
    public string Summa { get; init; } = string.Empty;
    public decimal VatRate { get; init; }
    public bool WithoutVat { get; init; }
    public bool HasVat { get; init; }

    // marking_code.id lar (agar shu qator markirovkali mahsulotga tegishli bo'lsa).
    // Xizmatlar yoki markirovkasiz mahsulotlar uchun bo'sh qoldiriladi.
    public List<long> MarkingCodeIds { get; init; } = [];
}

public sealed class EdocsFacturaCreateRequestDto
{
    // Ichki hujjat havolasi — marking_edocs_document.internal_document_type/internal_document_id
    // ga yoziladi. Standart qiymat "sale_doc" (sotuv hujjati), lekin chaqiruvchi boshqa
    // ichki hujjat turini ham ko'rsatishi mumkin.
    public long InternalDocumentId { get; init; }
    public string InternalDocumentType { get; init; } = "sale_doc";

    public string SellerTin { get; init; } = string.Empty;
    public string BuyerTin { get; init; } = string.Empty;
    public EdocsFacturaPartyDto Seller { get; init; } = new();
    public EdocsFacturaPartyDto Buyer { get; init; } = new();

    public string FacturaNo { get; init; } = string.Empty;
    public DateOnly FacturaDate { get; init; }

    public string? ContractNo { get; init; }
    public DateOnly? ContractDate { get; init; }

    public List<EdocsFacturaProductRequestDto> Products { get; init; } = [];

    // Chaqiruvchi tomonidan beriladi — xuddi shu kalit bilan qayta yuborilgan so'rov
    // Edocs'ga ikkinchi marta yubormasdan, saqlangan natijani qaytaradi.
    public string IdempotencyKey { get; init; } = string.Empty;
}

public sealed class EdocsFacturaCreateResultDto
{
    public long MarkingEdocsDocumentId { get; init; }
    public string? ProviderDocumentId { get; init; }
    public string Status { get; init; } = string.Empty;
    public bool IsReplay { get; init; }
}

public sealed class EdocsFacturaSignRequestDto
{
    public long MarkingEdocsDocumentId { get; init; }

    // Frontend/e-imzo vositasidan tayyor holda keladi (6.2-bosqichdagi /auth/complete bilan
    // bir xil tamoyil: backend imzolamaydi, faqat Edocs'ga uzatadi).
    public string Pkcs7 { get; init; } = string.Empty;
}

// Controller so'rov tanasi uchun — MarkingEdocsDocumentId marshrut ({id}) dan keladi,
// tanada faqat pkcs7 bo'ladi.
public sealed class EdocsFacturaSignBodyDto
{
    public string Pkcs7 { get; init; } = string.Empty;
}

public sealed class EdocsFacturaSignResultDto
{
    public long MarkingEdocsDocumentId { get; init; }
    public string Status { get; init; } = string.Empty;
}

public interface IEdocsFacturaService
{
    Task<EdocsFacturaCreateResultDto> CreateFacturaDocumentAsync(EdocsFacturaCreateRequestDto request, CancellationToken ct = default);
    Task<EdocsFacturaSignResultDto> SignFacturaDocumentAsync(EdocsFacturaSignRequestDto request, CancellationToken ct = default);
}
