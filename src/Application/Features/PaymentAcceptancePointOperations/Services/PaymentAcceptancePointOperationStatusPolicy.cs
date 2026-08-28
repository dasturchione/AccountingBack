using SharedKernel.Constants;

namespace Application.Features.PaymentAcceptancePointOperations;

public static class PaymentAcceptancePointOperationStatusPolicy
{
    public static bool CanConfirm(short statusId) =>
        statusId is DocumentStatusIdConst.DRAFT or DocumentStatusIdConst.PENDING;

    public static bool CanCancel(short statusId) =>
        statusId is DocumentStatusIdConst.DRAFT or
            DocumentStatusIdConst.PENDING or
            DocumentStatusIdConst.POSTED;
}
