using Application.Features.BankParsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;
using System.IO;

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
        const int maxFileSizeBytes = 5 * 1024 * 1024; // 5 MB
        var allowedContentTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "application/vnd.ms-excel",
            "application/octet-stream",
            "application/x-msexcel"
        };
        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".xlsx",
            ".xls"
        };

        if (file is null || file.Length == 0)
            return Results.BadRequest("Excel file is required.");

        if (file.Length > maxFileSizeBytes)
            return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            return Results.BadRequest("Invalid file extension.");

        if (!allowedContentTypes.Contains(file.ContentType))
            return Results.BadRequest("Invalid file type.");

        if (file.Length < 1024)
            return Results.BadRequest("Excel file is too small.");

        await using var stream = file.OpenReadStream();
        if (!stream.CanRead || stream.Length == 0)
            return Results.BadRequest("Invalid file stream.");

        var result = await _service.ParseAsync(stream, ct);

        return result.Match(Results.Ok, CustomResults.Problem);
    }
}
