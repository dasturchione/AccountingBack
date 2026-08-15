using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.AiAssistant;

public static class AiAssistantErrors
{
    public static Error OrganizationContextUnavailable(short? languageId = null) =>
        Error.Business("AiAssistant.OrganizationContextUnavailable", languageId switch
        {
            LanguageIdConst.UZ => "Tashkilot konteksti aniqlanmadi yoki foydalanish uchun ruxsat yo‘q.",
            LanguageIdConst.UZ_CYRL => "Ташкилот контексти аниқланмади ёки фойдаланиш учун рухсат йўқ.",
            LanguageIdConst.RU => "Контекст организации не определён или доступ запрещён.",
            _ => "The organization context could not be resolved or access is not allowed."
        });
}
