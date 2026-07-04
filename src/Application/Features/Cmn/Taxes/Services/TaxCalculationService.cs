using Application.Abstractions.Authentication;
using SharedKernel.Results;

namespace Application.Features.Cmn.Taxes;

public sealed class TaxCalculationService : ITaxCalculationService
{
    private readonly IUserContext _userContext;
    private readonly ITaxResolverService _resolverService;

    public TaxCalculationService(IUserContext userContext, ITaxResolverService resolverService)
    {
        _userContext = userContext;
        _resolverService = resolverService;
    }

    public async Task<Result<TaxCalculationResultDto>> CalculateAsync(TaxCalculationRequestDto request, CancellationToken ct = default)
    {
        var validation = await ValidateAsync(new TaxBusinessValidationRequestDto
        {
            OrganizationId = request.OrganizationId,
            TaxTypeId = request.TaxTypeId,
            Amount = request.Amount,
            CalculationMode = request.CalculationMode,
            EffectiveDate = request.EffectiveDate
        }, ct);

        if (!validation.IsSuccess)
            return Result.Failure<TaxCalculationResultDto>(validation.Error);

        var organizationId = request.OrganizationId ?? _userContext.OrganizationId!.Value;
        var effectiveDate = request.EffectiveDate ?? DateOnly.FromDateTime(DateTime.Now);

        var resolved = await _resolverService.ResolveAsync(organizationId, request.TaxTypeId, effectiveDate, ct);
        if (!resolved.IsSuccess)
            return Result.Failure<TaxCalculationResultDto>(resolved.Error);

        var precision = request.RoundingPrecision ?? 2;
        var rate = resolved.Value.Rate;
        var result = request.CalculationMode == TaxCalculationMode.Exclusive
            ? CalculateExclusive(request.Amount, rate, precision)
            : CalculateInclusive(request.Amount, rate, precision);

        return Result.Success(new TaxCalculationResultDto
        {
            OrganizationId = organizationId,
            TaxTypeId = resolved.Value.TaxTypeId,
            TaxTypeCode = resolved.Value.TaxTypeCode,
            TaxTypeName = resolved.Value.TaxTypeName,
            VatRateId = resolved.Value.VatRateId,
            VatRateCode = resolved.Value.VatRateCode,
            VatRateName = resolved.Value.VatRateName,
            Rate = rate,
            BaseAmount = result.BaseAmount,
            TaxAmount = result.TaxAmount,
            TotalAmount = result.TotalAmount,
            CalculationMode = request.CalculationMode,
            RoundingPrecision = precision
        });
    }

    public async Task<Result> ValidateAsync(TaxBusinessValidationRequestDto request, CancellationToken ct = default)
    {
        if (request.OrganizationId is null || request.OrganizationId <= 0)
            return Result.Failure(TaxBusinessErrors.OrganizationRequired(_userContext.LanguageId));

        if (request.TaxTypeId <= 0)
            return Result.Failure(TaxBusinessErrors.InvalidTaxType(request.TaxTypeId, _userContext.LanguageId));

        if (request.Amount < 0)
            return Result.Failure(TaxBusinessErrors.InvalidAmount(_userContext.LanguageId));

        if (!Enum.IsDefined(typeof(TaxCalculationMode), request.CalculationMode))
            return Result.Failure(TaxBusinessErrors.InvalidCalculationMode(_userContext.LanguageId));

        var resolved = await _resolverService.ResolveAsync(request.OrganizationId.Value, request.TaxTypeId, request.EffectiveDate, ct);
        return resolved.IsSuccess ? Result.Success() : Result.Failure(resolved.Error);
    }

    private static (decimal BaseAmount, decimal TaxAmount, decimal TotalAmount) CalculateExclusive(decimal amount, decimal rate, short precision)
    {
        var taxAmount = Round(amount * rate / 100m, precision);
        var totalAmount = Round(amount + taxAmount, precision);
        return (Round(amount, precision), taxAmount, totalAmount);
    }

    private static (decimal BaseAmount, decimal TaxAmount, decimal TotalAmount) CalculateInclusive(decimal amount, decimal rate, short precision)
    {
        var baseAmount = rate == 0m ? amount : Round(amount / (1m + rate / 100m), precision);
        var taxAmount = Round(amount - baseAmount, precision);
        return (baseAmount, taxAmount, Round(amount, precision));
    }

    private static decimal Round(decimal value, short precision) =>
        Math.Round(value, precision, MidpointRounding.AwayFromZero);
}
