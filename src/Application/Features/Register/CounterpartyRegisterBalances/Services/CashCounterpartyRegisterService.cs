using Application.Abstractions;
using Application.Features.CashOperations;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.CounterpartyRegisterBalances;

public class CashCounterpartyRegisterService : ICashCounterpartyRegisterService
{
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<CounterpartyRegisterBalance> _query;
    private readonly ICommandRepository<CounterpartyRegisterBalance> _command;

    public CashCounterpartyRegisterService(
        IQueryBuilder queryBuilder,
        IQueryRepository<CounterpartyRegisterBalance> query,
        ICommandRepository<CounterpartyRegisterBalance> command)
    {
        _queryBuilder = queryBuilder;
        _query = query;
        _command = command;
    }

    public async Task<Result<List<CounterpartyRegisterBalance>>> PostAsync(
        CashOperation cashOperation,
        long postingBatchId,
        CancellationToken ct = default)
    {
        if (cashOperation.Amount <= 0m || cashOperation.CounterpartyId is null)
            return Result.Success(new List<CounterpartyRegisterBalance>());

        var operationType = cashOperation.OperationTypeId == OperationTypeIdConst.IN
            ? OperationTypeIdConst.DEBT_DECREASE
            : OperationTypeIdConst.DEBT_INCREASE;

        var entry = new CounterpartyRegisterBalance
        {
            OrganizationId = cashOperation.OrganizationId,
            DocumentTypeId = DocumentTypeIdConst.CASHOPERATION,
            DocumentId = cashOperation.Id,
            CounterpartyId = cashOperation.CounterpartyId.Value,
            OperationTypeId = operationType,
            CurrencyId = cashOperation.CurrencyId,
            Amount = cashOperation.Amount,
            DocDate = cashOperation.DocDate,
            CreatedDate = DateTime.Now,
            PostingBatchId = postingBatchId
        };

        await _command.CreateAsync(entry, ct);
        return Result.Success(new List<CounterpartyRegisterBalance> { entry });
    }

    public async Task<Result<List<CounterpartyRegisterBalance>>> ReverseAsync(
        CashOperation cashOperation,
        long postingBatchId,
        CancellationToken ct = default)
    {
        if (cashOperation.CounterpartyId is null)
            return Result.Success(new List<CounterpartyRegisterBalance>());

        var originals = await GetOriginalEntriesAsync(cashOperation.Id, ct);
        if (originals.Count == 0)
            return Result.Failure<List<CounterpartyRegisterBalance>>(
                CashOperationErrors.MissingAccountingRegisterEntries(cashOperation.Id, null));

        var now = DateTime.Now;
        var reversals = originals.Select(entry => new CounterpartyRegisterBalance
        {
            OrganizationId = entry.OrganizationId,
            DocumentTypeId = entry.DocumentTypeId,
            DocumentId = entry.DocumentId,
            CounterpartyId = entry.CounterpartyId,
            OperationTypeId = entry.OperationTypeId == OperationTypeIdConst.DEBT_INCREASE
                ? OperationTypeIdConst.DEBT_DECREASE
                : OperationTypeIdConst.DEBT_INCREASE,
            CurrencyId = entry.CurrencyId,
            Amount = entry.Amount,
            DocDate = now,
            CreatedDate = now,
            PostingBatchId = postingBatchId,
            SourceLineId = entry.Id,
            ReversalEntryId = entry.Id
        }).ToList();

        if (reversals.Count > 0)
            await _command.CreateAsync(reversals, ct);

        return Result.Success(reversals);
    }

    private async Task<List<CounterpartyRegisterBalance>> GetOriginalEntriesAsync(long documentId, CancellationToken ct)
    {
        var query = _queryBuilder.For<CounterpartyRegisterBalance>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.CASHOPERATION &&
                        x.DocumentId == documentId &&
                        x.ReversalEntryId == null)
            .Build();

        return await _query.GetAllAsync(query, ct);
    }
}
