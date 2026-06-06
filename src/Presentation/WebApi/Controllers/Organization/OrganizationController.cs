using Application.Features.Organizations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/organizations")]
[ApiController]
[Authorize]
public class OrganizationController : ControllerBase
{
    private readonly IOrganizationService _organizationService;

    public OrganizationController(IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    [HttpGet]
    public async Task<IResult> GetAllAsync([FromQuery] OrganizationListFilter filter, CancellationToken ct = default)
    {
        var response = await _organizationService.GetAllAsync(filter, ct);
        return response.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:int}")]
    public async Task<IResult> GetByIdAsync([FromRoute] int id, CancellationToken ct = default)
    {
        var response = await _organizationService.GetByIdAsync(id, ct);
        return response.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost]
    public async Task<IResult> CreateAsync([FromBody] OrganizationCreateDto dto, CancellationToken ct = default)
    {
        var response = await _organizationService.CreateAsync(dto, ct);
        return response.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{id:int}")]
    public async Task<IResult> UpdateAsync([FromRoute] int id, [FromBody] OrganizationUpdateDto dto, CancellationToken ct = default)
    {
        var response = await _organizationService.UpdateAsync(id, dto, ct);
        return response.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpDelete("{id:int}")]
    public async Task<IResult> DeleteAsync([FromRoute] int id, CancellationToken ct = default)
    {
        var response = await _organizationService.DeleteAsync(id, ct);
        return response.Match(Results.NoContent, CustomResults.Problem);
    }
}
