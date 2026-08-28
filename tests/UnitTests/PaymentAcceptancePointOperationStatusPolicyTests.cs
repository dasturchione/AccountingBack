using Application.Features.PaymentAcceptancePointOperations;
using SharedKernel.Constants;

namespace UnitTests;

public sealed class PaymentAcceptancePointOperationStatusPolicyTests
{
    [Theory]
    [InlineData(DocumentStatusIdConst.DRAFT, true)]
    [InlineData(DocumentStatusIdConst.PENDING, true)]
    [InlineData(DocumentStatusIdConst.POSTED, false)]
    [InlineData(DocumentStatusIdConst.CANCELLED, false)]
    public void Confirm_AllowsOnlyDraftAndPending(short statusId, bool expected)
    {
        Assert.Equal(expected, PaymentAcceptancePointOperationStatusPolicy.CanConfirm(statusId));
    }

    [Theory]
    [InlineData(DocumentStatusIdConst.DRAFT, true)]
    [InlineData(DocumentStatusIdConst.PENDING, true)]
    [InlineData(DocumentStatusIdConst.POSTED, true)]
    [InlineData(DocumentStatusIdConst.CANCELLED, false)]
    public void Cancel_AllowsDraftPendingAndPosted(short statusId, bool expected)
    {
        Assert.Equal(expected, PaymentAcceptancePointOperationStatusPolicy.CanCancel(statusId));
    }
}
