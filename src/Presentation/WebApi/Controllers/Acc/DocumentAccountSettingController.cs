using Application.Features.Acc.DocumentAccountSettings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/document-account-settings")]
[ApiController]
[Authorize]
public class DocumentAccountSettingController : ControllerBase
{
    private readonly IDocumentAccountSettingService _service;

    public DocumentAccountSettingController(IDocumentAccountSettingService service)
    {
        _service = service;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.DocumentAccountSettingView)]
    public async Task<IResult> GetAllAsync([FromQuery] DocumentAccountSettingListFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{documentTypeId:short}/chart-accounts")]
    [ModuleAuthorize(PermissionCodeConst.DocumentAccountSettingView)]
    public async Task<IResult> GetSelectListAsync([FromRoute] short documentTypeId, [FromQuery] short? documentRoleId, [FromQuery] string? documentRoleCode, CancellationToken ct = default)
    {
        var result = await _service.GetSelectListAsync(documentTypeId, documentRoleId, documentRoleCode, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{documentTypeId:short}")]
    [ModuleAuthorize(PermissionCodeConst.DocumentAccountSettingViewDetail)]
    public async Task<IResult> GetByDocumentTypeIdAsync([FromRoute] short documentTypeId, CancellationToken ct = default)
    {
        var result = await _service.GetByDocumentTypeIdAsync(documentTypeId, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.DocumentAccountSettingSave)]
    public async Task<IResult> SaveAsync([FromBody] DocumentAccountRuleSettingSaveDto dto, CancellationToken ct = default)
    {
        var result = await _service.SaveAsync(dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }
}