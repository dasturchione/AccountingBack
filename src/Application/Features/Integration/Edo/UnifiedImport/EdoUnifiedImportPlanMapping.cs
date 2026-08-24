using Application.Features.SaleDocs.EdoSalePreflight;

namespace Application.Features.Integration.Edo.UnifiedImport;

public static class EdoUnifiedImportPlanMapping
{
    private static readonly HashSet<string> RecomputedCodes = new(StringComparer.Ordinal)
    {
        "SALE_SOURCE_LINK_REQUIRED",
        "SALE_COST_PRICE_SOURCE_REQUIRED",
        "MARKING_CODES_NOT_AVAILABLE_FOR_SAFE_PLAN",
        "MARKING_MAPPING_REQUIRED",
        "MARKING_PRODUCT_MAPPING_REQUIRED",
        "PRODUCT_TABLE_SELECTION_REQUIRED",
        "PRODUCT_TABLE_REQUIRES_PRODUCT_SELECTION",
        "EDO_UNIFIED_EXPLICIT_MAPPING_REQUIRED"
    };

    public static EdoUnifiedImportPlanMappingSnapshot FromSaleCandidate(
        EdoSalePreflightCandidateDto candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);

        return new EdoUnifiedImportPlanMappingSnapshot
        {
            CounterpartyId = ReadyId<int>(candidate.Counterparty),
            ContractId = ReadyId<long>(candidate.Contract),
            ContractCandidateIds = candidate.Contract.CandidateIds.OrderBy(x => x).ToArray(),
            CurrencyId = ReadyId<short>(candidate.Currency),
            WarehouseId = ReadyId<int>(candidate.Warehouse),
            CounterpartyMappingStatus = candidate.Counterparty.Status,
            ContractMappingStatus = candidate.Contract.Status,
            CurrencyMappingStatus = candidate.Currency.Status,
            WarehouseMappingStatus = candidate.Warehouse.Status,
            ProductMappingStatus = Aggregate(candidate.Lines.Select(x => x.Product.Status)),
            VatMappingStatus = Aggregate(candidate.Lines.Select(x => x.Vat.Status)),
            CostPriceStatus = Aggregate(candidate.Lines.Select(x => x.CostPrice.Status)),
            ProductTableMappingStatus = Aggregate(candidate.Lines.Select(x => x.ProductTable.Status)),
            MarkingMappingStatus = candidate.Marking.Status,
            Lines = candidate.Lines
                .OrderBy(x => x.Number)
                .Select(x => new EdoUnifiedImportPlanLineDto
                {
                    LineNumber = x.Number,
                    ProviderProductCode = x.ProviderProductCode,
                    ProviderProductName = x.ProviderProductName,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,
                    CostPrice = null,
                    CostPriceSource = null,
                    ProductId = ReadyId<int>(x.Product),
                    UnitId = ReadyId<short>(x.Unit),
                    VatRateId = ReadyId<short>(x.Vat),
                    ProductMappingStatus = x.Product.Status,
                    ProductTableMappingStatus = x.ProductTable.Status,
                    VatMappingStatus = x.Vat.Status,
                    CostPriceStatus = x.CostPrice.Status,
                    MarkingRequired = x.Marking.MarkingRequired,
                    MarkingSource = x.Marking.MarkingRequired ? "PROVIDER_SNAPSHOT" : "NONE"
                })
                .ToArray()
        };
    }

    public static bool IsReady(EdoUnifiedImportPlanMappingSnapshot mapping)
    {
        ArgumentNullException.ThrowIfNull(mapping);

        return mapping.CounterpartyMappingStatus == "READY"
            && mapping.ContractMappingStatus == "READY"
            && mapping.CurrencyMappingStatus == "READY"
            && mapping.WarehouseMappingStatus == "READY"
            && mapping.ProductMappingStatus == "READY"
            && mapping.VatMappingStatus == "READY"
            && mapping.CostPriceStatus == "READY"
            && (mapping.ProductTableMappingStatus == "READY"
                || mapping.ProductTableMappingStatus == "NOT_APPLICABLE")
            && (mapping.MarkingMappingStatus == "READY"
                || mapping.MarkingMappingStatus == "NOT_REQUIRED")
            && mapping.Lines.All(line =>
                line.ProductMappingStatus == "READY"
                && line.VatMappingStatus == "READY"
                && line.CostPriceStatus == "READY"
                && (line.ProductTableMappingStatus == "READY"
                    || line.ProductTableMappingStatus == "NOT_APPLICABLE"));
    }

    public static IReadOnlyCollection<string> BuildCurrentSafeErrorCodes(
        EdoSalePreflightCandidateDto candidate,
        EdoUnifiedImportPlanMappingSnapshot mapping)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(mapping);

        var codes = candidate.SafeErrorCodes
            .Where(code => !RecomputedCodes.Contains(code))
            .ToHashSet(StringComparer.Ordinal);

        if (mapping.CostPriceStatus != "READY")
            codes.Add("SALE_COST_PRICE_SOURCE_REQUIRED");

        var hasMarking = mapping.Lines.Any(line => line.MarkingRequired);
        if (hasMarking
            && mapping.MarkingMappingStatus != "READY"
            && mapping.MarkingMappingStatus != "NOT_REQUIRED")
            codes.Add("MARKING_MAPPING_REQUIRED");

        if (mapping.ProductTableMappingStatus != "READY"
            && mapping.ProductTableMappingStatus != "NOT_APPLICABLE")
            codes.Add("PRODUCT_TABLE_SELECTION_REQUIRED");

        if (!IsReady(mapping) && codes.Count == 0)
            codes.Add("EDO_UNIFIED_EXPLICIT_MAPPING_REQUIRED");

        return codes.OrderBy(code => code, StringComparer.Ordinal).ToArray();
    }

    private static T? ReadyId<T>(EdoSaleMappingStatusDto mapping)
        where T : struct
    {
        if (!string.Equals(mapping.Status, "READY", StringComparison.Ordinal)
            || !mapping.SelectedId.HasValue)
            return null;

        try
        {
            return (T)Convert.ChangeType(mapping.SelectedId.Value, typeof(T));
        }
        catch (InvalidCastException)
        {
            return null;
        }
        catch (OverflowException)
        {
            return null;
        }
    }

    private static string Aggregate(IEnumerable<string> statuses)
    {
        var values = statuses.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        if (values.Length == 0)
            return "BLOCKED";
        if (values.All(x => string.Equals(x, "READY", StringComparison.Ordinal)))
            return "READY";
        if (values.Any(x => string.Equals(x, "BLOCKED", StringComparison.Ordinal)))
            return "BLOCKED";
        return "REQUIRES_SELECTION";
    }
}

public sealed class EdoUnifiedImportPlanMappingSnapshot
{
    public int? CounterpartyId { get; init; }
    public long? ContractId { get; init; }
    public IReadOnlyCollection<long> ContractCandidateIds { get; init; } = [];
    public short? CurrencyId { get; init; }
    public int? WarehouseId { get; init; }
    public string CounterpartyMappingStatus { get; init; } = "BLOCKED";
    public string ContractMappingStatus { get; init; } = "BLOCKED";
    public string CurrencyMappingStatus { get; init; } = "BLOCKED";
    public string WarehouseMappingStatus { get; init; } = "BLOCKED";
    public string ProductMappingStatus { get; init; } = "BLOCKED";
    public string VatMappingStatus { get; init; } = "BLOCKED";
    public string CostPriceStatus { get; init; } = "REQUIRES_SELECTION";
    public string ProductTableMappingStatus { get; init; } = "BLOCKED";
    public string MarkingMappingStatus { get; init; } = "NOT_REQUIRED";
    public IReadOnlyCollection<string> SafeErrorCodes { get; init; } = [];
    public IReadOnlyCollection<EdoUnifiedImportPlanLineDto> Lines { get; init; } = [];
}
