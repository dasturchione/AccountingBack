using Application.Features.Integration.AslBelgi.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Integration;

[Route("api/integrations/asl-belgi/orders")]
[ApiController]
[Authorize]
public sealed class AslBelgiOrderController(IAslBelgiOrderService service) : ControllerBase
{
    [HttpPost]
    public async Task<IResult> CreateOrder([FromBody] MarkingOrderCreateRequestDto request, CancellationToken ct = default)
        => Results.Ok(await service.CreateOrderAsync(request, ct));

    // POST, GET emas: bu amal marking_code larni yaratadi/yangilaydi va marking_order.status ni
    // o'zgartiradi (INSERT/UPDATE) — STANDARDS.md §9 bo'yicha bu write operatsiya, CRPT tomonida
    // ham paket "iste'mol qilinishi" mumkin (yon ta'sirli), shuning uchun GET semantikasiga mos emas.
    [HttpPost("{orderId:long}/codes")]
    public async Task<IResult> FetchCodes([FromRoute] long orderId, CancellationToken ct = default)
        => Results.Ok(await service.FetchCodesAsync(orderId, ct));
}
