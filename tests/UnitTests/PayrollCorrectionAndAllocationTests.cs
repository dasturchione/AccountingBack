using Application.Features.Pay.Payments;
using Application.Features.Pay.PayrollDocuments;

namespace UnitTests;

public sealed class PayrollCorrectionDeltaResolverTests
{
    [Fact]
    public void Target_ComputesDeltaAgainstCurrentPosted()
    {
        // Salary posted low at 900 000; corrected target 1 000 000 → delta +100 000.
        Assert.Equal(100_000m, PayrollCorrectionDeltaResolver.Resolve(targetAmount: 1_000_000m, rawAmount: null, currentPosted: 900_000m));
    }

    [Fact]
    public void Target_LowerThanCurrent_ProducesNegativeDelta()
    {
        Assert.Equal(-50_000m, PayrollCorrectionDeltaResolver.Resolve(targetAmount: 850_000m, rawAmount: null, currentPosted: 900_000m));
    }

    [Fact]
    public void RawAmount_UsedAsIs_WhenNoTarget()
    {
        Assert.Equal(25_000m, PayrollCorrectionDeltaResolver.Resolve(targetAmount: null, rawAmount: 25_000m, currentPosted: 900_000m));
    }
}

public sealed class PayrollPaymentAllocatorTests
{
    [Fact]
    public void Allocate_FillsRegularFirstThenCorrection()
    {
        // Regular line outstanding 900, WITH_SALARY correction line outstanding 100.
        var targets = new List<PayrollPaymentAllocationTarget>
        {
            new(PayrollLineId: 10, Outstanding: 900m),
            new(PayrollLineId: 20, Outstanding: 100m)
        };

        var result = PayrollPaymentAllocator.Allocate(1_000m, targets);

        Assert.Collection(result,
            a => { Assert.Equal(10, a.PayrollLineId); Assert.Equal(900m, a.Amount); },
            a => { Assert.Equal(20, a.PayrollLineId); Assert.Equal(100m, a.Amount); });
    }

    [Fact]
    public void Allocate_PartialAmount_StopsWhenSatisfied()
    {
        var targets = new List<PayrollPaymentAllocationTarget>
        {
            new(10, 900m),
            new(20, 100m)
        };

        var result = PayrollPaymentAllocator.Allocate(500m, targets);

        var single = Assert.Single(result);
        Assert.Equal(10, single.PayrollLineId);
        Assert.Equal(500m, single.Amount);
    }

    [Fact]
    public void Allocate_CapsAtTotalOutstanding()
    {
        var targets = new List<PayrollPaymentAllocationTarget> { new(10, 300m) };

        // Even if asked for more than available, it never allocates beyond outstanding
        // (the service rejects over-payment before calling this).
        var result = PayrollPaymentAllocator.Allocate(999m, targets);

        var single = Assert.Single(result);
        Assert.Equal(300m, single.Amount);
        Assert.Equal(300m, PayrollPaymentAllocator.TotalOutstanding(targets));
    }
}
