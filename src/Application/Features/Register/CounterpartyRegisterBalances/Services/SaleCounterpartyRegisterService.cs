using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.SaleDocs;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.CounterpartyRegisterBalances;

public class SaleCounterpartyRegisterService : ISaleCounterpartyRegisterService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<CounterpartyRegisterBalance> _query;
    private readonly ICommandRepository<CounterpartyRegisterBalance> _command;

    public SaleCounterpartyRegisterService(IUserContext userContext,
                                           IQueryBuilder queryBuilder,
                                           IQueryRepository<CounterpartyRegisterBalance> query,
                                           ICommandRepository<CounterpartyRegisterBalance> command)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _query = query;
        _command = command;
    }

    public async Task<Result<List<CounterpartyRegisterBalance>>> PostAsync(SaleDoc sale, long postingBatchId, CancellationToken ct = default)
    {
        if (sale.FinalAmount == 0m)
            return Result.Success(new List<CounterpartyRegisterBalance>());

        var entry = new CounterpartyRegisterBalance
        {
            OrganizationId = sale.OrganizationId,
            DocumentTypeId = DocumentTypeIdConst.SALE,
            DocumentId = sale.Id,
            CounterpartyId = sale.CounterpartyId,
            OperationTypeId = OperationTypeIdConst.DEBT_INCREASE,
            CurrencyId = sale.CurrencyId,
            Amount = sale.FinalAmount,
            DocDate = sale.DocDate,
            CreatedDate = DateTime.Now,
            PostingBatchId = postingBatchId
        };

        await _command.CreateAsync(entry, ct);
        return Result.Success(new List<CounterpartyRegisterBalance> { entry });
    }

    public async Task<Result<List<CounterpartyRegisterBalance>>> ReverseAsync(SaleDoc sale, long postingBatchId, CancellationToken ct = default)
    {
        var originalEntries = await GetOriginalEntriesAsync(sale.Id, ct);
        if (originalEntries.Count == 0)
            return Result.Failure<List<CounterpartyRegisterBalance>>(
                SaleDocErrors.MissingCounterpartyRegisterEntries(sale.Id, _userContext.LanguageId));
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
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.SALE &&
                        x.DocumentId == documentId &&
                        x.ReversalEntryId == null &&
                        x.OperationTypeId == OperationTypeIdConst.DEBT_INCREASE)
            .Build();

        return await _query.GetAllAsync(query, ct);
    }
}
