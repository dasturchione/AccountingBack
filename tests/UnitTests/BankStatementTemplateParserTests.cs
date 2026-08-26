using Application.Features.BankParsers;
using ClosedXML.Excel;

namespace UnitTests;

public sealed class BankStatementTemplateParserTests
{
    [Fact]
    public void Parse_MapsUzsanoatqurilishbankColumnsRelativeToOrganization()
    {
        using var workbook = CreateUzsanoatqurilishbankWorkbook();

        var export = BankStatementTemplateParser.Parse(
            workbook,
            BankStatementTemplateTestData.CreateUzsanoatqurilishbank());

        var statement = Assert.Single(export.Accounts);
        Assert.Equal(2, statement.BankId);
        Assert.Equal("ТОШКЕНТ Ш., \"УЗСАНОАТКУРИЛИШБАНКИ\" АТБ БОШ ОФИСИ", statement.BankName);
        Assert.Equal("20208000607099548001", statement.AccountNumber);
        Assert.Equal("DANIEFF TEAM TRADERS MCHJ", statement.CompanyName);
        Assert.Equal("311444422", statement.CompanyInn);
        Assert.Equal(new DateTime(2023, 1, 1), statement.PeriodFrom);
        Assert.Equal(new DateTime(2026, 8, 24), statement.PeriodTo);
        Assert.Equal(0m, statement.OpeningBalance);
        Assert.Equal(7_829_465.93m, statement.ClosingBalance);
        Assert.Equal(1_000_000m, statement.TotalDebit);
        Assert.Equal(56_000m, statement.TotalCredit);

        Assert.Collection(
            statement.Transactions,
            incoming =>
            {
                Assert.Equal(1_000_000m, incoming.Debit);
                Assert.Equal(0m, incoming.Credit);
                Assert.Equal("incoming", incoming.Direction);
                Assert.Equal("00440", incoming.MfoCounterparty);
            },
            outgoing =>
            {
                Assert.Equal(0m, outgoing.Debit);
                Assert.Equal(56_000m, outgoing.Credit);
                Assert.Equal("outgoing", outgoing.Direction);
                Assert.Equal("00440", outgoing.MfoCounterparty);
            });
    }

    [Fact]
    public void Parse_RecognizesTextTransactionDateUsingConfiguredFormat()
    {
        using var workbook = CreateUzsanoatqurilishbankWorkbook(firstDateAsText: true);

        var export = BankStatementTemplateParser.Parse(
            workbook,
            BankStatementTemplateTestData.CreateUzsanoatqurilishbank());

        var transactions = Assert.Single(export.Accounts).Transactions;
        Assert.Equal(2, transactions.Count);
        Assert.Equal(new DateTime(2026, 8, 24), transactions[0].Date);
    }

    [Fact]
    public void Parse_ReturnsEveryTrastbankAccountBlockOnTheWorksheet()
    {
        using var workbook = CreateTrastbankWorkbookWithTwoAccounts();

        var export = BankStatementTemplateParser.Parse(
            workbook,
            BankStatementTemplateTestData.CreateTrastbank());

        Assert.Collection(
            export.Accounts,
            first =>
            {
                Assert.Equal("00491", first.BankMfo);
                Assert.Equal("Trastbank", first.BankName);
                Assert.Equal("20208000111111111111", first.AccountNumber);
                Assert.Equal("COMPANY ONE", first.CompanyName);
                Assert.Equal(0m, first.TotalDebit);
                Assert.Equal(100m, first.TotalCredit);
                var transaction = Assert.Single(first.Transactions);
                Assert.Equal(0m, transaction.Debit);
                Assert.Equal(100m, transaction.Credit);
                Assert.Equal("outgoing", transaction.Direction);
                Assert.Equal("20208000999999999999", transaction.CounterpartyAccount);
                Assert.Equal("998877665", transaction.CounterpartyInn);
                Assert.Equal("COUNTERPARTY ONE", transaction.CounterpartyName);
            },
            second =>
            {
                Assert.Equal("00491", second.BankMfo);
                Assert.Equal("20208000222222222222", second.AccountNumber);
                Assert.Equal("COMPANY TWO", second.CompanyName);
                Assert.Equal(250m, second.TotalDebit);
                Assert.Equal(0m, second.TotalCredit);
                var transaction = Assert.Single(second.Transactions);
                Assert.Equal(250m, transaction.Debit);
                Assert.Equal(0m, transaction.Credit);
                Assert.Equal("incoming", transaction.Direction);
            });
    }

