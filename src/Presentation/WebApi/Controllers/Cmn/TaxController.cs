using Application.Features.Cmn.Taxes;
using Application.Features.Cmn.Taxes.Integration.DTOs;
using Application.Features.Cmn.Taxes.Integration.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers.Cmn;

[Route("api/taxes")]
[ApiController]
[Authorize]
public sealed class TaxController : ControllerBase
{
    private readonly ITaxService _service;
    private readonly ITaxCalculationService _calculationService;
    private readonly ITaxResolverService _resolverService;
    private readonly ITaxIntegrationService _integrationService;

    public TaxController(
        ITaxService service,
        ITaxCalculationService calculationService,
        ITaxResolverService resolverService,
        ITaxIntegrationService integrationService)
    {
        _service = service;
        _calculationService = calculationService;
        _resolverService = resolverService;
        _integrationService = integrationService;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.TaxView)]
    public async Task<IResult> GetPaged([FromQuery] TaxListFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetPagedAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:short}")]
    [ModuleAuthorize(PermissionCodeConst.TaxViewDetail)]
    public async Task<IResult> GetById([FromRoute] short id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.TaxCreate)]
    public async Task<IResult> Create([FromBody] TaxCreateDto dto, CancellationToken ct = default)
    {
        var result = await _service.CreateAsync(dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{id:short}")]
    [ModuleAuthorize(PermissionCodeConst.TaxUpdate)]
    public async Task<IResult> Update([FromRoute] short id, [FromBody] TaxUpdateDto dto, CancellationToken ct = default)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpDelete("{id:short}")]
    [ModuleAuthorize(PermissionCodeConst.TaxDelete)]
    public async Task<IResult> Delete([FromRoute] short id, CancellationToken ct = default)
    {
        var result = await _service.DeleteAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPost("calculate")]
    [ModuleAuthorize(PermissionCodeConst.TaxCalculate)]
    public async Task<IResult> Calculate([FromBody] TaxCalculationRequestDto request, CancellationToken ct = default)
    {
        var result = await _calculationService.CalculateAsync(request, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("resolve")]
    [ModuleAuthorize(PermissionCodeConst.TaxResolve)]
    public async Task<IResult> Resolve([FromQuery] short taxTypeId, [FromQuery] int? organizationId, [FromQuery] DateOnly? effectiveDate, CancellationToken ct = default)
    {
        var resolvedOrganizationId = organizationId ?? 0;
        if (resolvedOrganizationId <= 0)
            return Results.BadRequest("OrganizationId is required.");

        var result = await _resolverService.ResolveAsync(resolvedOrganizationId, taxTypeId, effectiveDate, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("providers")]
    [ModuleAuthorize(PermissionCodeConst.TaxGetProviders)]
    public async Task<IResult> GetProviders(CancellationToken ct = default)
    {
        var result = await _integrationService.GetSupportedProvidersAsync(ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("providers/status")]
    [ModuleAuthorize(PermissionCodeConst.TaxGetProviderStatus)]
    public async Task<IResult> GetProviderStatus(CancellationToken ct = default)
    {
        var result = await _integrationService.GetProviderStatusAsync(ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("mxik/search")]
    [ModuleAuthorize(PermissionCodeConst.TaxSearchMxik)]
    public async Task<IResult> SearchMxik([FromBody] TaxLookupRequestDto request, CancellationToken ct = default)
    {
        var result = await _integrationService.SearchMxikAsync(request, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("mxik/{code}")]
    [ModuleAuthorize(PermissionCodeConst.TaxGetMxikByCode)]
    public async Task<IResult> GetMxikByCode([FromRoute] string code, CancellationToken ct = default)
    {
        var result = await _integrationService.GetMxikByCodeAsync(code, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("soliq/search")]
    [ModuleAuthorize(PermissionCodeConst.TaxSearchSoliq)]
    public async Task<IResult> SearchSoliq([FromBody] TaxLookupRequestDto request, CancellationToken ct = default)
    {
        var result = await _integrationService.SearchSoliqAsync(request, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("efaktura/submit")]
    [ModuleAuthorize(PermissionCodeConst.TaxSubmitEFaktura)]
    public async Task<IResult> SubmitEFaktura([FromBody] TaxDocumentRequestDto request, CancellationToken ct = default)
    {
        var result = await _integrationService.SubmitEFakturaAsync(request, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("efaktura/status")]
    [ModuleAuthorize(PermissionCodeConst.TaxGetEFakturaStatus)]
    public async Task<IResult> GetEFakturaStatus([FromBody] TaxDocumentRequestDto request, CancellationToken ct = default)
    {
        var result = await _integrationService.GetEFakturaStatusAsync(request, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("efaktura/cancel")]
    [ModuleAuthorize(PermissionCodeConst.TaxCancelEFaktura)]
    public async Task<IResult> CancelEFaktura([FromBody] TaxDocumentRequestDto request, CancellationToken ct = default)
    {
        var result = await _integrationService.CancelEFakturaAsync(request, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("didox/submit")]
    [ModuleAuthorize(PermissionCodeConst.TaxSubmitDidox)]
    public async Task<IResult> SubmitDidox([FromBody] TaxDocumentRequestDto request, CancellationToken ct = default)
    {
        var result = await _integrationService.SubmitDidoxAsync(request, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("didox/status")]
    [ModuleAuthorize(PermissionCodeConst.TaxGetDidoxStatus)]
    public async Task<IResult> GetDidoxStatus([FromBody] TaxDocumentRequestDto request, CancellationToken ct = default)
    {
        var result = await _integrationService.GetDidoxStatusAsync(request, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("didox/cancel")]
    [ModuleAuthorize(PermissionCodeConst.TaxCancelDidox)]
    public async Task<IResult> CancelDidox([FromBody] TaxDocumentRequestDto request, CancellationToken ct = default)
    {
        var result = await _integrationService.CancelDidoxAsync(request, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

}
