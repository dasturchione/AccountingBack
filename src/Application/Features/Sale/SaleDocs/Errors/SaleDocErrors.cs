using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.SaleDocs;

public static class SaleDocErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("SaleDoc.NotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan sotuv hujjati topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган сотув ҳужжати топилмади.",
            LanguageIdConst.RU      => $"Документ продажи с id {id} не найден.",
            _                       => $"Sale document with id {id} was not found."
        });

    public static Error DocNumberConflict(string docNumber, short? languageId = null) =>
        Error.Conflict("SaleDoc.DocNumberConflict", languageId switch
        {
            LanguageIdConst.UZ      => $"Hujjat raqami '{docNumber}' bo'lgan sotuv hujjati allaqachon mavjud.",
            LanguageIdConst.UZ_CYRL => $"Ҳужжат рақами '{docNumber}' бўлган сотув ҳужжати аллақачон мавжуд.",
            LanguageIdConst.RU      => $"Документ продажи с номером '{docNumber}' уже существует.",
            _                       => $"Sale document with number '{docNumber}' already exists."
        });

    public static Error AlreadyPosted(long id, short? languageId = null) =>
        Error.Conflict("SaleDoc.AlreadyPosted", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan sotuv hujjati o'tkazilgan, uni o'zgartirish yoki o'chirish mumkin emas.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган сотув ҳужжати ўтказилган, уни ўзгартириш ёки ўчириш мумкин эмас.",
            LanguageIdConst.RU      => $"Документ продажи с id {id} уже проведён, изменение или удаление невозможно.",
            _                       => $"Sale document with id {id} is already posted and cannot be modified or deleted."
        });

    public static Error ProductTableNotFound(int productTableId, short? languageId = null) =>
        Error.NotFound("SaleDoc.ProductTableNotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"ProductTable id-si {productTableId} topilmadi.",
            LanguageIdConst.UZ_CYRL => $"ProductTable id-си {productTableId} топилмади.",
            LanguageIdConst.RU      => $"ProductTable с id {productTableId} не найден.",
            _                       => $"ProductTable with id {productTableId} was not found."
        });

    public static Error CostPriceNotFound(int productId, short? languageId = null) =>
        Error.NotFound("SaleDoc.CostPriceNotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Tovar id-si {productId} uchun tannarx topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Товар id-си {productId} учун таннарх топилмади.",
            LanguageIdConst.RU      => $"Себестоимость товара с id {productId} не найдена.",
            _                       => $"Cost price for product with id {productId} was not found."
        });

    public static Error LineNotFound(long lineId, short? languageId = null) =>
        Error.NotFound("SaleDoc.LineNotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Sotuv satri id-si {lineId} topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Сотув сатри id-си {lineId} топилмади.",
            LanguageIdConst.RU      => $"Строка продажи с id {lineId} не найдена.",
            _                       => $"Sale document line with id {lineId} was not found."
        });

    public static Error NotPosted(long id, short? languageId = null) =>
        Error.Conflict("SaleDoc.NotPosted", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan sotuv hujjati tasdiqlanmagan, bekor qilish mumkin emas.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган сотув ҳужжати тасдиқланмаган, бекор қилиш мумкин эмас.",
            LanguageIdConst.RU      => $"Документ продажи с id {id} не проведён, отмена невозможна.",
            _                       => $"Sale document with id {id} is not posted and cannot be cancelled."
        });

    public static Error AlreadyCancelled(long id, short? languageId = null) =>
        Error.Conflict("SaleDoc.AlreadyCancelled", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan sotuv hujjati allaqachon bekor qilingan.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган сотув ҳужжати аллақачон бекор қилинган.",
            LanguageIdConst.RU      => $"Документ продажи с id {id} уже отменён.",
            _                       => $"Sale document with id {id} is already cancelled."
        });

    public static Error ProductTableNotAvailable(int productTableId, short? languageId = null) =>
        Error.Conflict("SaleDoc.ProductTableNotAvailable", languageId switch
        {
            LanguageIdConst.UZ      => $"ProductTable id-si {productTableId} omborda mavjud emas (sotilgan yoki band).",
            LanguageIdConst.UZ_CYRL => $"ProductTable id-си {productTableId} омборда мавжуд эмас (сотилган ёки банд).",
            LanguageIdConst.RU      => $"ProductTable с id {productTableId} недоступен на складе (продан или зарезервирован).",
            _                       => $"ProductTable with id {productTableId} is not available in stock (sold or reserved)."
        });
}
