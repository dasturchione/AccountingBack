using Application.Features.SaleDocs.EdoSalePreflight;
using Domain.Entities;

public sealed class EdoPartialMarkingRulesTests
{
    [Theory]
    [InlineData("FACTURA", true, true)]
    [InlineData(" factura ", true, true)]
    [InlineData("WAYBILL_LOCAL", true, false)]
    [InlineData("FACTURA", false, false)]
    public void UnmatchedMarkingPolicyIsExplicitlyFacturaOnly(
        string documentType,
        bool allowUnmatchedMarkings,
        bool expected)
    {
        Assert.Equal(
            expected,
            EdoPartialMarkingRules.IsFacturaUnmatchedMarkingPolicyEnabled(
                documentType,
                allowUnmatchedMarkings));
    }

    [Fact]
    public void SelectsExactProviderMarkingsAndFillsRemainingQuantityWithUnmarkedTables()
    {
        var providerMarkings = Enumerable.Range(1, 10)
            .Select(index => $"provider-marking-{index}")
            .ToArray();
        var tables = providerMarkings
            .Select((marking, index) => new ProductTable
            {
                Id = 2909 + index,
                ProductId = 158,
                MarkingNumber = marking
            })
            .Concat(
            [
                new ProductTable { Id = 2919, ProductId = 158, MarkingNumber = null },
                new ProductTable { Id = 2920, ProductId = 158, MarkingNumber = " " },
                new ProductTable { Id = 2921, ProductId = 158, MarkingNumber = null }
            ])
            .ToArray();

        var selected = EdoPartialMarkingRules.TrySelectTables(
            requiredCount: 13,
            tables,
            providerMarkings,
            out var selectedTables);

        Assert.True(selected);
        Assert.Equal(13, selectedTables.Count);
        Assert.Equal(
            new[] { 2919, 2920, 2921 },
            selectedTables
                .Where(table => string.IsNullOrWhiteSpace(table.MarkingNumber))
                .Select(table => table.Id)
                .OrderBy(id => id));
        Assert.True(EdoPartialMarkingRules.IsValidSelection(13, selectedTables, providerMarkings));
    }

    [Fact]
    public void ProviderMarkingCountAboveQuantityIsRejected()
    {
        var providerMarkings = Enumerable.Range(1, 11)
            .Select(index => $"provider-marking-{index}")
            .ToArray();

        Assert.False(EdoPartialMarkingRules.IsProviderMarkingSetValid(10, providerMarkings));
        Assert.False(EdoPartialMarkingRules.TrySelectTables(
            10,
            [],
            providerMarkings,
            out var selectedTables));
        Assert.Empty(selectedTables);
    }

    [Fact]
    public void ExplicitFacturaPolicyAllowsMissingProviderMarkingsWithUnmarkedStock()
    {
        var providerMarkings = new[] { "provider-marking-1", "provider-marking-2", "provider-marking-3" };
        var tables = new[]
        {
            new ProductTable { Id = 3101, ProductId = 158, MarkingNumber = "provider-marking-1" },
            new ProductTable { Id = 3102, ProductId = 158, MarkingNumber = null },
            new ProductTable { Id = 3103, ProductId = 158, MarkingNumber = " " },
            new ProductTable { Id = 3104, ProductId = 158, MarkingNumber = null }
        };

        Assert.False(EdoPartialMarkingRules.IsValidSelection(4, tables, providerMarkings));
        Assert.True(EdoPartialMarkingRules.TrySelectFacturaTablesAllowingUnmatchedMarkings(
            4,
            tables,
            providerMarkings,
            out var selectedTables));
        Assert.Equal(4, selectedTables.Count);
        Assert.True(EdoPartialMarkingRules.IsFacturaUnmatchedSelectionValid(
            4,
            selectedTables,
            providerMarkings));
    }

    [Fact]
    public void ExplicitFacturaPolicyAllowsZeroProviderMarkingsWithUnmarkedStock()
    {
        var tables = new[]
        {
            new ProductTable { Id = 3201, ProductId = 158, MarkingNumber = null },
            new ProductTable { Id = 3202, ProductId = 158, MarkingNumber = " " }
        };

        Assert.True(EdoPartialMarkingRules.TrySelectFacturaTablesAllowingUnmatchedMarkings(
            2,
            tables,
            [],
            out var selectedTables));
        Assert.All(selectedTables, table => Assert.True(string.IsNullOrWhiteSpace(table.MarkingNumber)));
    }

    [Fact]
    public void ExplicitFacturaPolicyBlocksWhenUnmarkedStockIsInsufficient()
    {
        var tables = new[]
        {
            new ProductTable { Id = 3301, ProductId = 158, MarkingNumber = "provider-marking-1" },
            new ProductTable { Id = 3302, ProductId = 158, MarkingNumber = null }
        };

        Assert.False(EdoPartialMarkingRules.TrySelectFacturaTablesAllowingUnmatchedMarkings(
            3,
            tables,
            ["provider-marking-1", "provider-marking-2"],
            out var selectedTables));
        Assert.Empty(selectedTables);
    }

    [Fact]
    public void ExplicitFacturaPolicyStillRejectsProviderMarkingsAboveQuantity()
    {
        Assert.False(EdoPartialMarkingRules.TrySelectFacturaTablesAllowingUnmatchedMarkings(
            1,
            [],
            ["provider-marking-1", "provider-marking-2"],
            out var selectedTables));
        Assert.Empty(selectedTables);
    }

    [Fact]
    public void WrongMarkedOrUnmarkedSelectionIsRejected()
    {
        var providerMarkings = new[] { "provider-marking-1", "provider-marking-2" };
        var wrongSelection = new[]
        {
            new ProductTable { Id = 3001, ProductId = 158, MarkingNumber = "wrong-marking" },
            new ProductTable { Id = 3002, ProductId = 158, MarkingNumber = null },
            new ProductTable { Id = 3003, ProductId = 158, MarkingNumber = "provider-marking-1" }
        };

        Assert.False(EdoPartialMarkingRules.IsValidSelection(3, wrongSelection, providerMarkings));

        var wrongUnmarkedSelection = new[]
        {
            new ProductTable { Id = 3004, ProductId = 158, MarkingNumber = "provider-marking-1" },
            new ProductTable { Id = 3005, ProductId = 158, MarkingNumber = "provider-marking-2" },
            new ProductTable { Id = 3006, ProductId = 158, MarkingNumber = "unexpected-marking" }
        };

        Assert.False(EdoPartialMarkingRules.IsValidSelection(3, wrongUnmarkedSelection, providerMarkings));
    }

    [Fact]
    public void DuplicateSelectedProductTablesRemainInvalid()
    {
        var providerMarkings = new[] { "provider-marking-1" };
        var duplicateSelection = new[]
        {
            new ProductTable { Id = 3010, ProductId = 158, MarkingNumber = "provider-marking-1" },
            new ProductTable { Id = 3010, ProductId = 158, MarkingNumber = null }
        };

        Assert.False(EdoPartialMarkingRules.IsValidSelection(2, duplicateSelection, providerMarkings));
    }
}
