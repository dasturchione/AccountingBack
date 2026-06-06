using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Roles;

public static class RoleErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("Role.NotFound", GetNotFoundDescription(id, languageId));

    public static Error Conflict(string name, short? languageId = null) =>
        Error.Conflict("Role.Conflict", GetConflictDescription(name, languageId));

    private static string GetNotFoundDescription(long id, short? languageId) =>
        languageId switch
        {
            LanguageIdConst.UZ      => $"Id-si {id} bo'lgan rol topilmadi.",
            LanguageIdConst.UZ_CYRL => $"Id-си {id} бўлган рол топилмади.",
            LanguageIdConst.RU      => $"Роль с id {id} не найдена.",
            _                       => $"Role with id {id} was not found."
        };

    private static string GetConflictDescription(string name, short? languageId) =>
        languageId switch
        {
            LanguageIdConst.UZ      => $"{name} nomli rol allaqachon mavjud.",
            LanguageIdConst.UZ_CYRL => $"{name} номли рол аллақачон мавжуд.",
            LanguageIdConst.RU      => $"Роль с названием \"{name}\" уже существует.",
            _                       => $"Role with name \"{name}\" already exists."
        };
}
