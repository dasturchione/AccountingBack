using Application.Features.CashDocuments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/cash-documents")]
[ApiController]
[Authorize]
public class CashDocumentController : ControllerBase
{
    private readonly ICashDocumentService _service;

    public CashDocumentController(ICashDocumentService service)
    {
        _service = service;
    }

    [HttpGet("pko")]
    [ModuleAuthorize(PermissionCodeConst.CashDocumentGetReceiptOrders)]
    public async Task<IResult> GetReceiptOrdersAsync([FromQuery] CashDocumentListFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetReceiptOrdersAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("rko")]
    [ModuleAuthorize(PermissionCodeConst.CashDocumentGetPaymentOrders)]
    public async Task<IResult> GetPaymentOrdersAsync([FromQuery] CashDocumentListFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetPaymentOrdersAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("pko/{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.CashDocumentGetReceiptOrderById)]
    public async Task<IResult> GetReceiptOrderByIdAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.GetReceiptOrderByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("rko/{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.CashDocumentGetPaymentOrderById)]
    public async Task<IResult> GetPaymentOrderByIdAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.GetPaymentOrderByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("pko")]
    [ModuleAuthorize(PermissionCodeConst.CashDocumentCreateReceiptOrder)]
    public async Task<IResult> CreateReceiptOrderAsync([FromBody] CashDocumentCreateDto dto, CancellationToken ct = default)
    {
        var result = await _service.CreateReceiptOrderAsync(dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("rko")]
    [ModuleAuthorize(PermissionCodeConst.CashDocumentCreatePaymentOrder)]
    public async Task<IResult> CreatePaymentOrderAsync([FromBody] CashDocumentCreateDto dto, CancellationToken ct = default)
    {
        var result = await _service.CreatePaymentOrderAsync(dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("pko/{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.CashDocumentUpdateReceiptOrder)]
    public async Task<IResult> UpdateReceiptOrderAsync([FromRoute] long id, [FromBody] CashDocumentUpdateDto dto, CancellationToken ct = default)
    {
        var result = await _service.UpdateReceiptOrderAsync(id, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPut("rko/{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.CashDocumentUpdatePaymentOrder)]
    public async Task<IResult> UpdatePaymentOrderAsync([FromRoute] long id, [FromBody] CashDocumentUpdateDto dto, CancellationToken ct = default)
    {
        var result = await _service.UpdatePaymentOrderAsync(id, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPut("pko/{id:long}/confirm")]
    [ModuleAuthorize(PermissionCodeConst.CashDocumentConfirmReceiptOrder)]
    public async Task<IResult> ConfirmReceiptOrderAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.ConfirmReceiptOrderAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPut("rko/{id:long}/confirm")]
    [ModuleAuthorize(PermissionCodeConst.CashDocumentConfirmPaymentOrder)]
    public async Task<IResult> ConfirmPaymentOrderAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.ConfirmPaymentOrderAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPut("pko/{id:long}/cancel")]
    [ModuleAuthorize(PermissionCodeConst.CashDocumentCancelReceiptOrder)]
    public async Task<IResult> CancelReceiptOrderAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.CancelReceiptOrderAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPut("rko/{id:long}/cancel")]
    [ModuleAuthorize(PermissionCodeConst.CashDocumentCancelPaymentOrder)]
    public async Task<IResult> CancelPaymentOrderAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.CancelPaymentOrderAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpDelete("pko/{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.CashDocumentDeleteReceiptOrder)]
    public async Task<IResult> DeleteReceiptOrderAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.DeleteReceiptOrderAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpDelete("rko/{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.CashDocumentDeletePaymentOrder)]
    public async Task<IResult> DeletePaymentOrderAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.DeletePaymentOrderAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
