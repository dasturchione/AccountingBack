using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Inv.WarehouseProducts;

public static class WarehouseProductErrors
{
    public static Error InvalidQuantity(int productId, decimal quantity, short? languageId = null) =>
        Error.Business("WarehouseProduct.InvalidQuantity", languageId switch
        {
            LanguageIdConst.UZ => $"{productId}-mahsulot uchun {quantity} miqdor noto'g'ri.",
            LanguageIdConst.UZ_CYRL => $"{productId}-маҳсулот учун {quantity} миқдор нотўғри.",
            LanguageIdConst.RU => $"Некорректное количество {quantity} для товара {productId}.",
            _ => $"Invalid quantity {quantity} for product {productId}."
        });

    public static Error ProductNotFound(int productId, short? languageId = null) =>
        Error.NotFound("WarehouseProduct.ProductNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"{productId}-mahsulot topilmadi.",
            LanguageIdConst.UZ_CYRL => $"{productId}-маҳсулот топилмади.",
            LanguageIdConst.RU => $"Товар {productId} не найден.",
            _ => $"Product {productId} was not found."
        });

    public static Error UnsupportedOperation(short operationTypeId, short? languageId = null) =>
        Error.Business("WarehouseProduct.UnsupportedOperation", languageId switch
        {
            LanguageIdConst.UZ => $"{operationTypeId}-ombor operatsiyasi turi ombor mahsuloti uchun qo'llab-quvvatlanmaydi.",
            LanguageIdConst.UZ_CYRL => $"{operationTypeId}-омбор операцияси тури омбор маҳсулоти учун қўллаб-қувватланмайди.",
            LanguageIdConst.RU => $"Тип складской операции {operationTypeId} не поддерживается для WarehouseProduct.",
            _ => $"Inventory operation type {operationTypeId} is not supported for WarehouseProduct."
        });

    public static Error NotEnoughQuantity(int warehouseId, int productId, decimal requested, decimal available, short? languageId = null) =>
        Error.Business("WarehouseProduct.NotEnoughQuantity", languageId switch
        {
            LanguageIdConst.UZ => $"{warehouseId}-omborda {productId}-mahsulot yetarli emas: talab {requested}, mavjud {available}.",
            LanguageIdConst.UZ_CYRL => $"{warehouseId}-омборда {productId}-маҳсулот етарли эмас: талаб {requested}, мавжуд {available}.",
            LanguageIdConst.RU => $"На складе {warehouseId} недостаточно товара {productId}: требуется {requested}, доступно {available}.",
            _ => $"Not enough quantity in warehouse {warehouseId} for product {productId}: requested {requested}, available {available}."
        });

    public static Error NotEnoughReserved(int warehouseId, int productId, decimal requested, decimal reserved, short? languageId = null) =>
        Error.Business("WarehouseProduct.NotEnoughReserved", languageId switch
        {
            LanguageIdConst.UZ => $"{warehouseId}-omborda {productId}-mahsulot rezervi yetarli emas: talab {requested}, rezerv {reserved}.",
            LanguageIdConst.UZ_CYRL => $"{warehouseId}-омборда {productId}-маҳсулот резерви етарли эмас: талаб {requested}, резерв {reserved}.",
            LanguageIdConst.RU => $"На складе {warehouseId} недостаточно резерва товара {productId}: требуется {requested}, зарезервировано {reserved}.",
            _ => $"Not enough reserved quantity in warehouse {warehouseId} for product {productId}: requested {requested}, reserved {reserved}."
        });

    public static Error NotEnoughBlocked(int warehouseId, int productId, decimal requested, decimal blocked, short? languageId = null) =>
        Error.Business("WarehouseProduct.NotEnoughBlocked", languageId switch
        {
            LanguageIdConst.UZ => $"{warehouseId}-omborda {productId}-mahsulotning bloklangan miqdori yetarli emas: talab {requested}, bloklangan {blocked}.",
            LanguageIdConst.UZ_CYRL => $"{warehouseId}-омборда {productId}-маҳсулотнинг блокланган миқдори етарли эмас: талаб {requested}, блокланган {blocked}.",
            LanguageIdConst.RU => $"На складе {warehouseId} недостаточно блокированного количества товара {productId}: требуется {requested}, заблокировано {blocked}.",
            _ => $"Not enough blocked quantity in warehouse {warehouseId} for product {productId}: requested {requested}, blocked {blocked}."
        });
    public static Error ProductTableNotFound(int productTableId, short? languageId = null) =>
        Error.NotFound("WarehouseProduct.ProductTableNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"{productTableId}-mahsulot partiyasi topilmadi.",
            LanguageIdConst.UZ_CYRL => $"{productTableId}-маҳсулот партияси топилмади.",
            LanguageIdConst.RU => $"Партия товара {productTableId} не найдена.",
            _ => $"Product table {productTableId} was not found."
        });

    public static Error ProductTableUnavailable(int productTableId, short statusId, short? languageId = null) =>
        Error.Business("WarehouseProduct.ProductTableUnavailable", languageId switch
        {
            LanguageIdConst.UZ => $"{productTableId}-mahsulot partiyasi {statusId} holatida mavjud emas.",
            LanguageIdConst.UZ_CYRL => $"{productTableId}-маҳсулот партияси {statusId} ҳолатида мавжуд эмас.",
            LanguageIdConst.RU => $"Партия товара {productTableId} недоступна в статусе {statusId}.",
            _ => $"Product table {productTableId} is unavailable with status {statusId}."
        });

    public static Error ProductTableWarehouseMismatch(int productTableId, int warehouseId, short? languageId = null) =>
        Error.Business("WarehouseProduct.ProductTableWarehouseMismatch", languageId switch
        {
            LanguageIdConst.UZ => $"{productTableId}-mahsulot partiyasi {warehouseId}-omborda joylashmagan.",
            LanguageIdConst.UZ_CYRL => $"{productTableId}-маҳсулот партияси {warehouseId}-омборда жойлашмаган.",
            LanguageIdConst.RU => $"Партия товара {productTableId} не находится на складе {warehouseId}.",
            _ => $"Product table {productTableId} is not in warehouse {warehouseId}."
        });

    public static Error ProductTableProductMismatch(int productTableId, int productId, short? languageId = null) =>
        Error.Business("WarehouseProduct.ProductTableProductMismatch", languageId switch
        {
            LanguageIdConst.UZ => $"{productTableId}-partiya {productId}-mahsulotga tegishli emas.",
            LanguageIdConst.UZ_CYRL => $"{productTableId}-партия {productId}-маҳсулотга тегишли эмас.",
            LanguageIdConst.RU => $"Партия товара {productTableId} не относится к товару {productId}.",
            _ => $"Product table {productTableId} does not belong to product {productId}."
        });

    public static Error SelectedBatchUnavailable(long batchId, int warehouseId, int productId, short? languageId = null) =>
        Error.Business("WarehouseProduct.SelectedBatchUnavailable", Message(languageId,
            $"Tanlangan {batchId}-partiya {warehouseId}-ombordagi {productId}-mahsulotga tegishli emas yoki mavjud emas.",
            $"Танланган {batchId}-партия {warehouseId}-омбордаги {productId}-маҳсулотга тегишли эмас ёки мавжуд эмас.",
            $"Выбранная партия {batchId} не относится к товару {productId} на складе {warehouseId} или недоступна.",
            $"Selected batch {batchId} does not belong to product {productId} in warehouse {warehouseId} or is unavailable."));

    public static Error SelectedBatchNotEnoughQuantity(long batchId, decimal requested, decimal available, short? languageId = null) =>
        Error.Business("WarehouseProduct.SelectedBatchNotEnoughQuantity", Message(languageId,
            $"Tanlangan {batchId}-partiyada miqdor yetarli emas: talab {requested}, mavjud {available}.",
            $"Танланган {batchId}-партияда миқдор етарли эмас: талаб {requested}, мавжуд {available}.",
            $"В выбранной партии {batchId} недостаточно количества: требуется {requested}, доступно {available}.",
            $"Selected batch {batchId} has insufficient quantity: requested {requested}, available {available}."));

    public static Error InvalidSaleAllocation(long saleDocProductId, short? languageId = null) =>
        Error.Business("WarehouseProduct.InvalidSaleAllocation", Message(languageId,
            $"{saleDocProductId}-sotuv qatori uchun ombor partiyalari taqsimoti noto'g'ri.",
            $"{saleDocProductId}-сотув қатори учун омбор партиялари тақсимоти нотўғри.",
            $"Некорректное распределение складских партий для строки продажи {saleDocProductId}.",
            $"Invalid warehouse batch allocation for sale product line {saleDocProductId}."));
    public static Error ReceiptDocumentNotFound(short documentTypeId, long documentId, short? languageId = null) =>
        Error.NotFound("WarehouseProduct.ReceiptDocumentNotFound", Message(languageId,
            $"{documentTypeId}/{documentId}-kirim hujjati topilmadi.", $"{documentTypeId}/{documentId}-кирим ҳужжати топилмади.",
            $"Документ поступления {documentTypeId}/{documentId} не найден.", $"Receipt document {documentTypeId}/{documentId} was not found."));
    public static Error OriginalMovementNotFound(long movementId, short? languageId = null) =>
        Error.Conflict("WarehouseProduct.OriginalMovementNotFound", Message(languageId,
            $"Dastlabki {movementId}-ombor harakati topilmadi.", $"Дастлабки {movementId}-омбор ҳаракати топилмади.",
            $"Исходное складское движение {movementId} не найдено.", $"Original warehouse movement {movementId} was not found."));

    public static Error OriginalBatchNotFound(long movementId, short? languageId = null) =>
        Error.Conflict("WarehouseProduct.OriginalBatchNotFound", Message(languageId,
            $"{movementId}-kirim harakati uchun ombor partiyasi topilmadi.", $"{movementId}-кирим ҳаракати учун омбор партияси топилмади.",
            $"Складская партия для движения поступления {movementId} не найдена.", $"Warehouse batch for receipt movement {movementId} was not found."));

    public static Error OriginalAllocationNotFound(long movementId, short? languageId = null) =>
        Error.Conflict("WarehouseProduct.OriginalAllocationNotFound", Message(languageId,
            $"{movementId}-chiqim harakatining partiya taqsimotlari topilmadi yoki bekor qilish miqdoriga mos emas.",
            $"{movementId}-чиқим ҳаракатининг партия тақсимотлари топилмади ёки бекор қилиш миқдорига мос эмас.",
            $"Распределения партий для движения расхода {movementId} не найдены или не соответствуют количеству сторно.",
            $"Batch allocations for issue movement {movementId} were not found or do not match the reversal quantity."));

    private static string Message(short? languageId, string uz, string uzCyrl, string ru, string en) => languageId switch
    {
        LanguageIdConst.UZ => uz,
        LanguageIdConst.UZ_CYRL => uzCyrl,
        LanguageIdConst.RU => ru,
        _ => en
    };
}
