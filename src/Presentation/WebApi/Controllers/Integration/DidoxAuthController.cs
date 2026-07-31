using Application.Features.Integration.Didox.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Integration;

[Route("api/integrations/didox/auth")]
[ApiController]
[Authorize]
public sealed class DidoxAuthController(IDidoxAuthService service) : ControllerBase
{
    // 1-qadam: joriy tashkilotning INN ini (base64) qaytaradi. Frontend shuni E-IMZO
    // bilan imzolab, natijani /auth/complete ga yuboradi.
    [HttpGet("challenge")]
    public async Task<IResult> Challenge(CancellationToken ct = default)
        => Results.Ok(await service.GetAuthChallengeAsync(ct));

    // 2-qadam: frontend'dan tayyor imzo bilan keladi (pkcs7, signatureHex), Didox'ga
    // uzatiladi. Muvaffaqiyatli bo'lsa, token DidoxTokenCache'ga yoziladi (360 daqiqa
    // - 5 daqiqa zaxira).
    [HttpPost("complete")]
    public async Task<IResult> Complete([FromBody] DidoxAuthCompleteRequestDto request, CancellationToken ct = default)
        => Results.Ok(await service.CompleteAuthAsync(request, ct));
}
