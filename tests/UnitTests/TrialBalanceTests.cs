using Application.Features.TrialBalance;

namespace UnitTests;

public sealed class TrialBalanceTests
{
    [Fact]
    public async Task GetAsync_MapsAccountNumberToItems()
    {
        var service = new TrialBalanceService(
            null!,
            null!,
            null!,
            null!,
            new StubTrialBalanceReadRepository(new TrialBalanceReadResult
            {
                Rows =
                [
                    new TrialBalanceReadRow
                    {
                        AccountId = 10,
                        AccountCode = "CASH",
                        AccountNumber = "5010",
                        AccountName = "Cash",
                        PeriodDebitTurnover = 100m
                    }
                ]
            }));

        var result = await service.GetAsync(new TrialBalanceFilter());

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value.Items);
        var accountNumber = item.GetType().GetProperty("AccountNumber")?.GetValue(item);
        Assert.Equal("5010", accountNumber);
    }

    private sealed class StubTrialBalanceReadRepository(TrialBalanceReadResult result)
        : ITrialBalanceReadRepository
    {
        public Task<TrialBalanceReadResult> GetAsync(
            TrialBalanceReadRequest request,
            CancellationToken ct = default) =>
            Task.FromResult(result);
    }
}
