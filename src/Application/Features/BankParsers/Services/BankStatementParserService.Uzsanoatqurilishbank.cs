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
            FindUzsanoatqurilishbankTextAbove(worksheet, headerRow, value => UzsanoatqurilishbankAccountRegex().IsMatch(NormalizeUzsanoatqurilishbankText(value))));
        var period = ParseUzsanoatqurilishbankPeriod(
            FindUzsanoatqurilishbankTextAbove(worksheet, headerRow, value => UzsanoatqurilishbankPeriodRegex().IsMatch(value)));
        var balanceRow = FindUzsanoatqurilishbankRowAbove(
            worksheet,
            headerRow,
            value => value.Contains("Остаток на начало периода", StringComparison.OrdinalIgnoreCase));

        var statement = new AccountStatementDto
        {
            BankName = GetText(worksheet, 1, 1).TrimStart('/', ' '),
            AccountNumber = account.AccountNumber,
            CompanyName = account.CompanyName,
            CompanyInn = account.CompanyInn,
            PeriodFrom = period.From,
            PeriodTo = period.To,
            OpeningBalance = balanceRow == 0
                ? 0m
                : ParseAmountFromText(GetText(worksheet, balanceRow, 1)),
            ClosingBalance = balanceRow == 0
                ? 0m
                : ParseAmountFromText(GetText(worksheet, balanceRow, 2))
        };

        for (var row = headerRow + 1; row <= lastRow; row++)
        {
            var firstCell = GetText(worksheet, row, 1);
            if (IsUzsanoatqurilishbankTotalRow(firstCell))
            {
                statement.TotalDebit = GetDecimal(worksheet, row, 9);
                statement.TotalCredit = GetDecimal(worksheet, row, 8);
                break;
            }

            var date = GetUzsanoatqurilishbankDateTime(worksheet, row);
            if (date is null)
                continue;

            var bankDebit = GetDecimal(worksheet, row, 8);
            var bankCredit = GetDecimal(worksheet, row, 9);
            if (bankDebit == 0m && bankCredit == 0m)
                continue;

            statement.Transactions.Add(new TransactionDto
            {
                Date = date.Value,
                DocNumber = GetText(worksheet, row, 2),
                MfoCounterparty = NormalizeUzsanoatqurilishbankMfo(GetText(worksheet, row, 3)),
                CounterpartyAccount = NormalizeKey(GetText(worksheet, row, 4)),
                CounterpartyName = GetText(worksheet, row, 5),
                CounterpartyInn = NormalizeKey(GetText(worksheet, row, 6)),
                Debit = bankCredit,
                Credit = bankDebit,
                Purpose = GetText(worksheet, row, 7),
                Direction = bankCredit > 0m ? "incoming" : "outgoing",
                Amount = bankCredit > 0m ? bankCredit : bankDebit
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
        GetText(worksheet, row, 2).Contains("Номер документа", StringComparison.OrdinalIgnoreCase) &&
        GetText(worksheet, row, 3).Contains("МФО корресп", StringComparison.OrdinalIgnoreCase) &&
        GetText(worksheet, row, 4).Contains("Счет корреспондента", StringComparison.OrdinalIgnoreCase) &&
        GetText(worksheet, row, 5).Contains("Наименование корресп", StringComparison.OrdinalIgnoreCase) &&
        GetText(worksheet, row, 6).Contains("ИНН корреспондента", StringComparison.OrdinalIgnoreCase) &&
        GetText(worksheet, row, 7).Contains("Назначение платежа", StringComparison.OrdinalIgnoreCase) &&
        GetText(worksheet, row, 8).Equals("Дебет", StringComparison.OrdinalIgnoreCase) &&
        GetText(worksheet, row, 9).Equals("Кредит", StringComparison.OrdinalIgnoreCase);

    private static bool IsUzsanoatqurilishbankTotalRow(string value) =>
        value.Contains("Итоговый оборот за период", StringComparison.OrdinalIgnoreCase);

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

    private static (DateTime From, DateTime To) ParseUzsanoatqurilishbankPeriod(string value)
    {
        var match = UzsanoatqurilishbankPeriodRegex().Match(value);
        return match.Success
            ? (ParseDate(match.Groups["from"].Value), ParseDate(match.Groups["to"].Value))
            : (default, default);
    }

    private static (string AccountNumber, string CompanyName, string CompanyInn)
        ParseUzsanoatqurilishbankAccount(string value)
    {
        var match = UzsanoatqurilishbankAccountRegex().Match(NormalizeUzsanoatqurilishbankText(value));
        return match.Success
            ? (
                match.Groups["account"].Value,
                match.Groups["name"].Value.Trim(),
                match.Groups["inn"].Value)
            : (string.Empty, string.Empty, string.Empty);
    }

    private static string NormalizeUzsanoatqurilishbankText(string value) =>
        Regex.Replace(value.Replace('\u00a0', ' ').Trim(), @"\s+", " ");

    private static string NormalizeUzsanoatqurilishbankMfo(string value)
    {
        var normalized = NormalizeKey(value);
        return normalized.All(char.IsDigit) && normalized.Length < 5
            ? normalized.PadLeft(5, '0')
            : normalized;
    }

    [GeneratedRegex(@"Сведения\s+о\s+работе\s+счета\s+[cс]\s+(?<from>\d{2}\.\d{2}\.\d{4})\s+по\s+(?<to>\d{2}\.\d{2}\.\d{4})", RegexOptions.IgnoreCase)]
    private static partial Regex UzsanoatqurilishbankPeriodRegex();

    [GeneratedRegex(@"[CС]чет\s*:\s*(?<account>\d+)\s+(?<name>.*?)\s+ИНН\s*:\s*(?<inn>\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex UzsanoatqurilishbankAccountRegex();
}
