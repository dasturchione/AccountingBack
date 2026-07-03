using SharedKernel.Results;

namespace Application.Features.InventoryCounts;

public static class InventoryCountErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("InventoryCount.NotFound", $"Inventory count with id {id} was not found.");

    public static Error AlreadyCounted(long id, short? languageId = null) =>
        Error.Conflict("InventoryCount.AlreadyCounted", $"Inventory count with id {id} is already counted.");

    public static Error AlreadyCancelled(long id, short? languageId = null) =>
        Error.Conflict("InventoryCount.AlreadyCancelled", $"Inventory count with id {id} is already cancelled.");

    public static Error CannotConfirmInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("InventoryCount.CannotConfirmInCurrentStatus", $"Inventory count with id {id} cannot be confirmed in status {statusId}.");

    public static Error CannotUpdateInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("InventoryCount.CannotUpdateInCurrentStatus", $"Inventory count with id {id} cannot be updated in status {statusId}.");

    public static Error CannotCancelInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("InventoryCount.CannotCancelInCurrentStatus", $"Inventory count with id {id} cannot be cancelled in status {statusId}.");

    public static Error CannotDeleteInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("InventoryCount.CannotDeleteInCurrentStatus", $"Inventory count with id {id} cannot be deleted in status {statusId}.");

    public static Error OrganizationNotFound(int organizationId, short? languageId = null) =>
        Error.NotFound("InventoryCount.OrganizationNotFound", $"Organization with id {organizationId} was not found.");

    public static Error WarehouseNotFound(int warehouseId, short? languageId = null) =>
        Error.NotFound("InventoryCount.WarehouseNotFound", $"Warehouse with id {warehouseId} was not found.");

    public static Error WarehouseInactive(int warehouseId, short? languageId = null) =>
        Error.Business("InventoryCount.WarehouseInactive", $"Warehouse {warehouseId} is not active.");

    public static Error WarehouseOrganizationMismatch(int warehouseId, int organizationId, short? languageId = null) =>
        Error.Business("InventoryCount.WarehouseOrganizationMismatch", $"Warehouse {warehouseId} does not belong to organization {organizationId}.");

    public static Error LinesRequired(short? languageId = null) =>
        Error.Business("InventoryCount.LinesRequired", "Inventory count must contain at least one line.");

    public static Error SimultaneousCountExists(int warehouseId, short? languageId = null) =>
        Error.Conflict("InventoryCount.SimultaneousCountExists", $"Another active inventory count already exists for warehouse {warehouseId}.");

    public static Error ProductNotFound(int productId, short? languageId = null) =>
        Error.NotFound("InventoryCount.ProductNotFound", $"Product with id {productId} was not found.");

    public static Error ProductServiceNotAllowed(int productId, short? languageId = null) =>
        Error.Business("InventoryCount.ProductServiceNotAllowed", $"Service product {productId} cannot be used in inventory count.");

    public static Error UnitNotFound(short unitId, short? languageId = null) =>
        Error.NotFound("InventoryCount.UnitNotFound", $"Unit with id {unitId} was not found.");

    public static Error InvalidQuantity(int productId, decimal quantity, short? languageId = null) =>
        Error.Business("InventoryCount.InvalidQuantity", $"Product {productId} has invalid counted quantity {quantity}.");

    public static Error InvalidItemCount(int productId, decimal quantity, int itemCount, short? languageId = null) =>
        Error.Business("InventoryCount.InvalidItemCount", $"Product {productId} counted quantity {quantity} cannot be lower than item count {itemCount}.");

    public static Error DuplicateLine(int productId, short unitId, short? languageId = null) =>
        Error.Conflict("InventoryCount.DuplicateLine", $"Duplicate inventory count line for product {productId} and unit {unitId}.");

    public static Error DuplicateProductTable(int productTableId, short? languageId = null) =>
        Error.Conflict("InventoryCount.DuplicateProductTable", $"Product table {productTableId} is duplicated in the count.");

    public static Error DuplicateBarcode(string barcode, short? languageId = null) =>
        Error.Conflict("InventoryCount.DuplicateBarcode", $"Barcode '{barcode}' is duplicated in the count.");

    public static Error DuplicateSerial(string serialNumber, short? languageId = null) =>
        Error.Conflict("InventoryCount.DuplicateSerial", $"Serial number '{serialNumber}' is duplicated in the count.");

    public static Error DuplicateMarking(string markingNumber, short? languageId = null) =>
        Error.Conflict("InventoryCount.DuplicateMarking", $"Marking number '{markingNumber}' is duplicated in the count.");

    public static Error ProductTableNotFound(int productTableId, short? languageId = null) =>
        Error.NotFound("InventoryCount.ProductTableNotFound", $"Product table with id {productTableId} was not found.");

    public static Error ProductTableProductMismatch(int productTableId, int productId, short? languageId = null) =>
        Error.Business("InventoryCount.ProductTableProductMismatch", $"Product table {productTableId} does not belong to product {productId}.");

    public static Error ProductTableWarehouseMismatch(int productTableId, int warehouseId, short? languageId = null) =>
        Error.Business("InventoryCount.ProductTableWarehouseMismatch", $"Product table {productTableId} is not located in warehouse {warehouseId}.");

    public static Error ProductTableInactive(int productTableId, short? languageId = null) =>
        Error.Business("InventoryCount.ProductTableInactive", $"Product table {productTableId} is not active.");

    public static Error ProductTableUnavailable(int productTableId, short statusId, short? languageId = null) =>
        Error.Business("InventoryCount.ProductTableUnavailable", $"Product table {productTableId} is not available for counting in status {statusId}.");

    public static Error MissingPostingBatch(long id, short? languageId = null) =>
        Error.Conflict("InventoryCount.MissingPostingBatch", $"Posting batch was not found for inventory count with id {id}.");

    public static Error BusinessEffectsAlreadyExist(long id, short? languageId = null) =>
        Error.Conflict("InventoryCount.BusinessEffectsAlreadyExist", $"Inventory count with id {id} already has business effects.");

    public static Error CountNotCompleted(long id, short? languageId = null) =>
        Error.Business("InventoryCount.CountNotCompleted", $"Inventory count with id {id} must be completed before confirm.");
}
