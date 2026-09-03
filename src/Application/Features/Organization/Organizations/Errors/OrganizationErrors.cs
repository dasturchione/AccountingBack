using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Organizations;

public static class OrganizationErrors
{
    public static Error InnLookupFailed(short? languageId = null) =>
        Error.Problem("Organization.InnLookupFailed", languageId switch
        {
            LanguageIdConst.UZ => "STIR bo'yicha tashkilot ma'lumotlarini olishda xatolik yuz berdi.",
            LanguageIdConst.UZ_CYRL => "СТИР бўйича ташкилот маълумотларини олишда хатолик юз берди.",
            LanguageIdConst.RU => "Не удалось получить данные организации по ИНН.",
            _ => "Organization details could not be retrieved by INN."
        });

    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("Organization.NotFound", GetNotFoundDescription(id, languageId));

    public static Error InnConflict(string inn, short? languageId = null) =>
        Error.Conflict("Organization.InnConflict", GetInnConflictDescription(inn, languageId));

    private static string GetNotFoundDescription(long id, short? languageId) =>
        languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan tashkilot topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган ташкилот топилмади.",
            LanguageIdConst.RU      => $"Организация с id {id} не найдена.",
            _                       => $"Organization with id {id} was not found."
        };

    private static string GetInnConflictDescription(string inn, short? languageId) =>
        languageId switch
        {
            LanguageIdConst.UZ      => $"INN {inn} bo'lgan tashkilot allaqachon mavjud.",
            LanguageIdConst.UZ_CYRL => $"ИНН {inn} бўлган ташкилот аллақачон мавжуд.",
            LanguageIdConst.RU      => $"Организация с ИНН {inn} уже существует.",
            _                       => $"Organization with INN {inn} already exists."
        };
}
