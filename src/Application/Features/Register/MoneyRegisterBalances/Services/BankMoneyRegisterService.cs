using Application.Abstractions;
using Application.Features.BankOperations;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.MoneyRegisterBalances;

public class BankMoneyRegisterService : IBankMoneyRegisterService
{
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<BankAccount> _bankAccountQuery;
    private readonly IQueryRepository<MoneyRegisterBalance> _query;
    private readonly ICommandRepository<MoneyRegisterBalance> _command;

    public BankMoneyRegisterService(
        IQueryBuilder queryBuilder,
        IQueryRepository<BankAccount> bankAccountQuery,
        IQueryRepository<MoneyRegisterBalance> query,
        ICommandRepository<MoneyRegisterBalance> command)
    {
        _queryBuilder = queryBuilder;
        _bankAccountQuery = bankAccountQuery;
        _query = query;
        _command = command;
    }

    public async Task<Result<List<MoneyRegisterBalance>>> PostAsync(BankOperation bankOperation, long postingBatchId, CancellationToken ct = default)
    {
        var now = DateTime.Now;
        var row = new MoneyRegisterBalance
        {
            OrganizationId = bankOperation.OrganizationId,
            DocumentTypeId = DocumentTypeIdConst.BANKOPERATION,
            DocumentId = bankOperation.Id,
            SourceType = "BANK_OPERATION",
            SourceId = bankOperation.BankAccountId,
            DirectionId = bankOperation.DirectionId,
            CurrencyId = bankOperation.CurrencyId,
            Amount = bankOperation.Amount,
            DocDate = bankOperation.DocDate,
            CreatedDate = now,
            PostingBatchId = postingBatchId
        };

        await _command.CreateAsync(row, ct);
        return Result.Success(new List<MoneyRegisterBalance> { row });
    }

    public async Task<Result<List<MoneyRegisterBalance>>> ReverseAsync(BankOperation bankOperation, long postingBatchId, CancellationToken ct = default)
    {
        var originals = await GetOriginalEntriesAsync(bankOperation.Id, ct);
        if (originals.Count == 0)
            return Result.Failure<List<MoneyRegisterBalance>>(BankOperationErrors.MissingMoneyRegisterEntries(bankOperation.Id, null));

        var now = DateTime.Now;
        var reversals = originals.Select(entry => new MoneyRegisterBalance
        {
            OrganizationId = entry.OrganizationId,
            DocumentTypeId = entry.DocumentTypeId,
            DocumentId = entry.DocumentId,
            SourceType = entry.SourceType,
            SourceId = entry.SourceId,
            DirectionId = MovementDirectionIdConst.Reverse(entry.DirectionId),
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

    public async Task<decimal> GetBankAccountBalanceAsync(int bankAccountId, DateTime asOfDate, CancellationToken ct = default)
    {
        var openingBalance = await GetOpeningBalanceAsync(bankAccountId, ct);
        var entries = await GetBankAccountEntriesAsync(bankAccountId, asOfDate, ct);

        return openingBalance + entries.Sum(x => x.DirectionId * x.Amount);
    }

    private async Task<List<MoneyRegisterBalance>> GetOriginalEntriesAsync(long bankOperationId, CancellationToken ct)
    {
        var query = _queryBuilder.For<MoneyRegisterBalance>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.BANKOPERATION &&
                        x.DocumentId == bankOperationId &&
                        x.ReversalEntryId == null)
            .Build();

        return await _query.GetAllAsync(query, ct);
    }

    private async Task<decimal> GetOpeningBalanceAsync(int bankAccountId, CancellationToken ct)
    {
        var query = _queryBuilder.For<BankAccount>()
            .Where(x => x.Id == bankAccountId)
            .Build();

        var bankAccount = await _bankAccountQuery.GetAsync(query, ct);
        return bankAccount?.OpeningBalance ?? 0m;
    }

    private async Task<List<MoneyRegisterBalance>> GetBankAccountEntriesAsync(int bankAccountId, DateTime asOfDate, CancellationToken ct)
    {
        var query = _queryBuilder.For<MoneyRegisterBalance>()
            .Where(x => x.SourceType == "BANK_OPERATION" &&
                        x.SourceId == bankAccountId &&
                        x.DocDate <= asOfDate)
            .Build();

        return await _query.GetAllAsync(query, ct);
    }
}
