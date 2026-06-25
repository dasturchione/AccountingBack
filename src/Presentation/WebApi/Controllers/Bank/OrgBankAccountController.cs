using Application.Features.OrgBankAccounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/org-bank-accounts")]
[ApiController]
[Authorize]
public class OrgBankAccountController : ControllerBase
{
    private readonly IOrgBankAccountService _service;

    public OrgBankAccountController(IOrgBankAccountService service)
    {
        _service = service;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.OrgBankAccountView)]
    public async Task<IResult> GetAllAsync([FromQuery] OrgBankAccountListFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:int}")]
    [ModuleAuthorize(PermissionCodeConst.OrgBankAccountViewDetail)]
    public async Task<IResult> GetByIdAsync([FromRoute] int id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.OrgBankAccountCreate)]
    public async Task<IResult> CreateAsync([FromBody] OrgBankAccountCreateDto dto, CancellationToken ct = default)
    {
        var result = await _service.CreateAsync(dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("many")]
    [ModuleAuthorize(PermissionCodeConst.OrgBankAccountCreate)]
    public async Task<IResult> CreateManyAsync([FromBody] OrgBankAccountCreateManyDto dto, CancellationToken ct = default)
    {
        var result = await _service.CreateManyAsync(dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{id:int}")]
    [ModuleAuthorize(PermissionCodeConst.OrgBankAccountUpdate)]
    public async Task<IResult> UpdateAsync([FromRoute] int id, [FromBody] OrgBankAccountUpdateDto dto, CancellationToken ct = default)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpDelete("{id:int}")]
    [ModuleAuthorize(PermissionCodeConst.OrgBankAccountDelete)]
    public async Task<IResult> DeleteAsync([FromRoute] int id, CancellationToken ct = default)
    {
        var result = await _service.DeleteAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
