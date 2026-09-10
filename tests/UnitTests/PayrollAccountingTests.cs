using System.Reflection;
using System.Runtime.CompilerServices;
using Application.Features.Pay.Components;
using Application.Features.Pay.Payments;
using Application.Features.Pay.PayrollDocuments;
using Application.Features.Pay.Validators;
using Application.Features.Register.PostingEngines;
using Domain.Entities;
using SharedKernel.Constants;

namespace UnitTests;

public sealed class PayrollAccountingTests
{
    [Fact]
    public void Reclassification_DoesNotReduceEmployeeNetAmount()
    {
        var components = new List<PayComponent>
        {
            Component(1, "Salary", PayrollComponentTypeConst.Earning, PayrollCalculationMethodConst.Fixed, amount: 2_000_000m),
            Component(2, "Personal income tax", PayrollComponentTypeConst.Deduction, PayrollCalculationMethodConst.PercentOfGross, rate: 12m),
            Component(3, "Mandatory individual pension contribution", PayrollComponentTypeConst.Reclassification, PayrollCalculationMethodConst.PercentOfGross, rate: 0.1m),
            Component(4, "Social tax", PayrollComponentTypeConst.EmployerTax, PayrollCalculationMethodConst.PercentOfGross, rate: 12m)
        };
        components[1].LiabilityAccountId = 64201;
        components[2].ExpenseAccountId = 64201;
        components[2].LiabilityAccountId = 65302;
        components[3].ExpenseAccountId = 9430;
        components[3].LiabilityAccountId = 65101;
        var service = (PayrollDocumentService)RuntimeHelpers.GetUninitializedObject(typeof(PayrollDocumentService));
        var method = typeof(PayrollDocumentService).GetMethod("BuildPayrollLine", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);

        var line = (PayPayrollLine?)method.Invoke(service,
        [
            2,
            new PayEmployment { EmployeeId = 7, EmploymentRate = 1m, CurrencyId = 1, ExpenseAccountId = 9410 },
            new PayTimesheetLine(),
            new PayPeriod(),
            components,
            new Dictionary<(long EmployeeId, int ComponentId), PayEmployeeComponent>(),
            new Dictionary<(long EmployeeId, int ComponentId), PayrollManualAdjustmentDto>(),
            0m,
            PayrollDocumentKindConst.Regular
        ]);

        Assert.NotNull(line);
        Assert.Equal(2_000_000m, line.GrossAmount);
        Assert.Equal(240_000m, line.DeductionAmount);
        Assert.Equal(240_000m, line.EmployerTaxAmount);
        Assert.Equal(1_760_000m, line.NetAmount);
        Assert.Equal(1_760_000m, line.PayableAmount);

        var assignAccounts = typeof(PayrollDocumentService).GetMethod(
            "AssignPostingAccounts",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(assignAccounts);
        assignAccounts.Invoke(null,
        [
            line,
            new PayrollCalculateDto
            {
                SalaryExpenseAccountId = 9420,
                SalaryPayableAccountId = 6710
            }
        ]);

        AssertPostingAccounts(line, 1, 9410, 6710);
        AssertPostingAccounts(line, 2, 6710, 64201);
        AssertPostingAccounts(line, 3, 64201, 65302);
        AssertPostingAccounts(line, 4, 9430, 65101);
    }

    [Fact]
    public void SalaryProrated_HourBasis_UsesEditedWorkedHours()
    {
        var component = Component(
            1,
            "Salary",
            PayrollComponentTypeConst.Earning,
            PayrollCalculationMethodConst.SalaryProrated);
        component.ProrationBasis = PayrollProrationBasisConst.Hours;

        var service = (PayrollDocumentService)RuntimeHelpers.GetUninitializedObject(typeof(PayrollDocumentService));
        var method = typeof(PayrollDocumentService).GetMethod("BuildPayrollLine", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);

        var line = (PayPayrollLine?)method.Invoke(service,
        [
            2,
            new PayEmployment { EmployeeId = 7, MonthlySalary = 2_200_000m, EmploymentRate = 1m, CurrencyId = 1 },
            new PayTimesheetLine
            {
                WorkedDays = 22m,
                WorkedHours = 172m,
                NormWorkDays = 22m,
                NormWorkHours = 176m
            },
            new PayPeriod { NormWorkDays = 22m, NormWorkHours = 176m },
            new List<PayComponent> { component },
            new Dictionary<(long EmployeeId, int ComponentId), PayEmployeeComponent>(),
            new Dictionary<(long EmployeeId, int ComponentId), PayrollManualAdjustmentDto>(),
            0m,
            PayrollDocumentKindConst.Regular
        ]);

        Assert.NotNull(line);
        Assert.Equal(2_150_000m, line.GrossAmount);
        Assert.Equal(172m, Assert.Single(line.CalcLines).Quantity);
    }

    [Fact]
    public void ReclassificationComponent_IsAcceptedByValidator()
    {
        var validator = new PayrollComponentCreateDtoValidator();
        var dto = new PayrollComponentCreateDto
        {
            Code = "INPS_MANDATORY",
            Name = "Mandatory individual pension contribution",
            ComponentType = "RECLASSIFICATION",
            CalculationMethod = PayrollCalculationMethodConst.PercentOfGross,
            DefaultRate = 0.1m,
            ExpenseAccountId = 64201,
            LiabilityAccountId = 65301,
            EffectiveFrom = new DateOnly(2026, 1, 1),
            SortOrder = 30
        };

        var result = validator.Validate(dto);

        Assert.True(result.IsValid, string.Join(Environment.NewLine, result.Errors));
    }

    [Fact]
    public async Task ReclassificationPosting_UsesPersistedAccountsWithoutDefaultResolver()
    {
        var calcLine = new PayPayrollCalcLine
        {
            Id = 31,
            Amount = 2_000m,
            Component = new PayComponent
            {
                Id = 4,
                Name = "Mandatory individual pension contribution",
                ComponentType = "RECLASSIFICATION",
                ExpenseAccountId = 9001,
                LiabilityAccountId = 9002,
                SortOrder = 4
            }
        };
        calcLine.DebitAccountId = 64201;
        calcLine.CreditAccountId = 65301;

        var document = new PayPayrollDoc
        {
            Id = 11,
            OrganizationId = 2,
            CurrencyId = 1,
            DocDate = new DateTime(2026, 9, 5),
            DocNumber = "1",
            Lines =
            [
                new PayPayrollLine
                {
                    Id = 21,
                    EmployeeId = 7,
                    Employee = new PayEmployee
                    {
                        Id = 7,
                        EmployeeNumber = "E-7",
                        FirstName = "Ali",
                        LastName = "Valiyev"
                    },
                    Employment = new PayEmployment(),
                    CalcLines = [calcLine]
                }
            ]
        };

        var builder = new PayrollDocumentContextBuilder(new StubAccountingPolicyResolver());

        var contexts = await builder.BuildAsync(document);

        var entry = Assert.Single(Assert.Single(contexts).Entries);
        Assert.Equal(64201, entry.DebitAccountId);
        Assert.Equal(65301, entry.CreditAccountId);
    }

    [Fact]
    public void PayrollPayment_WithoutOffsetAccount_IsRejected()
    {
        var dto = new PayrollPaymentCreateDto
        {
            PeriodId = 1,
            DocDate = new DateTime(2026, 9, 5),
            PaymentKind = PayrollPaymentKindConst.Final,
            SourceType = PayrollPaymentSourceConst.Bank,
            BankAccountId = 1,
            SourceChartAccountId = 5110,
            CurrencyId = 1,
            Lines = [new PayrollPaymentLineCreateDto { EmployeeId = 7, Amount = 1_760_000m }]
        };
        var result = new PayrollPaymentCreateDtoValidator().Validate(dto);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(PayrollPaymentCreateDto.OffsetAccountId));
    }

    private static PayComponent Component(
        int id,
        string name,
        string type,
        string method,
        decimal? amount = null,
        decimal? rate = null) =>
        new()
        {
            Id = id,
            Name = name,
            ComponentType = type,
            CalculationMethod = method,
            DefaultAmount = amount,
            DefaultRate = rate,
            SortOrder = id
        };

    private static void AssertPostingAccounts(
        PayPayrollLine line,
        int componentId,
        int debitAccountId,
        int creditAccountId)
    {
        var calcLine = Assert.Single(line.CalcLines, item => item.ComponentId == componentId);
        Assert.Equal(debitAccountId, calcLine.DebitAccountId);
        Assert.Equal(creditAccountId, calcLine.CreditAccountId);
    }

    private sealed class StubAccountingPolicyResolver : IOrganizationAccountingPolicyResolver
    {
        public Task<short> ResolveAsync(int organizationId, CancellationToken ct = default) =>
            Task.FromResult((short)1);
    }
}
