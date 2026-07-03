using Application.Abstractions;
using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.CashDocuments;

public class CashBookService : ICashBookService
{
    private const int DefaultPageSize = 50;

    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<CashBox> _cashBoxQuery;
    private readonly ICashBookReadRepository _readRepository;

    public CashBookService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IQueryRepository<CashBox> cashBoxQuery,
        ICashBookReadRepository readRepository)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _cashBoxQuery = cashBoxQuery;
        _readRepository = readRepository;
    }

    public async Task<Result<CashBookDto>> GetAsync(CashBookFilter filter, CancellationToken ct = default)
    {
        if (filter.CashBoxId <= 0)
            return Result.Failure<CashBookDto>(Error.Business("CashBook.CashBoxRequired", "Cash box is required."));

        if (filter.Page <= 0 || filter.PageSize <= 0)
            return Result.Failure<CashBookDto>(Error.Business("CashBook.InvalidPagination", "Invalid pagination parameters."));

        if (filter.DateFrom.HasValue && filter.DateTo.HasValue && filter.DateFrom.Value.Date > filter.DateTo.Value.Date)
            return Result.Failure<CashBookDto>(Error.Business("CashBook.InvalidDateRange", "DateFrom must be earlier than or equal to DateTo."));

        var cashBoxQuery = _queryBuilder.For<CashBox>()
            .Where(x => x.Id == filter.CashBoxId && x.StateId == StateIdConst.ACTIVE)
            .Build();
        var cashBox = await _cashBoxQuery.GetAsync(cashBoxQuery, ct);

        if (cashBox is null)
            return Result.Failure<CashBookDto>(Error.NotFound("CashBook.CashBoxNotFound", $"Cash box {filter.CashBoxId} was not found."));

        var pageSize = filter.PageSize ?? DefaultPageSize;
        var dateFrom = filter.DateFrom?.Date;
        var dateTo = filter.DateTo?.Date.AddDays(1).AddTicks(-1);

        var readResult = await _readRepository.GetAsync(new CashBookReadRequest
        {
            CashBoxId = filter.CashBoxId,
            DateFrom = dateFrom,
            DateTo = dateTo,
            Page = filter.Page,
            PageSize = pageSize
        }, ct);

        var runningBalance = readResult.PageOpeningBalance;
        var items = new List<CashBookEntryDto>(readResult.Entries.Count);

        foreach (var entry in readResult.Entries)
        {
            runningBalance += entry.Receipt - entry.Payment;

            items.Add(new CashBookEntryDto
            {
                MoneyRegisterEntryId = entry.MoneyRegisterEntryId,
                CashOperationId = entry.CashOperationId,
                DocDate = entry.DocDate,
                DocNumber = entry.DocNumber,
                DocumentKind = entry.DocumentKind,
                PaymentPurposeId = entry.PaymentPurposeId,
                PaymentPurposeName = entry.PaymentPurposeName,
                CounterpartyId = entry.CounterpartyId,
                CounterpartyName = entry.CounterpartyName,
                Comment = entry.Comment,
                CurrencyId = entry.CurrencyId,
                CurrencyName = entry.CurrencyName,
                Receipt = entry.Receipt,
                Payment = entry.Payment,
                RunningBalance = runningBalance
            });
        }

        var totalPages = readResult.TotalCount > 0
            ? (int)Math.Ceiling(readResult.TotalCount / (double)pageSize)
            : 0;

        return new CashBookDto
        {
            CashBoxId = cashBox.Id,
            CashBoxName = cashBox.Name,
            DateFrom = dateFrom,
            DateTo = dateTo,
            OpeningBalance = readResult.OpeningBalance,
            ClosingBalance = readResult.ClosingBalance,
            TotalReceipt = readResult.TotalReceipt,
            TotalPayment = readResult.TotalPayment,
            Page = filter.Page,
            PageSize = pageSize,
            TotalCount = readResult.TotalCount,
            TotalPages = totalPages,
            HasPreviousPage = filter.Page > 1,
            HasNextPage = filter.Page < totalPages,
            Items = items
        };
    }
}
