using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Fa;

public static class FaDocumentErrors
{
    public static Error AccountRequired(string roleCode, short? languageId = null) =>
        Error.Business("Fa.AccountRequired", languageId switch
        {
            LanguageIdConst.UZ => $"'{roleCode}' roli uchun hisob ko'rsatilishi shart.",
            LanguageIdConst.UZ_CYRL => $"'{roleCode}' роли учун ҳисоб кўрсатилиши шарт.",
            LanguageIdConst.RU => $"Для роли '{roleCode}' необходимо указать счёт.",
            _ => $"An account is required for role '{roleCode}'."
        });

    public static Error AccountUnavailable(int accountId, short? languageId = null) =>
        Error.Business("Fa.AccountUnavailable", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {accountId} bo'lgan hisob faol emas yoki joriy tashkilotga tegishli emas.",
            LanguageIdConst.UZ_CYRL => $"Id-си {accountId} бўлган ҳисоб фаол эмас ёки жорий ташкилотга тегишли эмас.",
            LanguageIdConst.RU => $"Счёт с id {accountId} неактивен или не принадлежит текущей организации.",
            _ => $"Account with id {accountId} is inactive or does not belong to the current organization."
        });

    public static Error AccountNotAllowed(int accountId, string roleCode, short? languageId = null) =>
        Error.Business("Fa.AccountNotAllowed", languageId switch
        {
            LanguageIdConst.UZ => $"Id-si {accountId} bo'lgan hisob '{roleCode}' roli uchun ruxsat etilmagan.",
            LanguageIdConst.UZ_CYRL => $"Id-си {accountId} бўлган ҳисоб '{roleCode}' роли учун рухсат этилмаган.",
            LanguageIdConst.RU => $"Счёт с id {accountId} нельзя использовать для роли '{roleCode}'.",
            _ => $"Account with id {accountId} is not allowed for role '{roleCode}'."
        });
}
