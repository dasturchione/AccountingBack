using ClosedXML.Excel;
using Application.Abstractions;
using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query.Specifications;
using SharedKernel.Results;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Application.Features.BankParsers;

public partial class BankStatementParserService : IBankStatementParserService
{
    private readonly IUserContext _userContext;
    private readonly IQueryRepository<Bank> _bankQuery;
    private readonly IQueryRepository<BankAccount> _bankAccountQuery;
    private readonly IQueryRepository<CounterpartyCard> _counterpartyQuery;
    private readonly IQueryRepository<CounterpartyBankAccount> _counterpartyBankAccountQuery;

    public BankStatementParserService(
        IUserContext userContext,
        IQueryRepository<Bank> bankQuery,
        IQueryRepository<BankAccount> bankAccountQuery,
        IQueryRepository<CounterpartyCard> counterpartyQuery,
        IQueryRepository<CounterpartyBankAccount> counterpartyBankAccountQuery)
    {
        _userContext = userContext;
        _bankQuery = bankQuery;
        _bankAccountQuery = bankAccountQuery;
        _counterpartyQuery = counterpartyQuery;
        _counterpartyBankAccountQuery = counterpartyBankAccountQuery;
    }

    public async Task<Result<BankExportDto>> ParseAsync(
        Stream stream,
        BankStatementBankType bankType,
        CancellationToken ct = default)
    {
        var parsedResult = await ParseExcelAsync(stream, bankType, ct);
        if (!parsedResult.IsSuccess)
            return Result.Failure<BankExportDto>(parsedResult.Error);

        return await EnrichAsync(parsedResult.Value, ct);
    }

    public Task<Result<BankExportDto>> ParseExcelAsync(
        Stream stream,
        BankStatementBankType bankType,
        CancellationToken ct = default)
    {
        if (!Enum.IsDefined(typeof(BankStatementBankType), bankType))
        {
            return Task.FromResult(Result.Failure<BankExportDto>(
                BankStatementParserErrors.InvalidBankType(_userContext.LanguageId)));
        }

        using var workbook = new XLWorkbook(stream);
        var export = bankType switch
        {
            BankStatementBankType.Trustbank => ParseTrustbankWorkbook(workbook),
            BankStatementBankType.Uzsanoatqurilishbank => ParseUzsanoatqurilishbankWorkbook(workbook),
            _ => new BankExportDto()
        };

        if (export.Accounts.Count == 0)
        {
            return Task.FromResult(Result.Failure<BankExportDto>(
                BankStatementParserErrors.StatementNotFound(bankType, _userContext.LanguageId)));
        }

        return Task.FromResult(Result.Success(export));
    }

