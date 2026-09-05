using Application.Features.Rnt.RentalAccruals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/rental-accrual-docs")]
[ApiController]
[Authorize]
public sealed class RentalAccrualController(IRentalAccrualService service) : ControllerBase
{
    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.RentalAccrualView)]
    public async Task<IResult> GetAllAsync([FromQuery] RentalAccrualListFilter filter, CancellationToken ct)
    {
        var result = await service.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.RentalAccrualViewDetail)]
    public async Task<IResult> GetByIdAsync(long id, CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.RentalAccrualUpdate)]
    public async Task<IResult> UpdateAsync(long id, [FromBody] RentalAccrualUpdateDto dto, CancellationToken ct)
    {
        var result = await service.UpdateAsync(id, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpDelete("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.RentalAccrualDelete)]
    public async Task<IResult> DeleteAsync(long id, CancellationToken ct)
    {
        var result = await service.DeleteAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPost("generate-due")]
    [ModuleAuthorize(PermissionCodeConst.RentalAccrualGenerate)]
    public async Task<IResult> GenerateDueAsync([FromBody] RentalAccrualGenerateDueDto dto, CancellationToken ct)
    {
        var result = await service.GenerateDueAsync(dto.Year, dto.Month, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{id:long}/post")]
    [ModuleAuthorize(PermissionCodeConst.RentalAccrualPost)]
    public async Task<IResult> PostAsync(long id, CancellationToken ct)
    {
        var result = await service.PostAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPut("{id:long}/cancel")]
    [ModuleAuthorize(PermissionCodeConst.RentalAccrualCancel)]
    public async Task<IResult> CancelAsync(long id, CancellationToken ct)
    {
        var result = await service.CancelAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
