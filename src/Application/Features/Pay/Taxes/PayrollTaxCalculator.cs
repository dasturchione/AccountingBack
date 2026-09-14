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

/// <summary>
/// What a tax has already taken from an employee inside the current payroll period, so a
/// later document in that period charges only the difference.
/// </summary>
public readonly record struct PayrollPriorTax(decimal BaseAmount, decimal Amount)
{
    public static readonly PayrollPriorTax None = new(0m, 0m);
}

public static class PayrollTaxCalculator
{
    public static PayrollTaxCalculationResult Calculate(PayrollTaxCalculationInput input) =>
        CalculateIncremental(input, PayrollPriorTax.None);

    /// <summary>
    /// Charges <paramref name="input"/>'s base on top of what the period has already taxed.
    /// <para>
    /// The tax is worked out on the period-to-date base (1C's «нарастающим итогом»), and what
    /// has already been charged is subtracted from it. That is the only way the monthly
    /// exemption and the taxable-base limit stay correct once a period holds more than one
    /// document: computing a correction from scratch handed the employee a second exemption
    /// and let the limit restart, and computing nothing at all — which is what correction
    /// documents used to do — left added earnings completely untaxed.
    /// </para>
    /// <para>
    /// The incremental base may be negative (an earning being taken back), in which case the
    /// result is negative too: tax that was over-withheld is returned. Only the period-to-date
    /// base is floored at zero. With no prior amounts this is an ordinary one-shot calculation.
    /// </para>
    /// </summary>
    public static PayrollTaxCalculationResult CalculateIncremental(
        PayrollTaxCalculationInput input,
        PayrollPriorTax prior)
    {
        ArgumentNullException.ThrowIfNull(input);

        var exemption = Math.Max(input.ExemptionAmount, 0m);
        var rate = Math.Clamp(input.Rate, 0m, 100m);

        var priorBase = Math.Max(prior.BaseAmount, 0m);
        var periodToDateBase = Math.Max(priorBase + input.BaseAmount, 0m);

        var priorTaxableBase = TaxableBase(priorBase, exemption, input.LimitAmount);
        var periodToDateTaxableBase = TaxableBase(periodToDateBase, exemption, input.LimitAmount);

        var periodToDateAmount = decimal.Round(
            periodToDateTaxableBase * rate / 100m,
            2,
            MidpointRounding.AwayFromZero);

        return new PayrollTaxCalculationResult(
            decimal.Round(periodToDateTaxableBase - priorTaxableBase, 2),
            decimal.Round(periodToDateAmount - prior.Amount, 2, MidpointRounding.AwayFromZero));
    }

    private static decimal TaxableBase(decimal baseAmount, decimal exemption, decimal? limitAmount)
    {
        var taxableBase = Math.Max(baseAmount - exemption, 0m);
        if (limitAmount is { } limit)
            taxableBase = Math.Min(taxableBase, Math.Max(limit, 0m));
        return taxableBase;
    }
}
