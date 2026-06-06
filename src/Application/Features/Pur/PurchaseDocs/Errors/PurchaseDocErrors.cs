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
}