    private static BankExportDto ParseTrustbankWorkbook(XLWorkbook workbook)
    {
        var export = new BankExportDto();

        foreach (var worksheet in workbook.Worksheets)
        {
            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;

            for (var row = 1; row <= lastRow - 4; row++)
            {
                if (!IsTrustbankHeaderRow(worksheet, row))
                    continue;

                var statement = ParseTrustbankAccountStatement(worksheet, row, lastRow);
                export.Accounts.Add(statement);
            }
        }

        return export;
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
        await SetCounterpartyIdsAsync(export, ct);
        await SetCounterpartyBankAccountIdsAsync(export, ct);
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

        var specification = new QuerySpecification<Bank, BankMatch>
        {
            Criteria = x => x.StateId == StateIdConst.ACTIVE &&
                            x.Inn != null &&
                            bankInns.Contains(x.Inn),
            Selector = x => new BankMatch
            {
                Id = x.Id,
                Inn = x.Inn!
            }
        };

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
        var specification = new QuerySpecification<BankAccount, BankAccountMatch>
        {
            Criteria = x => x.StateId == StateIdConst.ACTIVE &&
                            (!organizationId.HasValue || x.OrganizationId == organizationId.Value) &&
                            accountNumbers.Contains(x.AccountNumber),
            Selector = x => new BankAccountMatch
            {
                Id = x.Id,
                AccountNumber = x.AccountNumber,
                BankId = x.BankId,
                BankInn = x.Bank.Inn,
                BankMfo = x.Bank.Mfo,
                BankName = x.Bank.Name
            }
        };

        var matches = await _bankAccountQuery.GetAllAsync(specification, ct);
        var idsByKey = matches
            .GroupBy(x => NormalizeKey(x.AccountNumber))
            .ToDictionary(x => x.Key, x => x.First());

        foreach (var account in export.Accounts)
        {
            if (idsByKey.TryGetValue(NormalizeKey(account.AccountNumber), out var match))
            {
                account.BankAccountId = match.Id;
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
        var cardSpecification = new QuerySpecification<CounterpartyCard, CounterpartyMatch>
        {
            Criteria = x => x.StateId == StateIdConst.ACTIVE &&
                            x.Inn != null &&
                            (!organizationId.HasValue || x.OrganizationId == organizationId.Value) &&
                            counterpartyInns.Contains(x.Inn),
            Selector = x => new CounterpartyMatch
            {
                Id = x.Id,
                Inn = x.Inn!
            }
        };

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
        var accountSpecification = new QuerySpecification<CounterpartyBankAccount, CounterpartyAccountMatch>
        {
            Criteria = x => x.StateId == StateIdConst.ACTIVE &&
                            (!organizationId.HasValue || x.OrganizationId == organizationId.Value) &&
                            counterpartyAccounts.Contains(x.AccountNumber),
            Selector = x => new CounterpartyAccountMatch
            {
                Id = x.Id,
                CounterpartyId = x.CounterpartyId,
                AccountNumber = x.AccountNumber
            }
        };

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

    private static AccountStatementDto ParseTrustbankAccountStatement(IXLWorksheet worksheet, int bankRow, int lastRow)
    {
        var periodRow = bankRow + 1;
        var accountRow = bankRow + 2;
        var balanceRow = bankRow + 3;
        var headerRow = bankRow + 4;

        var bank = ParseBank(GetText(worksheet, bankRow, 1));
        var period = ParsePeriod(GetText(worksheet, periodRow, 1));
        var account = ParseAccount(GetText(worksheet, accountRow, 1));

        var statement = new AccountStatementDto
        {
            BankMfo = bank.Mfo,
            BankName = bank.Name,
            AccountNumber = account.AccountNumber,
            CompanyName = account.CompanyName,
            CompanyInn = account.CompanyInn,
            PeriodFrom = period.From,
            PeriodTo = period.To,
            OpeningBalance = ParseAmountFromText(GetText(worksheet, balanceRow, 1)),
            ClosingBalance = ParseAmountFromText(GetText(worksheet, balanceRow, 2))
        };

        for (var row = headerRow + 1; row <= lastRow; row++)
        {
            var firstCell = GetText(worksheet, row, 1);

            if (IsTotalRow(firstCell))
            {
                statement.TotalDebit = GetDecimal(worksheet, row, 6);
                statement.TotalCredit = GetDecimal(worksheet, row, 7);
                break;
            }

            var date = GetDateTime(worksheet, row, 1);
            if (date is null)
                break;

            var counterparty = ParseCounterparty(GetText(worksheet, row, 2));
            var debit = GetDecimal(worksheet, row, 6);
            var credit = GetDecimal(worksheet, row, 7);

            statement.Transactions.Add(new TransactionDto
            {
                Date = date.Value,
                DocNumber = GetText(worksheet, row, 3),
                OperationCode = GetText(worksheet, row, 4),
                MfoCounterparty = GetText(worksheet, row, 5),
                CounterpartyAccount = counterparty.Account,
                CounterpartyInn = counterparty.Inn,
                CounterpartyName = counterparty.Name,
                Debit = debit,
                Credit = credit,
                Purpose = GetText(worksheet, row, 8),
                Direction = debit > 0 ? "outgoing" : credit > 0 ? "incoming" : "",
                Amount = debit > 0 ? debit : credit
            });
        }

        if (statement.TotalDebit == 0 && statement.Transactions.Count > 0)
            statement.TotalDebit = statement.Transactions.Sum(x => x.Debit);

        if (statement.TotalCredit == 0 && statement.Transactions.Count > 0)
            statement.TotalCredit = statement.Transactions.Sum(x => x.Credit);

        statement.HasActivity = statement.Transactions.Count > 0 ||
                                statement.TotalDebit != 0 ||
                                statement.TotalCredit != 0;

        return statement;
    }

    private static bool IsTrustbankHeaderRow(IXLWorksheet worksheet, int row)
    {
        var current = GetText(worksheet, row, 1);
        var period = GetText(worksheet, row + 1, 1);
        var account = GetText(worksheet, row + 2, 1);
        var header = GetText(worksheet, row + 4, 1);

        return current.Contains('/') &&
               period.Contains("Сведения", StringComparison.OrdinalIgnoreCase) &&
               account.Contains("Cчет:", StringComparison.OrdinalIgnoreCase) &&
               header.Equals("Дата", StringComparison.OrdinalIgnoreCase);
    }

    private static (string Mfo, string Name) ParseBank(string value)
    {
        var parts = value.Split('/', 2, StringSplitOptions.TrimEntries);
        return parts.Length == 2 ? (parts[0], parts[1]) : ("", value.Trim());
    }

    private static (DateTime From, DateTime To) ParsePeriod(string value)
    {
        var match = PeriodRegex().Match(value);
        return match.Success
            ? (ParseDate(match.Groups[1].Value), ParseDate(match.Groups[2].Value))
            : (default, default);
    }

    private static (string AccountNumber, string CompanyName, string CompanyInn) ParseAccount(string value)
    {
        var normalized = value.Replace('\u00a0', ' ').Trim();
        var match = AccountRegex().Match(normalized);

        if (!match.Success)
            return ("", normalized, "");

        return (
            match.Groups["account"].Value.Trim(),
            match.Groups["name"].Value.Trim(),
            match.Groups["inn"].Value.Trim()
        );
    }

    private static (string Account, string Inn, string Name) ParseCounterparty(string value)
    {
        var parts = value.Split('/', 3, StringSplitOptions.TrimEntries);

        return parts.Length switch
        {
            >= 3 => (parts[0], parts[1], parts[2]),
            2 => (parts[0], parts[1], ""),
            1 => (parts[0], "", ""),
            _ => ("", "", "")
        };
    }

    private static string NormalizeKey(string value) =>
        value.Replace(" ", "").Replace("\u00a0", "").Trim();

    private static DateTime ParseDate(string value) =>
        DateTime.ParseExact(value, "dd.MM.yyyy", CultureInfo.InvariantCulture);

    private static decimal ParseAmountFromText(string value)
    {
        var match = AmountRegex().Match(value);
        return match.Success ? ParseDecimal(match.Value) : 0m;
    }

    private static string GetText(IXLWorksheet worksheet, int row, int column) =>
        worksheet.Cell(row, column).GetFormattedString().Trim();

    private static decimal GetDecimal(IXLWorksheet worksheet, int row, int column)
    {
        var cell = worksheet.Cell(row, column);
        if (cell.TryGetValue<decimal>(out var value))
            return value;

        return ParseDecimal(cell.GetFormattedString());
    }

    private static decimal ParseDecimal(string value)
    {
        var normalized = value.Replace(" ", "").Replace("\u00a0", "").Replace(",", ".").Trim();
        return decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
            ? result
            : 0m;
    }

    private static DateTime? GetDateTime(IXLWorksheet worksheet, int row, int column)
    {
        var cell = worksheet.Cell(row, column);
        if (cell.TryGetValue<DateTime>(out var date))
            return date;

        var text = cell.GetFormattedString().Trim();
        return DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? parsed
            : null;
    }

    private static bool IsTotalRow(string value) =>
        value.Contains("Итоговый оборот", StringComparison.OrdinalIgnoreCase);

    [GeneratedRegex(@"c\s+(\d{2}\.\d{2}\.\d{4})\s+по\s+(\d{2}\.\d{2}\.\d{4})", RegexOptions.IgnoreCase)]
    private static partial Regex PeriodRegex();

    [GeneratedRegex(@"Cчет:\s*(?<account>\d+)\s+(?<name>.*?)\s+ИНН\s*:\s*(?<inn>\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex AccountRegex();

    [GeneratedRegex(@"-?\d[\d\s\u00a0]*([.,]\d+)?")]
    private static partial Regex AmountRegex();

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
        public string? BankInn { get; set; }
        public string? BankMfo { get; set; }
        public string? BankName { get; set; }
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
