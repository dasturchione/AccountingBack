namespace Application.Features.Integration.AslBelgi.Aggregations;

public sealed class MarkingAggregationCreateRequestDto
{
    // Konteyner (quti/pallet) kodi — SSCC yoki guruh qadog'i kodi. Mavjud marking_code
    // bo'lishi shart emas: yangi qator sifatida yaratiladi.
    public string ParentCode { get; init; } = string.Empty;

    // Ichki kodlar — HAMMASI mavjud marking_code bo'lishi, status=in_circulation va
    // hali boshqa konteynerga bog'lanmagan (parent_marking_code_id bo'sh) bo'lishi shart.
    public IReadOnlyCollection<string> InnerCodes { get; init; } = Array.Empty<string>();

    public int? BusinessPlaceId { get; init; }
    public int? WarehouseId { get; init; }
    public DateOnly PackingDate { get; init; }

    // CRPT §5.3 aggregationUnitCapacity — rejalashtirilgan sig'im. Hujjatda bu InnerCodes
    // sonidan farqli, alohida maydon; task kirish ro'yxatida yo'q edi, lekin CRPT
    // kontraktida VA loyihaning marking_aggregation.planned_capacity ustunida majburiy.
    public int PlannedCapacity { get; init; }

    // Chaqiruvchi tomonidan beriladi — xuddi shu kalit bilan qayta yuborilgan so'rov CRPT ga
    // ikkinchi marta yubormasdan, saqlangan natijani qaytaradi.
    public string IdempotencyKey { get; init; } = string.Empty;
}

public sealed class MarkingAggregationCreateResultDto
{
    public long AggregationId { get; init; }
    public long ParentMarkingCodeId { get; init; }
    public string? CrptDocumentId { get; init; }
    public string Status { get; init; } = string.Empty;
    public bool IsReplay { get; init; }
}

public interface IAslBelgiAggregationService
{
    Task<MarkingAggregationCreateResultDto> CreateAggregationAsync(MarkingAggregationCreateRequestDto request, CancellationToken ct = default);
}
