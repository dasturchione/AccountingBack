using Application.Features.BankParsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/bank-statement-parser")]
[ApiController]
[Authorize]
public class BankStatementParserController : ControllerBase
{
    private readonly IBankStatementParserService _service;

    public BankStatementParserController(IBankStatementParserService service)
    {
        _service = service;
    }

    [HttpPost("parse")]
    [Consumes("multipart/form-data")]
    [ModuleAuthorize(PermissionCodeConst.BankStatementParse)]
    public async Task<IResult> ParseAsync(IFormFile file, CancellationToken ct = default)
    {
        if (file.Length == 0)
            return Results.BadRequest("Excel file is empty.");

        await using var stream = file.OpenReadStream();
        var result = await _service.ParseAsync(stream, ct);

        return result.Match(Results.Ok, CustomResults.Problem);
    }
}
