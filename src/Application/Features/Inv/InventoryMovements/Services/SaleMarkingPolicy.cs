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

    public static bool IsAvailableForSale(short statusId) =>
        statusId == SharedKernel.Constants.ProductTableStatusIdConst.IN_STOCK;

    public static bool IsRestorableAfterSale(short statusId) =>
        statusId == SharedKernel.Constants.ProductTableStatusIdConst.SOLD;
}
