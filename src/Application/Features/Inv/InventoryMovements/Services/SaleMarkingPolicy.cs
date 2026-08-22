namespace Application.Features.InventoryMovements;

public static class SaleMarkingPolicy
{
    public static bool IsOccurrenceCountAllowed(decimal quantity, int occurrenceCount) =>
        quantity >= 0m && occurrenceCount >= 0 && occurrenceCount <= quantity;

    public static IReadOnlyList<int> GetDistinctPhysicalMarkingIds(
        IEnumerable<IEnumerable<int>> markingOccurrences) =>
        markingOccurrences
            .SelectMany(ids => ids)
            .Distinct()
            .OrderBy(id => id)
            .ToArray();

    public static IReadOnlyList<SaleMarkingBatchSelection> SelectBatchMarkings(
        IEnumerable<SaleMarkingOccurrence> markingOccurrences,
        IEnumerable<ProductTableMarkingIdentity> productTables)
    {
        var productTablesById = productTables
            .GroupBy(table => table.ProductTableId)
            .ToDictionary(group => group.Key, group => group.First());

        return markingOccurrences
            .Where(occurrence =>
                productTablesById.TryGetValue(occurrence.ProductTableId, out var table) &&
                !string.IsNullOrWhiteSpace(table.MarkingNumber) &&
                table.ProductId == occurrence.LineProductId)
            .Select(occurrence =>
            {
                var table = productTablesById[occurrence.ProductTableId];
                return new SaleMarkingBatchSelection(
                    occurrence.LineId,
                    table.ProductId,
                    table.ProductTableId,
                    table.MarkingNumber!);
            })
            .GroupBy(selection => selection.MarkingNumber, StringComparer.Ordinal)
            .Select(group => group
                .OrderBy(selection => selection.LineId)
                .ThenBy(selection => selection.ProductTableId)
                .First())
            .OrderBy(selection => selection.LineId)
            .ThenBy(selection => selection.ProductTableId)
            .ToArray();
    }

    public static bool IsAvailableForSale(short statusId) =>
        statusId == SharedKernel.Constants.ProductTableStatusIdConst.IN_STOCK;

    public static bool IsRestorableAfterSale(short statusId) =>
        statusId == SharedKernel.Constants.ProductTableStatusIdConst.SOLD;
}

public sealed record SaleMarkingOccurrence(long LineId, int LineProductId, int ProductTableId);

public sealed record ProductTableMarkingIdentity(int ProductTableId, int ProductId, string? MarkingNumber);

public sealed record SaleMarkingBatchSelection(
    long LineId,
    int ProductId,
    int ProductTableId,
    string MarkingNumber);
