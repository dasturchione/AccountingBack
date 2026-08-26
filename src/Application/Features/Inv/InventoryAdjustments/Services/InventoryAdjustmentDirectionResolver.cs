using Domain.Entities;
using SharedKernel.Constants;

namespace Application.Features.InventoryAdjustments;

internal static class InventoryAdjustmentDirectionResolver
{
    public static string? GetRequiredDirectionCode(string adjustmentType) =>
        adjustmentType.Trim().ToUpperInvariant() switch
        {
            "POSITIVE_ADJUSTMENT" or "FOUND_STOCK" or "CORRECTION" => "IN",
            "NEGATIVE_ADJUSTMENT" or "WRITE_OFF" or "DAMAGE" or "LOSS" => "OUT",
            _ => null
        };

    public static bool TryResolve(
        string adjustmentType,
        MovementDirection? lookup,
        out short directionId)
    {
        directionId = default;

        var requiredDirectionCode = GetRequiredDirectionCode(adjustmentType);
        if (lookup is null || requiredDirectionCode is null)
            return false;

        if (!string.Equals(lookup.Code, requiredDirectionCode, StringComparison.Ordinal))
            return false;

        directionId = lookup.Id;
        return true;
    }
}
