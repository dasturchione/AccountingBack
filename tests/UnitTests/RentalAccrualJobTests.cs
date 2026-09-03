using Application.Features.Rnt.RentalAccruals;
using Infrastructure.BackgroundServices;
using Microsoft.Extensions.Logging.Abstractions;
using SharedKernel.Results;

namespace UnitTests;

public sealed class RentalAccrualJobTests
{
    [Fact]
    public async Task ExecuteAsync_InvokesAllOrganizationGenerationForSpecifiedDate()
    {
        var generator = new RecordingGenerator();
        var job = new RentalAccrualJob(generator, NullLogger<RentalAccrualJob>.Instance);
        var date = new DateTime(2026, 8, 29, 23, 15, 0);

        await job.ExecuteAsync(date, CancellationToken.None);

        Assert.Equal(date.Date, generator.AsOfDate);
        Assert.Null(generator.OrganizationId);
        Assert.Equal(1, generator.CallCount);
    }

    private sealed class RecordingGenerator : IRentalAccrualGenerationService
    {
        public DateTime? AsOfDate { get; private set; }
        public int? OrganizationId { get; private set; }
        public int CallCount { get; private set; }

        public Task<Result<RentalAccrualGenerationResult>> GenerateDueAsync(
            DateTime asOfDate,
            int? organizationId,
            CancellationToken ct = default)
        {
            AsOfDate = asOfDate;
            OrganizationId = organizationId;
            CallCount++;
            return Task.FromResult(Result.Success(new RentalAccrualGenerationResult(0, 0, [])));
        }
    }
}
