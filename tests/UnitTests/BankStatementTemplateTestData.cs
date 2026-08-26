using Domain.Entities;

namespace UnitTests;

internal static class BankStatementTemplateTestData
{
    public static BankStatementTemplate CreateUzsanoatqurilishbank(int version = 1)
    {
        var template = CreateTemplate(2, "UZSANOATQURILISHBANK_XLSX_V1", version, 1);
        template.HeaderRules =
        [
            Header(1, "EXACT", "Дата"),
            Header(2, "CONTAINS", "Номер документа"),
            Header(3, "CONTAINS", "МФО корресп"),
            Header(4, "CONTAINS", "Счет корреспондента"),
            Header(5, "CONTAINS", "Наименование корресп"),
            Header(6, "CONTAINS", "ИНН корреспондента"),
            Header(7, "CONTAINS", "Назначение платежа"),
            Header(8, "EXACT", "Дебет"),
            Header(9, "EXACT", "Кредит")
        ];
        template.RowRules =
        [
            Row(1, "TOTAL", 1, "CONTAINS", "Итоговый оборот за период"),
            Row(2, "DATA", 1, "IS_DATE"),
            Row(3, "SKIP", 1, "REGEX", ".*")
        ];
        template.Fields =
        [
            Absolute("STATEMENT", "BANK_NAME", 1, 1, "STRING", "TRIM", "^\\s*/?\\s*(?<name>.*)$", "name"),
            Search("STATEMENT", "ACCOUNT_NUMBER", 1, "REGEX", "^[CС]чет\\s*:", "STRING", "NORMALIZE_KEY", "[CС]чет\\s*:\\s*(?<account>\\d+)", "account"),
            Search("STATEMENT", "COMPANY_NAME", 1, "REGEX", "^[CС]чет\\s*:", "STRING", "NORMALIZE_WHITESPACE", "[CС]чет\\s*:\\s*\\d+\\s+(?<name>.*?)\\s+ИНН\\s*:", "name"),
            Search("STATEMENT", "COMPANY_INN", 1, "REGEX", "^[CС]чет\\s*:", "STRING", "NORMALIZE_KEY", "ИНН\\s*:\\s*(?<inn>\\d+)", "inn"),
            Search("STATEMENT", "PERIOD_FROM", 1, "REGEX", "Сведения\\s+о\\s+работе\\s+счета", "DATE", "TRIM", "[cс]\\s+(?<from>\\d{2}\\.\\d{2}\\.\\d{4})\\s+по", "from", "dd.MM.yyyy"),
            Search("STATEMENT", "PERIOD_TO", 1, "REGEX", "Сведения\\s+о\\s+работе\\s+счета", "DATE", "TRIM", "\\s+по\\s+(?<to>\\d{2}\\.\\d{2}\\.\\d{4})", "to", "dd.MM.yyyy"),
            Search("STATEMENT", "OPENING_BALANCE", 1, "CONTAINS", "Остаток на начало периода", "DECIMAL", "AMOUNT_FROM_TEXT"),
            Search("STATEMENT", "CLOSING_BALANCE", 2, "CONTAINS", "Остаток на конец периода", "DECIMAL", "AMOUNT_FROM_TEXT"),
            Relative("TRANSACTION", "DATE", "DATA_ROW", 1, "DATE", "TRIM", format: "dd.MM.yyyy"),
            Relative("TRANSACTION", "DOC_NUMBER", "DATA_ROW", 2),
            Relative("TRANSACTION", "COUNTERPARTY_MFO", "DATA_ROW", 3, transform: "NORMALIZE_MFO"),
            Relative("TRANSACTION", "COUNTERPARTY_ACCOUNT", "DATA_ROW", 4, transform: "NORMALIZE_KEY"),
            Relative("TRANSACTION", "COUNTERPARTY_NAME", "DATA_ROW", 5),
            Relative("TRANSACTION", "COUNTERPARTY_INN", "DATA_ROW", 6, transform: "NORMALIZE_KEY"),
            Relative("TRANSACTION", "PURPOSE", "DATA_ROW", 7),
            Relative("TRANSACTION", "DEBIT", "DATA_ROW", 9, "DECIMAL", "NONE"),
            Relative("TRANSACTION", "CREDIT", "DATA_ROW", 8, "DECIMAL", "NONE"),
            Relative("TOTAL", "TOTAL_DEBIT", "TOTAL_ROW", 9, "DECIMAL", "NONE"),
            Relative("TOTAL", "TOTAL_CREDIT", "TOTAL_ROW", 8, "DECIMAL", "NONE")
        ];
        return template;
    }

