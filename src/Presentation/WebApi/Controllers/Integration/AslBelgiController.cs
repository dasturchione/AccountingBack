using Application.Features.Integration.AslBelgi.Aggregations;
using Application.Features.Integration.AslBelgi.DTOs;
using Application.Features.Integration.AslBelgi.Orders;
using Application.Features.Integration.AslBelgi.Services;
using Application.Features.Integration.AslBelgi.Utilizations;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Integration;

[Route("api/integrations/asl-belgi")]
[ApiController]
[Authorize]
public sealed class AslBelgiController(
    IAslBelgiAggregationService aggregationService,
    IAslBelgiOrderService orderService,
    IAslBelgiUtilizationService utilizationService,
    IAslBelgiVerificationService verificationService,
    IValidator<CounterpartyStatusRequestDto> counterpartyStatusValidator) : ControllerBase
{
    [HttpPost("aggregations")]
    public async Task<IResult> CreateAggregation(
        [FromBody] MarkingAggregationCreateRequestDto request,
        CancellationToken ct = default) =>
        Results.Ok(await aggregationService.CreateAggregationAsync(request, ct));

    [HttpPost("orders")]
    public async Task<IResult> CreateOrder(
        [FromBody] MarkingOrderCreateRequestDto request,
        CancellationToken ct = default) =>
        Results.Ok(await orderService.CreateOrderAsync(request, ct));

    // POST, GET emas: marking_code larni yaratadi/yangilaydi va order statusini o'zgartiradi.
    [HttpPost("orders/{orderId:long}/codes")]
    public async Task<IResult> FetchCodes(
        [FromRoute] long orderId,
        CancellationToken ct = default) =>
        Results.Ok(await orderService.FetchCodesAsync(orderId, ct));

    [HttpPost("utilizations")]
    public async Task<IResult> CreateUtilization(
        [FromBody] MarkingUtilizationCreateRequestDto request,
        CancellationToken ct = default) =>
        Results.Ok(await utilizationService.CreateUtilizationAsync(request, ct));

    [HttpPost("codes/public")]
    public async Task<IResult> GetPublicCodeInformation(
        [FromBody] MarkingCodeCheckRequestDto request,
        CancellationToken ct = default) =>
        Results.Json(await verificationService.GetPublicCodeInformationAsync(request, ct));

    [HttpPost("codes/private")]
    public async Task<IResult> GetPrivateCodeInformation(
        [FromBody] MarkingCodeCheckRequestDto request,
        CancellationToken ct = default) =>
        Results.Json(await verificationService.GetPrivateCodeInformationAsync(request, ct));

    [HttpGet("products/by-gtin")]
    public async Task<IResult> GetProductsByGtin(
        [FromQuery] ProductRegistryByGtinRequestDto request,
        CancellationToken ct = default) =>
        Results.Json(await verificationService.GetProductsByGtinAsync(request, ct));

    [HttpGet("counterparties/{tin}/status")]
    public async Task<IResult> GetCounterpartyStatus(
        [FromRoute] string tin,
        CancellationToken ct = default)
    {
        var validation = await counterpartyStatusValidator.ValidateAsync(
            new CounterpartyStatusRequestDto { Tin = tin },
            ct);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).Distinct().ToArray());

            return Results.ValidationProblem(errors);
        }

        return Results.Json(await verificationService.GetCounterpartyStatusAsync(tin, ct));
    }
}
