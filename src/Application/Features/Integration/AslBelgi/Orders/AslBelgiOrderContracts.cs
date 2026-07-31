namespace Application.Features.Integration.AslBelgi.Orders;

public sealed class MarkingOrderCreateRequestDto
{
    public int ProductId { get; init; }
    public int Quantity { get; init; }
    public int? BusinessPlaceId { get; init; }
    public int? WarehouseId { get; init; }

    // Chaqiruvchi tomonidan beriladi (AslBelgiTransferService dagi eski naqsh bilan bir xil) —
    // xuddi shu kalit bilan qayta yuborilgan so'rov CRPT ga ikkinchi marta yubormasdan,
    // saqlangan natijani qaytaradi.
    public string IdempotencyKey { get; init; } = string.Empty;
}

public sealed class MarkingOrderCreateResultDto
{
    public long OrderId { get; init; }
    public string? CrptOrderId { get; init; }
    public string Status { get; init; } = string.Empty;
    public bool IsReplay { get; init; }
}

public sealed class MarkingOrderFetchCodesResultDto
{
    public long OrderId { get; init; }
    public string OrderStatus { get; init; } = string.Empty;

    // false — CRPT hali kod tayyorlamagan (normal oraliq holat, xato emas). true — kodlar
    // muvaffaqiyatli olindi va marking_code ga yozildi.
    public bool IsReady { get; init; }
    public int CodesFetched { get; init; }
}

public interface IAslBelgiOrderService
{
    Task<MarkingOrderCreateResultDto> CreateOrderAsync(MarkingOrderCreateRequestDto request, CancellationToken ct = default);
    Task<MarkingOrderFetchCodesResultDto> FetchCodesAsync(long orderId, CancellationToken ct = default);
}
