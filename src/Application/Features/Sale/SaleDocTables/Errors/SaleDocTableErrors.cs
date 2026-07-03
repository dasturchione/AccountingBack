using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.SaleDocTables;

public static class SaleDocTableErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("SaleDocTable.NotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan sotuv hujjati qatori topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган сотув ҳужжати қатори топилмади.",
            LanguageIdConst.RU      => $"Строка документа продажи с id {id} не найдена.",
            _                       => $"Sale document line with id {id} was not found."
        });

    public static Error OwnerNotFound(long ownerId, short? languageId = null) =>
        Error.NotFound("SaleDocTable.OwnerNotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {ownerId} bo'lgan sotuv hujjati topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {ownerId} бўлган сотув ҳужжати топилмади.",
            LanguageIdConst.RU      => $"Документ продажи с id {ownerId} не найден.",
            _                       => $"Sale document with id {ownerId} was not found."
        });

    public static Error OwnerAlreadyPosted(long ownerId, short? languageId = null) =>
        Error.Conflict("SaleDocTable.OwnerAlreadyPosted", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {ownerId} bo'lgan sotuv hujjati o'tkazilgan, qator qo'shish, o'zgartirish yoki o'chirish mumkin emas.",
            LanguageIdConst.UZ_CYRL => $"Id-си {ownerId} бўлган сотув ҳужжати ўтказилган, қатор қўшиш, ўзгартириш ёки ўчириш мумкин эмас.",
            LanguageIdConst.RU      => $"Документ продажи с id {ownerId} уже проведён, добавление, изменение или удаление строк невозможно.",
            _                       => $"Sale document with id {ownerId} is already posted. Lines cannot be added, modified or deleted."
        });

    public static Error VatRateNotFound(short vatRateId, short? languageId = null) =>
        Error.NotFound("SaleDocTable.VatRateNotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {vatRateId} bo'lgan QQS stavkasi topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {vatRateId} бўлган ҚҚС ставкаси топилмади.",
            LanguageIdConst.RU      => $"Ставка НДС с id {vatRateId} не найдена.",
            _                       => $"VAT rate with id {vatRateId} was not found."
        });
    public static Error DirectTableMutationUnsupported(short? languageId = null) =>
        Error.Business("SaleDocTable.DirectTableMutationUnsupported", languageId switch
        {
            LanguageIdConst.UZ => "Sotuv item qatorlari faqat sotuv hujjati aggregate orqali o'zgartiriladi.",
            _ => "Sale item rows can only be changed through the sale document aggregate."
        });
}
