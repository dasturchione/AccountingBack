using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Positions;

public static class PositionErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("Position.NotFound", languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan lavozim topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган лавозим топилмади.",
            LanguageIdConst.RU      => $"Должность с id {id} не найдена.",
            _                       => $"Position with id {id} was not found."
        });

    public static Error CodeConflict(string code, short? languageId = null) =>
        Error.Conflict("Position.CodeConflict", languageId switch
        {
            LanguageIdConst.UZ      => $"Kodi {code} bo'lgan lavozim allaqachon mavjud.",
            LanguageIdConst.UZ_CYRL => $"Коди {code} бўлган лавозим аллақачон мавжуд.",
            LanguageIdConst.RU      => $"Должность с кодом {code} уже существует.",
            _                       => $"Position with code {code} already exists."
        });
}
