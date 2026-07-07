using Application.Abstractions.Barcode;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers.Cmn;

[Route("api/barcode")]
[ApiController]
[Authorize]
public sealed class BarcodeController : ControllerBase
{
    private readonly IBarcodeGenerator _generator;

    public BarcodeController(IBarcodeGenerator generator)
    {
        _generator = generator;
    }

    [ModuleAuthorize(PermissionCodeConst.BarcodeGenerateQr)]
    [HttpGet("qr")]
    public IResult GenerateQr([FromQuery] string content, [FromQuery] int pixelsPerModule = 20)
        => _generator.GenerateQr(content, pixelsPerModule)
            .Match(bytes => Results.File(bytes, "image/png"), CustomResults.Problem);

    [ModuleAuthorize(PermissionCodeConst.BarcodeGenerateCode128)]
    [HttpGet("code128")]
    public IResult GenerateCode128([FromQuery] string content)
        => _generator.GenerateBarcode(content, BarcodeFormat.Code128)
            .Match(bytes => Results.File(bytes, "image/png"), CustomResults.Problem);

    [ModuleAuthorize(PermissionCodeConst.BarcodeGenerateEan13)]
    [HttpGet("ean13")]
    public IResult GenerateEan13([FromQuery] string content)
        => _generator.GenerateBarcode(content, BarcodeFormat.Ean13)
            .Match(bytes => Results.File(bytes, "image/png"), CustomResults.Problem);
}
