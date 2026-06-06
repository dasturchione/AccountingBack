using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Branches;

public static class BranchErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("Branch.NotFound", GetNotFoundDescription(id, languageId));

    public static Error CodeConflict(string code, short? languageId = null) =>
        Error.Conflict("Branch.CodeConflict", GetCodeConflictDescription(code, languageId));

    private static string GetNotFoundDescription(long id, short? languageId) =>
        languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan filial topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган филиал топилмади.",
            LanguageIdConst.RU      => $"Филиал с id {id} не найден.",
            _                       => $"Branch with id {id} was not found."
        };

    private static string GetCodeConflictDescription(string code, short? languageId) =>
        languageId switch
        {
            LanguageIdConst.UZ      => $"Kodi {code} bo'lgan filial allaqachon mavjud.",
            LanguageIdConst.UZ_CYRL => $"Коди {code} бўлган филиал аллақачон мавжуд.",
            LanguageIdConst.RU      => $"Филиал с кодом {code} уже существует.",
            _                       => $"Branch with code {code} already exists."
        };
}
