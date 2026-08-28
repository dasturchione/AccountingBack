using Application.Abstractions;
using Application.Features.MoneyRegisterBalances;
using Application.Features.Register;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.CashCollections;

public interface ICashCollectionMoneyService
{
    Task<Result<List<MoneyRegisterBalance>>> PostAsync(CashCollectionDoc document, long postingBatchId, CancellationToken ct);
    Task<Result<List<MoneyRegisterBalance>>> ReverseAsync(CashCollectionDoc document, long postingBatchId, CancellationToken ct);
    Task<decimal> GetCashBoxBalanceAsync(int cashBoxId, DateTime asOfDate, CancellationToken ct);
}

public sealed class CashCollectionMoneyService : ICashCollectionMoneyService
{
    private readonly IQueryBuilder _queryBuilder;
    private readonly ICashMoneyRegisterService _cashMoneyRegisterService;
    private readonly IQueryRepository<MoneyRegisterBalance> _query;
    private readonly ICommandRepository<MoneyRegisterBalance> _command;

    public CashCollectionMoneyService(
        IQueryBuilder queryBuilder,
        ICashMoneyRegisterService cashMoneyRegisterService,
        IQueryRepository<MoneyRegisterBalance> query,
        ICommandRepository<MoneyRegisterBalance> command)
    {
        _queryBuilder = queryBuilder;
        _cashMoneyRegisterService = cashMoneyRegisterService;
        _query = query;
        _command = command;
    }

    public async Task<Result<List<MoneyRegisterBalance>>> PostAsync(
        CashCollectionDoc document,
        long postingBatchId,
        CancellationToken ct)
    {
        var row = new MoneyRegisterBalance
        {
            OrganizationId = document.OrganizationId,
            DocumentTypeId = DocumentTypeIdConst.CASHCOLLECTION,
            DocumentId = document.Id,
            SourceType = RegisterDefaultsConst.CashCollection,
            SourceId = document.CashBoxId,
            DirectionId = MovementDirectionIdConst.OUT,
            CurrencyId = document.CurrencyId,
            Amount = document.Amount,
            DocDate = document.DocDate,
            CreatedDate = DateTime.Now,
            PostingBatchId = postingBatchId
        };

        await _command.CreateAsync(row, ct);
        return Result.Success(new List<MoneyRegisterBalance> { row });
    }

    public async Task<Result<List<MoneyRegisterBalance>>> ReverseAsync(
        CashCollectionDoc document,
        long postingBatchId,
        CancellationToken ct)
    {
        var query = _queryBuilder.For<MoneyRegisterBalance>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.CASHCOLLECTION &&
                        x.DocumentId == document.Id &&
                        x.ReversalEntryId == null)
            .Build();
        var originals = await _query.GetAllAsync(query, ct);
        if (originals.Count == 0)
            return Result.Failure<List<MoneyRegisterBalance>>(CashCollectionErrors.MissingMoneyEntries(document.Id));

        var now = DateTime.Now;
        var reversals = originals.Select(x => new MoneyRegisterBalance
        {
            OrganizationId = x.OrganizationId,
            DocumentTypeId = x.DocumentTypeId,
            DocumentId = x.DocumentId,
            SourceType = x.SourceType,
            SourceId = x.SourceId,
            DirectionId = MovementDirectionIdConst.Reverse(x.DirectionId),
            CurrencyId = x.CurrencyId,
            Amount = x.Amount,
            DocDate = now,
            CreatedDate = now,
            PostingBatchId = postingBatchId,
            SourceLineId = x.SourceLineId,
            ReversalEntryId = x.Id
        }).ToList();

        await _command.CreateAsync(reversals, ct);
        return Result.Success(reversals);
    }

    public Task<decimal> GetCashBoxBalanceAsync(int cashBoxId, DateTime asOfDate, CancellationToken ct) =>
        _cashMoneyRegisterService.GetCashBoxBalanceAsync(cashBoxId, asOfDate, ct);
}
