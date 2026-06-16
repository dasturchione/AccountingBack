using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Acc.PostingRules;

public static class PostingRuleErrors
{
    public static Error NotFound(int id, short? languageId = null) =>
        Error.NotFound("PostingRule.NotFound", GetNotFoundDescription(id, languageId));

    public static Error Conflict(string code, short? languageId = null) =>
        Error.Conflict("PostingRule.Conflict", GetConflictDescription(code, languageId));

    public static Error CannotModifyGlobalRule(short? languageId = null) =>
        Error.NotFound("PostingRule.CannotModifyGlobalRule", GetCannotModifyGlobalRuleDescription(languageId));

    private static string GetNotFoundDescription(int id, short? languageId)
    {
        return languageId switch
        {
            LanguageIdConst.UZ => $"Id {id} ga ega provodka qoidasi topilmadi.",
            LanguageIdConst.UZ_CYRL => $"ИД {id} га эга проводка қоидаси топилмади.",
            LanguageIdConst.RU => $"Правило проводок с id {id} не найдено.",
            _ => $"Posting rule with id {id} was not found."
        };
    }

    private static string GetConflictDescription(string code, short? languageId)
    {
        return languageId switch
        {
            LanguageIdConst.UZ => $"Kod {code} bo'yicha o'tkazma qoidasi allaqachon mavjud.",
            LanguageIdConst.UZ_CYRL => $"Код {code} бўйича ўтказма қоидаси аллақачон мавжуд.",
            LanguageIdConst.RU => $"Правило проводок с кодом {code} уже существует.",
            _ => $"Posting rule with code {code} already exists."
        };
    }

    private static string GetCannotModifyGlobalRuleDescription(short? languageId)
    {
        return languageId switch
        {
            LanguageIdConst.RU => "Нельзя изменять глобальное правило проводок.",
            LanguageIdConst.UZ => "Global o'tkazma qoidalarini o'zgartirish mumkin emas.",
            LanguageIdConst.UZ_CYRL => "Глобал ўтказма қоидаларини ўзгартириш мумкин эмас.",
            _ => "Cannot modify global posting rule."
        };
    }
}
