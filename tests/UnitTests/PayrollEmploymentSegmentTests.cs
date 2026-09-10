using Application.Features.Pay.PayrollDocuments;
using Domain.Entities;

namespace UnitTests;

public sealed class PayrollEmploymentSegmentTests
{
    [Fact]
    public void Split_CreatesEffectiveDateSegmentsInsidePeriod()
    {
        var period = new PayPeriod
        {
            StartDate = new DateOnly(2026, 9, 1),
            EndDate = new DateOnly(2026, 9, 30)
        };
        var employments = new[]
        {
            new PayEmployment
            {
                Id = 10,
                EmployeeId = 1,
                StartDate = new DateOnly(2026, 8, 1),
                EndDate = new DateOnly(2026, 9, 14),
                MonthlySalary = 1_000m,
                EmploymentRate = 1m
            },
            new PayEmployment
            {
                Id = 11,
                EmployeeId = 1,
                StartDate = new DateOnly(2026, 9, 15),
                MonthlySalary = 1_200m,
                EmploymentRate = .5m
            }
        };

        var segments = PayrollEmploymentSegmentCalculator.Split(period, employments);

        Assert.Collection(
            segments,
            first =>
            {
                Assert.Equal(10, first.EmploymentId);
                Assert.Equal(new DateOnly(2026, 9, 1), first.StartDate);
                Assert.Equal(new DateOnly(2026, 9, 14), first.EndDate);
                Assert.Equal(1_000m, first.MonthlySalary);
            },
            second =>
            {
                Assert.Equal(11, second.EmploymentId);
                Assert.Equal(new DateOnly(2026, 9, 15), second.StartDate);
                Assert.Equal(new DateOnly(2026, 9, 30), second.EndDate);
                Assert.Equal(.5m, second.EmploymentRate);
            });
    }

    [Fact]
    public void Split_LeavesGapsWithoutAnEmploymentSegment()
    {
        var period = new PayPeriod
        {
            StartDate = new DateOnly(2026, 9, 1),
            EndDate = new DateOnly(2026, 9, 30)
        };
        var employment = new PayEmployment
        {
            Id = 10,
            EmployeeId = 1,
            StartDate = new DateOnly(2026, 9, 10),
            EndDate = new DateOnly(2026, 9, 20)
        };

        var segments = PayrollEmploymentSegmentCalculator.Split(period, [employment]);

        var segment = Assert.Single(segments);
        Assert.Equal(new DateOnly(2026, 9, 10), segment.StartDate);
        Assert.Equal(new DateOnly(2026, 9, 20), segment.EndDate);
    }

    [Fact]
    public void Split_ResolvesOverlappingRecordsByLatestEffectiveStart()
    {
        var period = new PayPeriod
        {
            StartDate = new DateOnly(2026, 9, 1),
            EndDate = new DateOnly(2026, 9, 30)
        };
        var employments = new[]
        {
            new PayEmployment
            {
                Id = 10,
                EmployeeId = 1,
                StartDate = new DateOnly(2026, 9, 1),
                EndDate = new DateOnly(2026, 9, 30),
                MonthlySalary = 1_000m
            },
            new PayEmployment
            {
                Id = 11,
                EmployeeId = 1,
                StartDate = new DateOnly(2026, 9, 15),
                EndDate = new DateOnly(2026, 9, 20),
                MonthlySalary = 1_200m
            }
        };

        var segments = PayrollEmploymentSegmentCalculator.Split(period, employments);

        Assert.Collection(
            segments,
            first =>
            {
                Assert.Equal(10, first.EmploymentId);
                Assert.Equal(new DateOnly(2026, 9, 1), first.StartDate);
                Assert.Equal(new DateOnly(2026, 9, 14), first.EndDate);
            },
            second =>
            {
                Assert.Equal(11, second.EmploymentId);
                Assert.Equal(new DateOnly(2026, 9, 15), second.StartDate);
                Assert.Equal(new DateOnly(2026, 9, 20), second.EndDate);
            },
            third =>
            {
                Assert.Equal(10, third.EmploymentId);
                Assert.Equal(new DateOnly(2026, 9, 21), third.StartDate);
                Assert.Equal(new DateOnly(2026, 9, 30), third.EndDate);
            });
    }
}
