using Application.Features.FaRevaluations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/fa/revaluations")]
[ApiController]
[Authorize]
public class FaRevaluationController : ControllerBase
{
    private readonly IFaRevaluationService _service;

    public FaRevaluationController(IFaRevaluationService service)
    {
        _service = service;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.FaRevaluationView)]
    public async Task<IResult> GetAllAsync([FromQuery] FaRevaluationListFilter filter, CancellationToken ct = default) =>
        (await _service.GetAllAsync(filter, ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpGet("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.FaRevaluationViewDetail)]
    public async Task<IResult> GetByIdAsync([FromRoute] long id, CancellationToken ct = default) =>
        (await _service.GetByIdAsync(id, ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.FaRevaluationCreate)]
    public async Task<IResult> CreateAsync([FromBody] FaRevaluationCreateDto dto, CancellationToken ct = default) =>
        (await _service.CreateAsync(dto, ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpPut("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.FaRevaluationUpdate)]
    public async Task<IResult> UpdateAsync([FromRoute] long id, [FromBody] FaRevaluationUpdateDto dto, CancellationToken ct = default) =>
        (await _service.UpdateAsync(id, dto, ct)).Match(Results.NoContent, CustomResults.Problem);

    [HttpPut("{id:long}/confirm")]
    [ModuleAuthorize(PermissionCodeConst.ConfirmFaRevaluation)]
    public async Task<IResult> ConfirmAsync([FromRoute] long id, CancellationToken ct = default) =>
        (await _service.ConfirmAsync(id, ct)).Match(Results.NoContent, CustomResults.Problem);

    [HttpPut("{id:long}/cancel")]
    [ModuleAuthorize(PermissionCodeConst.CancelFaRevaluation)]
    public async Task<IResult> CancelAsync([FromRoute] long id, CancellationToken ct = default) =>
        (await _service.CancelAsync(id, ct)).Match(Results.NoContent, CustomResults.Problem);
}
