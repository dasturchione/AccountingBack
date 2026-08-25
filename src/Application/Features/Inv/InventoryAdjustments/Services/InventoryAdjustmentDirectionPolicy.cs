using SharedKernel.Constants;

namespace Application.Features.InventoryAdjustments;

internal static class InventoryAdjustmentDirectionPolicy
{
    public static bool IsCompatible(string adjustmentType, short directionId) =>
        adjustmentType.Trim().ToUpperInvariant() switch
        {
            "POSITIVE_ADJUSTMENT" or "FOUND_STOCK" => directionId == MovementDirectionIdConst.IN,
            "NEGATIVE_ADJUSTMENT" or "WRITE_OFF" or "DAMAGE" or "LOSS" => directionId == MovementDirectionIdConst.OUT,
            "CORRECTION" => MovementDirectionIdConst.IsValid(directionId),
            _ => false
        };
}
