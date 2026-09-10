using Application.Features.Pay.Periods;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/payroll/periods")]
[ApiController]
[Authorize]
public sealed class PayrollPeriodController : ControllerBase
{
    private readonly IPayrollPeriodService _service;

    public PayrollPeriodController(IPayrollPeriodService service)
    {
        _service = service;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.PayrollPeriodView)]
    public async Task<IResult> GetAllAsync([FromQuery] PayrollPeriodListFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.PayrollPeriodView)]
    public async Task<IResult> GetByIdAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.PayrollPeriodManage)]
    public async Task<IResult> CreateAsync([FromBody] PayrollPeriodCreateDto dto, CancellationToken ct = default)
    {
        var result = await _service.CreateAsync(dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.PayrollPeriodManage)]
    public async Task<IResult> UpdateAsync(
        [FromRoute] long id,
        [FromBody] PayrollPeriodUpdateDto dto,
        CancellationToken ct = default)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPost("{id:long}/close")]
    [ModuleAuthorize(PermissionCodeConst.PayrollPeriodManage)]
    public async Task<IResult> CloseAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.CloseAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPost("{id:long}/reopen")]
    [ModuleAuthorize(PermissionCodeConst.PayrollPeriodManage)]
    public async Task<IResult> ReopenAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.ReopenAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
