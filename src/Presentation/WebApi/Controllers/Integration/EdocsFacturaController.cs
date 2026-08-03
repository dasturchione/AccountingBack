using Application.Features.Integration.Edocs.Facturas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Integration;

[Route("api/integrations/edocs/documents")]
[ApiController]
[Authorize]
public sealed class EdocsFacturaController(IEdocsFacturaService service) : ControllerBase
{
    // Deprecated: yangi clientlar /api/edo/outbox/facturas va /api/edo/outbox/{id}/sign
    // common endpointlaridan foydalanishi kerak; Edocs reject/file contractlari UNKNOWN.
    [HttpPost("factura")]
    public async Task<IResult> CreateFactura([FromBody] EdocsFacturaCreateRequestDto request, CancellationToken ct = default)
        => Results.Ok(await service.CreateFacturaDocumentAsync(request, ct));

    [HttpPost("factura/{id:long}/sign")]
    public async Task<IResult> SignFactura([FromRoute] long id, [FromBody] EdocsFacturaSignBodyDto body, CancellationToken ct = default)
        => Results.Ok(await service.SignFacturaDocumentAsync(
            new EdocsFacturaSignRequestDto { MarkingEdocsDocumentId = id, Pkcs7 = body.Pkcs7 }, ct));
}
