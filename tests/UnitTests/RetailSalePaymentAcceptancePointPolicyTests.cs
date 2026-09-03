using Application.Features.RetailSaleDocs;
using SharedKernel.Constants;

namespace UnitTests;

public sealed class RetailSalePaymentAcceptancePointPolicyTests
{
    [Fact]
    public void CashPayment_DoesNotRequirePointAndCreatesNoOperations()
    {
        var result = RetailSalePaymentAcceptancePointPolicy.Build(PaymentMethodCodeConst.CASH, null);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public void CashPayment_WithPointIsRejected()
    {
        var result = RetailSalePaymentAcceptancePointPolicy.Build(PaymentMethodCodeConst.CASH, 10);

        Assert.False(result.IsSuccess);
    }

    [Theory]
    [InlineData("CARD")]
    [InlineData("TRANSFER")]
    [InlineData("CLICK")]
    [InlineData("PAYME")]
    [InlineData("MOBILE_PAYMENT")]
    [InlineData("OTHER")]
    public void NonCashPayment_WithoutPointIsRejected(string paymentMethodCode)
    {
        var result = RetailSalePaymentAcceptancePointPolicy.Build(paymentMethodCode, null);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void CardPayment_CreatesPostedInAndPendingOut()
    {
        var result = RetailSalePaymentAcceptancePointPolicy.Build(PaymentMethodCodeConst.CARD, 10);

        Assert.True(result.IsSuccess);
        Assert.Collection(
            result.Value,
            incoming =>
            {
                Assert.Equal(MovementDirectionIdConst.IN, incoming.DirectionId);
                Assert.Equal(DocumentStatusIdConst.POSTED, incoming.StatusId);
            },
            outgoing =>
            {
                Assert.Equal(MovementDirectionIdConst.OUT, outgoing.DirectionId);
                Assert.Equal(DocumentStatusIdConst.PENDING, outgoing.StatusId);
            });
    }

    [Theory]
    [InlineData("TRANSFER")]
    [InlineData("CLICK")]
    [InlineData("PAYME")]
    [InlineData("MOBILE_PAYMENT")]
    [InlineData("OTHER")]
    public void OtherNonCashPayment_CreatesOnlyPostedIn(string paymentMethodCode)
    {
        var result = RetailSalePaymentAcceptancePointPolicy.Build(paymentMethodCode, 10);

        Assert.True(result.IsSuccess);
        var movement = Assert.Single(result.Value);
        Assert.Equal(MovementDirectionIdConst.IN, movement.DirectionId);
        Assert.Equal(DocumentStatusIdConst.POSTED, movement.StatusId);
    }
}
