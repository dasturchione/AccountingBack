using Application.Features.BankParsers;
using ClosedXML.Excel;

namespace UnitTests;

public sealed class UzsanoatqurilishbankStatementParserTests
{
    [Fact]
    public void ParseExcel_ParsesNineColumnUzsanoatqurilishbankTemplate()
    {
        using var stream = CreateWorkbook();
        using var workbook = new XLWorkbook(stream);

        var export = BankStatementTemplateParser.Parse(
            workbook,
            BankStatementTemplateTestData.CreateUzsanoatqurilishbank());

        var statement = Assert.Single(export.Accounts);
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
                Assert.Equal(new DateTime(2024, 7, 8), incoming.Date);
                Assert.Equal("27", incoming.DocNumber);
                Assert.Equal("00440", incoming.MfoCounterparty);
                Assert.Equal("10101000800010900101", incoming.CounterpartyAccount);
                Assert.Equal("Айланма кассадаги накд пуллар", incoming.CounterpartyName);
                Assert.Equal("200833707", incoming.CounterpartyInn);
                Assert.Equal("Устав фондини шакллантириш учун тулов", incoming.Purpose);
                Assert.Equal(1_000_000m, incoming.Debit);
                Assert.Equal(0m, incoming.Credit);
                Assert.Equal("incoming", incoming.Direction);
            },
            outgoing =>
            {
                Assert.Equal(new DateTime(2024, 7, 8), outgoing.Date);
                Assert.Equal("204243890", outgoing.DocNumber);
                Assert.Equal("00440", outgoing.MfoCounterparty);
                Assert.Equal("16401000907099548001", outgoing.CounterpartyAccount);
                Assert.Equal("Начисленные %% DANIEFF TEAM TRADERS MCHJ", outgoing.CounterpartyName);
                Assert.Equal("311444422", outgoing.CounterpartyInn);
                Assert.Equal("Комиссия банка", outgoing.Purpose);
                Assert.Equal(0m, outgoing.Debit);
                Assert.Equal(56_000m, outgoing.Credit);
                Assert.Equal("outgoing", outgoing.Direction);
            });
    }

    private static MemoryStream CreateWorkbook()
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Hisobot-24-7-2026");

        sheet.Cell(1, 1).Value = "/ ТОШКЕНТ Ш., \"УЗСАНОАТКУРИЛИШБАНКИ\" АТБ БОШ ОФИСИ";
        sheet.Cell(1, 2).Value = "IABS/Клиент-Банк Изг: 24.08.2026";
        sheet.Cell(2, 1).Value = "Сведения о работе счета c 01.01.2023 по 24.08.2026";
        sheet.Cell(3, 1).Value = "Cчет: 20208000607099548001           DANIEFF TEAM TRADERS MCHJ          ИНН: 311444422";
        sheet.Cell(4, 1).Value = "Остаток на начало периода: 0.00";
        sheet.Cell(4, 2).Value = "Остаток на конец периода: 7 829 465.93";

        sheet.Cell(5, 1).Value = "Дата";
        sheet.Cell(5, 2).Value = "Номер документа";
        sheet.Cell(5, 3).Value = "МФО корресп.";
        sheet.Cell(5, 4).Value = "Счет корреспондента";
        sheet.Cell(5, 5).Value = "Наименование корресп.";
        sheet.Cell(5, 6).Value = "ИНН корреспондента";
        sheet.Cell(5, 7).Value = "Назначение платежа";
        sheet.Cell(5, 8).Value = "Дебет";
        sheet.Cell(5, 9).Value = "Кредит";

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

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }
}
