using Application.Features.FaCommissionings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/fa-commissionings")]
[ApiController]
[Authorize]
public class FaCommissioningController : ControllerBase
{
    private readonly IFaCommissioningService _service;

    public FaCommissioningController(IFaCommissioningService service)
    {
        _service = service;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.FaCommissioningView)]
    public async Task<IResult> GetAllAsync(
        [FromQuery] FaCommissioningListFilter filter,
        CancellationToken ct = default) =>
        (await _service.GetAllAsync(filter, ct))
        .Match(Results.Ok, CustomResults.Problem);

    [HttpGet("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.FaCommissioningViewDetail)]
    public async Task<IResult> GetByIdAsync(
        [FromRoute] long id,
        CancellationToken ct = default) =>
        (await _service.GetByIdAsync(id, ct))
        .Match(Results.Ok, CustomResults.Problem);

    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.FaCommissioningCreate)]
    public async Task<IResult> CreateAsync(
        [FromBody] FaCommissioningCreateDto dto,
        CancellationToken ct = default) =>
        (await _service.CreateAsync(dto, ct))
        .Match(Results.Ok, CustomResults.Problem);

    [HttpPut("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.FaCommissioningUpdate)]
    public async Task<IResult> UpdateAsync(
        [FromRoute] long id,
        [FromBody] FaCommissioningUpdateDto dto,
        CancellationToken ct = default) =>
        (await _service.UpdateAsync(id, dto, ct))
        .Match(Results.NoContent, CustomResults.Problem);

    [HttpPut("{id:long}/confirm")]
    [ModuleAuthorize(PermissionCodeConst.ConfirmFaCommissioning)]
    public async Task<IResult> ConfirmAsync(
        [FromRoute] long id,
        CancellationToken ct = default) =>
        (await _service.ConfirmAsync(id, ct))
        .Match(Results.NoContent, CustomResults.Problem);

    [HttpPut("{id:long}/cancel")]
    [ModuleAuthorize(PermissionCodeConst.CancelFaCommissioning)]
    public async Task<IResult> CancelAsync(
        [FromRoute] long id,
        CancellationToken ct = default) =>
        (await _service.CancelAsync(id, ct))
        .Match(Results.NoContent, CustomResults.Problem);
}
