using Domain.Entities;

namespace Application.Features.Pay.Components;

/// <summary>
/// Resolves component formulas deterministically and applies explicit 1C-style
/// minimum/maximum limits. Dependencies are component ids from the same snapshot.
/// </summary>
public static class PayrollComponentFormulaPolicy
{
    public static IReadOnlyList<PayComponent> Order(IEnumerable<PayComponent> components)
    {
        var source = components.ToList();
        var byId = source.ToDictionary(x => x.Id);
        var ordered = new List<PayComponent>(source.Count);
        var visited = new HashSet<int>();
        var visiting = new HashSet<int>();

        foreach (var component in source
                     .OrderBy(x => x.SortOrder)
                     .ThenBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(x => x.Id))
            Visit(component);

        return ordered;

        void Visit(PayComponent component)
        {
            if (visited.Contains(component.Id))
                return;
            if (!visiting.Add(component.Id))
                throw new InvalidOperationException($"Payroll component dependency cycle detected at '{component.Code}'.");

            if (component.DependsOnComponentId is { } dependencyId)
            {
                if (!byId.TryGetValue(dependencyId, out var dependency))
                    throw new InvalidOperationException($"Payroll component '{component.Code}' depends on missing component '{dependencyId}'.");
                Visit(dependency);
            }

            visiting.Remove(component.Id);
            visited.Add(component.Id);
            ordered.Add(component);
        }
    }

    public static decimal ApplyCaps(decimal amount, decimal? minimumAmount, decimal? maximumAmount)
    {
        if (minimumAmount.HasValue && maximumAmount.HasValue && minimumAmount > maximumAmount)
            throw new InvalidOperationException("Payroll component minimum amount cannot exceed maximum amount.");

        var result = amount;
        if (minimumAmount.HasValue)
            result = Math.Max(result, minimumAmount.Value);
        if (maximumAmount.HasValue)
            result = Math.Min(result, maximumAmount.Value);
        return Math.Round(result, 2, MidpointRounding.AwayFromZero);
    }
}
