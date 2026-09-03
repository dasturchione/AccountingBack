using ClosedXML.Excel;
using Domain.Entities;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Application.Features.BankParsers;

internal static class BankStatementTemplateParser
{
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(1);

    public static BankExportDto Parse(XLWorkbook workbook, BankStatementTemplate template)
    {
        var export = new BankExportDto();

        foreach (var worksheet in workbook.Worksheets)
        {
            if (!MatchesSheetName(worksheet.Name, template))
                continue;

            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;
            if (lastRow == 0)
                continue;

            var headerRows = Enumerable.Range(1, lastRow)
                .Where(row => MatchesHeader(worksheet, row, template.HeaderRules))
                .ToList();

            for (var index = 0; index < headerRows.Count; index++)
            {
                var headerRow = headerRows[index];
                var nextHeaderRow = index + 1 < headerRows.Count
                    ? headerRows[index + 1]
                    : lastRow + 1;
                var statement = ParseStatement(
                    worksheet,
                    template,
                    headerRow,
                    nextHeaderRow,
                    lastRow);

                if (statement is not null)
                    export.Accounts.Add(statement);
            }
        }

        return export;
    }

    private static AccountStatementDto? ParseStatement(
        IXLWorksheet worksheet,
        BankStatementTemplate template,
        int headerRow,
        int nextHeaderRow,
        int lastRow)
    {
        var statement = new AccountStatementDto { BankId = template.BankId };
        var statementFields = template.Fields
            .Where(field => field.SectionCode == "STATEMENT")
            .ToList();

        foreach (var field in statementFields)
        {
            if (!TryResolveValue(worksheet, field, headerRow, null, null, out var value))
            {
                if (field.IsRequired)
                    return null;

                continue;
            }

            ApplyStatementValue(statement, field.TargetCode, value);
        }

        var firstDataRow = headerRow + template.DataStartRowOffset;
        var finalRow = Math.Min(lastRow, nextHeaderRow - 1);
        var transactionDateFormat = template.Fields
            .FirstOrDefault(field =>
                field.SectionCode == "TRANSACTION" &&
                field.TargetCode == "DATE")
            ?.Format;

        for (var row = firstDataRow; row <= finalRow; row++)
        {
            var rowKind = ClassifyRow(
                worksheet,
                row,
                template.RowRules,
                transactionDateFormat);
            switch (rowKind)
            {
                case "DATA":
                {
                    var transaction = ParseTransaction(worksheet, template.Fields, headerRow, row);
                    if (transaction is not null)
                        statement.Transactions.Add(transaction);
                    break;
                }
                case "TOTAL":
                    ApplyTotalValues(worksheet, template.Fields, statement, headerRow, row);
                    row = finalRow;
                    break;
                case "STOP":
                    row = finalRow;
                    break;
                case "SKIP":
                case null:
                    break;
            }
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

    private static TransactionDto? ParseTransaction(
        IXLWorksheet worksheet,
        IEnumerable<BankStatementTemplateField> allFields,
        int headerRow,
        int dataRow)
    {
        var transaction = new TransactionDto();

        foreach (var field in allFields.Where(field => field.SectionCode == "TRANSACTION"))
        {
            if (!TryResolveValue(worksheet, field, headerRow, dataRow, null, out var value))
            {
                if (field.IsRequired)
                    return null;

                continue;
            }

            ApplyTransactionValue(transaction, field.TargetCode, value);
        }

        transaction.Direction = transaction.Debit > transaction.Credit
            ? "incoming"
            : transaction.Credit > transaction.Debit
                ? "outgoing"
                : string.Empty;
        transaction.Amount = transaction.Debit != 0m ? transaction.Debit : transaction.Credit;
        return transaction;
    }

    private static void ApplyTotalValues(
        IXLWorksheet worksheet,
        IEnumerable<BankStatementTemplateField> allFields,
        AccountStatementDto statement,
        int headerRow,
        int totalRow)
    {
        foreach (var field in allFields.Where(field => field.SectionCode == "TOTAL"))
        {
            object? value;
            if (field.SourceType == "SUM_TRANSACTIONS")
            {
                value = field.TargetCode == "TOTAL_DEBIT"
                    ? statement.Transactions.Sum(transaction => transaction.Debit)
                    : statement.Transactions.Sum(transaction => transaction.Credit);
            }
            else if (!TryResolveValue(worksheet, field, headerRow, null, totalRow, out value))
            {
                continue;
            }

            var amount = AsDecimal(value);
            if (field.TargetCode == "TOTAL_DEBIT")
                statement.TotalDebit = amount;
            else if (field.TargetCode == "TOTAL_CREDIT")
                statement.TotalCredit = amount;
        }
    }

    private static string? ClassifyRow(
        IXLWorksheet worksheet,
        int row,
        IEnumerable<BankStatementTemplateRowRule> rowRules,
        string? transactionDateFormat)
    {
        foreach (var rule in rowRules.OrderBy(rule => rule.Priority))
        {
            var cell = worksheet.Cell(row, rule.ColumnIndex);
            if (MatchesRowRule(cell, rule, transactionDateFormat))
                return rule.RowKind;
        }

        return null;
    }

    private static bool MatchesHeader(
        IXLWorksheet worksheet,
        int candidateRow,
        IEnumerable<BankStatementTemplateHeaderRule> headerRules)
    {
        var requiredRules = headerRules.Where(rule => rule.IsRequired).ToList();
        if (requiredRules.Count == 0)
            return false;

        foreach (var rule in requiredRules)
        {
            var row = candidateRow + rule.RowOffset;
            if (row <= 0 || row > XLHelper.MaxRowNumber)
                return false;

            var value = NormalizeText(
                worksheet.Cell(row, rule.ColumnIndex).GetFormattedString(),
                rule.NormalizationCode);
            if (!MatchesText(value, rule.MatchType, rule.ExpectedValue))
                return false;
        }

        return true;
    }

    private static bool MatchesSheetName(string sheetName, BankStatementTemplate template) =>
        template.SheetNameMatchType switch
        {
            "ANY" => true,
            "EXACT" => string.Equals(sheetName, template.SheetNamePattern, StringComparison.OrdinalIgnoreCase),
            "CONTAINS" => sheetName.Contains(template.SheetNamePattern ?? string.Empty, StringComparison.OrdinalIgnoreCase),
            "REGEX" => IsRegexMatch(sheetName, template.SheetNamePattern),
            _ => false
        };

    private static bool MatchesRowRule(
        IXLCell cell,
        BankStatementTemplateRowRule rule,
        string? transactionDateFormat)
    {
        if (rule.OperatorCode == "IS_DATE")
            return TryGetDate(cell, transactionDateFormat, out _);

        var value = NormalizeText(cell.GetFormattedString(), rule.NormalizationCode);
        return rule.OperatorCode switch
        {
            "IS_EMPTY" => string.IsNullOrWhiteSpace(value),
            "EXACT" => MatchesText(value, "EXACT", rule.CompareValue),
            "CONTAINS" => MatchesText(value, "CONTAINS", rule.CompareValue),
            "REGEX" => MatchesText(value, "REGEX", rule.CompareValue),
            _ => false
        };
    }

    private static bool TryResolveValue(
        IXLWorksheet worksheet,
        BankStatementTemplateField field,
        int headerRow,
        int? dataRow,
        int? totalRow,
        out object? value)
    {
        value = null;
        object? rawValue;

        switch (field.SourceType)
        {
            case "CONSTANT":
                rawValue = field.ConstantValue;
                break;
            case "ABSOLUTE_CELL":
                if (field.AbsoluteRowIndex is null || field.ColumnIndex is null)
                    return false;
                rawValue = ReadCellValue(
                    worksheet.Cell(field.AbsoluteRowIndex.Value, field.ColumnIndex.Value),
                    field.ValueType);
                break;
            case "RELATIVE_CELL":
            {
                if (field.RowOffset is null || field.ColumnIndex is null)
                    return false;

                var anchorRow = field.AnchorCode switch
                {
                    "HEADER" => headerRow,
                    "DATA_ROW" => dataRow,
                    "TOTAL_ROW" => totalRow,
                    _ => null
                };
                if (anchorRow is null || anchorRow.Value + field.RowOffset.Value <= 0)
                    return false;

                rawValue = ReadCellValue(
                    worksheet.Cell(anchorRow.Value + field.RowOffset.Value, field.ColumnIndex.Value),
                    field.ValueType);
                break;
            }
            case "SEARCH_CELL":
                if (!TryFindCell(worksheet, field, headerRow, out var foundCell))
                    return false;
                rawValue = ReadCellValue(foundCell, field.ValueType);
                break;
            default:
                return false;
        }

        return TryConvertValue(rawValue, field, out value);
    }

    private static bool TryFindCell(
        IXLWorksheet worksheet,
        BankStatementTemplateField field,
        int headerRow,
        out IXLCell cell)
    {
        cell = worksheet.Cell(1, 1);
        if (field.ColumnIndex is null || field.SearchLimit is null)
            return false;

        var direction = field.SearchDirection == "DOWN" ? 1 : -1;
        for (var offset = 1; offset <= field.SearchLimit.Value; offset++)
        {
            var row = headerRow + direction * offset;
            if (row <= 0 || row > XLHelper.MaxRowNumber)
                break;

            var candidate = worksheet.Cell(row, field.ColumnIndex.Value);
            if (!MatchesText(
                    candidate.GetFormattedString(),
                    field.LocatorMatchType,
                    field.LocatorValue))
            {
                continue;
            }

            cell = candidate;
            return true;
        }

        return false;
    }

    private static object? ReadCellValue(IXLCell cell, string valueType)
    {
        if (valueType == "DATE" && cell.TryGetValue<DateTime>(out var date))
            return date;

        if (valueType == "DECIMAL" && cell.TryGetValue<decimal>(out var number))
            return number;

        return cell.GetFormattedString();
    }

    private static bool TryConvertValue(
        object? rawValue,
        BankStatementTemplateField field,
        out object? value)
    {
        value = null;
        if (rawValue is null)
            return false;

        if (rawValue is DateTime date)
        {
            value = date;
            return true;
        }

        if (rawValue is decimal number)
        {
            value = number;
            return true;
        }

        var text = Convert.ToString(rawValue, CultureInfo.InvariantCulture) ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(field.ExtractRegex))
        {
            if (!TryExtract(text, field.ExtractRegex, field.ExtractGroup, out text))
                return false;
        }

        text = ApplyTransform(text, field.TransformCode);
        switch (field.ValueType)
        {
            case "STRING":
                value = text;
                return !field.IsRequired || !string.IsNullOrWhiteSpace(text);
            case "DATE":
                if (TryParseDate(text, field.Format, out var parsedDate))
                {
                    value = parsedDate;
                    return true;
                }
                return false;
            case "DECIMAL":
                if (TryParseDecimal(text, out var parsedDecimal))
                {
                    value = parsedDecimal;
                    return true;
                }
                return false;
            default:
                return false;
        }
    }

    private static string ApplyTransform(string value, string transformCode) =>
        transformCode switch
        {
            "NONE" => value,
            "TRIM" => value.Trim(),
            "NORMALIZE_WHITESPACE" => Regex.Replace(
                value.Replace('\u00a0', ' ').Trim(),
                @"\s+",
                " ",
                RegexOptions.CultureInvariant,
                RegexTimeout),
            "NORMALIZE_KEY" => value.Replace(" ", string.Empty).Replace("\u00a0", string.Empty).Trim(),
            "NORMALIZE_MFO" => NormalizeMfo(value),
            "AMOUNT_FROM_TEXT" => ExtractAmount(value),
            _ => value
        };

    private static string NormalizeMfo(string value)
    {
        var normalized = value.Replace(" ", string.Empty).Replace("\u00a0", string.Empty).Trim();
        return normalized.All(char.IsDigit) && normalized.Length < 5
            ? normalized.PadLeft(5, '0')
            : normalized;
    }

    private static string ExtractAmount(string value)
    {
        var match = Regex.Match(
            value,
            @"-?\d[\d\s\u00a0]*([.,]\d+)?",
            RegexOptions.CultureInvariant,
            RegexTimeout);
        return match.Success ? match.Value : string.Empty;
    }

    private static bool TryExtract(
        string value,
        string pattern,
        string? groupName,
        out string extracted)
    {
        extracted = string.Empty;
        try
        {
            var match = Regex.Match(
                value,
                pattern,
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
                RegexTimeout);
            if (!match.Success)
                return false;

            if (string.IsNullOrWhiteSpace(groupName))
            {
                extracted = match.Value;
                return true;
            }

            Group group;
            if (int.TryParse(groupName, out var groupNumber))
                group = match.Groups[groupNumber];
            else
                group = match.Groups[groupName];

            if (!group.Success)
                return false;

            extracted = group.Value;
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    private static bool MatchesText(string value, string? matchType, string? expectedValue)
    {
        if (expectedValue is null)
            return false;

        return matchType switch
        {
            "EXACT" => string.Equals(value.Trim(), expectedValue.Trim(), StringComparison.OrdinalIgnoreCase),
            "CONTAINS" => value.Contains(expectedValue, StringComparison.OrdinalIgnoreCase),
            "REGEX" => IsRegexMatch(value, expectedValue),
            _ => false
        };
    }

    private static bool IsRegexMatch(string value, string? pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return false;

        try
        {
            return Regex.IsMatch(
                value,
                pattern,
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
                RegexTimeout);
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    private static string NormalizeText(string value, string normalizationCode) =>
        normalizationCode switch
        {
            "NONE" => value,
            "TRIM" => value.Trim(),
            "NORMALIZE_WHITESPACE" => Regex.Replace(
                value.Replace('\u00a0', ' ').Trim(),
                @"\s+",
                " ",
                RegexOptions.CultureInvariant,
                RegexTimeout),
            "UPPER" => value.Trim().ToUpperInvariant(),
            _ => value.Trim()
        };

    private static bool TryGetDate(IXLCell cell, string? format, out DateTime date)
    {
        if (cell.TryGetValue<DateTime>(out date))
            return true;

        return TryParseDate(cell.GetFormattedString(), format, out date);
    }

    private static bool TryParseDate(string value, string? format, out DateTime date)
    {
        if (!string.IsNullOrWhiteSpace(format))
        {
            return DateTime.TryParseExact(
                value.Trim(),
                format,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out date);
        }

        return DateTime.TryParse(
            value.Trim(),
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out date);
    }

    private static bool TryParseDecimal(string value, out decimal amount)
    {
        var normalized = value
            .Replace(" ", string.Empty)
            .Replace("\u00a0", string.Empty)
            .Replace(',', '.')
            .Trim();
        return decimal.TryParse(
            normalized,
            NumberStyles.Any,
            CultureInfo.InvariantCulture,
            out amount);
    }

    private static decimal AsDecimal(object? value) =>
        value is decimal amount ? amount : 0m;

    private static void ApplyStatementValue(AccountStatementDto statement, string targetCode, object? value)
    {
        switch (targetCode)
        {
            case "BANK_MFO": statement.BankMfo = AsString(value); break;
            case "BANK_NAME": statement.BankName = AsString(value); break;
            case "ACCOUNT_NUMBER": statement.AccountNumber = AsString(value); break;
            case "COMPANY_NAME": statement.CompanyName = AsString(value); break;
            case "COMPANY_INN": statement.CompanyInn = AsString(value); break;
            case "PERIOD_FROM": statement.PeriodFrom = AsDate(value); break;
            case "PERIOD_TO": statement.PeriodTo = AsDate(value); break;
            case "OPENING_BALANCE": statement.OpeningBalance = AsDecimal(value); break;
            case "CLOSING_BALANCE": statement.ClosingBalance = AsDecimal(value); break;
        }
    }

    private static void ApplyTransactionValue(TransactionDto transaction, string targetCode, object? value)
    {
        switch (targetCode)
        {
            case "DATE": transaction.Date = AsDate(value); break;
            case "DOC_NUMBER":
                transaction.DocNumber = AsString(value);
                transaction.BankDocumentNumber = transaction.DocNumber;
                break;
            case "OPERATION_CODE": transaction.OperationCode = AsString(value); break;
            case "COUNTERPARTY_MFO": transaction.MfoCounterparty = AsString(value); break;
            case "COUNTERPARTY_ACCOUNT": transaction.CounterpartyAccount = AsString(value); break;
            case "COUNTERPARTY_INN": transaction.CounterpartyInn = AsString(value); break;
            case "COUNTERPARTY_NAME": transaction.CounterpartyName = AsString(value); break;
            case "DEBIT": transaction.Debit = AsDecimal(value); break;
            case "CREDIT": transaction.Credit = AsDecimal(value); break;
            case "PURPOSE": transaction.Purpose = AsString(value); break;
        }
    }

    private static string AsString(object? value) => value as string ?? string.Empty;

    private static DateTime AsDate(object? value) => value is DateTime date ? date : default;
}
