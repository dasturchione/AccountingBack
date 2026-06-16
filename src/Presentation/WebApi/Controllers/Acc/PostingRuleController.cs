using Application.Features.Acc.PostingRules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers.Acc;

[Route("api/posting-rules")]
[ApiController]
[Authorize]
public class PostingRuleController : ControllerBase
{
    private readonly IPostingRuleService _service;

    public PostingRuleController(IPostingRuleService service)
    {
        _service = service;
    }

    [HttpGet]
    //[ModuleAuthorize(PermissionCodeConst.ChartAccountView)]
    public async Task<IResult> GetAllAsync([FromQuery] PostingRuleListFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:int}")]
    //[ModuleAuthorize(PermissionCodeConst.ChartAccountViewDetail)]
    public async Task<IResult> GetByIdAsync([FromRoute] int id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost]
    //[ModuleAuthorize(PermissionCodeConst.ChartAccountCreate)]
    public async Task<IResult> CreateAsync([FromBody] PostingRuleCreateDto dto, CancellationToken ct = default)
    {
        var result = await _service.CreateAsync(dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{id:int}")]
    //[ModuleAuthorize(PermissionCodeConst.ChartAccountUpdate)]
    public async Task<IResult> UpdateAsync([FromRoute] int id, [FromBody] PostingRuleUpdateDto dto, CancellationToken ct = default)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpDelete("{id:int}")]
    //[ModuleAuthorize(PermissionCodeConst.ChartAccountDelete)]
    public async Task<IResult> DeleteAsync([FromRoute] int id, CancellationToken ct = default)
    {
        var result = await _service.DeleteAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
