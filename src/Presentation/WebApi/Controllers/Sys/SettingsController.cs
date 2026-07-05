using Application.Features.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers.Sys;

[Route("api/settings")]
[ApiController]
[Authorize]
[GlobalAccessAuthorize]
[ModuleAuthorize(PermissionCodeConst.SettingsManage)]
public sealed class SettingsController : ControllerBase
{
    private readonly ISettingService _service;

    public SettingsController(ISettingService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IResult> GetAll([FromQuery] string? category, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(category, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{code}")]
    public async Task<IResult> GetByCode([FromRoute] string code, CancellationToken ct = default)
    {
        var result = await _service.GetAsync(code, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{code}")]
    public async Task<IResult> Update([FromRoute] string code, [FromBody] SettingUpdateDto dto, CancellationToken ct = default)
    {
        var result = await _service.UpdateAsync(code, dto.Value, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
