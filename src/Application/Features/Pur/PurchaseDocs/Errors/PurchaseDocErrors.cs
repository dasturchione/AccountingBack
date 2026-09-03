using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.PurchaseDocs;

public static class PurchaseDocErrors
{
    public static Error PreviewSignedDocumentRequired(short? languageId = null) => Business("PurchasePreview.SignedDocumentRequired", languageId,
        "Xaridni oldindan ko'rish uchun faqat imzolangan EDO hujjatlaridan foydalanish mumkin.",
        "Харидни олдиндан кўриш учун фақат имзоланган EDO ҳужжатларидан фойдаланиш мумкин.",
        "Для предварительного просмотра закупки можно использовать только подписанные документы EDO.",
        "Only SIGNED EDO documents can be used for Purchase preview.");

    public static Error PreviewUnsupportedDocumentType(short? languageId = null) => Business("PurchasePreview.UnsupportedDocumentType", languageId,
        "Xaridni oldindan ko'rish uchun faqat FACTURA turidagi EDO hujjatlaridan foydalanish mumkin.",
        "Харидни олдиндан кўриш учун фақат FACTURA туридаги EDO ҳужжатларидан фойдаланиш мумкин.",
        "Для предварительного просмотра закупки можно использовать только EDO-документы типа FACTURA.",
        "Only FACTURA EDO documents can be used for Purchase preview.");

    public static Error InboxDocumentNotFound(short? languageId = null) =>
        Error.NotFound("PurchaseFromEdo.InboxDocumentRequired", Message(languageId,
            "EDO hujjati avval tashkilotning kiruvchi hujjatlarida mavjud bo'lishi kerak.",
            "EDO ҳужжати аввал ташкилотнинг кирувчи ҳужжатларида мавжуд бўлиши керак.",
            "Документ EDO сначала должен быть доступен во входящих документах организации.",
            "The EDO document must first be available in the organization inbox scope."));

    public static Error InboxDocumentRequired(short? languageId = null) => Business("PurchaseFromEdo.InboxDocumentRequired", languageId,
        "Faqat kiruvchi EDO hujjatlarini xarid sifatida import qilish mumkin.",
        "Фақат кирувчи EDO ҳужжатларини харид сифатида импорт қилиш мумкин.",
        "В качестве закупки можно импортировать только входящие документы EDO.",
        "Only EDO inbox documents can be imported as a Purchase.");

    public static Error SignedDocumentRequired(short? languageId = null) => Business("PurchaseFromEdo.SignedDocumentRequired", languageId,
        "Xarid sifatida faqat imzolangan EDO hujjatlarini import qilish mumkin.",
        "Харид сифатида фақат имзоланган EDO ҳужжатларини импорт қилиш мумкин.",
        "В качестве закупки можно импортировать только подписанные документы EDO.",
        "Only SIGNED EDO documents can be imported as a Purchase.");

    public static Error UnsupportedDocumentType(short? languageId = null) => Business("PurchaseFromEdo.UnsupportedDocumentType", languageId,
        "Xarid sifatida faqat FACTURA turidagi EDO hujjatlarini import qilish mumkin.",
        "Харид сифатида фақат FACTURA туридаги EDO ҳужжатларини импорт қилиш мумкин.",
        "В качестве закупки можно импортировать только EDO-документы типа FACTURA.",
        "Only FACTURA EDO documents can be imported as a Purchase.");

    public static Error DocumentDateRequired(short? languageId = null) => Business("PurchaseFromEdo.DocumentDateRequired", languageId,
        "Provayder hujjatining sanasi ko'rsatilishi kerak.", "Провайдер ҳужжатининг санаси кўрсатилиши керак.",
        "Необходимо указать дату документа провайдера.", "The provider document date is required.");

    public static Error EdoValidationFailed(short? languageId = null) => Business("PurchaseFromEdo.ValidationFailed", languageId,
        "EDO hujjatidagi moslashtirishlar yoki provayder qiymatlari xarid yaratish uchun noto'g'ri.",
        "EDO ҳужжатидаги мослаштиришлар ёки провайдер қийматлари харид яратиш учун нотўғри.",
        "Сопоставления или значения провайдера в документе EDO некорректны для создания закупки.",
        "The EDO document mappings or provider values are not valid for Purchase creation.");

