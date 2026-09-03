using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.SaleDocs;

public static partial class SaleDocErrors
{
    public static Error EmptyProducts(long id, short? languageId = null) =>
        Error.Business("SaleDoc.EmptyProducts", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan sotuv hujjatida mahsulot qatorlari mavjud emas.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган сотув ҳужжатида маҳсулот қаторлари мавжуд эмас.",
            LanguageIdConst.RU => $"В документе продажи с id {id} отсутствуют товарные строки.",
            _ => $"Sale document with id {id} has no product lines."
        });

    public static Error InvalidProductQuantity(long saleDocProductId, decimal quantity, short? languageId = null) =>
        Error.Business("SaleDoc.InvalidProductQuantity", languageId switch
        {
            LanguageIdConst.UZ => $"SaleDocProduct id-si {saleDocProductId}: miqdor musbat butun son bo'lishi kerak. Joriy miqdor: {quantity}.",
            LanguageIdConst.UZ_CYRL => $"SaleDocProduct id-си {saleDocProductId}: миқдор мусбат бутун сон бўлиши керак. Жорий миқдор: {quantity}.",
            LanguageIdConst.RU => $"SaleDocProduct id {saleDocProductId}: количество должно быть положительным целым числом. Текущее значение: {quantity}.",
            _ => $"SaleDocProduct id {saleDocProductId}: quantity must be a positive whole number. Current quantity: {quantity}."
        });

    public static Error InsufficientStock(int productId, int required, int available, short? languageId = null) =>
        Error.Conflict("SaleDoc.InsufficientStock", languageId switch
        {
            LanguageIdConst.UZ => $"Tovar id-si {productId} uchun omborda yetarli qoldiq yo'q. Kerak: {required}, mavjud: {available}.",
            LanguageIdConst.UZ_CYRL => $"Товар id-си {productId} учун омборда етарли қолдиқ йўқ. Керак: {required}, мавжуд: {available}.",
            LanguageIdConst.RU => $"Недостаточно остатка товара с id {productId}. Требуется: {required}, доступно: {available}.",
            _ => $"Not enough stock for product id {productId}. Required: {required}, available: {available}."
        });

    public static Error InvalidInventoryValuationMethod(string method, short? languageId = null) =>
        Error.Business("SaleDoc.InvalidInventoryValuationMethod", languageId switch
        {
            LanguageIdConst.UZ => $"InventoryValuationMethod noto'g'ri: '{method}'.",
            LanguageIdConst.UZ_CYRL => $"InventoryValuationMethod нотўғри: '{method}'.",
            LanguageIdConst.RU => $"Недопустимый метод оценки запасов: '{method}'.",
            _ => $"Invalid InventoryValuationMethod: '{method}'."
        });

    public static Error InventoryReservationConflict(short? languageId = null) =>
        Error.Conflict("SaleDoc.InventoryReservationConflict", languageId switch
        {
            LanguageIdConst.UZ => "Tanlangan tovarlardan biri boshqa hujjat tomonidan band qilingan. Qayta urinib ko'ring.",
            LanguageIdConst.UZ_CYRL => "Танланган товарлардан бири бошқа ҳужжат томонидан банд қилинган. Қайта уриниб кўринг.",
            LanguageIdConst.RU => "Одна или несколько выбранных складских позиций зарезервированы другим документом. Повторите попытку.",
            _ => "One or more selected inventory items were reserved by another document. Please retry."
        });
}
