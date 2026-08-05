using ClosedXML.Excel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Application.Features.BankParsers;

public partial class BankStatementParserService
{
    private static BankExportDto ParseUzsanoatqurilishbankWorkbook(XLWorkbook workbook)
    {
        var export = new BankExportDto();

        foreach (var worksheet in workbook.Worksheets)
        {
            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;
            var headerRow = FindUzsanoatqurilishbankHeaderRow(worksheet, lastRow);
            if (headerRow == 0)
                continue;

            var statement = ParseUzsanoatqurilishbankStatement(worksheet, headerRow, lastRow);
            if (statement.HasActivity)
                export.Accounts.Add(statement);
        }

        return export;
    }

    private static int FindUzsanoatqurilishbankHeaderRow(IXLWorksheet worksheet, int lastRow)
    {
        for (var row = 1; row <= lastRow; row++)
        {
            if (IsUzsanoatqurilishbankHeaderRow(worksheet, row))
                return row;
        }

        return 0;
    }

    private static AccountStatementDto ParseUzsanoatqurilishbankStatement(
        IXLWorksheet worksheet,
        int headerRow,
        int lastRow)
    {
        var account = ParseUzsanoatqurilishbankAccount(
            FindUzsanoatqurilishbankTextAbove(worksheet, headerRow, value => UzsanoatqurilishbankAccountRegex().IsMatch(value)));
        var client = ParseUzsanoatqurilishbankClient(
            FindUzsanoatqurilishbankTextAbove(worksheet, headerRow, value => UzsanoatqurilishbankClientRegex().IsMatch(value)));
        var period = ParseUzsanoatqurilishbankPeriod(
            FindUzsanoatqurilishbankTextAbove(worksheet, headerRow, value => UzsanoatqurilishbankPeriodRegex().IsMatch(value)));
        var openingBalanceRow = FindUzsanoatqurilishbankRowAbove(
            worksheet,
            headerRow,
            IsUzsanoatqurilishbankOpeningBalanceRow);

        var statement = new AccountStatementDto
        {
            BankName = GetText(worksheet, Math.Max(1, headerRow - 6), 1),
            AccountNumber = account,
            CompanyName = client.Name,
            CompanyInn = client.Inn,
            PeriodFrom = period.From,
            PeriodTo = period.To,
            OpeningBalance = openingBalanceRow == 0 ? 0m : GetDecimal(worksheet, openingBalanceRow, 5)
        };

        for (var row = headerRow + 1; row <= lastRow; row++)
        {
            var firstCell = GetText(worksheet, row, 1);
            if (IsUzsanoatqurilishbankTotalRow(firstCell))
            {
                statement.TotalDebit = GetDecimal(worksheet, row, 4);
                statement.TotalCredit = GetDecimal(worksheet, row, 5);

                var closingBalanceRow = FindUzsanoatqurilishbankRowBelow(
                    worksheet,
                    row + 1,
                    lastRow,
                    IsUzsanoatqurilishbankClosingBalanceRow);
                if (closingBalanceRow != 0)
                    statement.ClosingBalance = GetDecimal(worksheet, closingBalanceRow, 5);

                break;
            }

            var date = GetUzsanoatqurilishbankDateTime(worksheet, row);
            if (date is null)
                continue;

            var debit = GetDecimal(worksheet, row, 4);
            var credit = GetDecimal(worksheet, row, 5);
            if (debit == 0m && credit == 0m)
                continue;

            var counterparty = ParseUzsanoatqurilishbankCounterparty(GetText(worksheet, row, 3));
            statement.Transactions.Add(new TransactionDto
            {
                Date = date.Value,
                DocNumber = GetText(worksheet, row, 2),
                MfoCounterparty = counterparty.Mfo,
                CounterpartyAccount = counterparty.Account,
                CounterpartyInn = counterparty.Inn,
                CounterpartyName = counterparty.Name,
                Debit = debit,
                Credit = credit,
                Purpose = counterparty.Purpose,
                Direction = debit > 0m ? "outgoing" : "incoming",
                Amount = debit > 0m ? debit : credit
            });
        }

        if (statement.TotalDebit == 0m)
            statement.TotalDebit = statement.Transactions.Sum(transaction => transaction.Debit);

        if (statement.TotalCredit == 0m)
            statement.TotalCredit = statement.Transactions.Sum(transaction => transaction.Credit);

        statement.HasActivity = statement.Transactions.Count > 0 ||
                                statement.TotalDebit != 0m ||
                                statement.TotalCredit != 0m;

        return statement;
    }

    private static DateTime? GetUzsanoatqurilishbankDateTime(IXLWorksheet worksheet, int row)
    {
        var cell = worksheet.Cell(row, 1);
        if (cell.TryGetValue<DateTime>(out var date))
            return date;

        var value = GetText(worksheet, row, 1);
        return DateTime.TryParseExact(
            value,
            "dd.MM.yyyy",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var parsed)
            ? parsed
            : null;
    }


