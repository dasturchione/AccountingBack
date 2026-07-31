namespace Application.Features.Integration.AslBelgi.Utilizations;

public sealed class MarkingUtilizationCreateRequestDto
{
    public long? MarkingOrderId { get; init; }
    public IReadOnlyCollection<string>? Codes { get; init; }
    public int? BusinessPlaceId { get; init; }
    public int? WarehouseId { get; init; }
    public DateOnly ProductionDate { get; init; }
    public DateOnly? ExpirationDate { get; init; }

    // §13.3: PRODUCTION | IMPORT | CIRCULATION. Hujjatda default yo'q — har bir hisobot uchun
    // aniq ko'rsatilishi shart, shuning uchun bu yerda taxminiy qiymat berilmadi.
    public string ReleaseType { get; init; } = string.Empty;

    // §13.27: ISO ikki harfli mamlakat kodi. Tovar kartochkasidagi ishlab chiqarish mamlakatiga
    // mos bo'lishi kerak — hujjatda default yo'q, shuning uchun har doim aniq berilishi shart.
    public string ManufacturerCountry { get; init; } = string.Empty;

    // Chaqiruvchi tomonidan beriladi — xuddi shu kalit bilan qayta yuborilgan so'rov CRPT ga
    // ikkinchi marta yubormasdan, saqlangan natijani qaytaradi.
    public string IdempotencyKey { get; init; } = string.Empty;
}

public sealed class MarkingUtilizationCreateResultDto
{
    public long UtilizationId { get; init; }
    public string? CrptDocumentId { get; init; }
    public string Status { get; init; } = string.Empty;
    public bool IsReplay { get; init; }
}

public interface IAslBelgiUtilizationService
{
    Task<MarkingUtilizationCreateResultDto> CreateUtilizationAsync(MarkingUtilizationCreateRequestDto request, CancellationToken ct = default);
}
