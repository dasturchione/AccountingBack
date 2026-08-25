using Application.Features.InventoryMovements;
using Application.Features.PurchaseDocs;
using Domain.Entities;

namespace UnitTests;

public sealed class PurchaseMarkingPolicyTests
{
    [Theory]
    [InlineData(5, 0)]
    [InlineData(5, 2)]
    [InlineData(5, 5)]
    public void EdoStructure_AllowsMarkingCountUpToQuantity(int quantity, int markingCount)
    {
        var markings = Enumerable.Range(1, markingCount).Select(x => $"mark-{x}").ToArray();

        var result = EdoImportMarkingPolicy.ValidateStructure(
            isPieceTracked: true,
            isService: false,
            quantity,
            markings);

        Assert.Null(result);
    }

    [Fact]
    public void EdoStructure_RejectsMarkingCountGreaterThanQuantity()
    {
        var result = EdoImportMarkingPolicy.ValidateStructure(
            isPieceTracked: true,
            isService: false,
            quantity: 2,
            ["mark-1", "mark-2", "mark-3"]);

        Assert.Equal(EdoImportMarkingPolicy.CountMismatch, result);
    }

    [Fact]
    public void PurchasePreview_AllowsNoSelectedMarkings()
    {
        var result = PurchaseDocService.GetMarkingValidationErrorCode(
            new Product { IsPieceTracked = true, IsService = false },
            quantity: 5,
            providerMarkings: ["mark-1", "mark-2"],
            requestItems: []);

        Assert.Null(result);
    }

    [Fact]
    public void PurchasePreview_AllowsSelectedMarkingSubset()
    {
        var result = PurchaseDocService.GetMarkingValidationErrorCode(
            new Product { IsPieceTracked = true, IsService = false },
            quantity: 5,
            providerMarkings: ["mark-1", "mark-2"],
            requestItems: [new PurchaseDocLineItemDto { MarkingNumber = "mark-1" }]);

        Assert.Null(result);
    }

    [Fact]
    public async Task PurchaseHandler_UsesFullQuantityWhenOnlyPartHasMarkings()
    {
        var purchase = new PurchaseDoc
        {
            Id = 100,
            OrganizationId = 1,
            WarehouseId = 2,
            DocDate = new DateTime(2026, 8, 24),
            PurchaseDocProducts =
            [
                new PurchaseDocProduct
                {
                    Id = 11,
                    ProductId = 101,
                    Product = new Product { Id = 101, IsPieceTracked = true, IsService = false },
                    Quantity = 5,
                    TotalAmount = 500,
                    PurchaseDocTables =
                    [
                        new PurchaseDocTable { Id = 21, ProductTableId = 900, TotalAmount = 100 },
                        new PurchaseDocTable { Id = 22, ProductTableId = 901, TotalAmount = 100 }
                    ]
                }
            ]
        };

        var result = await new PurchaseInventoryHandler().HandleAsync(purchase);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.Sum(entry => entry.Quantity));
        Assert.Equal(500, result.Value.Sum(entry => entry.Amount));
        Assert.Equal(2, result.Value.Count(entry => entry.ProductTableId.HasValue));
        Assert.Contains(result.Value, entry => entry.ProductTableId is null && entry.Quantity == 3);
        Assert.All(result.Value, entry => Assert.Equal(11, entry.SourceLineId));
    }
}