    private static bool IsUzsanoatqurilishbankHeaderRow(IXLWorksheet worksheet, int row) =>
        GetText(worksheet, row, 1).Equals("Дата", StringComparison.OrdinalIgnoreCase) &&
        GetText(worksheet, row, 2).Contains("Номер", StringComparison.OrdinalIgnoreCase) &&
        GetText(worksheet, row, 3).Contains("Корреспондент", StringComparison.OrdinalIgnoreCase) &&
        GetText(worksheet, row, 4).Equals("Дебет", StringComparison.OrdinalIgnoreCase) &&
        GetText(worksheet, row, 5).Equals("Кредит", StringComparison.OrdinalIgnoreCase);

    private static bool IsUzsanoatqurilishbankOpeningBalanceRow(string value) =>
        value.Contains("Входящий остаток", StringComparison.OrdinalIgnoreCase);

    private static bool IsUzsanoatqurilishbankClosingBalanceRow(string value) =>
        value.Contains("Исходящий остаток", StringComparison.OrdinalIgnoreCase);

    private static bool IsUzsanoatqurilishbankTotalRow(string value) =>
        value.Contains("Сумма оборотов", StringComparison.OrdinalIgnoreCase);

    private static string FindUzsanoatqurilishbankTextAbove(
        IXLWorksheet worksheet,
        int headerRow,
        Func<string, bool> predicate)
    {
        var row = FindUzsanoatqurilishbankRowAbove(worksheet, headerRow, predicate);
        return row == 0 ? string.Empty : GetText(worksheet, row, 1);
    }

    private static int FindUzsanoatqurilishbankRowAbove(
        IXLWorksheet worksheet,
        int startRow,
        Func<string, bool> predicate)
    {
        for (var row = startRow - 1; row >= Math.Max(1, startRow - 10); row--)
        {
            if (predicate(GetText(worksheet, row, 1)))
                return row;
        }

        return 0;
    }

    private static int FindUzsanoatqurilishbankRowBelow(
        IXLWorksheet worksheet,
        int startRow,
        int lastRow,
        Func<string, bool> predicate)
    {
        for (var row = startRow; row <= Math.Min(lastRow, startRow + 5); row++)
        {
            if (predicate(GetText(worksheet, row, 1)))
                return row;
        }

        return 0;
    }

    private static (DateTime From, DateTime To) ParseUzsanoatqurilishbankPeriod(string value)
    {
        var match = UzsanoatqurilishbankPeriodRegex().Match(value);
        return match.Success
            ? (ParseDate(match.Groups["from"].Value), ParseDate(match.Groups["to"].Value))
            : (default, default);
    }

    private static string ParseUzsanoatqurilishbankAccount(string value)
    {
        var match = UzsanoatqurilishbankAccountRegex().Match(value);
        return match.Success ? match.Groups["account"].Value : string.Empty;
    }

    private static (string Name, string Inn) ParseUzsanoatqurilishbankClient(string value)
    {
        var match = UzsanoatqurilishbankClientRegex().Match(value);
        return match.Success
            ? (match.Groups["name"].Value.Trim(), match.Groups["inn"].Value)
            : (string.Empty, string.Empty);
    }

    private static (string Mfo, string Account, string Inn, string Name, string Purpose)
        ParseUzsanoatqurilishbankCounterparty(string value)
    {
        var match = UzsanoatqurilishbankCounterpartyRegex().Match(value.Replace('\u00a0', ' '));
        if (!match.Success)
            return (string.Empty, string.Empty, string.Empty, string.Empty, value.Trim());

        var tailLines = match.Groups["tail"].Value
            .Split(['\r', '\n'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        return (
            match.Groups["mfo"].Value,
            match.Groups["account"].Value,
            match.Groups["inn"].Value,
            tailLines.FirstOrDefault() ?? string.Empty,
            string.Join(Environment.NewLine, tailLines.Skip(1)));
    }

    [GeneratedRegex(@"Период\s+выписки\s+с\s+(?<from>\d{2}\.\d{2}\.\d{4})\s+по\s+(?<to>\d{2}\.\d{2}\.\d{4})", RegexOptions.IgnoreCase)]
    private static partial Regex UzsanoatqurilishbankPeriodRegex();

    [GeneratedRegex(@"Лицевой\s+счет\s*:\s*(?<account>\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex UzsanoatqurilishbankAccountRegex();

    [GeneratedRegex(@"Клиент\s*(?<name>.*?)\s+Инн\s*(?<inn>\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex UzsanoatqurilishbankClientRegex();

    [GeneratedRegex(@"МФО\s*:\s*(?<mfo>\d+)\s+Счет\s*:\s*(?<account>\d+)\s+ИНН\s*:\s*(?<inn>\d+)\s*(?<tail>.*)", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex UzsanoatqurilishbankCounterpartyRegex();
}