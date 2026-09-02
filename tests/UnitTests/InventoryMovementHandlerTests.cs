using Application.Features.InventoryMovements;
using Domain.Entities;
using SharedKernel.Constants;

namespace UnitTests;

public sealed class InventoryMovementHandlerTests
{
    [Fact]
    public async Task PurchaseHandlerPreservesMarkedAndUnmarkedQuantitiesAndAmounts()
    {
        var product = Product(pieceTracked: true);
        var line = new PurchaseDocProduct
        {
            Id = 101,
            ProductId = product.Id,
            Product = product,
            Quantity = 3m,
            TotalAmount = 60m
        };
        line.PurchaseDocTables.Add(new PurchaseDocTable { ProductTableId = 201, TotalAmount = 10m });
        line.PurchaseDocTables.Add(new PurchaseDocTable { ProductTableId = 202, TotalAmount = 20m });
        var document = new PurchaseDoc
        {
            Id = 301,
            OrganizationId = 401,
            WarehouseId = 501,
            DocDate = new DateTime(2026, 9, 2),
            PurchaseDocProducts = [line]
        };

        var result = await new PurchaseInventoryHandler().HandleAsync(document);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Count);
        Assert.Equal(3m, result.Value.Sum(entry => entry.Quantity));
        Assert.Equal(60m, result.Value.Sum(entry => entry.Amount));
        Assert.Equal(2, result.Value.Count(entry => entry.ProductTableId.HasValue));
        var unmarked = Assert.Single(result.Value, entry => !entry.ProductTableId.HasValue);
        Assert.Equal(1m, unmarked.Quantity);
        Assert.Equal(30m, unmarked.Amount);
    }

    [Fact]
    public async Task TransferHandlerCreatesBalancedDirectionsForEveryTrackedItem()
    {
        var product = Product(pieceTracked: true);
        var line = new WarehouseTransferLine
        {
            Id = 102,
            ProductId = product.Id,
            Product = product,
            Quantity = 2m
        };
        line.WarehouseTransferDocTables.Add(new WarehouseTransferDocTable { Id = 211, ProductTableId = 201, CostPrice = 10m });
        line.WarehouseTransferDocTables.Add(new WarehouseTransferDocTable { Id = 212, ProductTableId = 202, CostPrice = 20m });
        var document = new WarehouseTransferDoc
        {
            Id = 302,
            OrganizationId = 402,
            SourceWarehouseId = 502,
            DestinationWarehouseId = 503,
            DocDate = new DateTime(2026, 9, 2),
            WarehouseTransferLines = [line]
        };

        var result = await new WarehouseTransferInventoryHandler().HandleAsync(document);

        Assert.True(result.IsSuccess);
        Assert.Equal(4, result.Value.Count);
        Assert.Equal(2m, result.Value.Where(entry => entry.DirectionId == MovementDirectionIdConst.OUT).Sum(entry => entry.Quantity));
        Assert.Equal(2m, result.Value.Where(entry => entry.DirectionId == MovementDirectionIdConst.IN).Sum(entry => entry.Quantity));
        Assert.Equal([201, 202], result.Value.Select(entry => entry.ProductTableId).Where(id => id.HasValue).Select(id => id!.Value).Distinct().Order());
    }

    [Fact]
    public async Task SaleHandlersExcludeServicesAndUseInventoryCost()
    {
        var stockProduct = Product(pieceTracked: false);
        var service = Product(pieceTracked: false, isService: true, id: 2);
        var sale = new SaleDoc
        {
            Id = 303,
            OrganizationId = 403,
            WarehouseId = 504,
            DocDate = new DateTime(2026, 9, 2),
            SaleDocProducts =
            [
                new SaleDocProduct { Id = 103, ProductId = stockProduct.Id, Product = stockProduct, Quantity = 3m, CostPrice = 7m },
                new SaleDocProduct { Id = 104, ProductId = service.Id, Product = service, Quantity = 1m, CostPrice = 100m }
            ]
        };
        var retail = new RetailSaleDoc
        {
            Id = 304,
            OrganizationId = 403,
            WarehouseId = 504,
            DocDate = sale.DocDate,
            RetailSaleDocProducts =
            [
                new RetailSaleDocProduct { Id = 105, ProductId = stockProduct.Id, Product = stockProduct, Quantity = 2m, CostPrice = 8m },
                new RetailSaleDocProduct { Id = 106, ProductId = service.Id, Product = service, Quantity = 1m, CostPrice = 100m }
            ]
        };

        var saleResult = await new SaleInventoryHandler().HandleAsync(sale);
        var retailResult = await new RetailSaleInventoryHandler().HandleAsync(retail);

        Assert.Equal(21m, Assert.Single(saleResult.Value).Amount);
        Assert.Equal(16m, Assert.Single(retailResult.Value).Amount);
        Assert.All(saleResult.Value.Concat(retailResult.Value), entry => Assert.Equal(MovementDirectionIdConst.OUT, entry.DirectionId));
    }

    private static Product Product(bool pieceTracked, bool isService = false, int id = 1) => new()
    {
        Id = id,
        IsPieceTracked = pieceTracked,
        IsService = isService
    };
}
