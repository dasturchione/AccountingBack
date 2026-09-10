using SharedKernel.Constants;

namespace Application.Features.Pay.Periods;

public static class PayrollPeriodClosePolicy
{
    /// <summary>
    /// A period cannot be closed while a recalculation failed or is still
    /// waiting for/doing work. Completed requests are safe because their
    /// correction document (if any) is already linked and can be handled by
    /// the normal draft-document checks.
    /// </summary>
    public static bool HasBlockingRecalculation(string status) =>
        status is PayrollRecalculationStatusConst.Pending
            or PayrollRecalculationStatusConst.Processing
            or PayrollRecalculationStatusConst.Failed;
}
