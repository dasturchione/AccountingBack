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
        var factor = periodUnit switch
        {
            "DAY" => actualDayCount,
            "MONTH" => (decimal)actualDayCount / DateTime.DaysInMonth(month.Year, month.Month),
            _ => throw new ArgumentOutOfRangeException(nameof(periodUnit))
        };

        return new RentalAccrualPeriod(
            periodFrom,
            periodTo,
            GetNextDate(periodTo),
            factor);
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

    public static RentalAccrualPeriod GetPeriod(
        DateTime nextAccrualDate,
        string periodUnit,
        DateTime contractEndDate)
    {
        var periodFrom = nextAccrualDate.Date;
        var endDate = contractEndDate.Date;
        if (periodFrom > endDate)
            throw new ArgumentOutOfRangeException(nameof(nextAccrualDate));

        var calculatedPeriodTo = periodUnit switch
        {
            "DAY" => periodFrom,
            "MONTH" => new DateTime(
                periodFrom.Year,
                periodFrom.Month,
                DateTime.DaysInMonth(periodFrom.Year, periodFrom.Month)),
            _ => throw new ArgumentOutOfRangeException(nameof(periodUnit))
        };
        var periodTo = calculatedPeriodTo <= endDate ? calculatedPeriodTo : endDate;
        var scheduledDayCount = periodUnit == "MONTH"
            ? DateTime.DaysInMonth(periodFrom.Year, periodFrom.Month)
            : 1;
        var actualDayCount = (periodTo - periodFrom).Days + 1;
        var prorationFactor = actualDayCount == scheduledDayCount
            ? 1m
            : (decimal)actualDayCount / scheduledDayCount;

        return new RentalAccrualPeriod(
            periodFrom,
            periodTo,
            GetNextDate(periodTo),
            prorationFactor);
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
        var cursor = startDate.Date;
        if (cursor > accrualLimit)
            return new RentalContractTotals(0m, 0m, 0m);

        var contractAmount = 0m;
        var contractTaxBaseAmount = 0m;
        var contractTaxAmount = 0m;
        while (cursor <= accrualLimit)
        {
            var period = GetPeriod(cursor, periodUnit, accrualLimit);
            var accruedAmount = ProrateAmount(periodAmount, period.ProrationFactor);
            var accruedTaxBaseAmount = ProrateAmount(taxBaseAmount, period.ProrationFactor);
            var accruedAmounts = RentalAccrualCalculator.Calculate(accruedAmount, accruedTaxBaseAmount, taxRate);

            contractAmount += accruedAmount;
            contractTaxBaseAmount += accruedTaxBaseAmount;
            contractTaxAmount += accruedAmounts.TaxAmount;
            if (period.NextAccrualDate <= cursor)
                break;
            cursor = period.NextAccrualDate;
        }

        return new RentalContractTotals(contractAmount, contractTaxBaseAmount, contractTaxAmount);
    }

    public static decimal ProrateAmount(decimal amount, decimal factor) =>
        factor == 1m
            ? amount
            : Math.Round(amount * factor, 2, MidpointRounding.AwayFromZero);

    private static DateTime GetNextDate(DateTime date) =>
        date.Date == DateTime.MaxValue.Date
            ? DateTime.MaxValue.Date
            : date.Date.AddDays(1);
}

public readonly record struct RentalAccrualPeriod(
    DateTime PeriodFrom,
    DateTime PeriodTo,
    DateTime NextAccrualDate,
    decimal ProrationFactor = 1m);

public readonly record struct RentalContractTotals(
    decimal? ContractAmount,
    decimal? ContractTaxBaseAmount,
    decimal? ContractTaxAmount);

public readonly record struct RentalAccrualMonth(
    int Year,
    int Month,
    DateTime StartDate,
    DateTime EndDate);
