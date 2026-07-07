using Application.Abstractions.Import;
using Application.Features.Imports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers.Inv;

[Route("api/products/import")]
[ApiController]
[Authorize]
public sealed class ProductImportController : ControllerBase
{
    private readonly IExcelImporter _importer;

    public ProductImportController(IExcelImporter importer)
    {
        _importer = importer;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [ModuleAuthorize(PermissionCodeConst.ProductImportImport)]
    public async Task<IResult> Import(IFormFile file, CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            return Results.BadRequest("Fayl talab qilinadi.");

        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".xlsx", ".csv" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            return Results.BadRequest("Faqat .xlsx yoki .csv qabul qilinadi.");

        await using var stream = file.OpenReadStream();

        // DIQQAT: bu bosqichda FAQAT parse — natija (qatorlar + qator-xatolar) qaytariladi,
        // DB ga SAQLANMAYDI. Saqlash/validatsiya keyingi bosqich (ProductService bilan).
        var options = new ExcelImportOptions();
        var result = await _importer.ImportAsync<ProductImportRowDto>(stream, options, ct);

        return result.Match(Results.Ok, CustomResults.Problem);
    }
}
