using Domain.Entities;
using SharedKernel.Constants;

namespace Application.Features.SaleDocs.EdoSalePreflight;

public sealed record EdoSaleProductEligibilityResult(
    Product? Product,
    string Status,
    string? SafeErrorCode);

public static class EdoSaleProductEligibility
{
    public static EdoSaleProductEligibilityResult Resolve(
        int organizationId,
        string? providerProductCode,
        decimal requiredQuantity,
        IReadOnlyCollection<Product> products,
        IReadOnlyDictionary<int, decimal> availableQuantityByProductId,
        bool inventoryAvailable)
    {
        var activeMatches = products
            .Where(x => x.OrganizationId == organizationId
                && x.StateId == StateIdConst.ACTIVE
                && string.Equals(x.Mxik, providerProductCode, StringComparison.Ordinal))
            .ToArray();
        var soldMatches = activeMatches.Where(x => x.IsSold).ToArray();

        if (soldMatches.Length == 0)
        {
            return new EdoSaleProductEligibilityResult(
                null,
                "BLOCKED",
                activeMatches.Length > 0 ? "SALE_PRODUCT_NOT_ENABLED" : "PRODUCT_MAPPING_REQUIRED");
        }

        if (soldMatches.Length != 1)
            return new EdoSaleProductEligibilityResult(null, "REQUIRES_SELECTION", "PRODUCT_MAPPING_AMBIGUOUS");

        var selected = soldMatches[0];
        if (!selected.IsService
            && (!inventoryAvailable
                || !availableQuantityByProductId.TryGetValue(selected.Id, out var availableQuantity)
                || availableQuantity < requiredQuantity))
        {
            return new EdoSaleProductEligibilityResult(null, "BLOCKED", "SALE_PRODUCT_STOCK_MAPPING_REQUIRED");
        }

        return new EdoSaleProductEligibilityResult(selected, "READY", null);
    }
}
