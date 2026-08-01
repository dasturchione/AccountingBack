using Application.Features.Integration.Didox.Facturas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Integration;

[Route("api/integrations/didox/documents")]
[ApiController]
[Authorize]
public sealed class DidoxFacturaController(IDidoxFacturaService service) : ControllerBase
{
    // Deprecated: yangi clientlar /api/edo/outbox/facturas va /api/edo/outbox/{id}/sign
    // common endpointlaridan foydalanishi kerak; legacy DTO va route'lar saqlanadi.
    [HttpPost("factura")]
    public async Task<IResult> CreateFactura([FromBody] DidoxFacturaCreateRequestDto request, CancellationToken ct = default)
        => Results.Ok(await service.CreateFacturaDocumentAsync(request, ct));

    [HttpGet("factura/{id:long}/sign-challenge")]
    public async Task<IResult> GetSignChallenge(long id, CancellationToken ct = default)
        => Results.Ok(await service.GetSignChallengeAsync(id, ct));

    [HttpPost("factura/{id:long}/sign")]
    public async Task<IResult> SignFactura(long id, [FromBody] DidoxFacturaSignBodyDto body, CancellationToken ct = default)
        => Results.Ok(await service.SignFacturaDocumentAsync(
            new DidoxFacturaSignRequestDto { MarkingDidoxDocumentId = id, Pkcs7 = body.Pkcs7, SignatureHex = body.SignatureHex },
            ct));
}
