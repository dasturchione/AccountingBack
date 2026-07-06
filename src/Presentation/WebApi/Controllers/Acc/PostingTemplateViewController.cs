using Application.Features.Acc.PostingTemplateViews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers.Acc;

[Route("api/posting-template-views")]
[ApiController]
[Authorize]
public class PostingTemplateViewController : ControllerBase
{
    private readonly IPostingTemplateViewService _service;

    public PostingTemplateViewController(IPostingTemplateViewService service)
    {
        _service = service;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.PostingRuleView)]
    public async Task<IResult> GetAllAsync(CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:int}")]
    [ModuleAuthorize(PermissionCodeConst.PostingRuleViewDetail)]
    public async Task<IResult> GetByIdAsync([FromRoute] short id, [FromQuery] short policyId = 1, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, policyId, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }
}
