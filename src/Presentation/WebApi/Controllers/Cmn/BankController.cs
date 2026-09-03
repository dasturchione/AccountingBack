using Application.Features.Banks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/banks")]
[ApiController]
[Authorize]
public class BankController : ControllerBase
{
    private readonly IBankService _service;

    public BankController(IBankService service)
    {
        _service = service;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.BankView)]
    public async Task<IResult> GetAll([FromQuery] BankListFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:int}")]
    [ModuleAuthorize(PermissionCodeConst.BankViewDetail)]
    public async Task<IResult> GetById([FromRoute] int id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:int}/branches")]
    [ModuleAuthorize(PermissionCodeConst.BankViewDetail)]
    public async Task<IResult> GetBranches([FromRoute] int id, CancellationToken ct = default)
    {
        var result = await _service.GetBranchesAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("branches")]
    [ModuleAuthorize(PermissionCodeConst.BankViewDetail)]
    public async Task<IResult> GetBranchByMfo([FromQuery] string mfo, CancellationToken ct = default)
    {
        var result = await _service.GetBranchByMfoAsync(mfo, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }
}
