using Domain.Entities;

namespace Application.Features.SaleDocs.EdoSalePreflight;

internal static class EdoPartialMarkingRules
{
    internal static bool IsProviderMarkingSetValid(
        int requiredCount,
        IReadOnlyCollection<string> providerMarkings)
    {
        if (requiredCount < 0
            || providerMarkings.Count > requiredCount
            || providerMarkings.Any(string.IsNullOrWhiteSpace))
            return false;

        return providerMarkings.Distinct(StringComparer.Ordinal).Count() == providerMarkings.Count;
    }

    internal static bool TrySelectTables(
        int requiredCount,
        IReadOnlyCollection<ProductTable> tables,
        IReadOnlyCollection<string> providerMarkings,
        out IReadOnlyCollection<ProductTable> selectedTables)
    {
        selectedTables = [];
        if (!IsProviderMarkingSetValid(requiredCount, providerMarkings))
            return false;

        var providerSet = providerMarkings.ToHashSet(StringComparer.Ordinal);
        var markedTables = tables
            .Where(table => !string.IsNullOrWhiteSpace(table.MarkingNumber)
                && providerSet.Contains(table.MarkingNumber!))
            .ToArray();
        if (markedTables.Length != providerMarkings.Count
            || markedTables.GroupBy(table => table.MarkingNumber!, StringComparer.Ordinal)
                .Any(group => group.Count() != 1))
            return false;

        var unmarkedTables = tables
            .Where(table => string.IsNullOrWhiteSpace(table.MarkingNumber))
            .OrderBy(table => table.Id)
            .ToArray();
        var unmarkedRequiredCount = requiredCount - providerMarkings.Count;
        if (unmarkedTables.Length < unmarkedRequiredCount)
            return false;

        var selected = markedTables
            .Concat(unmarkedTables.Take(unmarkedRequiredCount))
            .OrderBy(table => table.Id)
            .ToArray();
        if (!IsValidSelection(requiredCount, selected, providerMarkings))
            return false;

        selectedTables = selected;
        return true;
    }

    internal static bool IsValidSelection(
        int requiredCount,
        IReadOnlyCollection<ProductTable> selectedTables,
        IReadOnlyCollection<string> providerMarkings)
    {
        if (!IsProviderMarkingSetValid(requiredCount, providerMarkings)
            || selectedTables.Count != requiredCount
            || selectedTables.Select(table => table.Id).Distinct().Count() != selectedTables.Count)
            return false;

        var selectedMarkedValues = selectedTables
            .Where(table => !string.IsNullOrWhiteSpace(table.MarkingNumber))
            .Select(table => table.MarkingNumber!)
            .ToArray();
        if (selectedMarkedValues.Length != providerMarkings.Count
            || selectedMarkedValues.Distinct(StringComparer.Ordinal).Count() != selectedMarkedValues.Length)
            return false;

        return selectedMarkedValues
            .OrderBy(value => value, StringComparer.Ordinal)
            .SequenceEqual(
                providerMarkings.OrderBy(value => value, StringComparer.Ordinal),
                StringComparer.Ordinal);
    }
}
