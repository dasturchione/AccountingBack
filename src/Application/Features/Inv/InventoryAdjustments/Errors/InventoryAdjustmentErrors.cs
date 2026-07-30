using SharedKernel.Results;

namespace Application.Features.InventoryAdjustments;

public static class InventoryAdjustmentErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("InventoryAdjustment.NotFound", $"Inventory adjustment with id {id} was not found.");

    public static Error AlreadyCancelled(long id, short? languageId = null) =>
        Error.Conflict("InventoryAdjustment.AlreadyCancelled", $"Inventory adjustment with id {id} is already cancelled.");

    public static Error CannotConfirmInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("InventoryAdjustment.CannotConfirmInCurrentStatus", $"Inventory adjustment with id {id} cannot be confirmed in status {statusId}.");

    public static Error CannotUpdateInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("InventoryAdjustment.CannotUpdateInCurrentStatus", $"Inventory adjustment with id {id} cannot be updated in status {statusId}.");

    public static Error CannotCancelInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("InventoryAdjustment.CannotCancelInCurrentStatus", $"Inventory adjustment with id {id} cannot be cancelled in status {statusId}.");

    public static Error CannotDeleteInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("InventoryAdjustment.CannotDeleteInCurrentStatus", $"Inventory adjustment with id {id} cannot be deleted in status {statusId}.");

    public static Error LinesRequired(long id, short? languageId = null) =>
        Error.Business("InventoryAdjustment.LinesRequired", $"Inventory adjustment with id {id} must contain at least one line.");

    public static Error OrganizationNotFound(int organizationId, short? languageId = null) =>
        Error.NotFound("InventoryAdjustment.OrganizationNotFound", $"Organization with id {organizationId} was not found.");

    public static Error WarehouseNotFound(int warehouseId, short? languageId = null) =>
        Error.NotFound("InventoryAdjustment.WarehouseNotFound", $"Warehouse with id {warehouseId} was not found.");

    public static Error WarehouseOrganizationMismatch(int warehouseId, int organizationId, short? languageId = null) =>
        Error.Business("InventoryAdjustment.WarehouseOrganizationMismatch", $"Warehouse {warehouseId} does not belong to organization {organizationId}.");

    public static Error WarehouseInactive(int warehouseId, short? languageId = null) =>
        Error.Business("InventoryAdjustment.WarehouseInactive", $"Warehouse {warehouseId} is not active.");

    public static Error ProductNotFound(int productId, short? languageId = null) =>
        Error.NotFound("InventoryAdjustment.ProductNotFound", $"Product with id {productId} was not found.");

    public static Error ProductServiceNotAllowed(int productId, short? languageId = null) =>
        Error.Business("InventoryAdjustment.ProductServiceNotAllowed", $"Service product {productId} cannot be used in inventory adjustment.");

    public static Error UnitNotFound(short unitId, short? languageId = null) =>
        Error.NotFound("InventoryAdjustment.UnitNotFound", $"Unit with id {unitId} was not found.");

    public static Error InvalidQuantity(int productId, decimal quantity, short? languageId = null) =>
        Error.Business("InventoryAdjustment.InvalidQuantity", $"Product {productId} has invalid quantity {quantity}.");

    public static Error InvalidAdjustmentType(string adjustmentType, short? languageId = null) =>
        Error.Business("InventoryAdjustment.InvalidAdjustmentType", $"Adjustment type '{adjustmentType}' is not supported.");

    public static Error BusinessEffectsAlreadyExist(long id, short? languageId = null) =>
        Error.Conflict("InventoryAdjustment.BusinessEffectsAlreadyExist", $"Inventory adjustment with id {id} already has inventory movements.");

    public static Error MissingPostingBatch(long id, short? languageId = null) =>
        Error.Conflict("InventoryAdjustment.MissingPostingBatch", $"Posting batch was not found for inventory adjustment with id {id}.");

    public static Error ItemsRequired(int productId, short? languageId = null) =>
        Error.Business("InventoryAdjustment.ItemsRequired", $"Product {productId} must contain at least one product table item.");

    public static Error QuantityItemsMismatch(int productId, decimal quantity, int itemCount, short? languageId = null) =>
        Error.Business("InventoryAdjustment.QuantityItemsMismatch", $"Product {productId} quantity {quantity} does not match item count {itemCount}.");

    public static Error DuplicateLine(int productId, short unitId, short? languageId = null) =>
        Error.Conflict("InventoryAdjustment.DuplicateLine", $"Duplicate inventory adjustment line for product {productId} and unit {unitId}.");

    public static Error DuplicateProductTable(int productTableId, short? languageId = null) =>
        Error.Conflict("InventoryAdjustment.DuplicateProductTable", $"Product table {productTableId} is duplicated in the adjustment.");

    public static Error ProductTableNotFound(int productTableId, short? languageId = null) =>
        Error.NotFound("InventoryAdjustment.ProductTableNotFound", $"Product table with id {productTableId} was not found.");

    public static Error ProductTableProductMismatch(int productTableId, int productId, short? languageId = null) =>
        Error.Business("InventoryAdjustment.ProductTableProductMismatch", $"Product table {productTableId} does not belong to product {productId}.");

    public static Error ProductTableInactive(int productTableId, short? languageId = null) =>
        Error.Business("InventoryAdjustment.ProductTableInactive", $"Product table {productTableId} is not active.");

    public static Error ProductTableUnavailable(int productTableId, short statusId, short? languageId = null) =>
        Error.Business("InventoryAdjustment.ProductTableUnavailable", $"Product table {productTableId} is not available for adjustment in status {statusId}.");

    public static Error ProductTableWarehouseMismatch(int productTableId, int warehouseId, short? languageId = null) =>
        Error.Business("InventoryAdjustment.ProductTableWarehouseMismatch", $"Product table {productTableId} is not located in warehouse {warehouseId}.");
}
