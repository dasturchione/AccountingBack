using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.PurchaseDocTables;

public static class PurchaseDocTableErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("PurchaseDocTable.NotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan xarid hujjati qatori topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган харид ҳужжати қатори топилмади.",
            LanguageIdConst.RU      => $"Строка документа закупки с id {id} не найдена.",
            _                       => $"Purchase document line with id {id} was not found."
        });

    public static Error OwnerNotFound(long ownerId, short? languageId = null) =>
        Error.NotFound("PurchaseDocTable.OwnerNotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {ownerId} bo'lgan xarid hujjati topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {ownerId} бўлган харид ҳужжати топилмади.",
            LanguageIdConst.RU      => $"Документ закупки с id {ownerId} не найден.",
            _                       => $"Purchase document with id {ownerId} was not found."
        });

    public static Error OwnerAlreadyPosted(long ownerId, short? languageId = null) =>
        Error.Conflict("PurchaseDocTable.OwnerAlreadyPosted", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {ownerId} bo'lgan xarid hujjati o'tkazilgan, qator qo'shish, o'zgartirish yoki o'chirish mumkin emas.",
            LanguageIdConst.UZ_CYRL => $"Id-си {ownerId} бўлган харид ҳужжати ўтказилган, қатор қўшиш, ўзгартириш ёки ўчириш мумкин эмас.",
            LanguageIdConst.RU      => $"Документ закупки с id {ownerId} уже проведён, добавление, изменение или удаление строк невозможно.",
            _                       => $"Purchase document with id {ownerId} is already posted. Lines cannot be added, modified or deleted."
        });

    public static Error VatRateNotFound(short vatRateId, short? languageId = null) =>
        Error.NotFound("PurchaseDocTable.VatRateNotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {vatRateId} bo'lgan QQS stavkasi topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {vatRateId} бўлган ҚҚС ставкаси топилмади.",
            LanguageIdConst.RU      => $"Ставка НДС с id {vatRateId} не найдена.",
            _                       => $"VAT rate with id {vatRateId} was not found."
        });
}
