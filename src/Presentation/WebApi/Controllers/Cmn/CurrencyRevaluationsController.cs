using Application.Features.Cmn.CurrencyRevaluations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers.Cmn;

[Route("api/currency-revaluations")]
[ApiController]
[Authorize]
public sealed class CurrencyRevaluationsController : ControllerBase
{
    private readonly ICurrencyRevaluationService _service;
    public CurrencyRevaluationsController(ICurrencyRevaluationService service) => _service = service;

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.CurrencyRevaluationView)]
    public async Task<IResult> GetAll([FromQuery] CurrencyRevaluationListFilter filter, CancellationToken ct = default)
        => (await _service.GetAllAsync(filter, ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpGet("{id}")]
    [ModuleAuthorize(PermissionCodeConst.CurrencyRevaluationViewDetail)]
    public async Task<IResult> GetById([FromRoute] long id, CancellationToken ct = default)
        => (await _service.GetByIdAsync(id, ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpPost("preview")]
    [ModuleAuthorize(PermissionCodeConst.CurrencyRevaluationPreview)]
    public async Task<IResult> Preview([FromBody] CurrencyRevaluationPreviewDto dto, CancellationToken ct = default)
        => (await _service.PreviewAsync(dto, ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.CurrencyRevaluationCreate)]
    public async Task<IResult> Create([FromBody] CurrencyRevaluationCreateDto dto, CancellationToken ct = default)
        => (await _service.CreateAsync(dto, ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpPost("{id}/confirm")]
    [ModuleAuthorize(PermissionCodeConst.CurrencyRevaluationConfirm)]
    public async Task<IResult> Confirm([FromRoute] long id, CancellationToken ct = default)
        => (await _service.ConfirmAsync(id, ct)).Match(Results.NoContent, CustomResults.Problem);

    [HttpPost("{id}/cancel")]
    [ModuleAuthorize(PermissionCodeConst.CurrencyRevaluationCancel)]
    public async Task<IResult> Cancel([FromRoute] long id, CancellationToken ct = default)
        => (await _service.CancelAsync(id, ct)).Match(Results.NoContent, CustomResults.Problem);
}
