using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.SaleDocs;

public static partial class SaleDocErrors
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

    public static Error NotDraft(long id, short? languageId = null) =>
        Error.Conflict("SaleDoc.NotDraft", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan sotuv hujjati qoralama holatda emas.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган сотув ҳужжати қоралама ҳолатда эмас.",
            LanguageIdConst.RU      => $"Документ продажи с id {id} не в черновике.",
            _                       => $"Sale document with id {id} is not in draft status."
        });

    public static Error NotPending(long id, short? languageId = null) =>
        Error.Conflict("SaleDoc.NotPending", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan sotuv hujjati kutilmoqda holatda emas.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган сотув ҳужжати кутилмоқда ҳолатда эмас.",
            LanguageIdConst.RU      => $"Документ продажи с id {id} не в статусе ожидания.",
            _                       => $"Sale document with id {id} is not in pending status."
        });

    public static Error QuantityMismatch(long saleDocProductId, decimal expected, int actual, short? languageId = null) =>
        Error.Conflict("SaleDoc.QuantityMismatch", languageId switch
        {
            LanguageIdConst.UZ      => $"SaleDocProduct id-si {saleDocProductId}: kutilgan son {expected}, kelgan son {actual}.",
            LanguageIdConst.UZ_CYRL => $"SaleDocProduct id-си {saleDocProductId}: кутилган сон {expected}, келган сон {actual}.",
            LanguageIdConst.RU      => $"SaleDocProduct id {saleDocProductId}: ожидалось {expected}, получено {actual}.",
            _                       => $"SaleDocProduct id {saleDocProductId}: expected {expected}, received {actual}."
        });

    public static Error SaleDocProductNotFound(long saleDocProductId, short? languageId = null) =>
        Error.NotFound("SaleDoc.SaleDocProductNotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"SaleDocProduct id-si {saleDocProductId} topilmadi.",
            LanguageIdConst.UZ_CYRL => $"SaleDocProduct id-си {saleDocProductId} топилмади.",
            LanguageIdConst.RU      => $"SaleDocProduct с id {saleDocProductId} не найден.",
            _                       => $"SaleDocProduct with id {saleDocProductId} was not found."
        });

    public static Error ProductMismatch(int productTableId, int expectedProductId, int actualProductId, short? languageId = null) =>
        Error.Conflict("SaleDoc.ProductMismatch", languageId switch
        {
            LanguageIdConst.UZ      => $"ProductTable id-si {productTableId} mahsuloti ({actualProductId}) SaleDocProduct mahsulotiga ({expectedProductId}) mos kelmaydi.",
            LanguageIdConst.UZ_CYRL => $"ProductTable id-си {productTableId} маҳсулоти ({actualProductId}) SaleDocProduct маҳсулотига ({expectedProductId}) мос келмайди.",
            LanguageIdConst.RU      => $"Товар ProductTable id {productTableId} ({actualProductId}) не соответствует товару SaleDocProduct ({expectedProductId}).",
            _                       => $"ProductTable id {productTableId} product ({actualProductId}) does not match SaleDocProduct product ({expectedProductId})."
        });

    public static Error InvalidInventorySelection(short? languageId = null) =>
        Error.Conflict("SaleDoc.InvalidInventorySelection", languageId switch
        {
            LanguageIdConst.UZ      => $"Tanlangan partiyalar inventar baholash usuliga mos kelmaydi.",
            LanguageIdConst.UZ_CYRL => $"Танланган партиялар инвентар баҳолаш усулига мос келмайди.",
            LanguageIdConst.RU      => $"Выбранные партии не соответствуют методу оценки запасов.",
            _                       => $"Selected inventory batches do not match the inventory valuation method."
        });

    public static Error CannotUpdateInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Conflict("SaleDoc.CannotUpdateInCurrentStatus", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan sotuv hujjatini joriy holatda o'zgartirish mumkin emas.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган сотув ҳужжатини жорий ҳолатда ўзгартириш мумкин эмас.",
            LanguageIdConst.RU      => $"Документ продажи с id {id} нельзя изменить в текущем статусе.",
            _                       => $"Sale document with id {id} cannot be updated in current status."
        });
    public static Error CannotConfirmInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("SaleDoc.CannotConfirmInCurrentStatus", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan sotuv hujjatini status {statusId} holatida tasdiqlab bo'lmaydi.",
            _ => $"Sale document with id {id} cannot be confirmed in status {statusId}."
        });

    public static Error CannotCancelInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("SaleDoc.CannotCancelInCurrentStatus", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan sotuv hujjatini status {statusId} holatida bekor qilib bo'lmaydi.",
            _ => $"Sale document with id {id} cannot be cancelled in status {statusId}."
        });

    public static Error BusinessEffectsAlreadyExist(long id, short? languageId = null) =>
        Error.Conflict("SaleDoc.BusinessEffectsAlreadyExist", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan sotuv hujjatida allaqachon biznes o'tkazmalar mavjud.",
            _ => $"Sale document with id {id} already has business postings or register movements."
        });

    public static Error MissingPostingBatch(long id, short? languageId = null) =>
        Error.Conflict("SaleDoc.MissingPostingBatch", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan sotuv hujjati uchun posting batch topilmadi.",
            _ => $"Posting batch was not found for sale document with id {id}."
        });

    public static Error InvalidDraftInventoryState(long id, short? languageId = null) =>
        Error.Business("SaleDoc.InvalidDraftInventoryState", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan sotuv hujjatidagi tovarlar tasdiqlashga tayyor emas.",
            _ => $"Inventory rows for sale document with id {id} are not ready for confirmation."
        });

    public static Error CannotCancelMovedInventory(long id, short? languageId = null) =>
        Error.Business("SaleDoc.CannotCancelMovedInventory", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan sotuv hujjatini bekor qilib bo'lmaydi: mahsulotlar qaytarish holatida emas.",
            _ => $"Sale document with id {id} cannot be cancelled because one or more items are not in sold state."
        });

    public static Error ProductNotFound(int productId, short? languageId = null) =>
        Error.NotFound("SaleDoc.ProductNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {productId} bo'lgan mahsulot topilmadi.",
            _ => $"Product with id {productId} was not found."
        });

    public static Error ServiceItemsNotAllowed(int productId, short? languageId = null) =>
        Error.Business("SaleDoc.ServiceItemsNotAllowed", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {productId} bo'lgan xizmat uchun ombor itemlari kiritilmasligi kerak.",
            _ => $"Service product id {productId} must not contain inventory items."
        });

    public static Error InvalidProductUnitPrice(long lineId, decimal unitPrice, short? languageId = null) =>
        Error.Business("SaleDoc.InvalidProductUnitPrice", languageId switch
        {
            LanguageIdConst.UZ => $"SaleDocProduct id-si {lineId}: narx manfiy bo'lishi mumkin emas. Joriy narx: {unitPrice}.",
            _ => $"SaleDocProduct id {lineId}: unit price cannot be negative. Current price: {unitPrice}."
        });

    public static Error InvalidProductCostPrice(long lineId, decimal costPrice, short? languageId = null) =>
        Error.Business("SaleDoc.InvalidProductCostPrice", languageId switch
        {
            LanguageIdConst.UZ => $"SaleDocProduct id-si {lineId}: tannarx manfiy bo'lishi mumkin emas. Joriy tannarx: {costPrice}.",
            _ => $"SaleDocProduct id {lineId}: cost price cannot be negative. Current cost price: {costPrice}."
        });

    public static Error MissingAccountingRegisterEntries(long id, short? languageId = null) =>
        Error.Conflict("SaleDoc.MissingAccountingRegisterEntries", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan sotuv hujjati uchun buxgalteriya registr yozuvlari topilmadi.",
            _ => $"Accounting register entries were not found for sale document with id {id}."
        });

    public static Error MissingCounterpartyRegisterEntries(long id, short? languageId = null) =>
        Error.Conflict("SaleDoc.MissingCounterpartyRegisterEntries", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan sotuv hujjati uchun kontragent registr yozuvlari topilmadi.",
            _ => $"Counterparty register entries were not found for sale document with id {id}."
        });
}