    internal static XLWorkbook CreateUzsanoatqurilishbankWorkbook(bool firstDateAsText = false)
    {
        var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Hisobot-24-7-2026");
        sheet.Cell(1, 1).Value = "/ ТОШКЕНТ Ш., \"УЗСАНОАТКУРИЛИШБАНКИ\" АТБ БОШ ОФИСИ";
        sheet.Cell(2, 1).Value = "Сведения о работе счета c 01.01.2023 по 24.08.2026";
        sheet.Cell(3, 1).Value = "Cчет: 20208000607099548001           DANIEFF TEAM TRADERS MCHJ          ИНН: 311444422";
        sheet.Cell(4, 1).Value = "Остаток на начало периода: 0.00";
        sheet.Cell(4, 2).Value = "Остаток на конец периода: 7 829 465.93";
        WriteUzsanoatqurilishbankHeader(sheet, 5);

        if (firstDateAsText)
            sheet.Cell(6, 1).Value = "24.08.2026";
        else
            sheet.Cell(6, 1).Value = new DateTime(2024, 7, 8);
        sheet.Cell(6, 2).Value = 27;
        sheet.Cell(6, 3).Value = "00440";
        sheet.Cell(6, 4).Value = " 10101000800010900101 ";
        sheet.Cell(6, 5).Value = "Айланма кассадаги накд пуллар";
        sheet.Cell(6, 6).Value = 200833707;
        sheet.Cell(6, 7).Value = "Устав фондини шакллантириш учун тулов";
        sheet.Cell(6, 9).Value = 1_000_000m;

        sheet.Cell(7, 1).Value = new DateTime(2024, 7, 8);
        sheet.Cell(7, 2).Value = 204243890;
        sheet.Cell(7, 3).Value = 440;
        sheet.Cell(7, 4).Value = " 16401000907099548001 ";
        sheet.Cell(7, 5).Value = "Начисленные %% DANIEFF TEAM TRADERS MCHJ";
        sheet.Cell(7, 6).Value = 311444422;
        sheet.Cell(7, 7).Value = "Комиссия банка";
        sheet.Cell(7, 8).Value = 56_000m;

        sheet.Cell(8, 1).Value = "Итоговый оборот за период:";
        sheet.Cell(8, 8).Value = 56_000m;
        sheet.Cell(8, 9).Value = 1_000_000m;
        return workbook;
    }

    private static XLWorkbook CreateTrastbankWorkbookWithTwoAccounts()
    {
        var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Statement");
        WriteTrastbankBlock(sheet, 1, "20208000111111111111", "COMPANY ONE", "111222333", 100m, 0m);
        WriteTrastbankBlock(sheet, 9, "20208000222222222222", "COMPANY TWO", "444555666", 0m, 250m);
        return workbook;
    }

    private static void WriteUzsanoatqurilishbankHeader(IXLWorksheet sheet, int row)
    {
        var values = new[]
        {
            "Дата", "Номер документа", "МФО корресп.", "Счет корреспондента",
            "Наименование корресп.", "ИНН корреспондента", "Назначение платежа", "Дебет", "Кредит"
        };

        for (var column = 1; column <= values.Length; column++)
            sheet.Cell(row, column).Value = values[column - 1];
    }

    private static void WriteTrastbankBlock(
        IXLWorksheet sheet,
        int bankRow,
        string account,
        string company,
        string companyInn,
        decimal debit,
        decimal credit)
    {
        sheet.Cell(bankRow, 1).Value = "00491 / Trastbank";
        sheet.Cell(bankRow + 1, 1).Value = "Сведения о работе счета c 01.08.2026 по 02.08.2026";
        sheet.Cell(bankRow + 2, 1).Value = $"Cчет: {account} {company} ИНН: {companyInn}";
        sheet.Cell(bankRow + 3, 1).Value = "Остаток на начало периода: 10.00";
        sheet.Cell(bankRow + 3, 2).Value = "Остаток на конец периода: 20.00";
        sheet.Cell(bankRow + 4, 1).Value = "Дата";

        var dataRow = bankRow + 5;
        sheet.Cell(dataRow, 1).Value = new DateTime(2026, 8, 1);
        sheet.Cell(dataRow, 2).Value = "20208000999999999999 / 998877665 / COUNTERPARTY ONE";
        sheet.Cell(dataRow, 3).Value = "DOC-1";
        sheet.Cell(dataRow, 4).Value = "01";
        sheet.Cell(dataRow, 5).Value = "00491";
        sheet.Cell(dataRow, 6).Value = debit;
        sheet.Cell(dataRow, 7).Value = credit;
        sheet.Cell(dataRow, 8).Value = "Payment purpose";

        sheet.Cell(bankRow + 6, 1).Value = "Итоговый оборот:";
        sheet.Cell(bankRow + 6, 6).Value = debit;
        sheet.Cell(bankRow + 6, 7).Value = credit;
    }
}
