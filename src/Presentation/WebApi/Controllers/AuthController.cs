using Application.Features.Auth;
using Application.Features.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/[controller]")]
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

    [Authorize]
    [HttpGet("check-token")]
    public async Task<IActionResult> CheckToken()
    {

        return Ok(new { isValid = true });

    }
}
