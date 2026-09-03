namespace Application.Features.RegulatedObligationSettings;

internal static class RegulatedObligationSettingPeriodPolicy
{
    public static bool IsEffectiveOn(DateOnly effectiveFrom, DateOnly? effectiveTo, DateOnly choosedDate) =>
        effectiveFrom <= choosedDate && (!effectiveTo.HasValue || effectiveTo.Value >= choosedDate);

    public static bool Overlaps(
        DateOnly firstFrom,
        DateOnly? firstTo,
        DateOnly secondFrom,
        DateOnly? secondTo) =>
        firstFrom <= (secondTo ?? DateOnly.MaxValue)
        && secondFrom <= (firstTo ?? DateOnly.MaxValue);
}
