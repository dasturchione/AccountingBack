namespace Application.Features.Pay.PayrollDocuments;

/// <summary>
/// Average-earnings benefit maths (1C отпускные/больничные). The daily average is the
/// lookback gross divided by the worked days in that window; a benefit is that daily
/// average times the paid absence days times a percentage (100 for leave, the sick
/// type's percentage for sickness).
/// </summary>
public static class PayrollAverageEarningsCalculator
{
    public static decimal DailyAverage(decimal totalGross, decimal totalWorkedDays) =>
        totalWorkedDays <= 0m
            ? 0m
            : decimal.Round(totalGross / totalWorkedDays, 2, MidpointRounding.AwayFromZero);

    public static decimal Benefit(decimal dailyAverage, decimal days, decimal percent) =>
        decimal.Round(dailyAverage * days * percent / 100m, 2, MidpointRounding.AwayFromZero);
}
