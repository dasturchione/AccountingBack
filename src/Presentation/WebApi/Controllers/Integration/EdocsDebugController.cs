using Application.Features.Integration.Edocs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Integration;

// VAQTINCHALIK — 6.3-bosqichdagi qo'lda sinov uchun. Haqiqiy hujjat tuzilishi o'qib
// chiqilgach, bu controller BUTUNLAY o'chiriladi. Productionga chiqmasligi uchun
// Development muhitidan tashqarida 404 qaytaradi.
[Route("api/integrations/edocs/debug")]
[ApiController]
[Authorize]
public sealed class EdocsDebugController(IEdocsDebugService service, IHostEnvironment environment) : ControllerBase
{
    // Deprecated/debug-only: common EDO API replacement yo'q. Development guard saqlanadi.
    [HttpGet("document/{id}")]
    public async Task<IResult> GetDocument(string id, CancellationToken ct = default)
    {
        if (!environment.IsDevelopment())
            return Results.NotFound();

        var document = await service.GetDocumentAsync("factura", id, ct);
        return Results.Ok(document);
    }
}
