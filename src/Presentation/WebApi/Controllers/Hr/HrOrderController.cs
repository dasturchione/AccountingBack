using Application.Features.Pay.HrOrders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/hr/orders")]
[ApiController]
[Authorize]
public sealed class HrOrderController : ControllerBase
{
    private readonly IPayrollHrOrderService _orderService;

    public HrOrderController(IPayrollHrOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.HrOrderView)]
    public async Task<IResult> GetAllAsync([FromQuery] PayrollHrOrderListFilter filter, CancellationToken ct = default)
    {
        var result = await _orderService.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.HrOrderView)]
    public async Task<IResult> GetByIdAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _orderService.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:long}/print")]
    [ModuleAuthorize(PermissionCodeConst.HrOrderView)]
    public async Task<IResult> GetPrintAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _orderService.GetPrintAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.HrOrderCreate)]
    public async Task<IResult> CreateAsync([FromBody] PayrollHrOrderSaveDto dto, CancellationToken ct = default)
    {
        var result = await _orderService.CreateAsync(dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.HrOrderUpdate)]
    public async Task<IResult> UpdateAsync([FromRoute] long id, [FromBody] PayrollHrOrderSaveDto dto, CancellationToken ct = default)
    {
        var result = await _orderService.UpdateAsync(id, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPost("{id:long}/confirm")]
    [ModuleAuthorize(PermissionCodeConst.HrOrderConfirm)]
    public async Task<IResult> ConfirmAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _orderService.ConfirmAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPost("{id:long}/cancel")]
    [ModuleAuthorize(PermissionCodeConst.HrOrderCancel)]
    public async Task<IResult> CancelAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _orderService.CancelAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpDelete("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.HrOrderDelete)]
    public async Task<IResult> DeleteAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _orderService.DeleteAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
