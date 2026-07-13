using Application.Abstractions;
using Application.Features.BankOperations;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.CounterpartyRegisterBalances;

public class BankCounterpartyRegisterService : IBankCounterpartyRegisterService
{
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<CounterpartyRegisterBalance> _query;
    private readonly ICommandRepository<CounterpartyRegisterBalance> _command;

    public BankCounterpartyRegisterService(
        IQueryBuilder queryBuilder,
        IQueryRepository<CounterpartyRegisterBalance> query,
        ICommandRepository<CounterpartyRegisterBalance> command)
    {
        _queryBuilder = queryBuilder;
        _query = query;
        _command = command;
    }

    public async Task<Result<List<CounterpartyRegisterBalance>>> PostAsync(BankOperation bankOperation, long postingBatchId, CancellationToken ct = default)
    {
        if (bankOperation.CounterpartyId is null || bankOperation.Amount <= 0m)
            return Result.Success(new List<CounterpartyRegisterBalance>());

        return Result.Success(new List<CounterpartyRegisterBalance>());
    }

    public async Task<Result<List<CounterpartyRegisterBalance>>> ReverseAsync(BankOperation bankOperation, long postingBatchId, CancellationToken ct = default)
    {
        if (bankOperation.CounterpartyId is null)
            return Result.Success(new List<CounterpartyRegisterBalance>());

        var originals = await GetOriginalEntriesAsync(bankOperation.Id, ct);
        if (originals.Count == 0)
            return Result.Success(new List<CounterpartyRegisterBalance>());

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
            SourceLineId = entry.SourceLineId,
            ReversalEntryId = entry.Id
        }).ToList();

        await _command.CreateAsync(reversals, ct);
        return Result.Success(reversals);
    }

    private async Task<List<CounterpartyRegisterBalance>> GetOriginalEntriesAsync(long documentId, CancellationToken ct)
    {
        var query = _queryBuilder.For<CounterpartyRegisterBalance>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.BANKOPERATION &&
                        x.DocumentId == documentId &&
                        x.ReversalEntryId == null)
            .Build();

        return await _query.GetAllAsync(query, ct);
    }
}
