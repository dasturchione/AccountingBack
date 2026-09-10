using SharedKernel.Constants;

namespace Application.Features.Pay.PayrollDocuments;

public static class PayrollRecalculationPolicy
{
    public static bool CanRequest(short documentStatusId) =>
        documentStatusId == DocumentStatusIdConst.POSTED;

    public static bool IsActive(string status) =>
        status is PayrollRecalculationStatusConst.Pending or PayrollRecalculationStatusConst.Processing;
}
