namespace Application.Features.Pay.Payments;

/// <summary>A payroll line an employee payment can be applied to, with its remaining outstanding.</summary>
public readonly record struct PayrollPaymentAllocationTarget(long PayrollLineId, decimal Outstanding);

/// <summary>How much of a payment is applied to one payroll line.</summary>
public readonly record struct PayrollPaymentAllocationItem(long PayrollLineId, decimal Amount);

/// <summary>
/// Splits one employee payment amount across that employee's payroll lines in the given
/// order (regular run first, then WITH_SALARY corrections), capping each line at its own
/// outstanding. This lets a single final payment cover a regular run together with its
/// corrections while keeping each payroll line's paid amount tracked precisely.
/// </summary>
public static class PayrollPaymentAllocator
{
    public static IReadOnlyList<PayrollPaymentAllocationItem> Allocate(
        decimal requested,
        IReadOnlyList<PayrollPaymentAllocationTarget> targets)
    {
        ArgumentNullException.ThrowIfNull(targets);
        var result = new List<PayrollPaymentAllocationItem>();
        if (requested <= 0m)
            return result;

        var remaining = requested;
        foreach (var target in targets)
        {
            if (remaining <= 0m)
                break;
            var available = Math.Max(0m, target.Outstanding);
            var take = Math.Min(remaining, available);
            if (take > 0m)
            {
                result.Add(new PayrollPaymentAllocationItem(target.PayrollLineId, take));
                remaining -= take;
            }
        }
        return result;
    }

    /// <summary>Total remaining outstanding across the targets (never negative).</summary>
    public static decimal TotalOutstanding(IReadOnlyList<PayrollPaymentAllocationTarget> targets) =>
        targets.Sum(t => Math.Max(0m, t.Outstanding));
}
