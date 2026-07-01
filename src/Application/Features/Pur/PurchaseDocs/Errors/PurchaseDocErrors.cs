using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.PurchaseDocs;

public static class PurchaseDocErrors
{
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

    public static Error HasLines(long id, short? languageId = null) =>
        Error.Conflict("PurchaseDoc.HasLines", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan xarid hujjatida qatorlar mavjud, avval ularni o'chiring.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган харид ҳужжатида қаторлар мавжуд, аввал уларни ўчиринг.",
            LanguageIdConst.RU      => $"Документ закупки с id {id} содержит строки, сначала удалите их.",
            _                       => $"Purchase document with id {id} has lines. Delete them first."
        });
}
