using Application.Features.PaymentAcceptancePoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/payment-acceptance-points")]
[ApiController]
[Authorize]
public class PaymentAcceptancePointController : ControllerBase
{
    private readonly IPaymentAcceptancePointService _service;

    public PaymentAcceptancePointController(IPaymentAcceptancePointService service)
    {
        _service = service;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.PaymentAcceptancePointView)]
    public async Task<IResult> GetAllAsync([FromQuery] PaymentAcceptancePointListFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:int}")]
    [ModuleAuthorize(PermissionCodeConst.PaymentAcceptancePointViewDetail)]
    public async Task<IResult> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.PaymentAcceptancePointCreate)]
    public async Task<IResult> CreateAsync([FromBody] PaymentAcceptancePointCreateDto dto, CancellationToken ct = default)
    {
        var result = await _service.CreateAsync(dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{id:int}")]
    [ModuleAuthorize(PermissionCodeConst.PaymentAcceptancePointUpdate)]
    public async Task<IResult> UpdateAsync(int id, [FromBody] PaymentAcceptancePointUpdateDto dto, CancellationToken ct = default)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpDelete("{id:int}")]
    [ModuleAuthorize(PermissionCodeConst.PaymentAcceptancePointDelete)]
    public async Task<IResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var result = await _service.DeleteAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
