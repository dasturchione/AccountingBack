using Application.Abstractions;
using Domain.Entities;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Acc.AccountingPeriods;

public class AccountingPeriodValidator : IAccountingPeriodValidator
{
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<AccountingPeriod> _query;

    public AccountingPeriodValidator(IQueryBuilder queryBuilder,
                                     IQueryRepository<AccountingPeriod> query)
    {
        _queryBuilder = queryBuilder;
        _query = query;
    }

    public async Task<Result> EnsureOpenAsync(int organizationId, DateTime date, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<AccountingPeriod>()
            .Where(x => x.OrganizationId == organizationId &&
                        x.Year == date.Year &&
                        x.Month == date.Month)
            .Build();

        var period = await _query.GetAsync(query, ct);
        if (period is { IsClosed: true })
        {
            return Result.Failure(Error.Business(
                "AccountingPeriod.Closed",
                $"Accounting period {date:yyyy-MM} is closed."));
        }

        return Result.Success();
    }
}
