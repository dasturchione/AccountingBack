using Domain.Entities;
using SharedKernel.Constants;

namespace Application.Features.Pay.PayrollDocuments;

/// <summary>An account carrying part of a payroll line's earnings, and the amount put on it.</summary>
public readonly record struct PayrollAccountShare(int AccountId, decimal Amount);

/// <summary>
/// Resolves the counter-account of a payroll tax or deduction from the earnings of the same
/// employee line, instead of taking it from the document header.
/// <para>
/// A withholding (НДФЛ, ИНПС) reduces what the employee is owed, so it must be debited from
/// the very accounts the earnings credited. An employer tax (ЕСП) is a cost of employing that
/// person, so it must be debited to the very accounts the earnings were charged to. Taking
/// both from the document header meant a component or employment with its own accounts had its
/// payable credited on one account and debited on another (so it never closed), and had the
/// social tax booked to an administrative cost account while the salary sat in production.
/// </para>
/// <para>
/// When the earnings span several accounts the amount is split across them in proportion to
/// the earnings each one carries, using largest-remainder rounding so the parts always add up
/// to the amount being allocated.
/// </para>
/// </summary>
public static class PayrollPostingAccountAllocator
{
    /// <summary>Accounts the line's earnings were charged to (debit side), with their weights.</summary>
    public static IReadOnlyList<PayrollAccountShare> ExpenseWeights(PayPayrollLine line) =>
        Weights(line, static calc => calc.DebitAccountId);

    /// <summary>Accounts the line's earnings are payable on (credit side), with their weights.</summary>
    public static IReadOnlyList<PayrollAccountShare> PayableWeights(PayPayrollLine line) =>
        Weights(line, static calc => calc.CreditAccountId);

    /// <summary>
    /// Splits <paramref name="amount"/> over <paramref name="weights"/> in proportion to each
    /// account's share. Falls back to <paramref name="fallbackAccountId"/> when the line has no
    /// earnings to weigh (a correction that only carries deductions, for example).
    /// </summary>
    public static List<PayrollAccountShare> Allocate(
        IReadOnlyList<PayrollAccountShare> weights,
        decimal amount,
        int? fallbackAccountId)
    {
        ArgumentNullException.ThrowIfNull(weights);

        if (amount == 0m)
            return [];

        var weightTotal = weights.Sum(x => x.Amount);
        if (weights.Count == 0 || weightTotal <= 0m)
            return fallbackAccountId is { } fallback
                ? [new PayrollAccountShare(fallback, amount)]
                : [];

        if (weights.Count == 1)
            return [new PayrollAccountShare(weights[0].AccountId, amount)];

        // Largest-remainder: round every share down to the cent, then hand the cents that are
        // still missing to the accounts with the biggest remainders. Without this the parts
        // drift away from the total and the posting no longer matches the tax line.
        var exact = weights
            .Select(weight => amount * weight.Amount / weightTotal)
            .ToList();
        var shares = exact
            .Select(value => decimal.Truncate(value * 100m) / 100m)
            .ToList();

        var remainder = amount - shares.Sum();
        var cent = amount < 0m ? -0.01m : 0.01m;
        var order = Enumerable.Range(0, weights.Count)
            .OrderByDescending(i => Math.Abs(exact[i] - shares[i]))
            .ThenByDescending(i => weights[i].Amount)
            .ToList();

        for (var step = 0; Math.Abs(remainder) >= 0.005m && step < order.Count; step++)
        {
            shares[order[step]] += cent;
            remainder -= cent;
        }

        return weights
            .Select((weight, i) => new PayrollAccountShare(weight.AccountId, shares[i]))
            .Where(share => share.Amount != 0m)
            .ToList();
    }

    /// <summary>
    /// The single account carrying most of the line's earnings. Used where the posting has to
    /// fit on one stored account instead of being split over several.
    /// </summary>
    public static int? DominantAccountId(IReadOnlyList<PayrollAccountShare> weights) =>
        weights.Count == 0
            ? null
            : weights.OrderByDescending(x => x.Amount).ThenBy(x => x.AccountId).First().AccountId;

    private static IReadOnlyList<PayrollAccountShare> Weights(
        PayPayrollLine line,
        Func<PayPayrollCalcLine, int?> accountSelector)
    {
        ArgumentNullException.ThrowIfNull(line);

        return line.CalcLines
            .Where(calc =>
                calc.Component.ComponentType == PayrollComponentTypeConst.Earning &&
                calc.Amount > 0m &&
                accountSelector(calc) is > 0)
            .GroupBy(calc => accountSelector(calc)!.Value)
            .Select(group => new PayrollAccountShare(group.Key, group.Sum(calc => calc.Amount)))
            .OrderBy(share => share.AccountId)
            .ToList();
    }
}
