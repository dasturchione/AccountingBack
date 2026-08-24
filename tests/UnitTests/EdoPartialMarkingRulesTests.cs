using Application.Features.SaleDocs.EdoSalePreflight;
using Domain.Entities;

public sealed class EdoPartialMarkingRulesTests
{
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
