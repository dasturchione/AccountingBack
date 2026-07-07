using Application.Features.OrganizationSetup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/setup")]
[ApiController]
[Authorize]
public sealed class SetupController : ControllerBase
{
    private readonly IOrganizationSetupService _setupService;

    public SetupController(IOrganizationSetupService setupService)
    {
        _setupService = setupService;
    }

    [ModuleAuthorize(PermissionCodeConst.SetupGet)]
    [HttpGet]
    public async Task<IResult> GetAsync(CancellationToken ct = default)
    {
        var response = await _setupService.GetAsync(ct);
        return response.Match(Results.Ok, CustomResults.Problem);
    }

    [ModuleAuthorize(PermissionCodeConst.SetupUpdateCompanyProfile)]
    [HttpPut("company-profile")]
    public async Task<IResult> UpdateCompanyProfileAsync([FromBody] OrganizationSetupCompanyProfileDto dto, CancellationToken ct = default)
    {
        var response = await _setupService.UpdateCompanyProfileAsync(dto, ct);
        return response.Match(Results.NoContent, CustomResults.Problem);
    }

    [ModuleAuthorize(PermissionCodeConst.SetupUpdateTaxSettings)]
    [HttpPut("tax-settings")]
    public async Task<IResult> UpdateTaxSettingsAsync([FromBody] OrganizationSetupTaxSettingsDto dto, CancellationToken ct = default)
    {
        var response = await _setupService.UpdateTaxSettingsAsync(dto, ct);
        return response.Match(Results.NoContent, CustomResults.Problem);
    }

    [ModuleAuthorize(PermissionCodeConst.SetupUpdateAccountingPolicy)]
    [HttpPut("accounting-policy")]
    public async Task<IResult> UpdateAccountingPolicyAsync([FromBody] OrganizationSetupAccountingPolicyDto dto, CancellationToken ct = default)
    {
        var response = await _setupService.UpdateAccountingPolicyAsync(dto, ct);
        return response.Match(Results.NoContent, CustomResults.Problem);
    }

    [ModuleAuthorize(PermissionCodeConst.SetupUpdateDefaults)]
    [HttpPut("defaults")]
    public async Task<IResult> UpdateDefaultsAsync([FromBody] OrganizationSetupDefaultsDto dto, CancellationToken ct = default)
    {
        var response = await _setupService.UpdateDefaultsAsync(dto, ct);
        return response.Match(Results.NoContent, CustomResults.Problem);
    }

    [ModuleAuthorize(PermissionCodeConst.SetupUpdateUsers)]
    [HttpPut("users")]
    public async Task<IResult> UpdateUsersAsync([FromBody] OrganizationSetupUsersDto dto, CancellationToken ct = default)
    {
        var response = await _setupService.UpdateUsersAsync(dto, ct);
        return response.Match(Results.NoContent, CustomResults.Problem);
    }

    [ModuleAuthorize(PermissionCodeConst.SetupComplete)]
    [HttpPost("complete")]
    public async Task<IResult> CompleteAsync(CancellationToken ct = default)
    {
        var response = await _setupService.CompleteAsync(ct);
        return response.Match(Results.NoContent, CustomResults.Problem);
    }
}
