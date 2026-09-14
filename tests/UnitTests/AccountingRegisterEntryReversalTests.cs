using Application.Features.Register.AccountingRegisterEntries;
using Domain.Entities;
using SharedKernel.Constants;

namespace UnitTests;

public sealed class AccountingRegisterEntryReversalTests
{
    [Fact]
    public void Reversal_KeepsCorrespondence_AndNegatesAmount()
    {
        var original = Entry(debit: 2010, credit: 6710, amount: 10_000_000m);

        var reversal = AccountingRegisterEntryReversalFactory.Create([original], reversalBatchId: 77).Single();

        Assert.Equal(2010, reversal.DebitAccountId);
        Assert.Equal(6710, reversal.CreditAccountId);
        Assert.Equal(-10_000_000m, reversal.Amount);
    }

    [Fact]
    public void Reversal_LeavesZeroTurnoverOnBothAccounts()
    {
        var original = Entry(debit: 2010, credit: 6710, amount: 10_000_000m);

        var reversal = AccountingRegisterEntryReversalFactory.Create([original], reversalBatchId: 77).Single();

        var debitTurnover2010 = new[] { original, reversal }
            .Where(x => x.DebitAccountId == 2010)
            .Sum(x => x.Amount);
        var creditTurnover6710 = new[] { original, reversal }
            .Where(x => x.CreditAccountId == 6710)
            .Sum(x => x.Amount);

        Assert.Equal(0m, debitTurnover2010);
        Assert.Equal(0m, creditTurnover6710);
    }

    [Fact]
    public void Reversal_StaysInTheOriginalAccountingPeriod()
    {
        var original = Entry(debit: 2010, credit: 6710, amount: 10_000_000m);
        original.DocDate = new DateTime(2026, 3, 31, 18, 5, 43);

        var reversal = AccountingRegisterEntryReversalFactory.Create([original], reversalBatchId: 77).Single();

        Assert.Equal(new DateTime(2026, 3, 31, 18, 5, 43), reversal.DocDate);
    }

    [Fact]
    public void Reversal_NegatesQuantitiesOnTheirOwnSide()
    {
        var original = Entry(debit: 2910, credit: 2810, amount: 40_000m);
        original.DebitQuantity = 4m;
        original.CreditQuantity = null;

        var reversal = AccountingRegisterEntryReversalFactory.Create([original], reversalBatchId: 77).Single();

        Assert.Equal(-4m, reversal.DebitQuantity);
        Assert.Null(reversal.CreditQuantity);
    }

    [Fact]
    public void Reversal_KeepsSubkontoOnTheSameSideAsItsAccount()
    {
        var original = Entry(debit: 2010, credit: 6710, amount: 10_000_000m);
        original.RegisterEntrySubkontos.Add(new RegisterEntrySubkonto
        {
            Side = SubkontoSideConst.CREDIT,
            SubkontoTypeId = SubkontoTypeIdConst.OrganizationEmployees,
            SortOrder = 1,
            EntityId = 512,
            DisplayValue = "001 - Karimov Alisher"
        });

        var reversal = AccountingRegisterEntryReversalFactory.Create([original], reversalBatchId: 77).Single();

        var subkonto = reversal.RegisterEntrySubkontos.Single();
        Assert.Equal(SubkontoSideConst.CREDIT, subkonto.Side);
        Assert.Equal(512, subkonto.EntityId);
    }

    [Fact]
    public void Reversal_LinksBackToTheOriginalEntryAndReversalBatch()
    {
        var original = Entry(debit: 2010, credit: 6710, amount: 10_000_000m);
        original.Id = 4242;

        var reversal = AccountingRegisterEntryReversalFactory.Create([original], reversalBatchId: 77).Single();

        Assert.Equal(4242, reversal.ReversalEntryId);
        Assert.Equal(77, reversal.PostingBatchId);
        Assert.Equal("Reversal: Payroll accrual", reversal.Content);
    }

    private static AccountingRegisterEntry Entry(int debit, int credit, decimal amount) =>
        new()
        {
            Id = 1,
            OrganizationId = 8,
            DocumentTypeId = DocumentTypeIdConst.SALARY,
            DocumentId = 55,
            DebitAccountId = debit,
            CreditAccountId = credit,
            CurrencyId = 1,
            Amount = amount,
            DocDate = new DateTime(2026, 3, 31),
            CreatedDate = new DateTime(2026, 4, 1),
            Content = "Payroll accrual",
            JournalNumber = "SAL-2026-000001",
            SourceLineId = 9
        };
}
