using Application.Features.PaymentAcceptancePointOperations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/payment-acceptance-point-operations")]
[ApiController]
[Authorize]
public sealed class PaymentAcceptancePointOperationController : ControllerBase
{
    private readonly IPaymentAcceptancePointOperationService _service;

    public PaymentAcceptancePointOperationController(IPaymentAcceptancePointOperationService service)
    {
        _service = service;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.PaymentAcceptancePointOperationView)]
    public async Task<IResult> GetAllAsync(
        [FromQuery] PaymentAcceptancePointOperationListFilter filter,
        CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("balance")]
    [ModuleAuthorize(PermissionCodeConst.PaymentAcceptancePointOperationBalance)]
    public async Task<IResult> GetBalanceAsync(
        [FromQuery] int paymentAcceptancePointId,
        [FromQuery] short currencyId,
        [FromQuery] DateTime? asOfDate,
        CancellationToken ct = default)
    {
        var result = await _service.GetBalanceAsync(paymentAcceptancePointId, currencyId, asOfDate, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.PaymentAcceptancePointOperationViewDetail)]
    public async Task<IResult> GetByIdAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.PaymentAcceptancePointOperationCreate)]
    public async Task<IResult> CreateAsync(
        [FromBody] PaymentAcceptancePointOperationCreateDto dto,
        CancellationToken ct = default)
    {
        var result = await _service.CreateAsync(dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.PaymentAcceptancePointOperationUpdate)]
    public async Task<IResult> UpdateAsync(
        [FromRoute] long id,
        [FromBody] PaymentAcceptancePointOperationUpdateDto dto,
        CancellationToken ct = default)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPost("{id:long}/confirm")]
    [ModuleAuthorize(PermissionCodeConst.PaymentAcceptancePointOperationConfirm)]
    public async Task<IResult> ConfirmAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.ConfirmAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPost("{id:long}/cancel")]
    [ModuleAuthorize(PermissionCodeConst.PaymentAcceptancePointOperationCancel)]
    public async Task<IResult> CancelAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.CancelAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpDelete("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.PaymentAcceptancePointOperationDelete)]
    public async Task<IResult> DeleteAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.DeleteAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
