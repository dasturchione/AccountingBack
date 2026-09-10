using Application.Abstractions;
using Application.Features;
using Microsoft.Extensions.Logging.Abstractions;
using SharedKernel.Results;

namespace UnitTests;

public sealed class PayrollPostingIntegrityTests
{
    [Fact]
    public async Task Rolls_back_when_posting_operation_returns_failure()
    {
        var unitOfWork = new RecordingUnitOfWork();
        var service = new TransactionProbe(unitOfWork);

        var result = await service.RunAsync(() => Task.FromResult(Result.Failure(
            Error.Business("PostingFailed", "simulated posting failure"))));

        Assert.False(result.IsSuccess);
        Assert.Equal(1, unitOfWork.BeginCount);
        Assert.Equal(1, unitOfWork.RollbackCount);
        Assert.Equal(0, unitOfWork.CommitCount);
    }

    [Fact]
    public async Task Rolls_back_when_posting_operation_throws()
    {
        var unitOfWork = new RecordingUnitOfWork();
        var service = new TransactionProbe(unitOfWork);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RunAsync(() =>
            throw new InvalidOperationException("simulated posting exception")));

        Assert.Equal(1, unitOfWork.BeginCount);
        Assert.Equal(1, unitOfWork.RollbackCount);
        Assert.Equal(0, unitOfWork.CommitCount);
    }

    private sealed class TransactionProbe(RecordingUnitOfWork unitOfWork)
        : BaseService(NullLogger<TransactionProbe>.Instance, unitOfWork)
    {
        public Task<Result> RunAsync(Func<Task<Result>> operation) =>
            ExecuteInTransactionAsync("PayrollPostingProbe", operation, CancellationToken.None);
    }

    private sealed class RecordingUnitOfWork : IUnitOfWork
    {
        public int BeginCount { get; private set; }
        public int CommitCount { get; private set; }
        public int RollbackCount { get; private set; }

        public Task BeginAsync(CancellationToken ct = default) { BeginCount++; return Task.CompletedTask; }
        public Task SaveChangesAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task CommitAsync(CancellationToken ct = default) { CommitCount++; return Task.CompletedTask; }
        public Task RollbackAsync(CancellationToken ct = default) { RollbackCount++; return Task.CompletedTask; }
    }
}
