using Application.Abstractions.Integration.Edo;

namespace Application.Features.Integration.Edo.UnifiedImport;

public static class EdoUnifiedImportPlanRules
{
    public const string Factura = "FACTURA";
    public const string WaybillLocal = "WAYBILL_LOCAL";

    public static bool IsFactura(string? documentType) =>
        NormalizeDocumentType(documentType) == Factura;

    public static bool IsWaybillLocal(string? documentType) =>
        NormalizeDocumentType(documentType) == WaybillLocal;

    public static bool IsSupportedSaleDocumentType(string? documentType) =>
        IsFactura(documentType) || IsWaybillLocal(documentType);

    public static string NormalizeDocumentType(string? documentType)
    {
        var value = documentType?.Trim();
        return string.Equals(value, "waybillLocal", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "WAYBILL_LOCAL", StringComparison.OrdinalIgnoreCase)
            ? WaybillLocal
            : string.Equals(value, Factura, StringComparison.OrdinalIgnoreCase)
                ? Factura
                : value?.ToUpperInvariant() ?? string.Empty;
    }

    public static int GetMarkingCount(EdoDocumentDto document) =>
        document.PreviewLines.Count > 0
            ? document.PreviewLines.Sum(line => line.MarkingCount.GetValueOrDefault() > 0
                ? line.MarkingCount.Value
                : line.MarkingCodes.Count)
            : document.MarkingCount;

    public static bool HasMarking(EdoDocumentDto document) => GetMarkingCount(document) > 0;

    public static HashSet<string>? NormalizeSelection(IReadOnlyCollection<string>? providerDocumentIds)
    {
        if (providerDocumentIds is null || providerDocumentIds.Count == 0)
            return null;

        var values = providerDocumentIds
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        return values.Length == 0 ? null : values.ToHashSet(StringComparer.Ordinal);
    }
}
