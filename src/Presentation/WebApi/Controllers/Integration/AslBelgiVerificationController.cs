using Application.Features.Integration.AslBelgi.DTOs;
using Application.Features.Integration.AslBelgi.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Integration;

[Route("api/integrations/asl-belgi")]
[ApiController]
[Authorize]
public sealed class AslBelgiVerificationController : ControllerBase
{
    private readonly IAslBelgiVerificationService _service;
    private readonly IValidator<CounterpartyStatusRequestDto> _counterpartyStatusValidator;

    public AslBelgiVerificationController(
        IAslBelgiVerificationService service,
        IValidator<CounterpartyStatusRequestDto> counterpartyStatusValidator)
    {
        _service = service;
        _counterpartyStatusValidator = counterpartyStatusValidator;
    }

    [HttpPost("codes/public")]
    public async Task<IResult> GetPublicCodeInformation([FromBody] MarkingCodeCheckRequestDto request, CancellationToken ct = default)
        => Results.Json(await _service.GetPublicCodeInformationAsync(request, ct));

    [HttpPost("codes/private")]
    public async Task<IResult> GetPrivateCodeInformation([FromBody] MarkingCodeCheckRequestDto request, CancellationToken ct = default)
        => Results.Json(await _service.GetPrivateCodeInformationAsync(request, ct));

    [HttpGet("products/by-gtin")]
    public async Task<IResult> GetProductsByGtin([FromQuery] ProductRegistryByGtinRequestDto request, CancellationToken ct = default)
        => Results.Json(await _service.GetProductsByGtinAsync(request, ct));

    [HttpGet("counterparties/{tin}/status")]
    public async Task<IResult> GetCounterpartyStatus([FromRoute] string tin, CancellationToken ct = default)
    {
        var validation = await _counterpartyStatusValidator.ValidateAsync(new CounterpartyStatusRequestDto { Tin = tin }, ct);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).Distinct().ToArray());

            return Results.ValidationProblem(errors);
        }

        return Results.Json(await _service.GetCounterpartyStatusAsync(tin, ct));
    }
}
