using ClosedXML.Excel;
using Application.Abstractions;
using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.BankParsers;

public class BankStatementParserService : IBankStatementParserService
{
    private readonly IUserContext _userContext;
    private readonly IQueryRepository<BankStatementTemplate> _templateQuery;
    private readonly IQueryRepository<Bank> _bankQuery;
    private readonly IQueryRepository<BankBranch> _bankBranchQuery;
    private readonly IQueryRepository<BankAccount> _bankAccountQuery;
    private readonly IQueryRepository<CounterpartyCard> _counterpartyQuery;
    private readonly IQueryRepository<CounterpartyBankAccount> _counterpartyBankAccountQuery;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IBankOperationClassifier _operationClassifier;

    public BankStatementParserService(
        IUserContext userContext,
        IQueryRepository<BankStatementTemplate> templateQuery,
        IQueryRepository<Bank> bankQuery,
        IQueryRepository<BankBranch> bankBranchQuery,
        IQueryRepository<BankAccount> bankAccountQuery,
        IQueryRepository<CounterpartyCard> counterpartyQuery,
        IQueryRepository<CounterpartyBankAccount> counterpartyBankAccountQuery,
        IQueryBuilder queryBuilder,
        IBankOperationClassifier operationClassifier)
    {
        _userContext = userContext;
        _templateQuery = templateQuery;
        _bankQuery = bankQuery;
        _bankBranchQuery = bankBranchQuery;
        _bankAccountQuery = bankAccountQuery;
        _counterpartyQuery = counterpartyQuery;
        _counterpartyBankAccountQuery = counterpartyBankAccountQuery;
        _queryBuilder = queryBuilder;
        _operationClassifier = operationClassifier;
    }

    public async Task<Result<BankExportDto>> ParseAsync(
        Stream stream,
        int bankId,
        CancellationToken ct = default)
    {
        var parsedResult = await ParseExcelAsync(stream, bankId, ct);
        if (!parsedResult.IsSuccess)
            return Result.Failure<BankExportDto>(parsedResult.Error);

        var enrichedResult = await EnrichAsync(parsedResult.Value, ct);
        if (!enrichedResult.IsSuccess)
            return enrichedResult;

        return await _operationClassifier.ClassifyAsync(enrichedResult.Value, bankId, ct);
    }

    public async Task<Result<BankExportDto>> ParseExcelAsync(
        Stream stream,
        int bankId,
        CancellationToken ct = default)
    {
        var specification = _queryBuilder.For<BankStatementTemplate>()
            .Where(template =>
                template.BankId == bankId &&
                template.StateId == StateIdConst.ACTIVE)
            .AddIncludes(includes =>
            {
                includes.Include(template => template.HeaderRules);
                includes.Include(template => template.RowRules);
                includes.Include(template => template.Fields);
            })
            .Build();
        var templates = await _templateQuery.GetAllAsync(specification, ct);

        using var workbook = new XLWorkbook(stream);
        foreach (var template in templates
                     .OrderByDescending(template => template.Version)
                     .ThenBy(template => template.Id))
        {
            ct.ThrowIfCancellationRequested();
            var export = BankStatementTemplateParser.Parse(workbook, template);
            if (export.Accounts.Count > 0)
                return Result.Success(export);
        }

        return Result.Success(new BankExportDto());
    }

    public async Task<Result<BankExportDto>> EnrichAsync(BankExportDto export, CancellationToken ct = default)
    {
        await EnrichWithDatabaseIdsAsync(export, ct);
        return Result.Success(export);
    }

    private async Task EnrichWithDatabaseIdsAsync(BankExportDto export, CancellationToken ct)
    {
        await SetBankIdsAsync(export, ct);
        await SetBankAccountIdsAsync(export, ct);
        await SetBankBranchIdsAsync(export, ct);
        await SetCounterpartyIdsAsync(export, ct);
        await SetCounterpartyBankAccountIdsAsync(export, ct);
    }

