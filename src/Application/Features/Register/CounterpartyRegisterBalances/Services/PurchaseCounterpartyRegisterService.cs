using Application.Abstractions;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.CounterpartyRegisterBalances;

public class PurchaseCounterpartyRegisterService : IPurchaseCounterpartyRegisterService
{
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<CounterpartyRegisterBalance> _query;
    private readonly ICommandRepository<CounterpartyRegisterBalance> _command;

    public PurchaseCounterpartyRegisterService(IQueryBuilder queryBuilder,
                                               IQueryRepository<CounterpartyRegisterBalance> query,
                                               ICommandRepository<CounterpartyRegisterBalance> command)
    {
        _queryBuilder = queryBuilder;
        _query = query;
        _command = command;
    }

    public async Task<Result<List<CounterpartyRegisterBalance>>> PostAsync(PurchaseDoc purchase, long postingBatchId, CancellationToken ct = default)
    {
        if (purchase.FinalAmount == 0m)
            return Result.Success(new List<CounterpartyRegisterBalance>());

        var entry = new CounterpartyRegisterBalance
        {
            OrganizationId = purchase.OrganizationId,
            DocumentTypeId = DocumentTypeIdConst.PURCHASE,
            DocumentId = purchase.Id,
            CounterpartyId = purchase.CounterpartyId,
            OperationTypeId = OperationTypeIdConst.DEBT_INCREASE,
            CurrencyId = purchase.CurrencyId,
            Amount = purchase.FinalAmount,
            DocDate = purchase.DocDate,
            CreatedDate = DateTime.Now,
            PostingBatchId = postingBatchId
        };

        await _command.CreateAsync(entry, ct);
        return Result.Success(new List<CounterpartyRegisterBalance> { entry });
    }

    public async Task<Result<List<CounterpartyRegisterBalance>>> ReverseAsync(PurchaseDoc purchase, long postingBatchId, CancellationToken ct = default)
    {
        var originalEntries = await GetOriginalEntriesAsync(purchase.Id, ct);
        var now = DateTime.Now;

        var reversalEntries = originalEntries.Select(entry => new CounterpartyRegisterBalance
        {
            OrganizationId = entry.OrganizationId,
            DocumentTypeId = entry.DocumentTypeId,
            DocumentId = entry.DocumentId,
            CounterpartyId = entry.CounterpartyId,
            OperationTypeId = OperationTypeIdConst.DEBT_DECREASE,
            CurrencyId = entry.CurrencyId,
            Amount = entry.Amount,
            DocDate = now,
            CreatedDate = now,
            PostingBatchId = postingBatchId,
            SourceLineId = entry.SourceLineId,
            ReversalEntryId = entry.Id
        }).ToList();

        if (reversalEntries.Count > 0)
            await _command.CreateAsync(reversalEntries, ct);

        return Result.Success(reversalEntries);
    }

    private async Task<List<CounterpartyRegisterBalance>> GetOriginalEntriesAsync(long documentId, CancellationToken ct)
    {
        var query = _queryBuilder.For<CounterpartyRegisterBalance>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.PURCHASE &&
                        x.DocumentId == documentId &&
                        x.ReversalEntryId == null &&
                        x.OperationTypeId == OperationTypeIdConst.DEBT_INCREASE)
            .Build();

        return await _query.GetAllAsync(query, ct);
    }
}
