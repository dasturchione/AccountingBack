using Application.Features.Pay.Components;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/payroll/components")]
[ApiController]
[Authorize]
public sealed class PayrollComponentController : ControllerBase
{
    private readonly IPayrollComponentService _service;

    public PayrollComponentController(IPayrollComponentService service)
    {
        _service = service;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.PayrollComponentView)]
    public async Task<IResult> GetAllAsync([FromQuery] PayrollComponentListFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:int}")]
    [ModuleAuthorize(PermissionCodeConst.PayrollComponentView)]
    public async Task<IResult> GetByIdAsync([FromRoute] int id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.PayrollComponentCreate)]
    public async Task<IResult> CreateAsync([FromBody] PayrollComponentCreateDto dto, CancellationToken ct = default)
    {
        var result = await _service.CreateAsync(dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{id:int}")]
    [ModuleAuthorize(PermissionCodeConst.PayrollComponentUpdate)]
    public async Task<IResult> UpdateAsync(
        [FromRoute] int id,
        [FromBody] PayrollComponentUpdateDto dto,
        CancellationToken ct = default)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpDelete("{id:int}")]
    [ModuleAuthorize(PermissionCodeConst.PayrollComponentDelete)]
    public async Task<IResult> DeleteAsync([FromRoute] int id, CancellationToken ct = default)
    {
        var result = await _service.DeleteAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
