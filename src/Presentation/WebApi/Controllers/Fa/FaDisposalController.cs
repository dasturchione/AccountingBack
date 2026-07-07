using Application.Features.FaDisposals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/fa/disposals")]
[ApiController]
[Authorize]
public class FaDisposalController : ControllerBase
{
    private readonly IFaDisposalService _service;

    public FaDisposalController(IFaDisposalService service)
    {
        _service = service;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.FaDisposalView)]
    public async Task<IResult> GetAllAsync([FromQuery] FaDisposalListFilter filter, CancellationToken ct = default) =>
        (await _service.GetAllAsync(filter, ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpGet("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.FaDisposalViewDetail)]
    public async Task<IResult> GetByIdAsync([FromRoute] long id, CancellationToken ct = default) =>
        (await _service.GetByIdAsync(id, ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.FaDisposalCreate)]
    public async Task<IResult> CreateAsync([FromBody] FaDisposalCreateDto dto, CancellationToken ct = default) =>
        (await _service.CreateAsync(dto, ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpPut("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.FaDisposalUpdate)]
    public async Task<IResult> UpdateAsync([FromRoute] long id, [FromBody] FaDisposalUpdateDto dto, CancellationToken ct = default) =>
        (await _service.UpdateAsync(id, dto, ct)).Match(Results.NoContent, CustomResults.Problem);

    [HttpPut("{id:long}/confirm")]
    [ModuleAuthorize(PermissionCodeConst.ConfirmFaDisposal)]
    public async Task<IResult> ConfirmAsync([FromRoute] long id, CancellationToken ct = default) =>
        (await _service.ConfirmAsync(id, ct)).Match(Results.NoContent, CustomResults.Problem);

    [HttpPut("{id:long}/cancel")]
    [ModuleAuthorize(PermissionCodeConst.CancelFaDisposal)]
    public async Task<IResult> CancelAsync([FromRoute] long id, CancellationToken ct = default) =>
        (await _service.CancelAsync(id, ct)).Match(Results.NoContent, CustomResults.Problem);
}
