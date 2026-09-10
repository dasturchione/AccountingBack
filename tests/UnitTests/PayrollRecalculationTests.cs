using Application.Features.Pay.PayrollDocuments;
using Application.Features.Pay.Periods;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using System.Reflection;

namespace UnitTests;

public sealed class PayrollRecalculationTests
{
    [Fact]
    public void OnlyPostedPayrollDocumentsCanBeRecalculated()
    {
        Assert.True(PayrollRecalculationPolicy.CanRequest(DocumentStatusIdConst.POSTED));
        Assert.False(PayrollRecalculationPolicy.CanRequest(DocumentStatusIdConst.DRAFT));
        Assert.False(PayrollRecalculationPolicy.CanRequest(DocumentStatusIdConst.CANCELLED));
    }

    [Fact]
    public void PendingOrProcessingRequestsAreActiveForIdempotency()
    {
        Assert.True(PayrollRecalculationPolicy.IsActive(PayrollRecalculationStatusConst.Pending));
        Assert.True(PayrollRecalculationPolicy.IsActive(PayrollRecalculationStatusConst.Processing));
        Assert.False(PayrollRecalculationPolicy.IsActive(PayrollRecalculationStatusConst.Completed));
        Assert.False(PayrollRecalculationPolicy.IsActive(PayrollRecalculationStatusConst.Failed));
    }

    [Fact]
    public void PeriodCloseBlocksUnresolvedRecalculationRequests()
    {
        Assert.True(PayrollPeriodClosePolicy.HasBlockingRecalculation(PayrollRecalculationStatusConst.Pending));
        Assert.True(PayrollPeriodClosePolicy.HasBlockingRecalculation(PayrollRecalculationStatusConst.Processing));
        Assert.True(PayrollPeriodClosePolicy.HasBlockingRecalculation(PayrollRecalculationStatusConst.Failed));
        Assert.False(PayrollPeriodClosePolicy.HasBlockingRecalculation(PayrollRecalculationStatusConst.Completed));
    }

    [Fact]
    public void RecalculationEntityHasScopedDocumentRelationships()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=payroll_model_smoke")
            .Options;
        using var context = new AppDbContext(options);

        var entity = context.Model.FindEntityType(typeof(PayPayrollRecalculation));
        Assert.NotNull(entity);
        Assert.Equal(2, entity!.GetForeignKeys().Count(x => x.PrincipalEntityType.ClrType == typeof(PayPayrollDoc)));
    }

    [Fact]
    public void DeltaCalculatorKeepsPositiveAndNegativeComponentChanges()
    {
        var original = new PayPayrollLine
        {
            WorkedDays = 20m,
            WorkedHours = 160m,
            GrossAmount = 1000m,
            DeductionAmount = 100m,
            EmployerTaxAmount = 120m,
            NetAmount = 900m,
            PayableAmount = 900m,
            CalcLines =
            [
                new PayPayrollCalcLine { ComponentId = 1, Amount = 1000m },
                new PayPayrollCalcLine { ComponentId = 2, Amount = 100m }
            ],
            TaxLines =
            [new PayPayrollTaxLine { TaxDefinitionId = 10, Amount = 120m }]
        };
        var recalculated = new PayPayrollLine
        {
            WorkedDays = 21m,
            WorkedHours = 168m,
            GrossAmount = 1100m,
            DeductionAmount = 85m,
            EmployerTaxAmount = 132m,
            NetAmount = 1015m,
            PayableAmount = 1015m,
            CalcLines =
            [
                new PayPayrollCalcLine { ComponentId = 1, Amount = 1100m },
                new PayPayrollCalcLine { ComponentId = 2, Amount = 85m },
                new PayPayrollCalcLine { ComponentId = 3, Amount = 25m }
            ],
            TaxLines =
            [new PayPayrollTaxLine { TaxDefinitionId = 10, Amount = 132m }]
        };

        var delta = PayrollRecalculationDeltaCalculator.Calculate(original, recalculated);

        Assert.Equal(1m, delta.WorkedDays);
        Assert.Equal(8m, delta.WorkedHours);
        Assert.Equal(100m, delta.GrossAmount);
        Assert.Equal(-15m, delta.DeductionAmount);
        Assert.Equal(12m, delta.EmployerTaxAmount);
        Assert.Equal(115m, delta.NetAmount);
        Assert.Equal(115m, delta.PayableAmount);
        Assert.Equal(100m, delta.ComponentDeltas[1]);
        Assert.Equal(-15m, delta.ComponentDeltas[2]);
        Assert.Equal(25m, delta.ComponentDeltas[3]);
        Assert.Equal(12m, delta.TaxDeltas[10]);
    }

    [Fact]
    public void SeparateCorrectionDoesNotReverseOriginalAdvanceOffset()
    {
        var original = new PayPayrollLine
        {
            EmployeeId = 7,
            EmploymentId = 9,
            AdvanceAmount = 500m,
            NetAmount = 1_000m,
            PayableAmount = 500m,
            CalcLines = [new PayPayrollCalcLine { ComponentId = 1, Amount = 1_000m }]
        };
        var recalculated = new PayPayrollLine
        {
            EmployeeId = 7,
            EmploymentId = 9,
            NetAmount = 1_100m,
            PayableAmount = 1_100m,
            CalcLines = [new PayPayrollCalcLine { ComponentId = 1, Amount = 1_100m }]
        };
        var delta = PayrollRecalculationDeltaCalculator.Calculate(original, recalculated);
        var method = typeof(PayrollDocumentService).GetMethod(
            "BuildRecalculationDeltaLine",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(method);

        var line = (PayPayrollLine?)method!.Invoke(null, [original, recalculated, delta]);

        Assert.NotNull(line);
        Assert.Equal(0m, line!.AdvanceAmount);
        Assert.Equal(100m, line.NetAmount);
        Assert.Equal(100m, line.PayableAmount);
    }
}
