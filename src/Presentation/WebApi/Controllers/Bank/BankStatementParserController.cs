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

    public record BankStatementParseRequest(IFormFile File, BankStatementBankType BankType);

    [HttpPost("parse")]
    [Consumes("multipart/form-data")]
    [ModuleAuthorize(PermissionCodeConst.BankStatementParse)]
    public async Task<IResult> ParseAsync([FromForm] BankStatementParseRequest request, CancellationToken ct = default)
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

        if (request.File is null || request.File.Length == 0)
            return Results.BadRequest("Excel file is required.");

        if (request.File.Length > maxFileSizeBytes)
            return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);

        var extension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            return Results.BadRequest("Invalid file extension.");

        if (!allowedContentTypes.Contains(request.File.ContentType))
            return Results.BadRequest("Invalid file type.");

        if (request.File.Length < 1024)
            return Results.BadRequest("Excel file is too small.");

        await using var stream = request.File.OpenReadStream();
        if (!stream.CanRead || stream.Length == 0)
            return Results.BadRequest("Invalid file stream.");

        var result = await _service.ParseAsync(stream, request.BankType, ct);

        return result.Match(Results.Ok, CustomResults.Problem);
    }
}
