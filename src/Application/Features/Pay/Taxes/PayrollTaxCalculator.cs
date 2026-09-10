namespace Application.Features.Pay.Taxes;

/// <summary>Inputs captured by a payroll tax registry line.</summary>
public sealed record PayrollTaxCalculationInput(
    decimal BaseAmount,
    decimal Rate,
    decimal ExemptionAmount = 0m,
    decimal? LimitAmount = null);

/// <summary>Deterministic tax calculation result used for withholding and employer taxes.</summary>
public sealed record PayrollTaxCalculationResult(
    decimal TaxableBase,
    decimal Amount);

public static class PayrollTaxCalculator
{
    public static PayrollTaxCalculationResult Calculate(PayrollTaxCalculationInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var baseAmount = Math.Max(input.BaseAmount, 0m);
        var exemption = Math.Max(input.ExemptionAmount, 0m);
        var taxableBase = Math.Max(baseAmount - exemption, 0m);
        if (input.LimitAmount is { } limit)
            taxableBase = Math.Min(taxableBase, Math.Max(limit, 0m));

        var rate = Math.Clamp(input.Rate, 0m, 100m);
        var amount = decimal.Round(taxableBase * rate / 100m, 2, MidpointRounding.AwayFromZero);
        return new PayrollTaxCalculationResult(decimal.Round(taxableBase, 2), amount);
    }
}
