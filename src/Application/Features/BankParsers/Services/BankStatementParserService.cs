using ClosedXML.Excel;
using Application.Abstractions;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query.Specifications;
using SharedKernel.Results;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Application.Features.BankParsers;

public partial class BankStatementParserService : IBankStatementParserService
{
    private readonly IQueryRepository<BankAccount> _bankAccountQuery;
    private readonly IQueryRepository<CounterpartyCard> _counterpartyQuery;
    private readonly IQueryRepository<CounterpartyBankAccount> _counterpartyBankAccountQuery;

    public BankStatementParserService(
        IQueryRepository<BankAccount> bankAccountQuery,
        IQueryRepository<CounterpartyCard> counterpartyQuery,
        IQueryRepository<CounterpartyBankAccount> counterpartyBankAccountQuery)
    {
        _bankAccountQuery = bankAccountQuery;
        _counterpartyQuery = counterpartyQuery;
        _counterpartyBankAccountQuery = counterpartyBankAccountQuery;
    }

    public async Task<Result<BankExportDto>> ParseAsync(Stream stream, CancellationToken ct = default)
    {
        using var workbook = new XLWorkbook(stream);
        var export = new BankExportDto();

        foreach (var worksheet in workbook.Worksheets)
        {
            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;

            for (var row = 1; row <= lastRow - 4; row++)
            {
                if (!IsBankHeaderRow(worksheet, row))
                    continue;

                var statement = ParseAccountStatement(worksheet, row, lastRow);
                export.Accounts.Add(statement);
            }
        }

        await EnrichWithDatabaseIdsAsync(export, ct);

        return Result.Success(export);
    }

    private async Task EnrichWithDatabaseIdsAsync(BankExportDto export, CancellationToken ct)
    {
        await SetBankAccountIdsAsync(export, ct);
        await SetCounterpartyIdsAsync(export, ct);
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

        var specification = new QuerySpecification<BankAccount, BankAccountMatch>
        {
            Criteria = x => x.StateId == StateIdConst.ACTIVE &&
                            accountNumbers.Contains(x.AccountNumber),
            Selector = x => new BankAccountMatch
            {
                Id = x.Id,
                AccountNumber = x.AccountNumber
            }
        };

        var matches = await _bankAccountQuery.GetAllAsync(specification, ct);
        var idsByKey = matches
            .GroupBy(x => NormalizeKey(x.AccountNumber))
            .ToDictionary(x => x.Key, x => x.First().Id);

        foreach (var account in export.Accounts)
        {
            if (idsByKey.TryGetValue(NormalizeKey(account.AccountNumber), out var id))
                account.BankAccountId = id;
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

        var counterpartyAccounts = export.Accounts
            .SelectMany(x => x.Transactions)
            .Select(x => NormalizeKey(x.CounterpartyAccount))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();

        if (counterpartyInns.Count == 0 && counterpartyAccounts.Count == 0)
            return;

        var cardSpecification = new QuerySpecification<CounterpartyCard, CounterpartyMatch>
        {
            Criteria = x => x.StateId == StateIdConst.ACTIVE &&
                            x.Inn != null &&
                            (counterpartyInns.Contains(x.Inn) || counterpartyAccounts.Contains(x.Inn)),
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

        var accountSpecification = new QuerySpecification<CounterpartyBankAccount, CounterpartyAccountMatch>
        {
            Criteria = x => x.StateId == StateIdConst.ACTIVE &&
                            counterpartyAccounts.Contains(x.AccountNumber),
            Selector = x => new CounterpartyAccountMatch
            {
                CounterpartyId = x.CounterpartyId,
                AccountNumber = x.AccountNumber
            }
        };

        var accountMatches = await _counterpartyBankAccountQuery.GetAllAsync(accountSpecification, ct);
        var idsByAccount = accountMatches
            .GroupBy(x => NormalizeKey(x.AccountNumber))
            .ToDictionary(x => x.Key, x => x.First().CounterpartyId);

        foreach (var transaction in export.Accounts.SelectMany(x => x.Transactions))
        {
            if (idsByInn.TryGetValue(NormalizeKey(transaction.CounterpartyInn), out var id) ||
                idsByInn.TryGetValue(NormalizeKey(transaction.CounterpartyAccount), out id) ||
                idsByAccount.TryGetValue(NormalizeKey(transaction.CounterpartyAccount), out id))
                transaction.CounterpartyId = id;
        }
    }

    private static AccountStatementDto ParseAccountStatement(IXLWorksheet worksheet, int bankRow, int lastRow)
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

    private static bool IsBankHeaderRow(IXLWorksheet worksheet, int row)
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

    private sealed class BankAccountMatch
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; } = "";
    }

    private sealed class CounterpartyMatch
    {
        public int Id { get; set; }
        public string Inn { get; set; } = "";
    }

    private sealed class CounterpartyAccountMatch
    {
        public int CounterpartyId { get; set; }
        public string AccountNumber { get; set; } = "";
    }
}
