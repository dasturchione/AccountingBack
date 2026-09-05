using Application.Features.Rnt.RentalContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/rental-contracts")]
[ApiController]
[Authorize]
public sealed class RentalContractController(IRentalContractService service) : ControllerBase
{
    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.RentalContractView)]
    public async Task<IResult> GetAllAsync([FromQuery] RentalContractListFilter filter, CancellationToken ct)
    {
        var result = await service.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.RentalContractViewDetail)]
    public async Task<IResult> GetByIdAsync(long id, CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.RentalContractCreate)]
    public async Task<IResult> CreateAsync([FromBody] RentalContractCreateDto dto, CancellationToken ct)
    {
        var result = await service.CreateAsync(dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.RentalContractUpdate)]
    public async Task<IResult> UpdateAsync(long id, [FromBody] RentalContractUpdateDto dto, CancellationToken ct)
    {
        var result = await service.UpdateAsync(id, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpDelete("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.RentalContractDelete)]
    public async Task<IResult> DeleteAsync(long id, CancellationToken ct)
    {
        var result = await service.DeleteAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPut("{id:long}/activate")]
    [ModuleAuthorize(PermissionCodeConst.RentalContractActivate)]
    public async Task<IResult> ActivateAsync(long id, [FromQuery] DateTime? confirmationDate, CancellationToken ct)
    {
        var result = await service.ActivateAsync(id, confirmationDate, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPut("{id:long}/cancel")]
    [ModuleAuthorize(PermissionCodeConst.RentalContractCancel)]
    public async Task<IResult> CancelAsync(long id, [FromQuery] DateTime? terminationDate, CancellationToken ct)
    {
        var result = await service.CancelAsync(id, terminationDate, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
