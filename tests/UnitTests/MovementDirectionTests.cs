using Application.Features.InventoryMovements;
using Application.Features.InventoryAdjustments;
using Domain.Entities;
using SharedKernel.Constants;

namespace UnitTests;

public sealed class MovementDirectionTests
{
    [Theory]
    [InlineData(MovementDirectionIdConst.IN)]
    [InlineData(MovementDirectionIdConst.OUT)]
    public void IsValid_AcceptsKnownDirections(short directionId)
    {
        Assert.True(MovementDirectionIdConst.IsValid(directionId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(-2)]
    public void IsValid_RejectsUnknownDirections(short directionId)
    {
        Assert.False(MovementDirectionIdConst.IsValid(directionId));
    }

    [Theory]
    [InlineData(MovementDirectionIdConst.IN, MovementDirectionIdConst.OUT)]
    [InlineData(MovementDirectionIdConst.OUT, MovementDirectionIdConst.IN)]
    public void Reverse_ReturnsOppositeDirection(short directionId, short expected)
    {
        Assert.Equal(expected, MovementDirectionIdConst.Reverse(directionId));
    }

    [Fact]
    public async Task WarehouseTransfer_CreatesOutAndInMovementsWithZeroNetQuantity()
    {
        var document = new WarehouseTransferDoc
        {
            Id = 10,
            OrganizationId = 20,
            SourceWarehouseId = 30,
            DestinationWarehouseId = 40,
            DocDate = new DateTime(2026, 8, 25),
            WarehouseTransferLines =
            [
                new WarehouseTransferLine
                {
                    Id = 50,
                    ProductId = 60,
                    Product = new Product { Id = 60, IsService = false, IsPieceTracked = false },
                    Quantity = 7m
                }
            ]
        };

        var result = await new WarehouseTransferInventoryHandler().HandleAsync(document);

        Assert.True(result.IsSuccess);
        Assert.Collection(
            result.Value.OrderBy(x => x.DirectionId),
            movement =>
            {
                Assert.Equal(MovementDirectionIdConst.OUT, movement.DirectionId);
                Assert.Equal(document.SourceWarehouseId, movement.WarehouseId);
            },
            movement =>
            {
                Assert.Equal(MovementDirectionIdConst.IN, movement.DirectionId);
                Assert.Equal(document.DestinationWarehouseId, movement.WarehouseId);
            });
        Assert.Equal(0m, result.Value.Sum(x => x.DirectionId * x.Quantity));
    }

    [Theory]
    [InlineData("POSITIVE_ADJUSTMENT", MovementDirectionIdConst.IN, true)]
    [InlineData("POSITIVE_ADJUSTMENT", MovementDirectionIdConst.OUT, false)]
    [InlineData("WRITE_OFF", MovementDirectionIdConst.OUT, true)]
    [InlineData("WRITE_OFF", MovementDirectionIdConst.IN, false)]
    [InlineData("CORRECTION", MovementDirectionIdConst.IN, true)]
    [InlineData("CORRECTION", MovementDirectionIdConst.OUT, true)]
    public void InventoryAdjustmentPolicy_ValidatesDirection(
        string adjustmentType,
        short directionId,
        bool expected)
    {
        Assert.Equal(expected, InventoryAdjustmentDirectionPolicy.IsCompatible(adjustmentType, directionId));
    }
}