    public static BankStatementTemplate CreateTrastbank(int version = 1)
    {
        var template = CreateTemplate(1, "TRASTBANK_XLSX_V1", version, 5);
        template.HeaderRules =
        [
            Header(1, "CONTAINS", "/"),
            Header(1, "CONTAINS", "Сведения", rowOffset: 1),
            Header(1, "REGEX", "^[CС]чет\\s*:", rowOffset: 2),
            Header(1, "EXACT", "Дата", rowOffset: 4)
        ];
        template.RowRules =
        [
            Row(1, "TOTAL", 1, "CONTAINS", "Итоговый оборот"),
            Row(2, "DATA", 1, "IS_DATE"),
            Row(3, "STOP", 1, "REGEX", ".*")
        ];
        template.Fields =
        [
            Relative("STATEMENT", "BANK_MFO", "HEADER", 1, extractRegex: "^\\s*(?<mfo>[^/]+)\\s*/", extractGroup: "mfo"),
            Relative("STATEMENT", "BANK_NAME", "HEADER", 1, extractRegex: "^\\s*[^/]+/\\s*(?<name>.*)$", extractGroup: "name"),
            Relative("STATEMENT", "ACCOUNT_NUMBER", "HEADER", 1, transform: "NORMALIZE_KEY", rowOffset: 2, extractRegex: "[CС]чет\\s*:\\s*(?<account>\\d+)", extractGroup: "account"),
            Relative("STATEMENT", "COMPANY_NAME", "HEADER", 1, transform: "NORMALIZE_WHITESPACE", rowOffset: 2, extractRegex: "[CС]чет\\s*:\\s*\\d+\\s+(?<name>.*?)\\s+ИНН\\s*:", extractGroup: "name"),
            Relative("STATEMENT", "COMPANY_INN", "HEADER", 1, transform: "NORMALIZE_KEY", rowOffset: 2, extractRegex: "ИНН\\s*:\\s*(?<inn>\\d+)", extractGroup: "inn"),
            Relative("STATEMENT", "PERIOD_FROM", "HEADER", 1, "DATE", "TRIM", 1, "[cс]\\s+(?<from>\\d{2}\\.\\d{2}\\.\\d{4})\\s+по", "from", "dd.MM.yyyy"),
            Relative("STATEMENT", "PERIOD_TO", "HEADER", 1, "DATE", "TRIM", 1, "\\s+по\\s+(?<to>\\d{2}\\.\\d{2}\\.\\d{4})", "to", "dd.MM.yyyy"),
            Relative("STATEMENT", "OPENING_BALANCE", "HEADER", 1, "DECIMAL", "AMOUNT_FROM_TEXT", 3),
            Relative("STATEMENT", "CLOSING_BALANCE", "HEADER", 2, "DECIMAL", "AMOUNT_FROM_TEXT", 3),
            Relative("TRANSACTION", "DATE", "DATA_ROW", 1, "DATE", "TRIM", format: "dd.MM.yyyy"),
            Relative("TRANSACTION", "COUNTERPARTY_ACCOUNT", "DATA_ROW", 2, transform: "NORMALIZE_KEY", extractRegex: "^\\s*(?<account>[^/]*)", extractGroup: "account"),
            Relative("TRANSACTION", "COUNTERPARTY_INN", "DATA_ROW", 2, transform: "NORMALIZE_KEY", extractRegex: "^[^/]*/\\s*(?<inn>[^/]*)", extractGroup: "inn"),
            Relative("TRANSACTION", "COUNTERPARTY_NAME", "DATA_ROW", 2, extractRegex: "^[^/]*/[^/]*/\\s*(?<name>.*)$", extractGroup: "name"),
            Relative("TRANSACTION", "DOC_NUMBER", "DATA_ROW", 3),
            Relative("TRANSACTION", "OPERATION_CODE", "DATA_ROW", 4),
            Relative("TRANSACTION", "COUNTERPARTY_MFO", "DATA_ROW", 5, transform: "NORMALIZE_MFO"),
            Relative("TRANSACTION", "DEBIT", "DATA_ROW", 7, "DECIMAL", "NONE"),
            Relative("TRANSACTION", "CREDIT", "DATA_ROW", 6, "DECIMAL", "NONE"),
            Relative("TRANSACTION", "PURPOSE", "DATA_ROW", 8),
            Relative("TOTAL", "TOTAL_DEBIT", "TOTAL_ROW", 7, "DECIMAL", "NONE"),
            Relative("TOTAL", "TOTAL_CREDIT", "TOTAL_ROW", 6, "DECIMAL", "NONE")
        ];
        return template;
    }

