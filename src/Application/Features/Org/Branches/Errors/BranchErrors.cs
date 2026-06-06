using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Branches;

public static class BranchErrors
{
    public static Error NotFound(int id, short? languageId = null) =>
        Error.NotFound("Branch.NotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan filial topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган филиал топилмади.",
            LanguageIdConst.RU      => $"Филиал с id {id} не найден.",
            _                       => $"Branch with id {id} was not found."
        });

    public static Error CodeConflict(string code, short? languageId = null) =>
        Error.Conflict("Branch.CodeConflict", languageId switch
        {
            LanguageIdConst.UZ      => $"Kodi '{code}' bo'lgan filial allaqachon mavjud.",
            LanguageIdConst.UZ_CYRL => $"Коди '{code}' бўлган филиал аллақачон мавжуд.",
            LanguageIdConst.RU      => $"Филиал с кодом '{code}' уже существует.",
            _                       => $"Branch with code '{code}' already exists."
        });
}
