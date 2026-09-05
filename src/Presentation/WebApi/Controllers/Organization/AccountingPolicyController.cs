using Application.Features.AccountingPolicies.DTOs;
using Application.Features.AccountingPolicies.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/accounting-policies")]
[ApiController]
[Authorize]
[Produces("application/json")]
public sealed class AccountingPolicyController : ControllerBase
{
    private readonly IAccountingPolicyService _service;

    public AccountingPolicyController(IAccountingPolicyService service)
    {
        _service = service;
    }

    [HttpGet("current")]
    [ModuleAuthorize(PermissionCodeConst.SetupGet)]
    [ProducesResponseType(typeof(AccountingPolicyCurrentDto), StatusCodes.Status200OK)]
    public async Task<IResult> GetCurrentAsync(
        [FromQuery] DateOnly? effectiveOn,
        CancellationToken ct = default)
    {
        var result = await _service.GetCurrentAsync(effectiveOn, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("history")]
    [ModuleAuthorize(PermissionCodeConst.SetupGet)]
    [ProducesResponseType(typeof(AccountingPolicyHistoryDto), StatusCodes.Status200OK)]
    public async Task<IResult> GetHistoryAsync(
        [FromQuery] DateOnly? dateFrom,
        [FromQuery] DateOnly? dateTo,
        CancellationToken ct = default)
    {
        var result = await _service.GetHistoryAsync(dateFrom, dateTo, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("impact")]
    [ModuleAuthorize(PermissionCodeConst.SetupGet)]
    [ProducesResponseType(typeof(AccountingPolicyImpactDto), StatusCodes.Status200OK)]
    public async Task<IResult> GetImpactAsync(
        [FromQuery] DateOnly effectiveOn,
        [FromQuery] string? documentType,
        CancellationToken ct = default)
    {
        var result = await _service.GetImpactAsync(effectiveOn, documentType, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut]
    [ModuleAuthorize(PermissionCodeConst.SetupUpdateAccountingPolicy)]
    [ProducesResponseType(typeof(AccountingPolicyCurrentDto), StatusCodes.Status200OK)]
    public async Task<IResult> UpdateAsync(
        [FromBody] AccountingPolicyUpdateDto request,
        CancellationToken ct = default)
    {
        var result = await _service.UpdateAsync(request, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }
}
