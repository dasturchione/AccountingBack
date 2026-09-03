using Application.Features.RegulatedObligationSettings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/regulated-obligation-settings")]
[ApiController]
[Authorize]
public sealed class RegulatedObligationSettingController : ControllerBase
{
    private readonly IRegulatedObligationSettingService _service;

    public RegulatedObligationSettingController(IRegulatedObligationSettingService service)
    {
        _service = service;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.RegulatedObligationSettingView)]
    public async Task<IResult> GetAllAsync(
        [FromQuery] RegulatedObligationSettingListFilter filter,
        CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:int}")]
    [ModuleAuthorize(PermissionCodeConst.RegulatedObligationSettingViewDetail)]
    public async Task<IResult> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("by-code/{obligationCode}")]
    [ModuleAuthorize(PermissionCodeConst.RegulatedObligationSettingViewDetail)]
    public async Task<IResult> GetByCodeAsync(
        string obligationCode,
        [FromQuery] DateOnly? choosedDate = null,
        CancellationToken ct = default)
    {
        var result = await _service.GetByCodeAsync(obligationCode, choosedDate, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.RegulatedObligationSettingCreate)]
    public async Task<IResult> CreateAsync(
        [FromBody] RegulatedObligationSettingCreateDto dto,
        CancellationToken ct = default)
    {
        var result = await _service.CreateAsync(dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{id:int}")]
    [ModuleAuthorize(PermissionCodeConst.RegulatedObligationSettingUpdate)]
    public async Task<IResult> UpdateAsync(
        int id,
        [FromBody] RegulatedObligationSettingUpdateDto dto,
        CancellationToken ct = default)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
