using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Users;

public static class UserErrors
{
    public static Error NotFound(long id, short? languageId = null) =>
        Error.NotFound("User.NotFound", GetNotFoundDescription(id, languageId));

    public static Error NotFoundByUsername(string username, short? languageId = null) =>
        Error.NotFound("User.NotFoundByUsername", GetNotFoundByUsernameDescription(username, languageId));

    public static Error Conflict(string username, short? languageId = null) =>
        Error.Conflict("User.Conflict", GetConflictDescription(username, languageId));

    private static string GetNotFoundDescription(long id, short? languageId)
    {
        return languageId switch
        {
            LanguageIdConst.UZ =>
                $"Id-si {id} bo'lgan foydalanuvchi topilmadi.",

            LanguageIdConst.UZ_CYRL =>
                $"Id-si {id} бўлган фойдаланувчи топилмади.",

            LanguageIdConst.RU =>
                $"Пользователь с id {id} не найден.",

            _ =>
                $"User with id {id} was not found."
        };
    }

    private static string GetNotFoundByUsernameDescription(string username, short? languageId)
    {
        return languageId switch
        {
            LanguageIdConst.UZ =>
                $"{username} nomli foydalanuvchi topilmadi.",
            LanguageIdConst.UZ_CYRL =>
                $"{username} номли фойдаланувчи топилмади.",
            LanguageIdConst.RU =>
                $"Пользователь с username {username} не найден.",
            _ =>
                $"User with username {username} was not found."
        };
    }

    private static string GetConflictDescription(string username, short? languageId)
    {
        return languageId switch
        {
            LanguageIdConst.UZ =>
                $"{username} nomli foydalanuvchi allaqachon mavjud.",
            LanguageIdConst.UZ_CYRL =>
                $"{username} номли фойдаланувчи аллақачон мавжуд.",
            LanguageIdConst.RU =>
                $"Пользователь с username {username} уже существует.",
            _ =>
                $"User with username {username} already exists."
        };
    }
}
