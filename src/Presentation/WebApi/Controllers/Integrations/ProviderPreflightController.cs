using Application.Abstractions.Integration;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Results;

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
            return SafeFailure(Error.Problem("Integration.ProviderInvalid", "The provider is not supported."));

        var result = await service.GetAsync(parsed, ct);
        return result.IsSuccess ? Results.Ok(result.Value) : SafeFailure(result.Error);
    }

    private static IResult SafeFailure(Error error)
    {
        var status = error.Type switch
        {
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status422UnprocessableEntity
        };

        return Results.Json(new
        {
            Ready = false,
            CredentialPresent = false,
            SessionPresent = false,
            ContractState = "Blocked",
            SafeErrorCode = MapSafeErrorCode(error.Code)
        }, statusCode: status);
    }

    private static string MapSafeErrorCode(string code) => code switch
    {
        "Integration.ProviderInvalid" => "ProviderInvalid",
        "Integration.UserRequired" => "AuthenticationRequired",
        "Integration.OrganizationRequired" => "OrganizationScopeRequired",
        "Integration.OrganizationForbidden" => "OrganizationMembershipRequired",
        "Integration.OrganizationNotFound" => "OrganizationUnavailable",
        "Integration.OrganizationTinMissing" => "OrganizationUnavailable",
        _ => "PreflightUnavailable"
    };
}
