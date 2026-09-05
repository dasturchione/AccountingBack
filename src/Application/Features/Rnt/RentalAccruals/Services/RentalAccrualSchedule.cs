namespace Application.Features.Rnt.RentalAccruals;

public static class RentalAccrualSchedule
{
    public static RentalAccrualMonth GetMonth(int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        return new RentalAccrualMonth(
            year,
            month,
            startDate,
            new DateTime(year, month, DateTime.DaysInMonth(year, month)));
    }

    public static RentalAccrualMonth GetPreviousMonth(DateTime date)
    {
        var previousMonth = new DateTime(date.Year, date.Month, 1).AddMonths(-1);
        return GetMonth(previousMonth.Year, previousMonth.Month);
    }

    public static RentalAccrualPeriod? GetPeriodForMonth(
        RentalAccrualMonth month,
        string periodUnit,
        DateTime startDate,
        DateTime accrualEndDate)
    {
        var periodFrom = startDate.Date > month.StartDate
            ? startDate.Date
            : month.StartDate;
        var periodTo = accrualEndDate.Date < month.EndDate
            ? accrualEndDate.Date
            : month.EndDate;
        if (periodFrom > periodTo)
            return null;

        var actualDayCount = (periodTo - periodFrom).Days + 1;
        if (periodUnit == "DAY")
        {
            return new RentalAccrualPeriod(
                periodFrom,
                periodTo,
                GetNextDate(periodTo),
                actualDayCount);
        }

        if (periodUnit != "MONTH")
            throw new ArgumentOutOfRangeException(nameof(periodUnit));

        var amountFactors = GetContractMonthAmountFactors(startDate.Date, periodFrom, periodTo);

        return new RentalAccrualPeriod(
            periodFrom,
            periodTo,
            GetNextDate(periodTo),
            amountFactors.Sum(),
            amountFactors);
    }

    public static DateTime GetAccrualLimit(
        DateTime? contractEndDate,
        DateTime? objectEndDate,
        DateTime? terminationDate)
    {
        var limit = DateTime.MaxValue.Date;
        if (contractEndDate.HasValue && contractEndDate.Value.Date < limit)
            limit = contractEndDate.Value.Date;
        if (objectEndDate.HasValue && objectEndDate.Value.Date < limit)
            limit = objectEndDate.Value.Date;
        if (terminationDate.HasValue && terminationDate.Value.Date < limit)
            limit = terminationDate.Value.Date;
        return limit;
    }

    public static RentalContractTotals CalculateContractTotals(
        decimal periodAmount,
        decimal taxBaseAmount,
        decimal taxRate,
        string periodUnit,
        DateTime startDate,
        DateTime? contractEndDate,
        DateTime? objectEndDate,
        DateTime? terminationDate)
    {
        if (!contractEndDate.HasValue && !objectEndDate.HasValue && !terminationDate.HasValue)
            return new RentalContractTotals(null, null, null);

        var accrualLimit = GetAccrualLimit(contractEndDate, objectEndDate, terminationDate);
        var firstMonth = GetMonth(startDate.Year, startDate.Month);
        if (firstMonth.StartDate > accrualLimit)
            return new RentalContractTotals(0m, 0m, 0m);

        var contractAmount = 0m;
        var contractTaxBaseAmount = 0m;
        var contractTaxAmount = 0m;
        var month = firstMonth;
        while (month.StartDate <= accrualLimit)
        {
            var period = GetPeriodForMonth(month, periodUnit, startDate, accrualLimit);
            if (!period.HasValue)
                break;

            var accruedAmount = ProrateAmount(periodAmount, period.Value);
            var accruedTaxBaseAmount = ProrateAmount(taxBaseAmount, period.Value);
            var accruedAmounts = RentalAccrualCalculator.Calculate(accruedAmount, accruedTaxBaseAmount, taxRate);

            contractAmount += accruedAmount;
            contractTaxBaseAmount += accruedTaxBaseAmount;
            contractTaxAmount += accruedAmounts.TaxAmount;

            if (month.EndDate == DateTime.MaxValue.Date)
                break;

            var nextMonthDate = month.EndDate.AddDays(1);
            month = GetMonth(nextMonthDate.Year, nextMonthDate.Month);
        }

        return new RentalContractTotals(contractAmount, contractTaxBaseAmount, contractTaxAmount);
    }

    public static decimal ProrateAmount(decimal amount, decimal factor) =>
        factor == 1m
            ? amount
            : Math.Round(amount * factor, 2, MidpointRounding.AwayFromZero);

    public static decimal ProrateAmount(decimal amount, RentalAccrualPeriod period) =>
        period.AmountFactors is { Count: > 0 }
            ? period.AmountFactors.Sum(factor => ProrateAmount(amount, factor))
            : ProrateAmount(amount, period.ProrationFactor);

    private static IReadOnlyList<decimal> GetContractMonthAmountFactors(
        DateTime rentalStart,
        DateTime periodFrom,
        DateTime periodTo)
    {
        var factors = new List<decimal>(2);
        var monthIndex = Math.Max(0, GetMonthDifference(rentalStart, periodFrom) - 1);

        while (true)
        {
            var contractMonthStart = rentalStart.AddMonths(monthIndex);
            if (contractMonthStart > periodTo)
                break;

            var contractMonthEnd = GetContractMonthEnd(rentalStart, monthIndex);
            if (contractMonthEnd >= periodFrom)
            {
                var overlapFrom = contractMonthStart > periodFrom ? contractMonthStart : periodFrom;
                var overlapTo = contractMonthEnd < periodTo ? contractMonthEnd : periodTo;
                if (overlapFrom <= overlapTo)
                {
                    var contractMonthDayCount = (contractMonthEnd - contractMonthStart).Days + 1;
                    var overlapDayCount = (overlapTo - overlapFrom).Days + 1;
                    factors.Add((decimal)overlapDayCount / contractMonthDayCount);
                }
            }

            if (contractMonthEnd == DateTime.MaxValue.Date)
                break;

            monthIndex++;
        }

        return factors;
    }

    private static DateTime GetContractMonthEnd(DateTime rentalStart, int monthIndex)
    {
        var contractMonthStart = rentalStart.AddMonths(monthIndex);
        var monthsUntilMaximum = GetMonthDifference(contractMonthStart, DateTime.MaxValue.Date);
        return monthsUntilMaximum == 0
            ? DateTime.MaxValue.Date
            : rentalStart.AddMonths(monthIndex + 1).AddDays(-1);
    }

    private static int GetMonthDifference(DateTime from, DateTime to) =>
        (to.Year - from.Year) * 12 + to.Month - from.Month;

    private static DateTime GetNextDate(DateTime date) =>
        date.Date == DateTime.MaxValue.Date
            ? DateTime.MaxValue.Date
            : date.Date.AddDays(1);
}

public readonly record struct RentalAccrualPeriod(
    DateTime PeriodFrom,
    DateTime PeriodTo,
    DateTime NextAccrualDate,
    decimal ProrationFactor = 1m,
    IReadOnlyList<decimal>? AmountFactors = null);

public readonly record struct RentalContractTotals(
    decimal? ContractAmount,
    decimal? ContractTaxBaseAmount,
    decimal? ContractTaxAmount);

public readonly record struct RentalAccrualMonth(
    int Year,
    int Month,
    DateTime StartDate,
    DateTime EndDate);
