using Application.Features.Cmn.CurrencyRates;
using Application.Abstractions.Integration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers.Cmn;

[Route("api/currency-rates")]
[ApiController]
[Authorize]
public sealed class CurrencyRatesController : ControllerBase
{
    private readonly ICurrencyRateService _service;
    private readonly ICurrencyRateImportService _importService;

    public CurrencyRatesController(ICurrencyRateService service, ICurrencyRateImportService importService)
    {
        _service = service;
        _importService = importService;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.CurrencyRateView)]
    public async Task<IResult> GetAll([FromQuery] CurrencyRateListFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id}")]
    [ModuleAuthorize(PermissionCodeConst.CurrencyRateViewDetail)]
    public async Task<IResult> GetById([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("latest")]
    [ModuleAuthorize(PermissionCodeConst.CurrencyRatesGetLatest)]
    public async Task<IResult> GetLatest([FromQuery] short baseCurrencyId, [FromQuery] short targetCurrencyId, CancellationToken ct = default)
    {
        var result = await _service.GetLatestAsync(baseCurrencyId, targetCurrencyId, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("history")]
    [ModuleAuthorize(PermissionCodeConst.CurrencyRatesGetHistory)]
    public async Task<IResult> GetHistory([FromQuery] short baseCurrencyId, [FromQuery] short targetCurrencyId, [FromQuery] CurrencyRateListFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetHistoryAsync(baseCurrencyId, targetCurrencyId, filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.CurrencyRateCreate)]
    public async Task<IResult> Create([FromBody] CurrencyRateCreateDto dto, CancellationToken ct = default)
    {
        var result = await _service.CreateAsync(dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{id}")]
    [ModuleAuthorize(PermissionCodeConst.CurrencyRateUpdate)]
    public async Task<IResult> Update([FromRoute] long id, [FromBody] CurrencyRateUpdateDto dto, CancellationToken ct = default)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpDelete("{id}")]
    [ModuleAuthorize(PermissionCodeConst.CurrencyRateDelete)]
    public async Task<IResult> Delete([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.DeleteAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpGet("providers")]
    [ModuleAuthorize(PermissionCodeConst.CurrencyRatesGetProviders)]
    public async Task<IResult> GetProviders(CancellationToken ct = default)
    {
        var result = await _importService.GetProvidersAsync(ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("import/status")]
    [ModuleAuthorize(PermissionCodeConst.CurrencyRatesGetImportStatus)]
    public async Task<IResult> GetImportStatus(CancellationToken ct = default)
    {
        var result = await _importService.GetStatusAsync(ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("import/latest")]
    [ModuleAuthorize(PermissionCodeConst.CurrencyRateImport)]
    public async Task<IResult> ImportLatest([FromBody] CurrencyRateImportRequestDto request, CancellationToken ct = default)
    {
        var result = await _importService.ImportLatestAsync(request.ProviderCode, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("import/date")]
    [ModuleAuthorize(PermissionCodeConst.CurrencyRateSync)]
    public async Task<IResult> ImportByDate([FromBody] CurrencyRateImportRequestDto request, CancellationToken ct = default)
    {
        if (request.Date is null)
            return Results.BadRequest("Date is required.");

        var result = await _importService.ImportByDateAsync(request.ProviderCode, request.Date.Value, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }
}
