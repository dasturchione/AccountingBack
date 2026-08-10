using Application.Features.FiscalCashRegisters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/fiscal-cash-registers")]
[ApiController]
[Authorize]
public class FiscalCashRegisterController : ControllerBase
{
    private readonly IFiscalCashRegisterService _service;

    public FiscalCashRegisterController(IFiscalCashRegisterService service)
    {
        _service = service;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.FiscalCashRegisterView)]
    public async Task<IResult> GetAllAsync([FromQuery] FiscalCashRegisterListFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:int}")]
    [ModuleAuthorize(PermissionCodeConst.FiscalCashRegisterViewDetail)]
    public async Task<IResult> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.FiscalCashRegisterCreate)]
    public async Task<IResult> CreateAsync([FromBody] FiscalCashRegisterCreateDto dto, CancellationToken ct = default)
    {
        var result = await _service.CreateAsync(dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{id:int}")]
    [ModuleAuthorize(PermissionCodeConst.FiscalCashRegisterUpdate)]
    public async Task<IResult> UpdateAsync(int id, [FromBody] FiscalCashRegisterUpdateDto dto, CancellationToken ct = default)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpDelete("{id:int}")]
    [ModuleAuthorize(PermissionCodeConst.FiscalCashRegisterDelete)]
    public async Task<IResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var result = await _service.DeleteAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
