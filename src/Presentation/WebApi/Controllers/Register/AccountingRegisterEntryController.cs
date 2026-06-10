using Application.Features.AccountingRegisterEntries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/register/accounting-register-entries")]
[ApiController]
[Authorize]
public class AccountingRegisterEntryController : ControllerBase
{
    private readonly IAccountingRegisterEntryService _service;

    public AccountingRegisterEntryController(IAccountingRegisterEntryService service)
    {
        _service = service;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.AccRegEntryView)]
    public async Task<IResult> GetAllAsync([FromQuery] AccountingRegisterEntryListFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.AccRegEntryViewDetail)]
    public async Task<IResult> GetByIdAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    //[HttpPost]
    //[ModuleAuthorize(PermissionCodeConst.AccRegEntryCreate)]
    //public async Task<IResult> CreateAsync([FromBody] AccountingRegisterEntryCreateDto dto, CancellationToken ct = default)
    //{
    //    var result = await _service.CreateAsync(dto, ct);
    //    return result.Match(Results.Ok, CustomResults.Problem);
    //}

    //[HttpPut("{id:long}")]
    //[ModuleAuthorize(PermissionCodeConst.AccRegEntryUpdate)]
    //public async Task<IResult> UpdateAsync([FromRoute] long id, [FromBody] AccountingRegisterEntryUpdateDto dto, CancellationToken ct = default)
    //{
    //    var result = await _service.UpdateAsync(id, dto, ct);
    //    return result.Match(Results.NoContent, CustomResults.Problem);
    //}

    //[HttpDelete("{id:long}")]
    //[ModuleAuthorize(PermissionCodeConst.AccRegEntryDelete)]
    //public async Task<IResult> DeleteAsync([FromRoute] long id, CancellationToken ct = default)
    //{
    //    var result = await _service.DeleteAsync(id, ct);
    //    return result.Match(Results.NoContent, CustomResults.Problem);
    //}
}
