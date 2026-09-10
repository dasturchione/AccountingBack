using Application.Features.Pay.Taxes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/payroll/taxes")]
[ApiController]
[Authorize]
public sealed class PayrollTaxDefinitionController : ControllerBase
{
    private readonly IPayrollTaxDefinitionService _service;

    public PayrollTaxDefinitionController(IPayrollTaxDefinitionService service) => _service = service;

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.PayrollView)]
    public async Task<IResult> GetAllAsync([FromQuery] PayrollTaxDefinitionListFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:int}")]
    [ModuleAuthorize(PermissionCodeConst.PayrollView)]
    public async Task<IResult> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.PayrollView)]
    public async Task<IResult> CreateAsync(PayrollTaxDefinitionCreateDto dto, CancellationToken ct = default)
    {
        var result = await _service.CreateAsync(dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{id:int}")]
    [ModuleAuthorize(PermissionCodeConst.PayrollView)]
    public async Task<IResult> UpdateAsync(int id, PayrollTaxDefinitionUpdateDto dto, CancellationToken ct = default)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpDelete("{id:int}")]
    [ModuleAuthorize(PermissionCodeConst.PayrollView)]
    public async Task<IResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var result = await _service.DeleteAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
