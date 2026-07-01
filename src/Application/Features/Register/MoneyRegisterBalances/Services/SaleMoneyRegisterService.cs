using Application.Abstractions;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.MoneyRegisterBalances;

public class SaleMoneyRegisterService : ISaleMoneyRegisterService
{
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<MoneyRegisterBalance> _query;
    private readonly ICommandRepository<MoneyRegisterBalance> _command;

    public SaleMoneyRegisterService(IQueryBuilder queryBuilder,
                                    IQueryRepository<MoneyRegisterBalance> query,
                                    ICommandRepository<MoneyRegisterBalance> command)
    {
        _queryBuilder = queryBuilder;
        _query = query;
        _command = command;
    }

    public async Task<Result<List<MoneyRegisterBalance>>> PostAsync(SaleDoc sale, long postingBatchId, CancellationToken ct = default)
    {
        if (sale.FinalAmount == 0m)
            return Result.Success(new List<MoneyRegisterBalance>());

        var entry = new MoneyRegisterBalance
        {
            OrganizationId = sale.OrganizationId,
            DocumentTypeId = DocumentTypeIdConst.SALE,
            DocumentId = sale.Id,
            SourceType = "SALE_DOC",
            SourceId = (int)sale.Id,
            OperationTypeId = OperationTypeIdConst.IN,
            CurrencyId = sale.CurrencyId,
            Amount = sale.FinalAmount,
            DocDate = sale.DocDate,
            CreatedDate = DateTime.Now,
            PostingBatchId = postingBatchId
        };

        await _command.CreateAsync(entry, ct);
        return Result.Success(new List<MoneyRegisterBalance> { entry });
    }

    public async Task<Result<List<MoneyRegisterBalance>>> ReverseAsync(SaleDoc sale, long postingBatchId, CancellationToken ct = default)
    {
        var originalEntries = await GetOriginalEntriesAsync(sale.Id, ct);
        if (originalEntries.Count == 0)
            return Result.Failure<List<MoneyRegisterBalance>>(MoneyRegisterBalanceErrors.MissingOriginalEntries(sale.Id));

        var now = DateTime.Now;

        var reversalEntries = originalEntries.Select(entry => new MoneyRegisterBalance
        {
            OrganizationId = entry.OrganizationId,
            DocumentTypeId = entry.DocumentTypeId,
            DocumentId = entry.DocumentId,
            SourceType = entry.SourceType,
            SourceId = entry.SourceId,
            OperationTypeId = ReverseOperation(entry.OperationTypeId),
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

    private async Task<List<MoneyRegisterBalance>> GetOriginalEntriesAsync(long documentId, CancellationToken ct)
    {
        var query = _queryBuilder.For<MoneyRegisterBalance>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.SALE &&
                        x.DocumentId == documentId &&
                        x.ReversalEntryId == null &&
                        x.OperationTypeId == OperationTypeIdConst.IN)
            .Build();

        return await _query.GetAllAsync(query, ct);
    }

    private static short ReverseOperation(short operationTypeId) =>
        operationTypeId switch
        {
            OperationTypeIdConst.IN => OperationTypeIdConst.OUT,
            OperationTypeIdConst.OUT => OperationTypeIdConst.IN,
            _ => operationTypeId
        };
}
