using Application.Features.AccountingRegisterEntries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using System.ComponentModel.DataAnnotations;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/register/accounting-register-entries")]
[ApiController]
[Authorize]
public class AccountingRegisterEntryController : ControllerBase
{
    private readonly IAccountingRegisterEntryService _service;
    private readonly IAccountingRegisterEntryRebuildService _rebuildService;

    public AccountingRegisterEntryController(
        IAccountingRegisterEntryService service,
        IAccountingRegisterEntryRebuildService rebuildService)
    {
        _service = service;
        _rebuildService = rebuildService;
    }

    [HttpGet("postings")]
    [ModuleAuthorize(PermissionCodeConst.AccountingRegisterEntryGetPostings)]
    public async Task<IResult> GetPostingsAsync([Required] [FromQuery] short documentTypeId, [Required] [FromQuery] long documentId, CancellationToken ct = default)
    {
        var result = await _service.GetPostingAsync(documentTypeId, documentId, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("postings/daily")]
    [ModuleAuthorize(PermissionCodeConst.AccountingRegisterEntryGetDailyPostings)]
    public async Task<IResult> GetDailyPostingsAsync([Required][FromQuery] DateTime startDate, [Required][FromQuery] DateTime endDate, [FromQuery] short? documentTypeId, CancellationToken ct = default)
    {
        var result = await _service.GetDailyPostingAsync(startDate, endDate, documentTypeId, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("rebuild")]
    [ModuleAuthorize(PermissionCodeConst.RepostRepost)]
    public async Task<IResult> RebuildAsync(
        [Required][FromQuery] short documentTypeId,
        [Required][FromQuery] long documentId,
        CancellationToken ct = default)
    {
        var result = await _rebuildService.RebuildAsync(documentTypeId, documentId, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }
}
