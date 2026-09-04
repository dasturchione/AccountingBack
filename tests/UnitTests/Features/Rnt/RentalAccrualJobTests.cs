using Application.Features.Rnt.RentalAccruals;
using Infrastructure.BackgroundServices;
using Microsoft.Extensions.Logging.Abstractions;
using Quartz;
using SharedKernel.Results;
using SharedKernel.Time;

namespace UnitTests.Features.Rnt;

public sealed class RentalAccrualJobTests
{
    [Fact]
    public void ScheduleRunsAtMidnightOnFirstDayOfMonthInTashkent()
    {
        var cron = new CronExpression(RentalAccrualJob.CronSchedule)
        {
            TimeZone = TashkentTime.Zone
        };
        var afterLocal = DateTime.SpecifyKind(new DateTime(2024, 7, 15, 12, 0, 0), DateTimeKind.Unspecified);
        var afterUtc = TimeZoneInfo.ConvertTimeToUtc(afterLocal, TashkentTime.Zone);

        var nextUtc = cron.GetNextValidTimeAfter(new DateTimeOffset(afterUtc));

        Assert.NotNull(nextUtc);
        var nextLocal = TimeZoneInfo.ConvertTime(nextUtc.Value, TashkentTime.Zone);
        Assert.Equal(new DateTime(2024, 8, 1), nextLocal.DateTime);
    }

    [Fact]
    public async Task ExecuteGeneratesPreviousCalendarMonth()
    {
        var generationService = new RecordingRentalAccrualGenerationService();
        var job = new RentalAccrualJob(
            generationService,
            NullLogger<RentalAccrualJob>.Instance);

        await job.ExecuteAsync(new DateTime(2025, 1, 1), CancellationToken.None);

        Assert.Equal(2024, generationService.Year);
        Assert.Equal(12, generationService.Month);
        Assert.Null(generationService.OrganizationId);
    }

    private sealed class RecordingRentalAccrualGenerationService : IRentalAccrualGenerationService
    {
        public int? Year { get; private set; }
        public int? Month { get; private set; }
        public int? OrganizationId { get; private set; }

        public Task<Result<RentalAccrualGenerationResult>> GenerateDueAsync(
            int year,
            int month,
            int? organizationId,
            CancellationToken ct = default)
        {
            Year = year;
            Month = month;
            OrganizationId = organizationId;
            return Task.FromResult(Result.Success(
                new RentalAccrualGenerationResult(0, 0, Array.Empty<long>())));
        }
    }
}
