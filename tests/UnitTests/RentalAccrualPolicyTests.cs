using Application.Features.Rnt.RentalAccruals;
using Application.Features.Register.PostingEngines;
using Domain.Entities;
using SharedKernel.Constants;

namespace UnitTests;

public sealed class RentalAccrualPolicyTests
{
    [Fact]
    public void Calculate_UsesTaxBaseForTaxAndContractAmountForLessorPayable()
    {
        var result = RentalAccrualCalculator.Calculate(
            contractAmount: 5_000_000m,
            taxBaseAmount: 6_000_000m,
            taxRate: 12m);

        Assert.Equal(720_000m, result.TaxAmount);
        Assert.Equal(4_400_000m, result.PayableAmount);
        Assert.Equal(5_120_000m, result.Amount);
    }

    [Fact]
    public void GetPeriod_MonthlyUsesAnniversaryAndClampsToContractEnd()
    {
        var result = RentalAccrualSchedule.GetPeriod(
            nextAccrualDate: new DateTime(2026, 1, 15),
            periodUnit: "MONTH",
            periodValue: 1,
            contractEndDate: new DateTime(2026, 2, 10));

        Assert.Equal(new DateTime(2026, 1, 15), result.PeriodFrom);
        Assert.Equal(new DateTime(2026, 2, 10), result.PeriodTo);
        Assert.Equal(new DateTime(2026, 2, 11), result.NextAccrualDate);
    }

    [Fact]
    public void GetPeriod_DailyBuildsInclusiveTenDayPeriod()
    {
        var result = RentalAccrualSchedule.GetPeriod(
            nextAccrualDate: new DateTime(2026, 8, 1),
            periodUnit: "DAY",
            periodValue: 10,
            contractEndDate: new DateTime(2026, 8, 31));

        Assert.Equal(new DateTime(2026, 8, 1), result.PeriodFrom);
        Assert.Equal(new DateTime(2026, 8, 10), result.PeriodTo);
        Assert.Equal(new DateTime(2026, 8, 11), result.NextAccrualDate);
    }

    [Fact]
    public void DraftFactory_CopiesAccountsAndFreezesAgreedAmounts()
    {
        var contract = new RentalContract
        {
            Id = 10,
            OrganizationId = 2,
            CurrencyId = 1,
            LessorPayableAccountId = 202,
            TaxPayableAccountId = 203
        };
        var contractObject = new RentalContractObject
        {
            Id = 11,
            ContractId = 10,
            ContractAmount = 5_000_000m,
            TaxBaseAmount = 6_000_000m,
            TaxRate = 12m,
            ExpenseAccountId = 201
        };

        var draft = RentalAccrualDraftFactory.Create(
            contract,
            docNumber: "1",
            docDate: new DateTime(2026, 8, 29),
            [new RentalAccrualDraftSource(
                contractObject,
                new RentalAccrualPeriod(
                    new DateTime(2026, 8, 1),
                    new DateTime(2026, 8, 31),
                    new DateTime(2026, 9, 1)))],
            createdByUserId: null);

        var item = Assert.Single(draft.Items);
        Assert.Equal(DocumentStatusIdConst.DRAFT, draft.StatusId);
        Assert.Equal(202, draft.LessorPayableAccountId);
        Assert.Equal(203, draft.TaxPayableAccountId);
        Assert.Equal(201, item.ExpenseAccountId);
        Assert.Equal(720_000m, item.TaxAmount);
        Assert.Equal(4_400_000m, item.PayableAmount);
        Assert.Equal(5_120_000m, draft.Amount);
    }

    [Fact]
    public async Task PostingBuilder_CreatesPayableAndTaxEntriesFromPersistedAccounts()
    {
        var document = new RentalAccrualDoc
        {
            Id = 20,
            OrganizationId = 2,
            ContractId = 10,
            DocNumber = "1",
            DocDate = new DateTime(2026, 8, 29),
            CurrencyId = 1,
            LessorPayableAccountId = 202,
            TaxPayableAccountId = 203,
            Contract = new RentalContract
            {
                Id = 10,
                LessorFullName = "Ali Valiyev",
                LessorPinfl = "12345678901234"
            },
            Items =
            [
                new RentalAccrualDocItem
                {
                    Id = 21,
                    ExpenseAccountId = 201,
                    PayableAmount = 4_400_000m,
                    TaxAmount = 720_000m
                }
            ]
        };

        var builder = new RentalAccrualContextBuilder(new StubAccountingPolicyResolver(7));
        var context = Assert.Single(await builder.BuildAsync(document));

        Assert.Equal(DocumentTypeIdConst.RENTAL_ACCRUAL, context.DocumentTypeId);
        Assert.Collection(
            context.Entries,
            payable =>
            {
                Assert.Equal(201, payable.DebitAccountId);
                Assert.Equal(202, payable.CreditAccountId);
                Assert.Equal(4_400_000m, payable.Amount);
            },
            tax =>
            {
                Assert.Equal(201, tax.DebitAccountId);
                Assert.Equal(203, tax.CreditAccountId);
                Assert.Equal(720_000m, tax.Amount);
            });
    }

    [Fact]
    public void DraftFactory_RejectsDuplicateObjectPeriod()
    {
        var contract = new RentalContract { Id = 10, OrganizationId = 2, CurrencyId = 1 };
        var contractObject = new RentalContractObject
        {
            Id = 11,
            ContractAmount = 5_000_000m,
            TaxBaseAmount = 6_000_000m,
            TaxRate = 12m
        };
        var period = new RentalAccrualPeriod(
            new DateTime(2026, 8, 1),
            new DateTime(2026, 8, 31),
            new DateTime(2026, 9, 1));

        Assert.Throws<ArgumentException>(() => RentalAccrualDraftFactory.Create(
            contract,
            "1",
            new DateTime(2026, 8, 1),
            [new(contractObject, period), new(contractObject, period)],
            null));
    }

    private sealed class StubAccountingPolicyResolver(short value) : IOrganizationAccountingPolicyResolver
    {
        public Task<short> ResolveAsync(int organizationId, CancellationToken ct = default) =>
            Task.FromResult(value);
    }
}
