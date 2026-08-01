using Application.Features.Integration.Edocs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Integration;

[Route("api/integrations/edocs/auth")]
[ApiController]
[Authorize]
public sealed class EdocsAuthController(IEdocsAuthService service) : ControllerBase
{
    // Deprecated: yangi clientlar /api/edo/auth/challenge va /api/edo/auth/complete
    // common contractlaridan foydalanishi kerak; legacy response shakli saqlanadi.
    // 1-qadam: Edocs GET /authId/{serialNumber} ni proksi qiladi. Frontend qaytgan
    // authId'ni ЭЦП bilan imzolab, natijani /auth/complete ga yuboradi.
    [HttpGet("challenge")]
    public async Task<IResult> Challenge([FromQuery] string serialNumber, CancellationToken ct = default)
        => Results.Ok(await service.GetAuthChallengeAsync(serialNumber, ct));

    // 2-qadam: frontend'dan tayyor imzo bilan keladi, Edocs POST /login ga uzatiladi.
    // Muvaffaqiyatli bo'lsa, token EdocsTokenCache'ga yoziladi (24 soat + 5 daqiqa zaxira).
    [HttpPost("complete")]
    public async Task<IResult> Complete([FromBody] EdocsAuthCompleteRequestDto request, CancellationToken ct = default)
        => Results.Ok(await service.CompleteAuthAsync(request, ct));
}
