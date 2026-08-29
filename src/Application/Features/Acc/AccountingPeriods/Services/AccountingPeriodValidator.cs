using Application.Abstractions;
using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Acc.AccountingPeriods;

public class AccountingPeriodValidator : IAccountingPeriodValidator
{
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<AccountingPeriod> _query;
    private readonly IUserContext _userContext;

    public AccountingPeriodValidator(IQueryBuilder queryBuilder,
                                     IQueryRepository<AccountingPeriod> query,
                                     IUserContext userContext)
    {
        _queryBuilder = queryBuilder;
        _query = query;
        _userContext = userContext;
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
            return Result.Failure(AccountingPeriodErrors.Closed(date, _userContext.LanguageId));
        }

        return Result.Success();
    }
}
