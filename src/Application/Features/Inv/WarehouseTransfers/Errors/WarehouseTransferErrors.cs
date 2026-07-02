using SharedKernel.Results;

namespace Application.Features.WarehouseTransfers;

public static class WarehouseTransferErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("WarehouseTransfer.NotFound", $"Warehouse transfer with id {id} was not found.");

    public static Error AlreadyCancelled(long id, short? languageId = null) =>
        Error.Conflict("WarehouseTransfer.AlreadyCancelled", $"Warehouse transfer with id {id} is already cancelled.");

    public static Error CannotConfirmInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("WarehouseTransfer.CannotConfirmInCurrentStatus", $"Warehouse transfer with id {id} cannot be confirmed in status {statusId}.");

    public static Error CannotUpdateInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("WarehouseTransfer.CannotUpdateInCurrentStatus", $"Warehouse transfer with id {id} cannot be updated in status {statusId}.");

    public static Error CannotCancelInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("WarehouseTransfer.CannotCancelInCurrentStatus", $"Warehouse transfer with id {id} cannot be cancelled in status {statusId}.");

    public static Error CannotDeleteInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("WarehouseTransfer.CannotDeleteInCurrentStatus", $"Warehouse transfer with id {id} cannot be deleted in status {statusId}.");

    public static Error LinesRequired(long id, short? languageId = null) =>
        Error.Business("WarehouseTransfer.LinesRequired", $"Warehouse transfer with id {id} must contain at least one line.");

    public static Error BusinessEffectsAlreadyExist(long id, short? languageId = null) =>
        Error.Conflict("WarehouseTransfer.BusinessEffectsAlreadyExist", $"Warehouse transfer with id {id} already has inventory movements.");

    public static Error MissingPostingBatch(long id, short? languageId = null) =>
        Error.Conflict("WarehouseTransfer.MissingPostingBatch", $"Posting batch was not found for warehouse transfer with id {id}.");

    public static Error OrganizationNotFound(int organizationId, short? languageId = null) =>
        Error.NotFound("WarehouseTransfer.OrganizationNotFound", $"Organization with id {organizationId} was not found.");

    public static Error SourceWarehouseNotFound(int warehouseId, short? languageId = null) =>
        Error.NotFound("WarehouseTransfer.SourceWarehouseNotFound", $"Source warehouse with id {warehouseId} was not found.");

    public static Error DestinationWarehouseNotFound(int warehouseId, short? languageId = null) =>
        Error.NotFound("WarehouseTransfer.DestinationWarehouseNotFound", $"Destination warehouse with id {warehouseId} was not found.");

    public static Error WarehousesMustDiffer(short? languageId = null) =>
        Error.Business("WarehouseTransfer.WarehousesMustDiffer", "Source and destination warehouses must be different.");

    public static Error WarehouseOrganizationMismatch(int warehouseId, int organizationId, short? languageId = null) =>
        Error.Business("WarehouseTransfer.WarehouseOrganizationMismatch", $"Warehouse {warehouseId} does not belong to organization {organizationId}.");

    public static Error WarehouseInactive(int warehouseId, short? languageId = null) =>
        Error.Business("WarehouseTransfer.WarehouseInactive", $"Warehouse {warehouseId} is not active.");

    public static Error ProductNotFound(int productId, short? languageId = null) =>
        Error.NotFound("WarehouseTransfer.ProductNotFound", $"Product with id {productId} was not found.");

    public static Error ProductServiceNotAllowed(int productId, short? languageId = null) =>
        Error.Business("WarehouseTransfer.ProductServiceNotAllowed", $"Service product {productId} cannot be used in warehouse transfer.");

    public static Error UnitNotFound(short unitId, short? languageId = null) =>
        Error.NotFound("WarehouseTransfer.UnitNotFound", $"Unit with id {unitId} was not found.");

    public static Error InvalidQuantity(int productId, decimal quantity, short? languageId = null) =>
        Error.Business("WarehouseTransfer.InvalidQuantity", $"Product {productId} has invalid quantity {quantity}.");

    public static Error ItemsRequired(int productId, short? languageId = null) =>
        Error.Business("WarehouseTransfer.ItemsRequired", $"Product {productId} must contain at least one product table item.");

    public static Error QuantityItemsMismatch(int productId, decimal quantity, int itemCount, short? languageId = null) =>
        Error.Business("WarehouseTransfer.QuantityItemsMismatch", $"Product {productId} quantity {quantity} does not match item count {itemCount}.");

    public static Error DuplicateProductTable(int productTableId, short? languageId = null) =>
        Error.Conflict("WarehouseTransfer.DuplicateProductTable", $"Product table {productTableId} is duplicated in the transfer.");

    public static Error ProductTableNotFound(int productTableId, short? languageId = null) =>
        Error.NotFound("WarehouseTransfer.ProductTableNotFound", $"Product table with id {productTableId} was not found.");

    public static Error ProductTableProductMismatch(int productTableId, int productId, short? languageId = null) =>
        Error.Business("WarehouseTransfer.ProductTableProductMismatch", $"Product table {productTableId} does not belong to product {productId}.");

    public static Error ProductTableInactive(int productTableId, short? languageId = null) =>
        Error.Business("WarehouseTransfer.ProductTableInactive", $"Product table {productTableId} is not active.");

    public static Error ProductTableUnavailable(int productTableId, short statusId, short? languageId = null) =>
        Error.Business("WarehouseTransfer.ProductTableUnavailable", $"Product table {productTableId} is not available for transfer in status {statusId}.");

    public static Error ProductTableWarehouseMismatch(int productTableId, int warehouseId, short? languageId = null) =>
        Error.Business("WarehouseTransfer.ProductTableWarehouseMismatch", $"Product table {productTableId} is not located in warehouse {warehouseId}.");

    public static Error MissingInventoryRegisterEntries(long id, short? languageId = null) =>
        Error.Conflict("WarehouseTransfer.MissingInventoryRegisterEntries", $"Inventory register entries are missing or incomplete for warehouse transfer with id {id}.");
}
