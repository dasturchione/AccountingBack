using Application.Abstractions.Integration;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Results;
using WebApi.Infrastructure;

namespace WebApi.Controllers.Integrations;

[ApiController]
[Authorize]
[Route("api/integrations")]
public sealed class ProviderPreflightController(IProviderPreflightService service) : ControllerBase
{
    [HttpGet("{provider}/preflight")]
    public async Task<IResult> Get(string provider, CancellationToken ct = default)
    {
        if (!Enum.TryParse<Provider>(provider, ignoreCase: true, out var parsed)
            || !Enum.IsDefined(parsed)
            || !string.Equals(provider, parsed.ToString(), StringComparison.OrdinalIgnoreCase))
            return CustomResults.Problem(Result.Failure(
                Error.Problem("Integration.ProviderInvalid", "The provider is not supported.")));

        var result = await service.GetAsync(parsed, ct);
        return result.IsSuccess ? Results.Ok(result.Value) : CustomResults.Problem(result);
    }
}
