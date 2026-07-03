using Application.Abstractions;
using Application.Features.CashOperations;
using Application.Features.Register;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.MoneyRegisterBalances;

public class CashMoneyRegisterService : ICashMoneyRegisterService
{
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<CashBox> _cashBoxQuery;
    private readonly IQueryRepository<MoneyRegisterBalance> _query;
    private readonly ICommandRepository<MoneyRegisterBalance> _command;

    public CashMoneyRegisterService(
        IQueryBuilder queryBuilder,
        IQueryRepository<CashBox> cashBoxQuery,
        IQueryRepository<MoneyRegisterBalance> query,
        ICommandRepository<MoneyRegisterBalance> command)
    {
        _queryBuilder = queryBuilder;
        _cashBoxQuery = cashBoxQuery;
        _query = query;
        _command = command;
    }

    public async Task<Result<List<MoneyRegisterBalance>>> PostAsync(
        CashOperation cashOperation,
        long postingBatchId,
        CancellationToken ct = default)
    {
        if (cashOperation.OperationTypeId == OperationTypeIdConst.TRANSFER && cashOperation.DestinationCashBoxId is null)
            return Result.Success(new List<MoneyRegisterBalance>());

        var now = DateTime.Now;
        var rows = BuildEntries(cashOperation, postingBatchId, now, false);
        if (rows.Count == 0)
            return Result.Success(rows);

        await _command.CreateAsync(rows, ct);
        return Result.Success(rows);
    }

    public async Task<Result<List<MoneyRegisterBalance>>> ReverseAsync(
        CashOperation cashOperation,
        long postingBatchId,
        CancellationToken ct = default)
    {
        var originals = await GetOriginalEntriesAsync(cashOperation.Id, ct);
        if (originals.Count == 0)
            return Result.Failure<List<MoneyRegisterBalance>>(CashOperationErrors.MissingMoneyRegisterEntries(cashOperation.Id, null));

        var now = DateTime.Now;
        var reversals = originals.Select(entry => new MoneyRegisterBalance
        {
            OrganizationId = entry.OrganizationId,
            DocumentTypeId = entry.DocumentTypeId,
            DocumentId = entry.DocumentId,
            SourceType = entry.SourceType,
            SourceId = entry.SourceId,
            OperationTypeId = entry.OperationTypeId == OperationTypeIdConst.IN
                ? OperationTypeIdConst.OUT
                : OperationTypeIdConst.IN,
            CurrencyId = entry.CurrencyId,
            Amount = entry.Amount,
            DocDate = now,
            CreatedDate = now,
            PostingBatchId = postingBatchId,
            SourceLineId = entry.SourceLineId,
            ReversalEntryId = entry.Id
        }).ToList();

        if (reversals.Count > 0)
            await _command.CreateAsync(reversals, ct);

        return Result.Success(reversals);
    }

    public async Task<decimal> GetCashBoxBalanceAsync(int cashBoxId, DateTime asOfDate, CancellationToken ct = default)
    {
        var openingBalance = await GetOpeningBalanceAsync(cashBoxId, ct);
        var rows = await GetCashBoxEntriesAsync(cashBoxId, asOfDate, ct);

        return openingBalance + rows.Where(x => x.OperationTypeId == OperationTypeIdConst.IN).Sum(x => x.Amount)
                                  - rows.Where(x => x.OperationTypeId == OperationTypeIdConst.OUT).Sum(x => x.Amount);
    }

