using Application.Features.Pay.PayrollDocuments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/payroll/documents")]
[ApiController]
[Authorize]
public sealed class PayrollDocumentController : ControllerBase
{
    private readonly IPayrollDocumentService _service;

    public PayrollDocumentController(IPayrollDocumentService service)
    {
        _service = service;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.PayrollDocumentView)]
    public async Task<IResult> GetAllAsync([FromQuery] PayrollDocumentListFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.PayrollDocumentView)]
    public async Task<IResult> GetByIdAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("calculate")]
    [ModuleAuthorize(PermissionCodeConst.PayrollDocumentCalculate)]
    public async Task<IResult> CalculateAsync([FromBody] PayrollCalculateDto dto, CancellationToken ct = default)
    {
        var result = await _service.CalculateAsync(dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("{id:long}/recalculate")]
    [ModuleAuthorize(PermissionCodeConst.PayrollDocumentCalculate)]
    public async Task<IResult> RecalculateAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.RecalculateAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{id:long}/confirm")]
    [ModuleAuthorize(PermissionCodeConst.PayrollDocumentConfirm)]
    public async Task<IResult> ConfirmAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.ConfirmAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPut("{id:long}/cancel")]
    [ModuleAuthorize(PermissionCodeConst.PayrollDocumentCancel)]
    public async Task<IResult> CancelAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.CancelAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpDelete("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.PayrollDocumentDelete)]
    public async Task<IResult> DeleteAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.DeleteAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
