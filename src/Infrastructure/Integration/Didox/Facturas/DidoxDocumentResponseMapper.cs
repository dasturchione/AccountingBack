using System.Text.Json;

namespace Integration.Didox.Facturas;

public static class DidoxDocumentResponseMapper
{
    private static readonly string[] MarkCodePropertyNames =
        ["mark_codes", "markCodes", "kiz", "identtransupak", "nomupak"];
    private static readonly string[] MarkContainerPropertyNames = ["marks", "Marks"];
    private static readonly string[] ProductListPropertyNames = ["productlist", "ProductList", "productList"];

    public static IReadOnlyCollection<string> ReadMarkingCodes(JsonElement root)
    {
        var values = new List<string>();
        Collect(root, values, 0);
        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static void Collect(JsonElement element, ICollection<string> values, int depth)
    {
        if (depth > 8)
            return;

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
                Collect(item, values, depth + 1);
            return;
        }

        if (element.ValueKind != JsonValueKind.Object)
            return;

        foreach (var property in element.EnumerateObject())
        {
            if (MarkCodePropertyNames.Contains(property.Name, StringComparer.OrdinalIgnoreCase))
            {
                CollectScalarValues(property.Value, values, depth + 1);
                continue;
            }

            if (MarkContainerPropertyNames.Contains(property.Name, StringComparer.OrdinalIgnoreCase)
                || ProductListPropertyNames.Contains(property.Name, StringComparer.OrdinalIgnoreCase)
                || string.Equals(property.Name, "products", StringComparison.OrdinalIgnoreCase))
            {
                Collect(property.Value, values, depth + 1);
            }
        }
    }

    private static void CollectScalarValues(JsonElement element, ICollection<string> values, int depth)
    {
        if (element.ValueKind == JsonValueKind.String)
        {
            var value = element.GetString();
            if (!string.IsNullOrWhiteSpace(value))
                values.Add(value);
            return;
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
                CollectScalarValues(item, values, depth + 1);
            return;
        }

        if (element.ValueKind == JsonValueKind.Object && depth <= 8)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, "kiz", StringComparison.OrdinalIgnoreCase)
                    || MarkCodePropertyNames.Contains(property.Name, StringComparer.OrdinalIgnoreCase))
                {
                    CollectScalarValues(property.Value, values, depth + 1);
                }
            }
        }
    }
}
