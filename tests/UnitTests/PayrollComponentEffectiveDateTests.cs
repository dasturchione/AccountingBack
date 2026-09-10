using Application.Features.Pay.Components;

namespace UnitTests;

public sealed class PayrollComponentEffectiveDateTests
{
    [Fact]
    public void EffectiveRangesMustNotOverlapForTheSameComponentCode()
    {
        Assert.True(PayrollComponentEffectiveDatePolicy.Overlaps(
            new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31),
            new DateOnly(2026, 1, 31), null));
        Assert.False(PayrollComponentEffectiveDatePolicy.Overlaps(
            new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31),
            new DateOnly(2026, 2, 1), null));
    }

    [Fact]
    public void EffectiveToCannotPrecedeEffectiveFrom()
    {
        Assert.False(PayrollComponentEffectiveDatePolicy.IsValidRange(
            new DateOnly(2026, 2, 1), new DateOnly(2026, 1, 31)));
    }
}
