using Application.Features.Pay.PayrollDocuments;
using Application.Features.Register.PostingEngines;
using Domain.Entities;
using SharedKernel.Constants;

namespace UnitTests;

public sealed class PayrollTaxPostingAccountTests
{
    [Fact]
    public async Task EmployerTax_IsChargedToTheAccountTheSalaryWasChargedTo()
    {
        // Salary sits in production (2010); the header default is an administrative account.
        var document = Document(
            salaryExpenseAccountId: 9420,
            salaryPayableAccountId: 6710,
            line: Line(
                earnings: [(amount: 2_000_000m, debit: 2010, credit: 6710)],
                taxes: [(amount: 240_000m, liability: 6520, type: PayrollTaxTypeConst.Employer)]));

        var entries = await BuildEntriesAsync(document);

        var tax = Assert.Single(entries, x => x.Content!.StartsWith("Tax:"));
        Assert.Equal(2010, tax.DebitAccountId);
        Assert.Equal(6520, tax.CreditAccountId);
        Assert.Equal(240_000m, tax.Amount);
    }

    [Fact]
    public async Task Withholding_IsTakenOffTheAccountTheEarningsMadePayable()
    {
        // The earning component has its own payable account (6720), not the header's 6710.
        var document = Document(
            salaryExpenseAccountId: 9420,
            salaryPayableAccountId: 6710,
            line: Line(
                earnings: [(amount: 1_000_000m, debit: 9420, credit: 6720)],
                taxes: [(amount: 120_000m, liability: 6410, type: PayrollTaxTypeConst.Withholding)]));

        var entries = await BuildEntriesAsync(document);

        var tax = Assert.Single(entries, x => x.Content!.StartsWith("Tax:"));
        Assert.Equal(6720, tax.DebitAccountId);
        Assert.Equal(6410, tax.CreditAccountId);
    }

    [Fact]
    public async Task EmployerTax_IsSplitAcrossExpenseAccountsInProportionToTheEarnings()
    {
        var document = Document(
            salaryExpenseAccountId: 9420,
            salaryPayableAccountId: 6710,
            line: Line(
                earnings:
                [
                    (amount: 1_500_000m, debit: 2010, credit: 6710),
                    (amount: 500_000m, debit: 9420, credit: 6710)
                ],
                taxes: [(amount: 240_000m, liability: 6520, type: PayrollTaxTypeConst.Employer)]));

        var entries = await BuildEntriesAsync(document);

        var taxEntries = entries.Where(x => x.Content!.StartsWith("Tax:")).ToList();
        Assert.Equal(2, taxEntries.Count);
        Assert.Equal(180_000m, taxEntries.Single(x => x.DebitAccountId == 2010).Amount);
        Assert.Equal(60_000m, taxEntries.Single(x => x.DebitAccountId == 9420).Amount);
        Assert.Equal(240_000m, taxEntries.Sum(x => x.Amount));
    }

    [Fact]
    public async Task SplitTax_StillAddsUpToTheTaxLineWhenTheShareDoesNotDivideEvenly()
    {
        var document = Document(
            salaryExpenseAccountId: 9420,
            salaryPayableAccountId: 6710,
            line: Line(
                earnings:
                [
                    (amount: 1_000_000m, debit: 2010, credit: 6710),
                    (amount: 1_000_000m, debit: 2310, credit: 6710),
                    (amount: 1_000_000m, debit: 9420, credit: 6710)
                ],
                taxes: [(amount: 100m, liability: 6520, type: PayrollTaxTypeConst.Employer)]));

        var entries = await BuildEntriesAsync(document);

        var taxEntries = entries.Where(x => x.Content!.StartsWith("Tax:")).ToList();
        Assert.Equal(3, taxEntries.Count);
        Assert.Equal(100m, taxEntries.Sum(x => x.Amount));
    }

    [Fact]
    public async Task FallsBackToTheDocumentHeader_WhenTheLineCarriesNoEarnings()
    {
        // A correction line that only holds a tax has nothing to weigh against.
        var document = Document(
            salaryExpenseAccountId: 9420,
            salaryPayableAccountId: 6710,
            line: Line(
                earnings: [],
                taxes: [(amount: 50_000m, liability: 6410, type: PayrollTaxTypeConst.Withholding)]));

        var entries = await BuildEntriesAsync(document);

        var tax = Assert.Single(entries, x => x.Content!.StartsWith("Tax:"));
        Assert.Equal(6710, tax.DebitAccountId);
    }

    [Fact]
    public void Deduction_IsWithheldFromTheAccountTheEarningsMadePayable()
    {
        var line = new PayPayrollLine
        {
            Id = 10,
            EmployeeId = 7,
            Employment = new PayEmployment { ExpenseAccountId = 2010 }
        };
        line.CalcLines.Add(CalcLine(
            id: 1,
            type: PayrollComponentTypeConst.Earning,
            amount: 2_000_000m,
            // The component pays out on its own account instead of the header's 6710.
            liabilityAccountId: 6720));
        line.CalcLines.Add(CalcLine(
            id: 2,
            type: PayrollComponentTypeConst.Deduction,
            amount: 150_000m,
            liabilityAccountId: 6990));

        AssignPostingAccounts(line, salaryExpenseAccountId: 9420, salaryPayableAccountId: 6710);

        var earning = line.CalcLines.Single(x => x.Id == 1);
        Assert.Equal(2010, earning.DebitAccountId);
        Assert.Equal(6720, earning.CreditAccountId);

        var deduction = line.CalcLines.Single(x => x.Id == 2);
        Assert.Equal(6720, deduction.DebitAccountId);
        Assert.Equal(6990, deduction.CreditAccountId);
    }

