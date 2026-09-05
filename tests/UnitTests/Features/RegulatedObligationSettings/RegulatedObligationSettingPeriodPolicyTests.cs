using Application.Features.RegulatedObligationSettings;

namespace UnitTests.Features.RegulatedObligationSettings;

public sealed class RegulatedObligationSettingPeriodPolicyTests
{
    [Theory]
    [InlineData("2026-01-01", null, "2026-01-01", true)]
    [InlineData("2026-01-01", "2026-12-31", "2026-06-15", true)]
    [InlineData("2026-01-01", "2026-12-31", "2025-12-31", false)]
    [InlineData("2026-01-01", "2026-12-31", "2027-01-01", false)]
    public void IsEffectiveOn_UsesInclusiveBoundaries(
        string effectiveFrom,
        string? effectiveTo,
        string choosedDate,
        bool expected)
    {
        var actual = RegulatedObligationSettingPeriodPolicy.IsEffectiveOn(
            DateOnly.Parse(effectiveFrom),
            effectiveTo is null ? null : DateOnly.Parse(effectiveTo),
            DateOnly.Parse(choosedDate));

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("2026-01-01", "2026-12-31", "2026-12-31", null, true)]
    [InlineData("2026-01-01", "2026-12-31", "2027-01-01", null, false)]
    [InlineData("2026-01-01", null, "2030-01-01", "2030-12-31", true)]
    public void Overlaps_DetectsOnlyIntersectingInclusivePeriods(
        string firstFrom,
        string? firstTo,
        string secondFrom,
        string? secondTo,
        bool expected)
    {
        var actual = RegulatedObligationSettingPeriodPolicy.Overlaps(
            DateOnly.Parse(firstFrom),
            firstTo is null ? null : DateOnly.Parse(firstTo),
            DateOnly.Parse(secondFrom),
            secondTo is null ? null : DateOnly.Parse(secondTo));

        Assert.Equal(expected, actual);
    }
}
