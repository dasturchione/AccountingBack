namespace Application.Features.Rnt.RentalAccruals;

public static class RentalAccrualSchedule
{
    public static RentalAccrualPeriod GetPeriod(
        DateTime nextAccrualDate,
        string periodUnit,
        int periodValue,
        DateTime contractEndDate)
    {
        if (periodValue <= 0)
            throw new ArgumentOutOfRangeException(nameof(periodValue));

        var periodFrom = nextAccrualDate.Date;
        var endDate = contractEndDate.Date;
        if (periodFrom > endDate)
            throw new ArgumentOutOfRangeException(nameof(nextAccrualDate));

        var calculatedPeriodTo = periodUnit switch
        {
            "DAY" => periodFrom.AddDays(periodValue - 1),
            "MONTH" => periodFrom.AddMonths(periodValue).AddDays(-1),
            _ => throw new ArgumentOutOfRangeException(nameof(periodUnit))
        };
        var periodTo = calculatedPeriodTo <= endDate ? calculatedPeriodTo : endDate;

        return new RentalAccrualPeriod(
            periodFrom,
            periodTo,
            periodTo.AddDays(1));
    }
}

public readonly record struct RentalAccrualPeriod(
    DateTime PeriodFrom,
    DateTime PeriodTo,
    DateTime NextAccrualDate);