    private async Task SetBankBranchIdsAsync(BankExportDto export, CancellationToken ct)
    {
        var mfos = export.Accounts
            .Select(x => NormalizeKey(x.BankMfo))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();

        if (mfos.Count == 0)
            return;

        var specification = _queryBuilder.For<BankBranch>()
            .Where(x => x.StateId == StateIdConst.ACTIVE && mfos.Contains(x.Mfo))
            .As(x => new BankBranchMatch
            {
                Id = x.Id,
                BankId = x.BankId,
                Mfo = x.Mfo
            })
            .Build();

        var matches = await _bankBranchQuery.GetAllAsync(specification, ct);
        var branchesByMfo = matches
            .GroupBy(x => NormalizeKey(x.Mfo))
            .ToDictionary(x => x.Key, x => x.First());

        foreach (var account in export.Accounts)
        {
            if (!branchesByMfo.TryGetValue(NormalizeKey(account.BankMfo), out var match))
                continue;

            account.BankBranchId = match.Id;
            account.BankId = match.BankId;
        }
    }

    private async Task SetBankIdsAsync(BankExportDto export, CancellationToken ct)
    {
        var bankInns = export.Accounts
            .Select(x => NormalizeKey(x.BankInn ?? ""))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();

        if (bankInns.Count == 0)
            return;

        var specification = _queryBuilder.For<Bank>()
            .Where(x => x.StateId == StateIdConst.ACTIVE &&
                        x.Inn != null &&
                        bankInns.Contains(x.Inn))
            .As(x => new BankMatch
            {
                Id = x.Id,
                Inn = x.Inn!
            })
            .Build();

        var matches = await _bankQuery.GetAllAsync(specification, ct);
        var idsByKey = matches
            .GroupBy(x => NormalizeKey(x.Inn))
            .ToDictionary(x => x.Key, x => x.First().Id);

        foreach (var account in export.Accounts)
        {
            var key = NormalizeKey(account.BankInn ?? "");
            if (idsByKey.TryGetValue(key, out var id))
                account.BankId = id;
        }
    }

    private async Task SetBankAccountIdsAsync(BankExportDto export, CancellationToken ct)
    {
        var accountNumbers = export.Accounts
            .Select(x => NormalizeKey(x.AccountNumber))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();

        if (accountNumbers.Count == 0)
            return;

        var organizationId = _userContext.OrganizationId;
        var specification = _queryBuilder.For<BankAccount>()
            .Where(x => x.StateId == StateIdConst.ACTIVE &&
                        (!organizationId.HasValue || x.OrganizationId == organizationId.Value) &&
                        accountNumbers.Contains(x.AccountNumber))
            .As(x => new BankAccountMatch
            {
                Id = x.Id,
                AccountNumber = x.AccountNumber,
                BankId = x.BankId,
                BankBranchId = x.BankBranchId,
                BankInn = x.Bank.Inn,
                BankMfo = x.BankBranch != null ? x.BankBranch.Mfo : x.Bank.Mfo,
                BankName = x.Bank.Name
            })
            .Build();

        var matches = await _bankAccountQuery.GetAllAsync(specification, ct);
        var idsByKey = matches
            .GroupBy(x => NormalizeKey(x.AccountNumber))
            .ToDictionary(x => x.Key, x => x.First());

        foreach (var account in export.Accounts)
        {
            if (idsByKey.TryGetValue(NormalizeKey(account.AccountNumber), out var match))
            {
                account.BankAccountId = match.Id;
                account.BankBranchId ??= match.BankBranchId;
                if (account.BankId is null)
                    account.BankId = match.BankId;

                if (string.IsNullOrWhiteSpace(account.BankInn) && !string.IsNullOrWhiteSpace(match.BankInn))
                    account.BankInn = match.BankInn;

                if (string.IsNullOrWhiteSpace(account.BankMfo) && !string.IsNullOrWhiteSpace(match.BankMfo))
                    account.BankMfo = match.BankMfo;

                if (string.IsNullOrWhiteSpace(account.BankName) && !string.IsNullOrWhiteSpace(match.BankName))
                    account.BankName = match.BankName;
            }
        }
    }

