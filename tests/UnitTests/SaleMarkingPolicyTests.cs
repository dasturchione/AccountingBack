using Application.Features.InventoryMovements;
using Domain.Entities;
using SharedKernel.Constants;

namespace UnitTests;

public sealed class SaleMarkingPolicyTests
{
    [Theory]
    [InlineData(6, 6, true)]
    [InlineData(6, 4, true)]
    [InlineData(6, 0, true)]
    [InlineData(4, 3, true)]
    [InlineData(3, 4, false)]
    public void OccurrenceCount_IsComparedWithQuantity_WithoutDistinct(
        int quantity,
        int occurrenceCount,
        bool expected)
    {
        Assert.Equal(expected, SaleMarkingPolicy.IsOccurrenceCountAllowed(quantity, occurrenceCount));
    }

    [Fact]
    public void DistinctPhysicalMarkings_AreCalculatedAcrossTheWholeDocument()
    {
        var result = SaleMarkingPolicy.GetDistinctPhysicalMarkingIds(
        [
            new[] { 10, 10, 20 },
            new[] { 10, 30 }
        ]);

        Assert.Equal(new[] { 10, 20, 30 }, result);
    }

    [Fact]
    public async Task SaleHandler_UsesSoldProductQuantity_NotMarkingOccurrences()
    {
        var sale = new SaleDoc
        {
            Id = 100,
            OrganizationId = 1,
            WarehouseId = 2,
            DocDate = new DateTime(2026, 8, 21),
            SaleDocProducts =
            [
                new SaleDocProduct
                {
                    Id = 11,
                    ProductId = 101,
                    Product = new Product { Id = 101, IsPieceTracked = true, IsService = false },
                    Quantity = 6,
                    CostPrice = 5,
                    SaleDocTables =
                    [
                        Occurrence(1, 900),
                        Occurrence(2, 900),
                        Occurrence(3, 901),
                        Occurrence(4, 902)
                    ]
                }
            ]
        };

        var result = await new SaleInventoryHandler().HandleAsync(sale);

        Assert.True(result.IsSuccess);
        var entry = Assert.Single(result.Value);
        Assert.Equal(6, entry.Quantity);
        Assert.Equal(101, entry.ProductId);
        Assert.Null(entry.ProductTableId);
    }

    [Fact]
    public async Task RetailSaleHandler_UsesSoldProductQuantity_NotMarkingOccurrences()
    {
        var sale = new RetailSaleDoc
        {
            Id = 200,
            OrganizationId = 1,
            WarehouseId = 2,
            DocDate = new DateTime(2026, 8, 21),
            RetailSaleDocProducts =
            [
                new RetailSaleDocProduct
                {
                    Id = 21,
                    ProductId = 201,
                    Product = new Product { Id = 201, IsPieceTracked = true, IsService = false },
                    Quantity = 5,
                    CostPrice = 7,
                    RetailSaleDocTables =
                    [
                        new RetailSaleDocTable { Id = 1, ProductTableId = 900 },
                        new RetailSaleDocTable { Id = 2, ProductTableId = 900 },
                        new RetailSaleDocTable { Id = 3, ProductTableId = 901 }
                    ]
                }
            ]
        };

        var result = await new RetailSaleInventoryHandler().HandleAsync(sale);

        Assert.True(result.IsSuccess);
        var entry = Assert.Single(result.Value);
        Assert.Equal(5, entry.Quantity);
        Assert.Equal(201, entry.ProductId);
        Assert.Null(entry.ProductTableId);
    }

    [Theory]
    [InlineData(20, 20, 6, 6, 14, 14)]
    [InlineData(20, 20, 6, 4, 14, 16)]
    [InlineData(20, 20, 6, 0, 14, 20)]
    public async Task ProductAndMarkingBalances_AreChangedIndependently(
        int productBefore,
        int markingsBefore,
        int soldQuantity,
        int distinctMarkings,
        int expectedProductAfter,
        int expectedMarkingsAfter)
    {
        var sale = new SaleDoc
        {
            OrganizationId = 1,
            WarehouseId = 2,
            SaleDocProducts =
            [
                new SaleDocProduct
                {
                    Id = 1,
                    ProductId = 100,
                    Product = new Product { Id = 100, IsPieceTracked = true },
                    Quantity = soldQuantity
                }
            ]
        };

        var result = await new SaleInventoryHandler().HandleAsync(sale);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedProductAfter, productBefore - Assert.Single(result.Value).Quantity);
        Assert.Equal(expectedMarkingsAfter, markingsBefore - distinctMarkings);
    }

    [Fact]
    public async Task SamePhysicalMarking_CanOccurInDifferentProductLines_ButMovesOnce()
    {
        var sale = new SaleDoc
        {
            OrganizationId = 1,
            WarehouseId = 2,
            SaleDocProducts =
            [
                new SaleDocProduct
                {
                    Id = 1,
                    ProductId = 100,
                    Product = new Product { Id = 100, IsPieceTracked = true },
                    Quantity = 3,
                    SaleDocTables = [Occurrence(1, 10), Occurrence(2, 10), Occurrence(3, 20)]
                },
                new SaleDocProduct
                {
                    Id = 2,
                    ProductId = 200,
                    Product = new Product { Id = 200, IsPieceTracked = true },
                    Quantity = 2,
                    SaleDocTables = [Occurrence(4, 10), Occurrence(5, 30)]
                }
            ]
        };

        var movements = await new SaleInventoryHandler().HandleAsync(sale);
        var physicalIds = SaleMarkingPolicy.GetDistinctPhysicalMarkingIds(
            sale.SaleDocProducts.Select(line => line.SaleDocTables.Select(item => item.ProductTableId)));

        Assert.True(movements.IsSuccess);
        Assert.Collection(
            movements.Value.OrderBy(entry => entry.ProductId),
            entry => Assert.Equal((100, 3m), (entry.ProductId, entry.Quantity)),
            entry => Assert.Equal((200, 2m), (entry.ProductId, entry.Quantity)));
        Assert.Equal(new[] { 10, 20, 30 }, physicalIds);
    }

    [Fact]
    public void ConfirmAndCancel_UseTheSameDistinctPhysicalMarkingSet()
    {
        int[][] occurrences =
        [
            [10, 10, 20],
            [10, 30]
        ];

        var consumed = SaleMarkingPolicy.GetDistinctPhysicalMarkingIds(occurrences);
        var restored = SaleMarkingPolicy.GetDistinctPhysicalMarkingIds(occurrences);

        Assert.Equal(new[] { 10, 20, 30 }, consumed);
        Assert.Equal(consumed, restored);
    }

    [Theory]
    [InlineData(ProductTableStatusIdConst.IN_STOCK, true)]
    [InlineData(ProductTableStatusIdConst.SOLD, false)]
    [InlineData(ProductTableStatusIdConst.WRITTEN_OFF, false)]
    public void OnlyInStockMarkingCanBeSold(short statusId, bool expected)
    {
        Assert.Equal(expected, SaleMarkingPolicy.IsAvailableForSale(statusId));
    }

    [Theory]
    [InlineData(ProductTableStatusIdConst.SOLD, true)]
    [InlineData(ProductTableStatusIdConst.IN_STOCK, false)]
    [InlineData(ProductTableStatusIdConst.WRITTEN_OFF, false)]
    public void CancelRestoresOnlySoldMarking(short statusId, bool expected)
    {
        Assert.Equal(expected, SaleMarkingPolicy.IsRestorableAfterSale(statusId));
    }

    private static SaleDocTable Occurrence(long id, int productTableId) => new()
    {
        Id = id,
        ProductTableId = productTableId
    };
}
