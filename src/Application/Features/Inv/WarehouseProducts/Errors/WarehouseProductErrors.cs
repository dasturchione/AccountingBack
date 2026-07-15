using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Inv.WarehouseProducts;

public static class WarehouseProductErrors
{
    public static Error InvalidQuantity(int productId, decimal quantity, short? languageId = null) =>
        Error.Business("WarehouseProduct.InvalidQuantity", languageId switch
        {
            LanguageIdConst.RU => $"Некорректное количество {quantity} для товара {productId}.",
            _ => $"Invalid quantity {quantity} for product {productId}."
        });

    public static Error ProductNotFound(int productId, short? languageId = null) =>
        Error.NotFound("WarehouseProduct.ProductNotFound", languageId switch
        {
            LanguageIdConst.RU => $"Товар {productId} не найден.",
            _ => $"Product {productId} was not found."
        });

    public static Error UnsupportedOperation(short operationTypeId, short? languageId = null) =>
        Error.Business("WarehouseProduct.UnsupportedOperation", languageId switch
        {
            LanguageIdConst.RU => $"Тип складской операции {operationTypeId} не поддерживается для WarehouseProduct.",
            _ => $"Inventory operation type {operationTypeId} is not supported for WarehouseProduct."
        });

    public static Error NotEnoughQuantity(int warehouseId, int productId, decimal requested, decimal available, short? languageId = null) =>
        Error.Business("WarehouseProduct.NotEnoughQuantity", languageId switch
        {
            LanguageIdConst.RU => $"На складе {warehouseId} недостаточно товара {productId}: требуется {requested}, доступно {available}.",
            _ => $"Not enough quantity in warehouse {warehouseId} for product {productId}: requested {requested}, available {available}."
        });

    public static Error NotEnoughReserved(int warehouseId, int productId, decimal requested, decimal reserved, short? languageId = null) =>
        Error.Business("WarehouseProduct.NotEnoughReserved", languageId switch
        {
            LanguageIdConst.RU => $"На складе {warehouseId} недостаточно резерва товара {productId}: требуется {requested}, зарезервировано {reserved}.",
            _ => $"Not enough reserved quantity in warehouse {warehouseId} for product {productId}: requested {requested}, reserved {reserved}."
        });

    public static Error NotEnoughBlocked(int warehouseId, int productId, decimal requested, decimal blocked, short? languageId = null) =>
        Error.Business("WarehouseProduct.NotEnoughBlocked", languageId switch
        {
            LanguageIdConst.RU => $"На складе {warehouseId} недостаточно блокированного количества товара {productId}: требуется {requested}, заблокировано {blocked}.",
            _ => $"Not enough blocked quantity in warehouse {warehouseId} for product {productId}: requested {requested}, blocked {blocked}."
        });
    public static Error ProductTableNotFound(int productTableId, short? languageId = null) =>
        Error.NotFound("WarehouseProduct.ProductTableNotFound", languageId switch
        {
            LanguageIdConst.RU => $"Партия товара {productTableId} не найдена.",
            _ => $"Product table {productTableId} was not found."
        });

    public static Error ProductTableUnavailable(int productTableId, short statusId, short? languageId = null) =>
        Error.Business("WarehouseProduct.ProductTableUnavailable", languageId switch
        {
            LanguageIdConst.RU => $"Партия товара {productTableId} недоступна в статусе {statusId}.",
            _ => $"Product table {productTableId} is unavailable with status {statusId}."
        });

    public static Error ProductTableWarehouseMismatch(int productTableId, int warehouseId, short? languageId = null) =>
        Error.Business("WarehouseProduct.ProductTableWarehouseMismatch", languageId switch
        {
            LanguageIdConst.RU => $"Партия товара {productTableId} не находится на складе {warehouseId}.",
            _ => $"Product table {productTableId} is not in warehouse {warehouseId}."
        });

    public static Error ProductTableProductMismatch(int productTableId, int productId, short? languageId = null) =>
        Error.Business("WarehouseProduct.ProductTableProductMismatch", languageId switch
        {
            LanguageIdConst.RU => $"Партия товара {productTableId} не относится к товару {productId}.",
            _ => $"Product table {productTableId} does not belong to product {productId}."
        });
}
