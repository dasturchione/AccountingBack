using SharedKernel.Constants;

namespace Application.Features.Pay.PayrollDocuments;

public static class PayrollDocumentPaymentPolicy
{
    public static string DefaultPayoutMode(string documentKind) =>
        documentKind == PayrollDocumentKindConst.Correction
            ? PayrollCorrectionPayoutModeConst.Separate
            : PayrollCorrectionPayoutModeConst.WithSalary;

    public static string NormalizePayoutMode(string documentKind, string? payoutMode)
    {
        var normalized = payoutMode?.Trim().ToUpperInvariant();
        return PayrollCorrectionPayoutModeConst.All.Contains(normalized)
            ? normalized!
            : DefaultPayoutMode(documentKind);
    }

    public static bool IsIncludedInMainPayroll(string documentKind, string? payoutMode) =>
        documentKind == PayrollDocumentKindConst.Regular ||
        NormalizePayoutMode(documentKind, payoutMode) == PayrollCorrectionPayoutModeConst.WithSalary;
}
