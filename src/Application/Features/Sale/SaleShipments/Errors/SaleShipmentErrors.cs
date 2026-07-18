using SharedKernel.Results;

namespace Application.Features.SaleShipments;

public static class SaleShipmentErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("SaleShipment.NotFound", $"Sale shipment with id {id} was not found.");

    public static Error CurrentUserNotFound(short? languageId = null) =>
        Error.Business("SaleShipment.CurrentUserNotFound", "Current user is required to create a sale shipment.");

    public static Error WarehouseNotFound(int warehouseId, short? languageId = null) =>
        Error.NotFound("SaleShipment.WarehouseNotFound", $"Warehouse with id {warehouseId} was not found.");

    public static Error WarehouseOrganizationMismatch(int warehouseId, int organizationId, short? languageId = null) =>
        Error.Business("SaleShipment.WarehouseOrganizationMismatch", $"Warehouse {warehouseId} does not belong to organization {organizationId}.");

    public static Error WarehouseInactive(int warehouseId, short? languageId = null) =>
        Error.Business("SaleShipment.WarehouseInactive", $"Warehouse {warehouseId} is not active.");

    public static Error CounterpartyNotFound(int counterpartyId, short? languageId = null) =>
        Error.NotFound("SaleShipment.CounterpartyNotFound", $"Counterparty with id {counterpartyId} was not found.");

    public static Error CounterpartyOrganizationMismatch(int counterpartyId, int organizationId, short? languageId = null) =>
        Error.Business("SaleShipment.CounterpartyOrganizationMismatch", $"Counterparty {counterpartyId} does not belong to organization {organizationId}.");

    public static Error ProductsRequired(short? languageId = null) =>
        Error.Business("SaleShipment.ProductsRequired", "Sale shipment must contain at least one product.");

    public static Error ProductNotFound(int productId, short? languageId = null) =>
        Error.NotFound("SaleShipment.ProductNotFound", $"Product with id {productId} was not found.");

    public static Error ProductServiceNotAllowed(int productId, short? languageId = null) =>
        Error.Business("SaleShipment.ProductServiceNotAllowed", $"Service product {productId} cannot be included in a sale shipment.");

    public static Error ProductUnitMismatch(int productId, short unitId, short? languageId = null) =>
        Error.Business("SaleShipment.ProductUnitMismatch", $"Unit {unitId} does not match product {productId}.");

    public static Error InvalidQuantity(int productId, decimal quantity, short? languageId = null) =>
        Error.Business("SaleShipment.InvalidQuantity", $"Product {productId} has invalid quantity {quantity}.");

    public static Error BatchQuantityMismatch(int productId, decimal quantity, decimal batchQuantity, short? languageId = null) =>
        Error.Business("SaleShipment.BatchQuantityMismatch", $"Batch quantity {batchQuantity} does not match product {productId} quantity {quantity}.");

    public static Error ProductTableQuantityMismatch(int productId, decimal quantity, int tableCount, short? languageId = null) =>
        Error.Business("SaleShipment.ProductTableQuantityMismatch", $"Product table count {tableCount} does not match product {productId} quantity {quantity}.");

    public static Error DuplicateBatch(long batchId, short? languageId = null) =>
        Error.Conflict("SaleShipment.DuplicateBatch", $"Batch {batchId} is duplicated in the sale shipment product.");

    public static Error DuplicateProductTable(int productTableId, short? languageId = null) =>
        Error.Conflict("SaleShipment.DuplicateProductTable", $"Product table {productTableId} is duplicated in the sale shipment.");

    public static Error BatchUnavailable(long batchId, int warehouseId, int productId, short? languageId = null) =>
        Error.Business("SaleShipment.BatchUnavailable", $"Batch {batchId} is unavailable for product {productId} in warehouse {warehouseId}.");

    public static Error BatchQuantityUnavailable(long batchId, decimal requested, decimal available, short? languageId = null) =>
        Error.Business("SaleShipment.BatchQuantityUnavailable", $"Batch {batchId} has available quantity {available}, but {requested} was requested.");

    public static Error ProductTableNotFound(int productTableId, short? languageId = null) =>
        Error.NotFound("SaleShipment.ProductTableNotFound", $"Product table with id {productTableId} was not found.");

    public static Error ProductTableMismatch(int productTableId, int productId, short? languageId = null) =>
        Error.Business("SaleShipment.ProductTableMismatch", $"Product table {productTableId} does not belong to product {productId}.");

    public static Error ProductTableUnavailable(int productTableId, int warehouseId, short? languageId = null) =>
        Error.Business("SaleShipment.ProductTableUnavailable", $"Product table {productTableId} is not available in warehouse {warehouseId}.");

    public static Error ProductTablesNotAllowed(int productId, short? languageId = null) =>
        Error.Business("SaleShipment.ProductTablesNotAllowed", $"Product tables are not allowed for non-piece-tracked product {productId}.");

    public static Error InvalidDocNumber(short? languageId = null) =>
        Error.Business("SaleShipment.InvalidDocNumber", "Sale shipment document number is invalid.");
    public static Error LinkedDocumentCannotBeChanged(long id, short? languageId = null) =>
        Error.Conflict("SaleShipment.LinkedDocumentCannotBeChanged", $"Sale shipment {id} is already linked to a sale document and cannot be changed.");

    public static Error AlreadyLinkedToSale(long id, short? languageId = null) =>
        Error.Conflict("SaleShipment.AlreadyLinkedToSale", "Sale shipment is already linked to a sale document.");

    public static Error DuplicateShipmentProductLink(short? languageId = null) =>
        Error.Conflict("SaleShipment.DuplicateShipmentProductLink", "Shipment product cannot be linked to more than one sale document line.");
    public static Error ShipmentProductNotFound(long shipmentProductId, short? languageId = null) =>
        Error.NotFound("SaleShipment.ShipmentProductNotFound", $"Sale shipment product {shipmentProductId} was not found.");

    public static Error ShipmentProductAlreadyLinked(long shipmentProductId, short? languageId = null) =>
        Error.Conflict("SaleShipment.ShipmentProductAlreadyLinked", $"Sale shipment product {shipmentProductId} is already linked to a sale document product.");

    public static Error ShipmentProductMismatch(long shipmentProductId, int productId, decimal quantity, short? languageId = null) =>
        Error.Business("SaleShipment.ShipmentProductMismatch", $"Sale shipment product {shipmentProductId} does not match product {productId} and quantity {quantity}.");
}
