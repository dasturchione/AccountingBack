using Application.Features.Cmn.AslBelgi.Abstractions;
using Application.Features.Cmn.AslBelgi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using SharedKernel.Results;
using WebApi.Authorization;
using WebApi.Infrastructure;

namespace WebApi.Controllers.Cmn;

[Route("api/asl-belgisi")]
[ApiController]
[Authorize]
public sealed class AslBelgiController : ControllerBase
{
    private readonly IAslBelgiService _service;
    private readonly IAslBelgiMarkingService _markingService;

    public AslBelgiController(IAslBelgiService service, IAslBelgiMarkingService markingService)
    {
        _service = service;
        _markingService = markingService;
    }

    [HttpGet("check/{tin}")]
    [ModuleAuthorize(PermissionCodeConst.TaxView)]
    public async Task<IResult> CheckApiKey([FromRoute] string tin, CancellationToken ct)
    {
        var result = await _service.CheckApiKeyAsync(tin, ct);
        return result.IsSuccess ? Results.Ok(result.Value) : CustomResults.Problem(result);
    }

    [HttpPost("orders")]
    [ModuleAuthorize(PermissionCodeConst.TaxCreate)]
    public async Task<IResult> RegisterOrder([FromBody] AslBelgiOrderRequest request, CancellationToken ct)
    {
        var result = await _service.RegisterOrderAsync(request, ct);
        return result.IsSuccess ? Results.Ok(result.Value) : CustomResults.Problem(result);
    }

    [HttpGet("orders")]
    [ModuleAuthorize(PermissionCodeConst.TaxView)]
    public async Task<IResult> GetOrders([FromQuery] AslBelgiOrdersFilter filter, CancellationToken ct)
    {
        var result = await _service.GetOrdersAsync(filter, ct);
        return result.IsSuccess ? Results.Ok(result.Value) : CustomResults.Problem(result);
    }

    [HttpGet("codes")]
    [ModuleAuthorize(PermissionCodeConst.TaxView)]
    public async Task<IResult> GetCodes(
        [FromQuery] string orderId,
        [FromQuery] string? gtin,
        [FromQuery] int? quantity,
        [FromQuery] string? lastPackId,
        CancellationToken ct)
    {
        var result = await _service.GetCodesAsync(orderId, gtin, quantity, lastPackId, ct);
        return result.IsSuccess ? Results.Ok(result.Value) : CustomResults.Problem(result);
    }

    [HttpGet("documents/{documentId}")]
    [ModuleAuthorize(PermissionCodeConst.TaxView)]
    public async Task<IResult> GetDocument([FromRoute] string documentId, CancellationToken ct)
    {
        var result = await _service.GetDocumentAsync(documentId, ct);
        return result.IsSuccess ? Results.Ok(result.Value) : CustomResults.Problem(result);
    }

    [HttpGet("status/{id}")]
    [ModuleAuthorize(PermissionCodeConst.TaxView)]
    public async Task<IResult> GetStatus([FromRoute] string id, CancellationToken ct)
    {
        var result = await _service.GetStatusAsync(id, ct);
        return result.IsSuccess ? Results.Ok(result.Value) : CustomResults.Problem(result);
    }

    [HttpPost("api-keys/refresh")]
    [ModuleAuthorize(PermissionCodeConst.TaxCreate)]
    public async Task<IResult> RefreshApiKey([FromBody] AslBelgiRefreshApiKeyRequestDto request, CancellationToken ct)
    {
        var result = await _service.RefreshApiKeyAsync(request, ct);
        return result.IsSuccess ? Results.Ok(result.Value) : CustomResults.Problem(result);
    }

    [HttpPost("marking/request")]
    [ModuleAuthorize(PermissionCodeConst.TaxCreate)]
    public async Task<IResult> RequestMarking([FromBody] AslBelgiMarkingRequestDto request, CancellationToken ct)
    {
        var result = await _markingService.RequestMarkingAsync(request, ct);
        return result.IsSuccess ? Results.Ok(result.Value) : CustomResults.Problem(result);
    }

    [HttpPost("marking/bind")]
    [ModuleAuthorize(PermissionCodeConst.TaxCreate)]
    public async Task<IResult> BindMarkingCodes([FromBody] AslBelgiBindCodesRequestDto request, CancellationToken ct)
    {
        var result = await _markingService.FetchAndBindCodesAsync(request, ct);
        return result.IsSuccess ? Results.Ok(result.Value) : CustomResults.Problem(result);
    }
}
