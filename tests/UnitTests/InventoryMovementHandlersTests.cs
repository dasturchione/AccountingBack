using Application.Features.InventoryMovements;
using Domain.Entities;
using SharedKernel.Constants;

namespace UnitTests;

public sealed class InventoryMovementHandlersTests
{
    [Fact]
    public async Task PurchaseHandler_PreservesAggregateReceipt()
    {
        var document = new PurchaseDoc
        {
            Id = 10,
            OrganizationId = 1,
            WarehouseId = 2,
            DocDate = new DateTime(2026, 7, 30),
            PurchaseDocProducts =
            [
                new PurchaseDocProduct
                {
                    Id = 100,
                    ProductId = 3,
                    Quantity = 4m,
                    TotalAmount = 80m,
                    Product = Product(isPieceTracked: false)
                }
            ]
        };

        var result = await new PurchaseInventoryHandler().HandleAsync(document);

        Assert.True(result.IsSuccess);
        var entry = Assert.Single(result.Value);
        Assert.Equal(OperationTypeIdConst.IN, entry.OperationTypeId);
        Assert.Equal(4m, entry.Quantity);
        Assert.Equal(80m, entry.Amount);
        Assert.Null(entry.ProductTableId);
        Assert.Equal(100, entry.SourceLineId);
    }

    [Fact]
    public async Task SaleHandler_PreservesPieceTrackedIssue()
    {
        var product = Product(isPieceTracked: true);
        product.Id = 3;
        var productTable = new ProductTable { Id = 11, ProductId = 3, Product = product };
        var document = new SaleDoc
        {
            Id = 20,
            OrganizationId = 1,
            WarehouseId = 2,
            DocDate = new DateTime(2026, 7, 30),
            SaleDocProducts =
            [
                new SaleDocProduct
                {
                    Id = 200,
                    ProductId = 3,
                    Quantity = 1m,
                    Product = product,
                    SaleDocTables =
                    [
                        new SaleDocTable
                        {
                            Id = 201,
                            ProductTableId = 11,
                            ProductTable = productTable,
                            CostPrice = 25m
                        }
                    ]
                }
            ]
        };

        var result = await new SaleInventoryHandler().HandleAsync(document);

        Assert.True(result.IsSuccess);
        var entry = Assert.Single(result.Value);
        Assert.Equal(OperationTypeIdConst.OUT, entry.OperationTypeId);
        Assert.Equal(1m, entry.Quantity);
        Assert.Equal(25m, entry.Amount);
        Assert.Equal(11, entry.ProductTableId);
        Assert.Equal(201, entry.SourceLineId);
    }

    [Fact]
    public async Task WarehouseTransferHandler_PreservesOutAndInPair()
    {
        var document = new WarehouseTransferDoc
        {
            Id = 30,
            OrganizationId = 1,
            SourceWarehouseId = 2,
            DestinationWarehouseId = 4,
            DocDate = new DateTime(2026, 7, 30),
            WarehouseTransferLines =
            [
                new WarehouseTransferLine
                {
                    Id = 300,
                    ProductId = 3,
                    Quantity = 5m,
                    Product = Product(isPieceTracked: false)
                }
            ]
        };

        var result = await new WarehouseTransferInventoryHandler().HandleAsync(document);

        Assert.True(result.IsSuccess);
        Assert.Collection(
            result.Value,
            entry =>
            {
                Assert.Equal(2, entry.WarehouseId);
                Assert.Equal(OperationTypeIdConst.OUT, entry.OperationTypeId);
                Assert.Equal(5m, entry.Quantity);
            },
            entry =>
            {
                Assert.Equal(4, entry.WarehouseId);
                Assert.Equal(OperationTypeIdConst.IN, entry.OperationTypeId);
                Assert.Equal(5m, entry.Quantity);
            });
    }

    [Fact]
    public async Task InventoryAdjustmentHandler_PreservesDirection()
    {
        var document = new InventoryAdjustmentDoc
        {
            Id = 40,
            OrganizationId = 1,
            WarehouseId = 2,
            AdjustmentType = "NEGATIVE_ADJUSTMENT",
            DocDate = new DateTime(2026, 7, 30),
            InventoryAdjustmentLines =
            [
                new InventoryAdjustmentLine
                {
                    Id = 400,
                    ProductId = 3,
                    Quantity = 2m,
                    Product = Product(isPieceTracked: false)
                }
            ]
        };

        var result = await new InventoryAdjustmentInventoryHandler().HandleAsync(document);

        Assert.True(result.IsSuccess);
        var entry = Assert.Single(result.Value);
        Assert.Equal(OperationTypeIdConst.OUT, entry.OperationTypeId);
        Assert.Equal(2m, entry.Quantity);
    }

    private static Product Product(bool isPieceTracked) =>
        new()
        {
            Id = 3,
            IsService = false,
            IsPieceTracked = isPieceTracked
        };
}
