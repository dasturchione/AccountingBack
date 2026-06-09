using Application.Features.CounterpartyCards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/counterparty-cards")]
[ApiController]
[Authorize]
public class CounterpartyCardController : ControllerBase
{
    private readonly ICounterpartyCardService _service;

    public CounterpartyCardController(ICounterpartyCardService service)
    {
        _service = service;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.CounterpartyCardView)]
    public async Task<IResult> GetAll([FromQuery] CounterpartyCardListFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:int}")]
    [ModuleAuthorize(PermissionCodeConst.CounterpartyCardViewDetail)]
    public async Task<IResult> GetById([FromRoute] int id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.CounterpartyCardCreate)]
    public async Task<IResult> Create([FromBody] CounterpartyCardCreateDto dto, CancellationToken ct = default)
    {
        var result = await _service.CreateAsync(dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{id:int}")]
    [ModuleAuthorize(PermissionCodeConst.CounterpartyCardUpdate)]
    public async Task<IResult> Update([FromRoute] int id, [FromBody] CounterpartyCardUpdateDto dto, CancellationToken ct = default)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpDelete("{id:int}")]
    [ModuleAuthorize(PermissionCodeConst.CounterpartyCardDelete)]
    public async Task<IResult> Delete([FromRoute] int id, CancellationToken ct = default)
    {
        var result = await _service.DeleteAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