    public static Error HistoricalValidationFailed(string normalizedCode, short? languageId = null) =>
        Business($"PurchaseFromEdo.HistoricalValidation.{normalizedCode}", languageId,
            "Tarixiy EDO nusxasi xarid qoralamasini yaratish uchun noto'g'ri.",
            "Тарихий EDO нусхаси харид қораламасини яратиш учун нотўғри.",
            "Исторический снимок EDO некорректен для создания черновика закупки.",
            "The historical EDO snapshot is not valid for Draft Purchase creation.");

    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("PurchaseDoc.NotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan xarid hujjati topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган харид ҳужжати топилмади.",
            LanguageIdConst.RU      => $"Документ закупки с id {id} не найден.",
            _                       => $"Purchase document with id {id} was not found."
        });

    public static Error DocNumberConflict(string docNumber, short? languageId = null) =>
        Error.Conflict("PurchaseDoc.DocNumberConflict", languageId switch
        {
            LanguageIdConst.UZ      => $"Hujjat raqami '{docNumber}' bo'lgan xarid hujjati allaqachon mavjud.",
            LanguageIdConst.UZ_CYRL => $"Ҳужжат рақами '{docNumber}' бўлган харид ҳужжати аллақачон мавжуд.",
            LanguageIdConst.RU      => $"Документ закупки с номером '{docNumber}' уже существует.",
            _                       => $"Purchase document with number '{docNumber}' already exists."
        });

    public static Error AlreadyPosted(long id, short? languageId = null) =>
        Error.Conflict("PurchaseDoc.AlreadyPosted", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan xarid hujjati o'tkazilgan, uni o'zgartirish yoki o'chirish mumkin emas.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган харид ҳужжати ўтказилган, уни ўзгартириш ёки ўчириш мумкин эмас.",
            LanguageIdConst.RU      => $"Документ закупки с id {id} уже проведён, изменение или удаление невозможно.",
            _                       => $"Purchase document with id {id} is already posted and cannot be modified or deleted."
        });

    public static Error AlreadyCancelled(long id, short? languageId = null) =>
        Error.Conflict("PurchaseDoc.AlreadyCancelled", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan xarid hujjati allaqachon bekor qilingan.",
            _ => $"Purchase document with id {id} is already cancelled."
        });

    public static Error CannotConfirmInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("PurchaseDoc.CannotConfirmInCurrentStatus", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan xarid hujjatini status {statusId} holatida tasdiqlab bo'lmaydi.",
            _ => $"Purchase document with id {id} cannot be confirmed in status {statusId}."
        });

    public static Error CannotCancelInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("PurchaseDoc.CannotCancelInCurrentStatus", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan xarid hujjatini status {statusId} holatida bekor qilib bo'lmaydi.",
            _ => $"Purchase document with id {id} cannot be cancelled in status {statusId}."
        });

    public static Error CannotUpdateInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("PurchaseDoc.CannotUpdateInCurrentStatus", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan xarid hujjatini status {statusId} holatida o'zgartirib bo'lmaydi.",
            _ => $"Purchase document with id {id} cannot be updated in status {statusId}."
        });

    public static Error CannotDeleteInCurrentStatus(long id, short statusId, short? languageId = null) =>
        Error.Business("PurchaseDoc.CannotDeleteInCurrentStatus", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan xarid hujjatini status {statusId} holatida o'chirib bo'lmaydi.",
            _ => $"Purchase document with id {id} cannot be deleted in status {statusId}."
        });

    public static Error LinesRequired(long id, short? languageId = null) =>
        Error.Business("PurchaseDoc.LinesRequired", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan xarid hujjatida kamida bitta qator bo'lishi kerak.",
            _ => $"Purchase document with id {id} must contain at least one line."
        });

    public static Error BusinessEffectsAlreadyExist(long id, short? languageId = null) =>
        Error.Conflict("PurchaseDoc.BusinessEffectsAlreadyExist", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan xarid hujjatida allaqachon biznes o'tkazmalar mavjud.",
            _ => $"Purchase document with id {id} already has business postings or register movements."
        });

    public static Error MissingPostingBatch(long id, short? languageId = null) =>
        Error.Conflict("PurchaseDoc.MissingPostingBatch", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan xarid hujjati uchun posting batch topilmadi.",
            _ => $"Posting batch was not found for purchase document with id {id}."
        });

    public static Error CannotCancelMovedInventory(long id, short? languageId = null) =>
        Error.Business("PurchaseDoc.CannotCancelMovedInventory", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan xarid hujjatini bekor qilib bo'lmaydi: mahsulotlar ombordan harakatlangan.",
            _ => $"Purchase document with id {id} cannot be cancelled because one or more items are no longer in stock."
        });

    public static Error InvalidDraftInventoryState(long id, short? languageId = null) =>
        Error.Business("PurchaseDoc.InvalidDraftInventoryState", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan xarid hujjatidagi draft mahsulotlar tasdiqlashga tayyor emas.",
            _ => $"Draft item rows for purchase document with id {id} are not ready for confirmation."
        });

    public static Error ProductNotFound(int productId, short? languageId = null) =>
        Error.NotFound("PurchaseDoc.ProductNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {productId} bo'lgan mahsulot topilmadi.",
            _ => $"Product with id {productId} was not found."
        });

    public static Error CurrencyNotFound(short currencyId, short? languageId = null) =>
        Error.NotFound("PurchaseDoc.CurrencyNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {currencyId} bo'lgan valyuta topilmadi.",
            _ => $"Currency with id {currencyId} was not found."
        });

    public static Error UnitNotFound(short unitId, short? languageId = null) =>
        Error.NotFound("PurchaseDoc.UnitNotFound", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {unitId} bo'lgan o'lchov birligi topilmadi.",
            _ => $"Unit with id {unitId} was not found."
        });

    public static Error ServiceItemsNotAllowed(int productId, short? languageId = null) =>
        Error.Business("PurchaseDoc.ServiceItemsNotAllowed", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {productId} bo'lgan xizmat uchun marking/serial itemlar kiritilmasligi kerak.",
            _ => $"Service product id {productId} must not contain marking or serial items."
        });

    public static Error ProductPieceTrackingRequired(int productId, short? languageId = null) =>
        Error.Business("PRODUCT_PIECE_TRACKING_REQUIRED", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {productId} bo'lgan markingli tovar dona bo'yicha kuzatiladigan bo'lishi kerak.",
            _ => $"Marked goods product id {productId} must be piece-tracked."
        });

    public static Error MissingAccountingRegisterEntries(long id, short? languageId = null) =>
        Error.Conflict("PurchaseDoc.MissingAccountingRegisterEntries", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan xarid hujjati uchun buxgalteriya registr yozuvlari topilmadi.",
            _ => $"Accounting register entries were not found for purchase document with id {id}."
        });

    public static Error MissingCounterpartyRegisterEntries(long id, short? languageId = null) =>
        Error.Conflict("PurchaseDoc.MissingCounterpartyRegisterEntries", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {id} bo'lgan xarid hujjati uchun kontragent registr yozuvlari topilmadi.",
            _ => $"Counterparty register entries were not found for purchase document with id {id}."
        });

    public static Error HasLines(long id, short? languageId = null) =>
        Error.Conflict("PurchaseDoc.HasLines", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan xarid hujjatida qatorlar mavjud, avval ularni o'chiring.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган харид ҳужжатида қаторлар мавжуд, аввал уларни ўчиринг.",
            LanguageIdConst.RU      => $"Документ закупки с id {id} содержит строки, сначала удалите их.",
            _                       => $"Purchase document with id {id} has lines. Delete them first."
        });

    private static Error Business(string code, short? languageId, string uz, string uzCyrl, string ru, string en) =>
        Error.Business(code, Message(languageId, uz, uzCyrl, ru, en));

    private static string Message(short? languageId, string uz, string uzCyrl, string ru, string en) => languageId switch
    {
        LanguageIdConst.UZ => uz,
        LanguageIdConst.UZ_CYRL => uzCyrl,
        LanguageIdConst.RU => ru,
        _ => en
    };
}