    [Fact]
    public void Deduction_FallsBackToTheHeaderPayableAccount_WhenTheLineHasNoEarnings()
    {
        var line = new PayPayrollLine { Id = 10, EmployeeId = 7, Employment = new PayEmployment() };
        line.CalcLines.Add(CalcLine(
            id: 1,
            type: PayrollComponentTypeConst.Deduction,
            amount: 150_000m,
            liabilityAccountId: 6990));

        AssignPostingAccounts(line, salaryExpenseAccountId: 9420, salaryPayableAccountId: 6710);

        Assert.Equal(6710, line.CalcLines.Single().DebitAccountId);
    }

    private static PayPayrollCalcLine CalcLine(int id, string type, decimal amount, int liabilityAccountId) =>
        new()
        {
            Id = id,
            ComponentId = id,
            Component = new PayComponent
            {
                Id = id,
                Name = $"Component {id}",
                ComponentType = type,
                SortOrder = id,
                LiabilityAccountId = liabilityAccountId
            },
            Amount = amount
        };

    private static void AssignPostingAccounts(
        PayPayrollLine line,
        int salaryExpenseAccountId,
        int salaryPayableAccountId)
    {
        var method = typeof(Application.Features.Pay.PayrollDocuments.PayrollDocumentService).GetMethod(
            "AssignPostingAccounts",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        Assert.NotNull(method);
        method.Invoke(null,
        [
            line,
            new Application.Features.Pay.PayrollDocuments.PayrollCalculateDto
            {
                SalaryExpenseAccountId = salaryExpenseAccountId,
                SalaryPayableAccountId = salaryPayableAccountId
            }
        ]);
    }

    [Fact]
    public void Allocate_NegativeAmount_KeepsTheSignAndTheTotal()
    {
        var weights = new List<PayrollAccountShare>
        {
            new(2010, 1_500_000m),
            new(9420, 500_000m)
        };

        var shares = PayrollPostingAccountAllocator.Allocate(weights, -240_000m, fallbackAccountId: 6710);

        Assert.Equal(-240_000m, shares.Sum(x => x.Amount));
        Assert.All(shares, share => Assert.True(share.Amount < 0m));
    }

    private static async Task<List<PostingEntryContext>> BuildEntriesAsync(PayPayrollDoc document)
    {
        var builder = new PayrollDocumentContextBuilder(new StubPolicyResolver());
        var validation = await builder.ValidateAsync(document);
        Assert.True(validation.IsSuccess, validation.Error?.Code);

        var contexts = await builder.BuildAsync(document);
        return contexts.SelectMany(x => x.Entries).ToList();
    }

    private static PayPayrollDoc Document(
        int salaryExpenseAccountId,
        int salaryPayableAccountId,
        PayPayrollLine line) =>
        new()
        {
            Id = 1,
            OrganizationId = 8,
            CurrencyId = 1,
            DocDate = new DateTime(2026, 3, 31),
            DocNumber = "SAL-2026-000001",
            SalaryExpenseAccountId = salaryExpenseAccountId,
            SalaryPayableAccountId = salaryPayableAccountId,
            Lines = [line]
        };

    private static PayPayrollLine Line(
        (decimal amount, int debit, int credit)[] earnings,
        (decimal amount, int liability, string type)[] taxes)
    {
        var componentId = 1;
        var line = new PayPayrollLine
        {
            Id = 10,
            EmployeeId = 7,
            Employee = new PayEmployee { EmployeeNumber = "001", LastName = "Karimov", FirstName = "Alisher" },
            Employment = new PayEmployment()
        };

        foreach (var (amount, debit, credit) in earnings)
        {
            line.CalcLines.Add(new PayPayrollCalcLine
            {
                Id = componentId,
                ComponentId = componentId,
                Component = new PayComponent
                {
                    Id = componentId,
                    Name = $"Earning {componentId}",
                    ComponentType = PayrollComponentTypeConst.Earning,
                    SortOrder = componentId
                },
                Amount = amount,
                DebitAccountId = debit,
                CreditAccountId = credit
            });
            componentId++;
        }

        var taxId = 100;
        foreach (var (amount, liability, type) in taxes)
        {
            line.TaxLines.Add(new PayPayrollTaxLine
            {
                Id = taxId,
                TaxDefinitionId = taxId,
                TaxDefinition = new PayTaxDefinition
                {
                    Id = taxId,
                    Code = $"TAX{taxId}",
                    Name = $"Tax {taxId}",
                    TaxType = type
                },
                Amount = amount,
                LiabilityAccountId = liability
            });
            taxId++;
        }

        return line;
    }

    private sealed class StubPolicyResolver : IOrganizationAccountingPolicyResolver
    {
        public Task<short> ResolveAsync(int organizationId, CancellationToken ct = default) =>
            Task.FromResult((short)1);
    }
}
