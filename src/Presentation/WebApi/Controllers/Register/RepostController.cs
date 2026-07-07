using Application.Features.Reposting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/register/repost")]
[ApiController]
[Authorize]
public class RepostController : ControllerBase
{
    private readonly IRepostService _service;

    public RepostController(IRepostService service)
    {
        _service = service;
    }

    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.RepostRepost)]
    public async Task<IResult> RepostAsync([FromBody] RepostFilter filter, CancellationToken ct = default)
    {
        var result = await _service.RepostAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }
}
