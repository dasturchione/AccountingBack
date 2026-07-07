using Application.Features.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IResult> Login([FromBody] LoginDto dto, CancellationToken ct = default)
    {
        var result = await _authService.LoginAsync(dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("login-superadmin")]
    public async Task<IResult> SuperAdminLogin([FromBody] LoginDto dto, CancellationToken ct = default)
    {
        var result = await _authService.SuperAdminLoginAsync(dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [ModuleAuthorize(PermissionCodeConst.AuthCheckToken)]
    [Authorize]
    [HttpGet("check-token")]
    public IActionResult CheckToken()
    {
        return Ok(new { isValid = true });
    }
}