    private List<MoneyRegisterBalance> BuildEntries(
        CashOperation cashOperation,
        long postingBatchId,
        DateTime now,
        bool isReversed)
    {
        var sourceType = RegisterDefaultsConst.CashOperation;
        return cashOperation.OperationTypeId switch
        {
            OperationTypeIdConst.IN => new List<MoneyRegisterBalance>
            {
                new()
                {
                    OrganizationId = cashOperation.OrganizationId,
                    DocumentTypeId = DocumentTypeIdConst.CASHOPERATION,
                    DocumentId = cashOperation.Id,
                    SourceType = sourceType,
                    SourceId = cashOperation.CashBoxId,
                    OperationTypeId = isReversed ? OperationTypeIdConst.OUT : OperationTypeIdConst.IN,
                    CurrencyId = cashOperation.CurrencyId,
                    Amount = cashOperation.Amount,
                    DocDate = cashOperation.DocDate,
                    CreatedDate = now,
                    PostingBatchId = postingBatchId,
                }
            },
            OperationTypeIdConst.OUT => new List<MoneyRegisterBalance>
            {
                new()
                {
                    OrganizationId = cashOperation.OrganizationId,
                    DocumentTypeId = DocumentTypeIdConst.CASHOPERATION,
                    DocumentId = cashOperation.Id,
                    SourceType = sourceType,
                    SourceId = cashOperation.CashBoxId,
                    OperationTypeId = isReversed ? OperationTypeIdConst.IN : OperationTypeIdConst.OUT,
                    CurrencyId = cashOperation.CurrencyId,
                    Amount = cashOperation.Amount,
                    DocDate = cashOperation.DocDate,
                    CreatedDate = now,
                    PostingBatchId = postingBatchId,
                }
            },
            OperationTypeIdConst.TRANSFER => new List<MoneyRegisterBalance>
            {
                new()
                {
                    OrganizationId = cashOperation.OrganizationId,
                    DocumentTypeId = DocumentTypeIdConst.CASHOPERATION,
                    DocumentId = cashOperation.Id,
                    SourceType = RegisterDefaultsConst.CashOperationOut,
                    SourceId = cashOperation.CashBoxId,
                    OperationTypeId = isReversed ? OperationTypeIdConst.IN : OperationTypeIdConst.OUT,
                    CurrencyId = cashOperation.CurrencyId,
                    Amount = cashOperation.Amount,
                    DocDate = cashOperation.DocDate,
                    CreatedDate = now,
                    PostingBatchId = postingBatchId,
                },
                new()
                {
                    OrganizationId = cashOperation.OrganizationId,
                    DocumentTypeId = DocumentTypeIdConst.CASHOPERATION,
                    DocumentId = cashOperation.Id,
                    SourceType = RegisterDefaultsConst.CashOperationIn,
                    SourceId = cashOperation.DestinationCashBoxId!.Value,
                    OperationTypeId = isReversed ? OperationTypeIdConst.OUT : OperationTypeIdConst.IN,
                    CurrencyId = cashOperation.CurrencyId,
                    Amount = cashOperation.Amount,
                    DocDate = cashOperation.DocDate,
                    CreatedDate = now,
                    PostingBatchId = postingBatchId,
                }
            },
            _ => new List<MoneyRegisterBalance>()
        };
    }

    private async Task<List<MoneyRegisterBalance>> GetOriginalEntriesAsync(long cashOperationId, CancellationToken ct)
    {
        var query = _queryBuilder.For<MoneyRegisterBalance>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.CASHOPERATION &&
                        x.DocumentId == cashOperationId &&
                        x.ReversalEntryId == null)
            .Build();

        return await _query.GetAllAsync(query, ct);
    }

    private async Task<decimal> GetOpeningBalanceAsync(int cashBoxId, CancellationToken ct)
    {
        var query = _queryBuilder.For<CashBox>()
            .Where(x => x.Id == cashBoxId)
            .Build();
        var cashBox = await _cashBoxQuery.GetAsync(query, ct);
        return cashBox?.OpeningBalance ?? 0m;
    }

    private async Task<List<MoneyRegisterBalance>> GetCashBoxEntriesAsync(int cashBoxId, DateTime asOfDate, CancellationToken ct)
    {
        var query = _queryBuilder.For<MoneyRegisterBalance>()
            .Where(x => x.SourceType.StartsWith(RegisterDefaultsConst.CashOperation) &&
                        x.SourceId == cashBoxId &&
                        x.DocDate <= asOfDate)
            .Build();

        return await _query.GetAllAsync(query, ct);
    }
}
