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
}
