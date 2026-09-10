using Application.Features.Pay.PayrollDocuments;
using SharedKernel.Constants;

namespace UnitTests;

public sealed class PayrollCorrectionPayoutTests
{
    [Fact]
    public void ExistingCorrectionDefaultsToSeparatePayout()
    {
        Assert.Equal(
            PayrollCorrectionPayoutModeConst.Separate,
            PayrollDocumentPaymentPolicy.DefaultPayoutMode(PayrollDocumentKindConst.Correction));
    }

    [Fact]
    public void InvalidPayoutModeFallsBackToKindDefault()
    {
        Assert.Equal(
            PayrollCorrectionPayoutModeConst.Separate,
            PayrollDocumentPaymentPolicy.NormalizePayoutMode(
                PayrollDocumentKindConst.Correction,
                "unknown"));
    }

    [Fact]
    public void SeparateCorrectionIsExcludedFromMainPayrollTotals()
    {
        Assert.False(PayrollDocumentPaymentPolicy.IsIncludedInMainPayroll(
            PayrollDocumentKindConst.Correction,
            PayrollCorrectionPayoutModeConst.Separate));
        Assert.True(PayrollDocumentPaymentPolicy.IsIncludedInMainPayroll(
            PayrollDocumentKindConst.Correction,
            PayrollCorrectionPayoutModeConst.WithSalary));
    }

    [Fact]
    public void RegularDocumentDefaultsToWithSalaryPayout()
    {
        Assert.Equal(
            PayrollCorrectionPayoutModeConst.WithSalary,
            PayrollDocumentPaymentPolicy.DefaultPayoutMode(PayrollDocumentKindConst.Regular));
    }
}