    private async Task SetCounterpartyIdsAsync(BankExportDto export, CancellationToken ct)
    {
        var counterpartyInns = export.Accounts
            .SelectMany(x => x.Transactions)
            .Select(x => NormalizeKey(x.CounterpartyInn))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();

        if (counterpartyInns.Count == 0)
            return;

        var organizationId = _userContext.OrganizationId;
        var cardSpecification = _queryBuilder.For<CounterpartyCard>()
            .Where(x => x.StateId == StateIdConst.ACTIVE &&
                        x.Inn != null &&
                        (!organizationId.HasValue || x.OrganizationId == organizationId.Value) &&
                        counterpartyInns.Contains(x.Inn))
            .As(x => new CounterpartyMatch
            {
                Id = x.Id,
                Inn = x.Inn!
            })
            .Build();

        var cardMatches = await _counterpartyQuery.GetAllAsync(cardSpecification, ct);
        var idsByInn = cardMatches
            .GroupBy(x => NormalizeKey(x.Inn))
            .ToDictionary(x => x.Key, x => x.First().Id);

        foreach (var transaction in export.Accounts.SelectMany(x => x.Transactions))
        {
            var key = NormalizeKey(transaction.CounterpartyInn);
            if (idsByInn.TryGetValue(key, out var id))
                transaction.CounterpartyId = id;
        }
    }

    private async Task SetCounterpartyBankAccountIdsAsync(BankExportDto export, CancellationToken ct)
    {
        var counterpartyAccounts = export.Accounts
            .SelectMany(x => x.Transactions)
            .Select(x => NormalizeKey(x.CounterpartyAccount))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();

        if (counterpartyAccounts.Count == 0)
            return;

        var organizationId = _userContext.OrganizationId;
        var accountSpecification = _queryBuilder.For<CounterpartyBankAccount>()
            .Where(x => x.StateId == StateIdConst.ACTIVE &&
                        (!organizationId.HasValue || x.OrganizationId == organizationId.Value) &&
                        counterpartyAccounts.Contains(x.AccountNumber))
            .As(x => new CounterpartyAccountMatch
            {
                Id = x.Id,
                CounterpartyId = x.CounterpartyId,
                AccountNumber = x.AccountNumber
            })
            .Build();

        var accountMatches = await _counterpartyBankAccountQuery.GetAllAsync(accountSpecification, ct);
        var idsByAccount = accountMatches
            .GroupBy(x => NormalizeKey(x.AccountNumber))
            .ToDictionary(x => x.Key, x => x.First());

        foreach (var transaction in export.Accounts.SelectMany(x => x.Transactions))
        {
            if (idsByAccount.TryGetValue(NormalizeKey(transaction.CounterpartyAccount), out var match))
            {
                transaction.CounterpartyBankAccountId = match.Id;
                transaction.CounterpartyId ??= match.CounterpartyId;
            }
        }
    }

    private static string NormalizeKey(string value) =>
        value.Replace(" ", "").Replace("\u00a0", "").Trim();

    private sealed class BankMatch
    {
        public int Id { get; set; }
        public string Inn { get; set; } = "";
    }

    private sealed class BankAccountMatch
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; } = "";
        public int BankId { get; set; }
        public int? BankBranchId { get; set; }
        public string? BankInn { get; set; }
        public string? BankMfo { get; set; }
        public string? BankName { get; set; }
    }

    private sealed class BankBranchMatch
    {
        public int Id { get; set; }
        public int BankId { get; set; }
        public string Mfo { get; set; } = "";
    }

    private sealed class CounterpartyMatch
    {
        public int Id { get; set; }
        public string Inn { get; set; } = "";
    }

    private sealed class CounterpartyAccountMatch
    {
        public int Id { get; set; }
        public int CounterpartyId { get; set; }
        public string AccountNumber { get; set; } = "";
    }}