    private static BankStatementTemplate CreateTemplate(int bankId, string code, int version, short dataOffset) =>
        new()
        {
            BankId = bankId,
            Code = code,
            Name = code,
            Version = (short)version,
            SheetNameMatchType = "ANY",
            DataStartRowOffset = dataOffset,
            StateId = 1
        };

    private static BankStatementTemplateHeaderRule Header(
        short column,
        string matchType,
        string expected,
        short rowOffset = 0) =>
        new()
        {
            RowOffset = rowOffset,
            ColumnIndex = column,
            MatchType = matchType,
            ExpectedValue = expected,
            NormalizationCode = "TRIM",
            IsRequired = true
        };

    private static BankStatementTemplateRowRule Row(
        short priority,
        string kind,
        short column,
        string operation,
        string? value = null) =>
        new()
        {
            Priority = priority,
            RowKind = kind,
            ColumnIndex = column,
            OperatorCode = operation,
            CompareValue = value,
            NormalizationCode = "TRIM"
        };

    private static BankStatementTemplateField Relative(
        string section,
        string target,
        string anchor,
        short column,
        string valueType = "STRING",
        string transform = "TRIM",
        short rowOffset = 0,
        string? extractRegex = null,
        string? extractGroup = null,
        string? format = null) =>
        new()
        {
            SectionCode = section,
            TargetCode = target,
            SourceType = "RELATIVE_CELL",
            AnchorCode = anchor,
            RowOffset = rowOffset,
            ColumnIndex = column,
            ExtractRegex = extractRegex,
            ExtractGroup = extractGroup,
            ValueType = valueType,
            Format = format,
            TransformCode = transform,
            IsRequired = target is "DATE" or "ACCOUNT_NUMBER"
        };

    private static BankStatementTemplateField Absolute(
        string section,
        string target,
        int row,
        short column,
        string valueType,
        string transform,
        string? extractRegex = null,
        string? extractGroup = null) =>
        new()
        {
            SectionCode = section,
            TargetCode = target,
            SourceType = "ABSOLUTE_CELL",
            AnchorCode = "SHEET",
            AbsoluteRowIndex = row,
            ColumnIndex = column,
            ExtractRegex = extractRegex,
            ExtractGroup = extractGroup,
            ValueType = valueType,
            TransformCode = transform,
            IsRequired = true
        };

    private static BankStatementTemplateField Search(
        string section,
        string target,
        short column,
        string locatorMatchType,
        string locatorValue,
        string valueType,
        string transform,
        string? extractRegex = null,
        string? extractGroup = null,
        string? format = null) =>
        new()
        {
            SectionCode = section,
            TargetCode = target,
            SourceType = "SEARCH_CELL",
            AnchorCode = "HEADER",
            ColumnIndex = column,
            SearchDirection = "UP",
            SearchLimit = 10,
            LocatorMatchType = locatorMatchType,
            LocatorValue = locatorValue,
            ExtractRegex = extractRegex,
            ExtractGroup = extractGroup,
            ValueType = valueType,
            Format = format,
            TransformCode = transform,
            IsRequired = target is "ACCOUNT_NUMBER" or "PERIOD_FROM" or "PERIOD_TO"
        };
}
