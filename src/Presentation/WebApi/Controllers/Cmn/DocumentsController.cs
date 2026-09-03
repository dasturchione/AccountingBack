using Application.Features.Cmn.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers.Cmn;

[Route("api/documents")]
[ApiController]
[Authorize]
public sealed class DocumentsController(IDocumentRegistryService service) : ControllerBase
{
    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.BankOperationView)]
    public async Task<IResult> GetAll(
        [FromQuery] DocumentRegistryListFilter filter,
        CancellationToken ct = default)
    {
        var result = await service.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.BankOperationViewDetail)]
    public async Task<IResult> GetById([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await service.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }
}
