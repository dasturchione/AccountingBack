using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.CashBoxes;

public static class CashBoxErrors
{
    public static Error NotFound(int id, short? languageId = null) =>
        Error.NotFound("CashBox.NotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan kassa topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган касса топилмади.",
            LanguageIdConst.RU      => $"Касса с id {id} не найдена.",
            _                       => $"Cash box with id {id} was not found."
        });

    public static Error CodeConflict(string code, short? languageId = null) =>
        Error.Conflict("CashBox.CodeConflict", languageId switch
        {
            LanguageIdConst.UZ      => $"Kodi '{code}' bo'lgan kassa allaqachon mavjud.",
            LanguageIdConst.UZ_CYRL => $"Коди '{code}' бўлган касса аллақачон мавжуд.",
            LanguageIdConst.RU      => $"Касса с кодом '{code}' уже существует.",
            _                       => $"Cash box with code '{code}' already exists."
        });
}
