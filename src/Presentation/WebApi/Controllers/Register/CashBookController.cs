using Application.Features.CashDocuments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/register/cash-book")]
[ApiController]
[Authorize]
public class CashBookController : ControllerBase
{
    private readonly ICashBookService _service;

    public CashBookController(ICashBookService service)
    {
        _service = service;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.CashOperationView)]
    public async Task<IResult> GetAsync([FromQuery] CashBookFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }
}
