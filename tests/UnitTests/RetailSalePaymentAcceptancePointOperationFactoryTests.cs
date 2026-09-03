using Application.Features.RetailSaleDocs;
using SharedKernel.Constants;

namespace UnitTests;

public sealed class RetailSalePaymentAcceptancePointOperationFactoryTests
{
    [Fact]
    public void PostedIncomingOperation_IsLinkedToRetailSaleDocument()
    {
        var now = new DateTime(2026, 8, 28, 12, 30, 0);

        var operation = RetailSalePaymentAcceptancePointOperationFactory.Create(
            organizationId: 2,
            paymentAcceptancePointId: 11,
            relatedDocumentId: 77,
            docNumber: "15",
            docDate: now,
            currencyId: 1,
            amount: 4_200_000m,
            exchangeRate: 1m,
            transactionNumber: "TX-10",
            movement: new RetailSalePaymentAcceptancePointMovement(
                MovementDirectionIdConst.IN,
                DocumentStatusIdConst.POSTED),
            userId: 5,
            now: now);

        Assert.Equal(77, operation.RelatedDocumentId);
        Assert.Equal(MovementDirectionIdConst.IN, operation.DirectionId);
        Assert.Equal(DocumentStatusIdConst.POSTED, operation.StatusId);
        Assert.Equal(4_200_000m, operation.Amount);
        Assert.Equal("TX-10", operation.ExternalTransactionNumber);
        Assert.Equal(now, operation.PostedAt);
        Assert.Equal(5, operation.PostedByUserId);
    }

    [Fact]
    public void PendingOutgoingOperation_HasNoPostingMetadata()
    {
        var now = new DateTime(2026, 8, 28, 12, 30, 0);

        var operation = RetailSalePaymentAcceptancePointOperationFactory.Create(
            organizationId: 2,
            paymentAcceptancePointId: 11,
            relatedDocumentId: 77,
            docNumber: "16",
            docDate: now,
            currencyId: 1,
            amount: 4_200_000m,
            exchangeRate: 1m,
            transactionNumber: "TX-10",
            movement: new RetailSalePaymentAcceptancePointMovement(
                MovementDirectionIdConst.OUT,
                DocumentStatusIdConst.PENDING),
            userId: 5,
            now: now);

        Assert.Equal(77, operation.RelatedDocumentId);
        Assert.Equal(MovementDirectionIdConst.OUT, operation.DirectionId);
        Assert.Equal(DocumentStatusIdConst.PENDING, operation.StatusId);
        Assert.Null(operation.PostedAt);
        Assert.Null(operation.PostedByUserId);
    }
}
