using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Auth;

public static class AuthErrors
{
    public static Error InvalidCredentials(short? languageId = null) =>
        Error.Unauthorized("Auth.InvalidCredentials", GetInvalidCredentialsDescription(languageId));

    private static string GetInvalidCredentialsDescription(short? languageId)
    {
        return languageId switch
        {
            LanguageIdConst.UZ => "Login yoki parol noto'g'ri.",
            LanguageIdConst.UZ_CYRL => "Логин ёки парол нотўғри.",
            LanguageIdConst.RU => "Неверный логин или пароль.",
            _ => "Invalid login or password."
        };
    }
}
